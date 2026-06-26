using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
#endif

public sealed class WindowsPm5BleClient : IPm5BleClient
{
    public static readonly Guid Concept2ServiceUuid = new Guid("ce060000-43e5-11e4-916c-0800200c9a66");

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
        lock (gate)
        {
            discoveredDevices.Clear();
        }

#if ENABLE_WINMD_SUPPORT
        StopScan();
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
        StartPowerShellHelper(BuildScanScript(), HandlePowerShellLine);
#else
        SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        Debug.LogWarning("[Ski-Verse] BLE scanning requires Windows WinRT/WinMD support in this Unity build.");
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
        if (!device.IsValid)
        {
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            return;
        }

#if ENABLE_WINMD_SUPPORT
        _ = ConnectAsync(device);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        StopScan();
        SetStatus(Pm5BleConnectionStatus.Connecting);
        StartPowerShellHelper(BuildConnectScript(device.BluetoothAddress), HandlePowerShellLine);
#else
        SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        Debug.LogWarning("[Ski-Verse] BLE connection requires Windows WinRT/WinMD support in this Unity build.");
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

            SetStatus(connectedDevice != null ? Pm5BleConnectionStatus.Connected : Pm5BleConnectionStatus.ConnectionFailed);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Ski-Verse] PM5 BLE connection failed: " + exception.Message);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        var localName = args.Advertisement.LocalName;
        if (!IsConcept2Pm5Advertisement(localName, args.Advertisement.ServiceUuids))
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
            SetStatus(Pm5BleConnectionStatus.Pm5Found);
        }
    }

    private void OnWatcherStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
    {
        if (Status == Pm5BleConnectionStatus.Searching)
        {
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
            // Keep helper stderr quiet here: this callback runs off Unity's main thread.
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
            helperProcess.Start();
            helperProcess.BeginOutputReadLine();
            helperProcess.BeginErrorReadLine();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Ski-Verse] Could not start Windows PM5 BLE helper: " + exception.Message);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void HandlePowerShellLine(string line)
    {
        if (line.StartsWith("FOUND|", StringComparison.OrdinalIgnoreCase))
        {
            HandlePowerShellDeviceFound(line);
            return;
        }

        if (line.Equals("CONNECTED", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus(Pm5BleConnectionStatus.Connected);
            return;
        }

        if (line.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void HandlePowerShellDeviceFound(string line)
    {
        var parts = line.Split(new[] { '|' }, 3);
        if (parts.Length < 3 || !ulong.TryParse(parts[1], out var bluetoothAddress))
        {
            return;
        }

        var device = new Pm5BleDeviceInfo(parts[1], parts[2], bluetoothAddress);
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
            SetStatus(Pm5BleConnectionStatus.Pm5Found);
        }
    }

    private static string BuildScanScript()
    {
        return @"
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementWatcher,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Foundation.TypedEventHandler`2,Windows.Foundation,ContentType=WindowsRuntime] | Out-Null
$watcher = [Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementWatcher]::new()
$watcher.ScanningMode = [Windows.Devices.Bluetooth.Advertisement.BluetoothLEScanningMode]::Active
$service = [Guid]'ce060000-43e5-11e4-916c-0800200c9a66'
$seen = [hashtable]::Synchronized(@{})
$handler = [Windows.Foundation.TypedEventHandler[Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementWatcher,Windows.Devices.Bluetooth.Advertisement.BluetoothLEAdvertisementReceivedEventArgs]] {
    param($sender, $args)
    $name = $args.Advertisement.LocalName
    $hasService = $false
    foreach ($uuid in $args.Advertisement.ServiceUuids) {
        if ($uuid -eq $service) { $hasService = $true }
    }
    if (($name -match 'PM5|CONCEPT\s?2') -or $hasService) {
        $address = [string]$args.BluetoothAddress
        if (-not $seen.ContainsKey($address)) {
            $seen[$address] = $true
            if ([string]::IsNullOrWhiteSpace($name)) { $name = 'Concept2 PM5' }
            [Console]::WriteLine(('FOUND|{0}|{1}' -f $address, $name))
            [Console]::Out.Flush()
        }
    }
}
$token = $watcher.add_Received($handler)
$watcher.Start()
Start-Sleep -Seconds 20
$watcher.Stop()
$watcher.remove_Received($token)
";
    }

    private static string BuildConnectScript(ulong bluetoothAddress)
    {
        return @"
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Windows.Devices.Bluetooth.BluetoothLEDevice,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Foundation.AsyncStatus,Windows.Foundation,ContentType=WindowsRuntime] | Out-Null
$operation = [Windows.Devices.Bluetooth.BluetoothLEDevice]::FromBluetoothAddressAsync([UInt64]" + bluetoothAddress + @")
$end = (Get-Date).AddSeconds(12)
while ($operation.Status -eq [Windows.Foundation.AsyncStatus]::Started -and (Get-Date) -lt $end) {
    Start-Sleep -Milliseconds 100
}
if ($operation.Status -eq [Windows.Foundation.AsyncStatus]::Completed) {
    $device = $operation.GetResults()
    if ($null -ne $device) {
        [Console]::WriteLine('CONNECTED')
        [Console]::Out.Flush()
        while ($true) { Start-Sleep -Seconds 1 }
    }
}
[Console]::WriteLine('FAILED')
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

        StateChanged?.Invoke();
    }
}
