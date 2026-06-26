using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
#endif

public sealed class WindowsPm5BleClient : IPm5BleClient
{
    public static readonly Guid Concept2ServiceUuid = new Guid("ce060000-43e5-11e4-916c-0800200c9a66");

    private const string LogPrefix = "[Ski-Verse PM5 BLE] ";

    private readonly object gate = new object();
    private readonly List<Pm5BleDeviceInfo> discoveredDevices = new List<Pm5BleDeviceInfo>();
    private Pm5BleConnectionStatus status = Pm5BleConnectionStatus.NotConnected;

#if ENABLE_WINMD_SUPPORT
    private BluetoothLEAdvertisementWatcher watcher;
    private BluetoothLEDevice connectedDevice;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private System.Diagnostics.Process helperProcess;
#endif

    public event Action StateChanged;

    public Pm5BleConnectionStatus Status
    {
        get
        {
            lock (gate)
            {
                return status;
            }
        }
    }

    public IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices
    {
        get
        {
            lock (gate)
            {
                return discoveredDevices.ToArray();
            }
        }
    }

    public void StartScan()
    {
        Log("Start scan requested. This is real Windows BLE scanning, not placeholder UI.");

        lock (gate)
        {
            discoveredDevices.Clear();
        }

#if ENABLE_WINMD_SUPPORT
        StopScan();
        Log("Using Unity WinMD BluetoothLEAdvertisementWatcher.");
        watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };
        watcher.Received += OnAdvertisementReceived;
        watcher.Stopped += OnWatcherStopped;
        SetStatus(Pm5BleConnectionStatus.Searching);
        watcher.Start();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        StopScan();
        SetStatus(Pm5BleConnectionStatus.Searching);
        Log("Using Windows PowerShell WinRT BLE helper with AdvertisementWatcher + DeviceWatcher.");
        StartPowerShellHelper(BuildScanScript(), HandlePowerShellLine);
#else
        SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        LogWarning("BLE scanning requires Windows WinRT/WinMD support in this Unity build.");
#endif
    }

    public void StopScan()
    {
#if ENABLE_WINMD_SUPPORT
        if (watcher == null)
        {
            return;
        }

        watcher.Received -= OnAdvertisementReceived;
        watcher.Stopped -= OnWatcherStopped;
        if (watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started ||
            watcher.Status == BluetoothLEAdvertisementWatcherStatus.Created)
        {
            watcher.Stop();
        }

        watcher = null;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (helperProcess == null)
        {
            return;
        }

        try
        {
            if (!helperProcess.HasExited)
            {
                helperProcess.Kill();
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Ski-Verse] Could not stop PM5 BLE helper: " + exception.Message);
        }
        finally
        {
            helperProcess.Dispose();
            helperProcess = null;
        }
#endif
    }

    public void Connect(Pm5BleDeviceInfo device)
    {
        Log($"Connection attempt started. Name='{device.Name}', DeviceId='{device.DeviceId}', Address='{device.BluetoothAddress}'.");

        if (!device.IsValid)
        {
            LogWarning("Connection failed before start: selected PM5 device has no BLE id/address.");
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            return;
        }

#if ENABLE_WINMD_SUPPORT
        _ = ConnectAsync(device);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        StopScan();
        SetStatus(Pm5BleConnectionStatus.Connecting);
        StartPowerShellHelper(BuildConnectScript(device), HandlePowerShellLine);
#else
        SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        LogWarning("BLE connection requires Windows WinRT/WinMD support in this Unity build.");
#endif
    }

    public static bool IsConcept2Pm5Advertisement(string localName, IEnumerable<Guid> serviceUuids)
    {
        if (!string.IsNullOrWhiteSpace(localName))
        {
            var name = localName.ToUpperInvariant();
            if (name.Contains("PM5") || name.Contains("CONCEPT2") || name.Contains("CONCEPT 2"))
            {
                return true;
            }
        }

        if (serviceUuids == null)
        {
            return false;
        }

        foreach (var serviceUuid in serviceUuids)
        {
            if (serviceUuid == Concept2ServiceUuid)
            {
                return true;
            }
        }

        return false;
    }

#if ENABLE_WINMD_SUPPORT
    private async Task ConnectAsync(Pm5BleDeviceInfo device)
    {
        SetStatus(Pm5BleConnectionStatus.Connecting);
        StopScan();

        try
        {
            connectedDevice?.Dispose();
            connectedDevice = device.BluetoothAddress != 0
                ? await BluetoothLEDevice.FromBluetoothAddressAsync(device.BluetoothAddress)
                : await BluetoothLEDevice.FromIdAsync(device.DeviceId);

            if (connectedDevice == null)
            {
                LogWarning("GATT connection failure: Windows returned null BluetoothLEDevice.");
                SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
                return;
            }

            Log($"BluetoothLEDevice resolved. Name='{connectedDevice.Name}'. Verifying Concept2 PM5 GATT service...");
            var services = await connectedDevice.GetGattServicesForUuidAsync(Concept2ServiceUuid, BluetoothCacheMode.Uncached);
            if (services.Status == GattCommunicationStatus.Success && services.Services.Count > 0)
            {
                Log($"GATT connection success. Found Concept2 PM5 service count={services.Services.Count}.");
                SetStatus(Pm5BleConnectionStatus.Connected);
                return;
            }

            LogWarning($"GATT connection failure. Status={services.Status}, ServiceCount={services.Services.Count}.");
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
        catch (Exception exception)
        {
            LogWarning("GATT connection exception: " + exception.Message);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        var localName = args.Advertisement.LocalName;
        var isPm5 = IsConcept2Pm5Advertisement(localName, args.Advertisement.ServiceUuids);
        Log($"Advertisement discovered. Name='{localName}', Address='{args.BluetoothAddress}', MatchesPM5='{isPm5}'.");

        if (!isPm5)
        {
            return;
        }

        var device = new Pm5BleDeviceInfo(args.BluetoothAddress.ToString("X"), localName, args.BluetoothAddress);
        var added = false;

        lock (gate)
        {
            if (!ContainsDeviceLocked(device))
            {
                discoveredDevices.Add(device);
                added = true;
            }
        }

        if (added)
        {
            Log($"PM5 Found from advertisement. Name='{device.Name}', Address='{device.BluetoothAddress}'.");
            SetStatus(Pm5BleConnectionStatus.Pm5Found);
        }
    }

    private void OnWatcherStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
    {
        if (Status == Pm5BleConnectionStatus.Searching)
        {
            LogWarning("Advertisement watcher stopped while still searching.");
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }
#endif

#if !ENABLE_WINMD_SUPPORT && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    private void StartPowerShellHelper(string script, Action<string> lineHandler)
    {
        var encodedScript = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -EncodedCommand " + encodedScript,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        helperProcess = new System.Diagnostics.Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
        helperProcess.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                lineHandler(args.Data);
            }
        };
        helperProcess.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                LogWarning("Windows BLE helper stderr: " + args.Data);
            }
        };
        helperProcess.Exited += (sender, args) =>
        {
            if (Status == Pm5BleConnectionStatus.Searching || Status == Pm5BleConnectionStatus.Connecting)
            {
                SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            }
        };

        try
        {
            Log("Starting Windows BLE helper process.");
            helperProcess.Start();
            helperProcess.BeginOutputReadLine();
            helperProcess.BeginErrorReadLine();
        }
        catch (Exception exception)
        {
            LogWarning("Could not start Windows PM5 BLE helper: " + exception.Message);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void HandlePowerShellLine(string line)
    {
        if (line.StartsWith("LOG|", StringComparison.OrdinalIgnoreCase))
        {
            Log(line.Substring(4));
            return;
        }

        if (line.StartsWith("ERROR|", StringComparison.OrdinalIgnoreCase))
        {
            LogWarning(line.Substring(6));
            return;
        }

        if (line.StartsWith("FOUND|", StringComparison.OrdinalIgnoreCase))
        {
            HandlePowerShellDeviceFound(line);
            return;
        }

        if (line.StartsWith("CONNECTED|", StringComparison.OrdinalIgnoreCase) || line.Equals("CONNECTED", StringComparison.OrdinalIgnoreCase))
        {
            Log("Connection success reported by Windows BLE helper: " + line);
            SetStatus(Pm5BleConnectionStatus.Connected);
            return;
        }

        if (line.StartsWith("FAILED|", StringComparison.OrdinalIgnoreCase) || line.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
        {
            LogWarning("Connection failed reported by Windows BLE helper: " + line);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            return;
        }

        Log("Windows BLE helper output: " + line);
    }

    private void HandlePowerShellDeviceFound(string line)
    {
        var parts = line.Split('|');
        if (parts.Length < 5)
        {
            LogWarning("Could not parse PM5 found line: " + line);
            return;
        }

        var source = parts[1];
        var deviceId = parts[2];
        var bluetoothAddress = ulong.TryParse(parts[3], out var parsedAddress) ? parsedAddress : 0;
        var name = parts[4];

        var device = new Pm5BleDeviceInfo(deviceId, name, bluetoothAddress);
        var added = false;

        lock (gate)
        {
            if (!ContainsDeviceLocked(device))
            {
                discoveredDevices.Add(device);
                added = true;
            }
        }

        Log($"PM5 Found from {source}. Name='{device.Name}', DeviceId='{device.DeviceId}', Address='{device.BluetoothAddress}', Added='{added}'.");

        if (added)
        {
            SetStatus(Pm5BleConnectionStatus.Pm5Found);
        }
    }

    private static string BuildScanScript()
    {
        return @"
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    [Console]::WriteLine(('ERROR|Windows BLE scan helper error: {0}' -f $_.Exception.Message))
    [Console]::Out.Flush()
    exit 1
}
[Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementWatcher,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.BluetoothLEDevice,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Enumeration.DeviceInformation,Windows.Devices.Enumeration,ContentType=WindowsRuntime] | Out-Null
[Windows.Foundation.TypedEventHandler`2,Windows.Foundation,ContentType=WindowsRuntime] | Out-Null
[Console]::WriteLine('LOG|Windows BLE scan helper started. Scanning advertisements and BLE device list for 20 seconds.')
$service = [Guid]'ce060000-43e5-11e4-916c-0800200c9a66'
$seen = [hashtable]::Synchronized(@{})
function Test-Pm5Name($name) {
    if ([string]::IsNullOrWhiteSpace($name)) { return $false }
    return ($name -match 'PM5|CONCEPT\s?2')
}
function Get-BluetoothAddressFromPnpId($instanceId) {
    if ([string]::IsNullOrWhiteSpace($instanceId)) { return '0' }
    $match = [regex]::Match($instanceId, 'DEV_([0-9A-Fa-f]{12})')
    if (-not $match.Success) { return '0' }
    return [string][Convert]::ToUInt64($match.Groups[1].Value, 16)
}
function Report-Pm5($source, $deviceId, $address, $name) {
    if ([string]::IsNullOrWhiteSpace($name)) { $name = 'Concept2 PM5' }
    $key = ('{0}|{1}' -f $source, $deviceId)
    if (-not $seen.ContainsKey($key)) {
        $seen[$key] = $true
        [Console]::WriteLine(('FOUND|{0}|{1}|{2}|{3}' -f $source, $deviceId, $address, $name))
        [Console]::Out.Flush()
    }
}
$advertisementWatcher = [Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementWatcher]::new()
$advertisementWatcher.ScanningMode = [Windows.Devices.Bluetooth.Advertisement.BluetoothLEScanningMode]::Active
$advertisementHandler = [Windows.Foundation.TypedEventHandler[Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementWatcher,Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementReceivedEventArgs]] {
    param($sender, $args)
    $name = $args.Advertisement.LocalName
    $hasService = $false
    foreach ($uuid in $args.Advertisement.ServiceUuids) {
        if ($uuid -eq $service) { $hasService = $true }
    }
    $matches = (Test-Pm5Name $name) -or $hasService
    [Console]::WriteLine(('LOG|Advertisement discovered. Name=""{0}"", Address={1}, HasConcept2Service={2}, MatchesPM5={3}' -f $name, $args.BluetoothAddress, $hasService, $matches))
    [Console]::Out.Flush()
    if ($matches) {
        Report-Pm5 'Advertisement' ([string]$args.BluetoothAddress) ([string]$args.BluetoothAddress) $name
    }
}
$selector = [Windows.Devices.Bluetooth.BluetoothLEDevice]::GetDeviceSelector()
$deviceWatcher = [Windows.Devices.Enumeration.DeviceInformation]::CreateWatcher($selector)
$deviceHandler = [Windows.Foundation.TypedEventHandler[Windows.Devices.Enumeration.DeviceWatcher,Windows.Devices.Enumeration.DeviceInformation]] {
    param($sender, $deviceInfo)
    $matches = Test-Pm5Name $deviceInfo.Name
    [Console]::WriteLine(('LOG|BLE DeviceWatcher discovered. Name=""{0}"", Id=""{1}"", MatchesPM5={2}' -f $deviceInfo.Name, $deviceInfo.Id, $matches))
    [Console]::Out.Flush()
    if ($matches) {
        Report-Pm5 'DeviceWatcher' $deviceInfo.Id '0' $deviceInfo.Name
    }
}
try {
    [Console]::WriteLine('LOG|Checking Windows PnP BTHLE devices for already-known PM5 devices.')
    foreach ($pnpDevice in Get-PnpDevice -ErrorAction Stop) {
        $name = [string]$pnpDevice.FriendlyName
        $id = [string]$pnpDevice.InstanceId
        $isBle = $id -like 'BTHLE*'
        $matches = $isBle -and ((Test-Pm5Name $name) -or (Test-Pm5Name $id))
        if ($isBle) {
            [Console]::WriteLine(('LOG|PnP BTHLE device. Name=""{0}"", Id=""{1}"", MatchesPM5={2}' -f $name, $id, $matches))
            [Console]::Out.Flush()
        }
        if ($matches) {
            $address = Get-BluetoothAddressFromPnpId $id
            Report-Pm5 'WindowsPnP' $id $address $name
        }
    }
} catch {
    [Console]::WriteLine(('ERROR|Windows PnP PM5 lookup failed: {0}' -f $_.Exception.Message))
    [Console]::Out.Flush()
}
$advertisementToken = $advertisementWatcher.add_Received($advertisementHandler)
$deviceToken = $deviceWatcher.add_Added($deviceHandler)
$advertisementWatcher.Start()
$deviceWatcher.Start()
Start-Sleep -Seconds 20
$advertisementWatcher.Stop()
$deviceWatcher.Stop()
$advertisementWatcher.remove_Received($advertisementToken)
$deviceWatcher.remove_Added($deviceToken)
[Console]::WriteLine('LOG|Windows BLE scan helper finished.')
";
    }

    private static string BuildConnectScript(Pm5BleDeviceInfo device)
    {
        var encodedDeviceId = Convert.ToBase64String(Encoding.UTF8.GetBytes(device.DeviceId ?? string.Empty));

        return @"
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Add-Type -AssemblyName System.Runtime.WindowsRuntime
trap {
    [Console]::WriteLine(('ERROR|Windows BLE connect helper error: {0}' -f $_.Exception.Message))
    [Console]::Out.Flush()
    exit 1
}
[Windows.Devices.Bluetooth.BluetoothLEDevice,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceServicesResult,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
function Await-WinRtOperation($operation, [Type]$resultType, [int]$timeoutMs) {
    $method = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { $_.Name -eq 'AsTask' -and $_.IsGenericMethodDefinition -and $_.GetGenericArguments().Length -eq 1 -and $_.GetParameters().Length -eq 1 } | Select-Object -First 1
    $task = $method.MakeGenericMethod($resultType).Invoke($null, @($operation))
    if (-not $task.Wait($timeoutMs)) { throw 'Timed out waiting for Windows BLE operation.' }
    if ($task.IsFaulted) { throw $task.Exception.GetBaseException().Message }
    return $task.Result
}
$deviceId = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + encodedDeviceId + @"'))
$address = [UInt64]" + device.BluetoothAddress + @"
$service = [Guid]'ce060000-43e5-11e4-916c-0800200c9a66'
[Console]::WriteLine(('LOG|Windows BLE connect helper started. DeviceId=""{0}"", Address={1}' -f $deviceId, $address))
if (-not [string]::IsNullOrWhiteSpace($deviceId) -and $address -eq 0) {
    [Console]::WriteLine('LOG|Connecting with BluetoothLEDevice.FromIdAsync.')
    $device = Await-WinRtOperation ([Windows.Devices.Bluetooth.BluetoothLEDevice]::FromIdAsync($deviceId)) ([Windows.Devices.Bluetooth.BluetoothLEDevice]) 12000
} else {
    [Console]::WriteLine('LOG|Connecting with BluetoothLEDevice.FromBluetoothAddressAsync.')
    $device = Await-WinRtOperation ([Windows.Devices.Bluetooth.BluetoothLEDevice]::FromBluetoothAddressAsync($address)) ([Windows.Devices.Bluetooth.BluetoothLEDevice]) 12000
}
if ($null -eq $device) {
    [Console]::WriteLine('FAILED|BluetoothLEDevice resolve returned null.')
    [Console]::Out.Flush()
    exit 1
}
[Console]::WriteLine(('LOG|BluetoothLEDevice resolved. Name=""{0}"", DeviceId=""{1}"". Verifying Concept2 PM5 GATT service.' -f $device.Name, $device.DeviceId))
[Console]::Out.Flush()
$servicesResult = Await-WinRtOperation ($device.GetGattServicesForUuidAsync($service, [Windows.Devices.Bluetooth.BluetoothCacheMode]::Uncached)) ([Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceServicesResult]) 12000
$serviceCount = 0
if ($null -ne $servicesResult.Services) { $serviceCount = $servicesResult.Services.Count }
if ($servicesResult.Status -eq [Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus]::Success -and $serviceCount -gt 0) {
    [Console]::WriteLine(('CONNECTED|GATT Concept2 PM5 service verified. ServiceCount={0}, DeviceName=""{1}""' -f $serviceCount, $device.Name))
    [Console]::Out.Flush()
    while ($true) { Start-Sleep -Seconds 1 }
}
[Console]::WriteLine(('FAILED|GATT service verification failed. GattStatus={0}, ServiceCount={1}' -f $servicesResult.Status, $serviceCount))
[Console]::Out.Flush()
exit 1
";
    }
#endif

    private bool ContainsDeviceLocked(Pm5BleDeviceInfo device)
    {
        for (var i = 0; i < discoveredDevices.Count; i++)
        {
            if (discoveredDevices[i].BluetoothAddress != 0 && discoveredDevices[i].BluetoothAddress == device.BluetoothAddress)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(device.DeviceId) && discoveredDevices[i].DeviceId == device.DeviceId)
            {
                return true;
            }
        }

        return false;
    }

    private void SetStatus(Pm5BleConnectionStatus newStatus)
    {
        lock (gate)
        {
            status = newStatus;
        }

        Log("Status changed: " + newStatus);
        StateChanged?.Invoke();
    }

    private static void Log(string message)
    {
        Debug.Log(LogPrefix + message);
    }

    private static void LogWarning(string message)
    {
        Debug.LogWarning(LogPrefix + message);
    }
}
