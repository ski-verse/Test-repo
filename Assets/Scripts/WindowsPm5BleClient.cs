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

public sealed class WindowsPm5BleClient : IPm5BleClient, IPm5WorkoutDataClient
{
    public static readonly Guid Concept2ServiceUuid = new Guid("ce060000-43e5-11e4-916c-0800200c9a66");
    public static readonly Guid Concept2WorkoutDataServiceUuid = new Guid("ce060030-43e5-11e4-916c-0800200c9a66");

    private const string LogPrefix = "[Ski-Verse PM5 BLE] ";

    private readonly object gate = new object();
    private readonly List<Pm5BleDeviceInfo> discoveredDevices = new List<Pm5BleDeviceInfo>();
    private Pm5BleConnectionStatus status = Pm5BleConnectionStatus.NotConnected;
    private Pm5WorkoutMetrics latestWorkoutMetrics;

#if ENABLE_WINMD_SUPPORT
    private BluetoothLEAdvertisementWatcher watcher;
    private BluetoothLEDevice connectedDevice;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private System.Diagnostics.Process helperProcess;
    private string helperScriptPath;
    private string helperSourcePath;
#endif

    public event Action StateChanged;
    public event Action WorkoutDataChanged;

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

    public bool HasWorkoutData
    {
        get
        {
            lock (gate)
            {
                return latestWorkoutMetrics.HasAnyMetrics;
            }
        }
    }

    public Pm5WorkoutMetrics LatestWorkoutMetrics
    {
        get
        {
            lock (gate)
            {
                return latestWorkoutMetrics;
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
            DeleteHelperScriptFile();
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
        StartCSharpConnectHelper(device, HandlePowerShellLine);
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
            if (IsKnownConcept2ServiceUuid(serviceUuid))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsKnownConcept2ServiceUuid(Guid serviceUuid)
    {
        return serviceUuid == Concept2ServiceUuid ||
               serviceUuid == Concept2WorkoutDataServiceUuid ||
               serviceUuid == new Guid("ce060020-43e5-11e4-916c-0800200c9a66") ||
               serviceUuid == new Guid("ce060010-43e5-11e4-916c-0800200c9a66");
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

            Log($"BluetoothLEDevice resolved. Name='{connectedDevice.Name}'. Verifying Concept2 PM5 workout GATT service...");
            var services = await connectedDevice.GetGattServicesForUuidAsync(Concept2WorkoutDataServiceUuid, BluetoothCacheMode.Uncached);
            if (services.Status == GattCommunicationStatus.Success && services.Services.Count > 0)
            {
                Log($"GATT connection success. Found Concept2 PM5 workout service count={services.Services.Count}.");
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
        DeleteHelperScriptFile();

        try
        {
            helperScriptPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "SkiVersePm5Ble-" + Guid.NewGuid().ToString("N") + ".ps1");
            System.IO.File.WriteAllText(helperScriptPath, script, Encoding.UTF8);
        }
        catch (Exception exception)
        {
            LogWarning("Could not write Windows PM5 BLE helper script: " + exception.Message);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            return;
        }

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -File " + QuoteProcessArgument(helperScriptPath),
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
            DeleteHelperScriptFile();
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void StartCSharpConnectHelper(Pm5BleDeviceInfo device, Action<string> lineHandler)
    {
        DeleteHelperScriptFile();

        var helperId = Guid.NewGuid().ToString("N");
        helperSourcePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SkiVersePm5Ble-" + helperId + ".cs");
        helperScriptPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SkiVersePm5Ble-" + helperId + ".exe");

        try
        {
            System.IO.File.WriteAllText(helperSourcePath, BuildCSharpConnectHelperSource(), Encoding.UTF8);
        }
        catch (Exception exception)
        {
            LogWarning("Could not write C# PM5 BLE helper source: " + exception.Message + ". Falling back to PowerShell helper.");
            StartPowerShellHelper(BuildConnectScript(device), lineHandler);
            return;
        }

        var compilerPath = GetCSharpCompilerPath();
        if (string.IsNullOrWhiteSpace(compilerPath))
        {
            LogWarning("Could not find csc.exe for C# PM5 BLE helper. Falling back to PowerShell helper.");
            StartPowerShellHelper(BuildConnectScript(device), lineHandler);
            return;
        }

        var compileArguments =
            "/nologo " +
            "/out:" + QuoteProcessArgument(helperScriptPath) + " " +
            "/r:" + QuoteProcessArgument(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Runtime.dll") + " " +
            "/r:" + QuoteProcessArgument(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Runtime.InteropServices.WindowsRuntime.dll") + " " +
            "/r:" + QuoteProcessArgument(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Runtime.WindowsRuntime.dll") + " " +
            "/r:" + QuoteProcessArgument(@"C:\Windows\System32\WinMetadata\Windows.Foundation.winmd") + " " +
            "/r:" + QuoteProcessArgument(@"C:\Windows\System32\WinMetadata\Windows.Devices.winmd") + " " +
            "/r:" + QuoteProcessArgument(@"C:\Windows\System32\WinMetadata\Windows.Storage.winmd") + " " +
            QuoteProcessArgument(helperSourcePath);

        var compileStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = compilerPath,
            Arguments = compileArguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            Log("Compiling C# Windows BLE connect helper.");
            using (var compileProcess = System.Diagnostics.Process.Start(compileStartInfo))
            {
                var compileOutput = compileProcess.StandardOutput.ReadToEnd();
                var compileError = compileProcess.StandardError.ReadToEnd();
                compileProcess.WaitForExit(20000);
                if (!compileProcess.HasExited || compileProcess.ExitCode != 0)
                {
                    LogWarning("C# PM5 BLE helper compile failed. Output='" + compileOutput + "' Error='" + compileError + "'. Falling back to PowerShell helper.");
                    StartPowerShellHelper(BuildConnectScript(device), lineHandler);
                    return;
                }
            }
        }
        catch (Exception exception)
        {
            LogWarning("Could not compile C# PM5 BLE helper: " + exception.Message + ". Falling back to PowerShell helper.");
            StartPowerShellHelper(BuildConnectScript(device), lineHandler);
            return;
        }

        var encodedDeviceId = Convert.ToBase64String(Encoding.UTF8.GetBytes(device.DeviceId ?? string.Empty));
        var helperArguments = QuoteProcessArgument(encodedDeviceId) + " " + device.BluetoothAddress;
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = helperScriptPath,
            Arguments = helperArguments,
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
                LogWarning("C# Windows BLE helper stderr: " + args.Data);
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
            Log("Starting C# Windows BLE connect helper process.");
            helperProcess.Start();
            helperProcess.BeginOutputReadLine();
            helperProcess.BeginErrorReadLine();
        }
        catch (Exception exception)
        {
            LogWarning("Could not start C# PM5 BLE helper: " + exception.Message + ". Falling back to PowerShell helper.");
            DeleteHelperScriptFile();
            StartPowerShellHelper(BuildConnectScript(device), lineHandler);
        }
    }

    private static string GetCSharpCompilerPath()
    {
        var compilerPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe";
        if (System.IO.File.Exists(compilerPath))
        {
            return compilerPath;
        }

        compilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
        return System.IO.File.Exists(compilerPath) ? compilerPath : null;
    }

    private static string BuildCSharpConnectHelperSource()
    {
        return @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;

internal static class SkiVersePm5BleConnectHelper
{
    private static readonly Guid WorkoutServiceUuid = new Guid(""ce060030-43e5-11e4-916c-0800200c9a66"");
    private static readonly Guid MultiplexedInformationUuid = new Guid(""ce060080-43e5-11e4-916c-0800200c9a66"");
    private static readonly Guid RowingStrokeDataUuid = new Guid(""ce060035-43e5-11e4-916c-0800200c9a66"");
    private static readonly Guid RowingAdditionalStrokeDataUuid = new Guid(""ce060036-43e5-11e4-916c-0800200c9a66"");
    private static readonly Guid RowingAdditionalStatus1Uuid = new Guid(""ce060032-43e5-11e4-916c-0800200c9a66"");
    private static readonly Guid RowingAdditionalStatus2Uuid = new Guid(""ce060033-43e5-11e4-916c-0800200c9a66"");

    private sealed class Subscription
    {
        public GattCharacteristic Characteristic;
        public TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> Handler;
        public string Name;
    }

    public static int Main(string[] args)
    {
        try
        {
            return MainAsync(args).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            Error(""C# Windows BLE connect helper fatal error: "" + exception);
            return 1;
        }
    }

    private static async Task<int> MainAsync(string[] args)
    {
        var encodedDeviceId = args.Length > 0 ? args[0] : string.Empty;
        var deviceId = string.Empty;
        if (!string.IsNullOrWhiteSpace(encodedDeviceId))
        {
            deviceId = Encoding.UTF8.GetString(Convert.FromBase64String(encodedDeviceId));
        }

        ulong address = 0;
        if (args.Length > 1)
        {
            ulong.TryParse(args[1], out address);
        }

        Log(string.Format(""C# Windows BLE connect helper started. DeviceId=\""{0}\"", Address={1}"", deviceId, address));

        var directService = await TryOpenWorkoutServiceFromSelector(address);
        if (directService != null)
        {
            await StartWorkoutNotifications(directService, ""Concept2 PM5"");
            return 0;
        }

        BluetoothLEDevice device;
        if (!string.IsNullOrWhiteSpace(deviceId) && address == 0)
        {
            Log(""Connecting with BluetoothLEDevice.FromIdAsync."");
            device = await TimeoutAfter(BluetoothLEDevice.FromIdAsync(deviceId), 12000, ""BluetoothLEDevice.FromIdAsync"");
        }
        else
        {
            Log(""Connecting with BluetoothLEDevice.FromBluetoothAddressAsync."");
            device = await TimeoutAfter(BluetoothLEDevice.FromBluetoothAddressAsync(address), 12000, ""BluetoothLEDevice.FromBluetoothAddressAsync"");
        }

        if (device == null)
        {
            Console.WriteLine(""FAILED|BluetoothLEDevice resolve returned null."");
            Flush();
            return 1;
        }

        Log(string.Format(""BluetoothLEDevice resolved. Name=\""{0}\"", DeviceId=\""{1}\"". Verifying Concept2 PM5 workout GATT service {2}."", device.Name, device.DeviceId, WorkoutServiceUuid));

        var lastServiceStatus = ""NotAttempted"";
        var lastServiceCount = 0;
        foreach (var cacheMode in new[] { BluetoothCacheMode.Cached, BluetoothCacheMode.Uncached })
        {
            try
            {
                Log(string.Format(""PM5 workout service lookup started. CacheMode={0}"", cacheMode));
                var servicesResult = await TimeoutAfter(device.GetGattServicesForUuidAsync(WorkoutServiceUuid, cacheMode), 20000, ""PM5 workout service lookup"");
                var serviceCount = servicesResult.Services == null ? 0 : servicesResult.Services.Count;
                lastServiceStatus = servicesResult.Status.ToString();
                lastServiceCount = serviceCount;
                Log(string.Format(""PM5 workout service lookup result. CacheMode={0}, GattStatus={1}, ServiceCount={2}"", cacheMode, servicesResult.Status, serviceCount));
                if (servicesResult.Status == GattCommunicationStatus.Success && serviceCount > 0)
                {
                    await StartWorkoutNotifications(servicesResult.Services[0], device.Name);
                    return 0;
                }
            }
            catch (Exception exception)
            {
                lastServiceStatus = exception.Message;
                Error(string.Format(""PM5 workout service lookup failed. CacheMode={0}, Error={1}"", cacheMode, exception.Message));
            }
        }

        Console.WriteLine(string.Format(""FAILED|GATT workout service verification failed. LastStatus={0}, LastServiceCount={1}"", lastServiceStatus, lastServiceCount));
        Flush();
        return 1;
    }

    private static async Task<GattDeviceService> TryOpenWorkoutServiceFromSelector(ulong address)
    {
        var addressHex = address == 0 ? string.Empty : address.ToString(""X12"");
        try
        {
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(WorkoutServiceUuid);
            Log(""Looking up PM5 workout service with WinRT selector: "" + selector);
            var serviceDevices = await TimeoutAfter(DeviceInformation.FindAllAsync(selector), 20000, ""PM5 workout service selector lookup"");
            var count = serviceDevices == null ? 0 : serviceDevices.Count;
            Log(""WinRT workout service lookup returned Count="" + count);
            if (serviceDevices == null)
            {
                return null;
            }

            foreach (var serviceDevice in serviceDevices)
            {
                var id = serviceDevice.Id ?? string.Empty;
                var matchesAddress = string.IsNullOrWhiteSpace(addressHex) || id.ToUpperInvariant().Contains(addressHex);
                Log(string.Format(""WinRT workout service candidate. Name=\""{0}\"", Id=\""{1}\"", IsEnabled={2}, Access={3}, MatchesAddress={4}"", serviceDevice.Name, id, serviceDevice.IsEnabled, GetDeviceAccessStatus(id), matchesAddress));
                if (!matchesAddress)
                {
                    continue;
                }

                Log(string.Format(""Connecting directly with GattDeviceService.FromIdAsync for PM5 workout service. Access={0}"", GetDeviceAccessStatus(id)));
                var service = await TimeoutAfter(GattDeviceService.FromIdAsync(id), 20000, ""GattDeviceService.FromIdAsync"");
                if (service != null)
                {
                    return service;
                }
            }
        }
        catch (Exception exception)
        {
            Error(""PM5 workout service WinRT lookup failed: "" + exception.Message);
        }

        return null;
    }

    private static async Task StartWorkoutNotifications(GattDeviceService service, string deviceName)
    {
        Console.WriteLine(string.Format(""CONNECTED|GATT Concept2 PM5 workout service verified. DeviceName=\""{0}\"""", deviceName));
        Flush();

        try
        {
            var session = service.Session;
            if (session != null)
            {
                session.MaintainConnection = true;
                Log(""PM5 GATT session maintain connection enabled. MaxPduSize="" + session.MaxPduSize);
            }
        }
        catch (Exception exception)
        {
            Error(""PM5 GATT session setup failed: "" + exception.Message);
        }

        var subscriptions = new List<Subscription>();
        var subscriptionAttempt = 0;
        while (true)
        {
            if (subscriptions.Count == 0)
            {
                subscriptionAttempt++;
                Log(""PM5 workout data subscription attempt "" + subscriptionAttempt + "" started."");
                var primaryName = ""Multiplexed Information"";
                var primarySubscription = await SubscribeCharacteristic(service, ""Multiplexed Information"", MultiplexedInformationUuid);
                if (primarySubscription == null)
                {
                    Log(""PM5 multiplexed workout data subscription unavailable. Falling back to direct Rowing Stroke Data."");
                    primaryName = ""Rowing Stroke Data"";
                    primarySubscription = await SubscribeCharacteristic(service, ""Rowing Stroke Data"", RowingStrokeDataUuid);
                }

                if (primarySubscription != null)
                {
                    subscriptions.Add(primarySubscription);
                    Log(""PM5 primary workout data subscription active. Name="" + primaryName);
                    if (primaryName == ""Rowing Stroke Data"")
                    {
                        await AddOptionalSubscription(subscriptions, service, ""Rowing Additional Stroke Data"", RowingAdditionalStrokeDataUuid);
                        await AddOptionalSubscription(subscriptions, service, ""Rowing Additional Status 1"", RowingAdditionalStatus1Uuid);
                        await AddOptionalSubscription(subscriptions, service, ""Rowing Additional Status 2"", RowingAdditionalStatus2Uuid);
                    }
                    else
                    {
                        Log(""PM5 multiplexed workout data subscription active. Direct characteristic subscriptions skipped per Concept2 multiplexing rules."");
                    }
                }
                else
                {
                    Log(""PM5 primary workout data subscription unavailable. Optional subscriptions deferred."");
                }

                Log(""PM5 workout data subscriptions active. Count="" + subscriptions.Count);
                if (subscriptions.Count == 0)
                {
                    Log(""PM5 workout data subscriptions unavailable. Retrying while PM5 connection stays alive."");
                }
            }

            await Task.Delay(subscriptions.Count == 0 ? 3000 : 1000);
        }
    }

    private static async Task AddOptionalSubscription(List<Subscription> subscriptions, GattDeviceService service, string name, Guid uuid)
    {
        await Task.Delay(500);
        var subscription = await SubscribeCharacteristic(service, name, uuid);
        if (subscription != null)
        {
            subscriptions.Add(subscription);
        }
    }

    private static async Task<Subscription> SubscribeCharacteristic(GattDeviceService service, string name, Guid uuid)
    {
        foreach (var cacheMode in new[] { BluetoothCacheMode.Uncached, BluetoothCacheMode.Cached })
        {
            GattCharacteristic characteristic = null;
            try
            {
                Log(string.Format(""PM5 characteristic lookup started. Name={0}, Uuid={1}, CacheMode={2}"", name, uuid, cacheMode));
                var result = await TimeoutAfter(service.GetCharacteristicsForUuidAsync(uuid, cacheMode), 8000, ""PM5 characteristic lookup "" + name);
                var count = result.Characteristics == null ? 0 : result.Characteristics.Count;
                Log(string.Format(""PM5 characteristic lookup result. Name={0}, Uuid={1}, CacheMode={2}, Status={3}, Count={4}"", name, uuid, cacheMode, result.Status, count));
                if (result.Status == GattCommunicationStatus.Success && count > 0)
                {
                    characteristic = result.Characteristics[0];
                }
            }
            catch (Exception exception)
            {
                Error(string.Format(""PM5 characteristic lookup failed. Name={0}, Uuid={1}, CacheMode={2}, Error={3}"", name, uuid, cacheMode, exception.Message));
            }

            if (characteristic == null)
            {
                continue;
            }

            TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> handler = delegate(GattCharacteristic sender, GattValueChangedEventArgs args)
            {
                try
                {
                    Console.WriteLine(string.Format(""METRIC_RAW|{0}|{1}|{2}"", name, uuid, ConvertBufferToHex(args.CharacteristicValue)));
                    Flush();
                }
                catch (Exception exception)
                {
                    Error(""PM5 workout notification parse failed for "" + name + "": "" + exception.Message);
                }
            };

            var handlerAttached = false;
            Exception subscribeException = null;
            try
            {
                characteristic.ValueChanged += handler;
                handlerAttached = true;
                Log(string.Format(""PM5 notification handler attached before CCCD write. Name={0}, Uuid={1}, CacheMode={2}"", name, uuid, cacheMode));
                await ReadCccd(characteristic, name, uuid, cacheMode, ""BeforeNotifyWrite"");
                Log(string.Format(""PM5 notification subscribe started. Name={0}, Uuid={1}, CacheMode={2}, Properties={3}, Descriptor=Notify"", name, uuid, cacheMode, characteristic.CharacteristicProperties));
                var status = await TimeoutAfter(characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify), 12000, ""PM5 Notify "" + name);
                Log(string.Format(""PM5 notification subscribe completed. Name={0}, Uuid={1}, CacheMode={2}, Descriptor=Notify, Status={3}"", name, uuid, cacheMode, status));
                await ReadCccd(characteristic, name, uuid, cacheMode, ""AfterNotifyWrite"");
                if (status == GattCommunicationStatus.Success)
                {
                    Log(string.Format(""PM5 notification subscribed. Name={0}, Uuid={1}, CacheMode={2}"", name, uuid, cacheMode));
                    return new Subscription { Characteristic = characteristic, Handler = handler, Name = name };
                }

                if (handlerAttached)
                {
                    characteristic.ValueChanged -= handler;
                }
                Error(string.Format(""PM5 notification subscribe failed. Name={0}, Uuid={1}, CacheMode={2}, Status={3}"", name, uuid, cacheMode, status));
            }
            catch (Exception exception)
            {
                subscribeException = exception;
            }

            if (subscribeException != null)
            {
                try
                {
                    if (handlerAttached)
                    {
                        characteristic.ValueChanged -= handler;
                    }
                }
                catch {}

                Error(string.Format(""PM5 notification subscribe timed out or failed. Name={0}, Uuid={1}, CacheMode={2}, Descriptor=Notify, Error={3}"", name, uuid, cacheMode, subscribeException.Message));
                await ReadCccd(characteristic, name, uuid, cacheMode, ""AfterNotifyWriteFailure"");
            }
        }

        Log(string.Format(""PM5 characteristic could not be subscribed after uncached/cached attempts. Name={0}, Uuid={1}"", name, uuid));
        return null;
    }

    private static async Task ReadCccd(GattCharacteristic characteristic, string name, Guid uuid, BluetoothCacheMode cacheMode, string phase)
    {
        try
        {
            var result = await TimeoutAfter(characteristic.ReadClientCharacteristicConfigurationDescriptorAsync(), 8000, ""PM5 CCCD read "" + phase + "" "" + name);
            Log(string.Format(""PM5 CCCD read. Phase={0}, Name={1}, Uuid={2}, CacheMode={3}, Status={4}, Descriptor={5}"", phase, name, uuid, cacheMode, result.Status, result.ClientCharacteristicConfigurationDescriptor));
        }
        catch (Exception exception)
        {
            Error(string.Format(""PM5 CCCD read failed. Phase={0}, Name={1}, Uuid={2}, CacheMode={3}, Error={4}"", phase, name, uuid, cacheMode, exception.Message));
        }
    }

    private static async Task<T> TimeoutAfter<T>(IAsyncOperation<T> operation, int timeoutMs, string label)
    {
        var task = ConvertToTask(operation);
        var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
        if (completed != task)
        {
            throw new TimeoutException(""Timed out waiting for Windows BLE operation. Label="" + label);
        }

        return await task;
    }

    private static Task<T> ConvertToTask<T>(IAsyncOperation<T> operation)
    {
        foreach (var method in typeof(System.WindowsRuntimeSystemExtensions).GetMethods())
        {
            if (method.Name == ""AsTask"" &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
            {
                return (Task<T>)method.MakeGenericMethod(typeof(T)).Invoke(null, new object[] { operation });
            }
        }

        throw new InvalidOperationException(""Could not find generic WindowsRuntimeSystemExtensions.AsTask overload."");
    }

    private static string ConvertBufferToHex(IBuffer buffer)
    {
        if (buffer == null)
        {
            return string.Empty;
        }

        var reader = DataReader.FromBuffer(buffer);
        var bytes = new byte[buffer.Length];
        reader.ReadBytes(bytes);
        return BitConverter.ToString(bytes).Replace(""-"", string.Empty);
    }

    private static string GetDeviceAccessStatus(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return ""Unknown"";
        }

        try
        {
            var access = DeviceAccessInformation.CreateFromId(id);
            return access == null ? ""Unknown"" : access.CurrentStatus.ToString();
        }
        catch (Exception exception)
        {
            return ""Error: "" + exception.Message;
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine(""LOG|"" + message);
        Flush();
    }

    private static void Error(string message)
    {
        Console.WriteLine(""ERROR|"" + message);
        Flush();
    }

    private static void Flush()
    {
        Console.Out.Flush();
    }
}
";
    }

    private static string QuoteProcessArgument(string argument)
    {
        return "\"" + argument.Replace("\"", "\\\"") + "\"";
    }

    private void DeleteHelperScriptFile()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(helperScriptPath) && System.IO.File.Exists(helperScriptPath))
            {
                System.IO.File.Delete(helperScriptPath);
            }

            if (!string.IsNullOrWhiteSpace(helperSourcePath) && System.IO.File.Exists(helperSourcePath))
            {
                System.IO.File.Delete(helperSourcePath);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Ski-Verse] Could not delete PM5 BLE helper file: " + exception.Message);
        }
        finally
        {
            helperScriptPath = null;
            helperSourcePath = null;
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

        if (line.StartsWith("METRIC_RAW|", StringComparison.OrdinalIgnoreCase))
        {
            HandlePowerShellWorkoutData(line);
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

    private void HandlePowerShellWorkoutData(string line)
    {
        var parts = line.Split('|');
        if (parts.Length < 4)
        {
            LogWarning("Could not parse PM5 workout data line: " + line);
            return;
        }

        var characteristicName = parts[1];
        if (!Guid.TryParse(parts[2], out var characteristicUuid))
        {
            LogWarning("Could not parse PM5 workout characteristic UUID: " + line);
            return;
        }

        if (!Pm5WorkoutDataParser.TryParseHexPayload(parts[3], out var payload))
        {
            LogWarning("Could not parse PM5 workout hex payload: " + line);
            return;
        }

        Log($"Workout data received. Characteristic='{characteristicName}', Uuid='{characteristicUuid}', Bytes={payload.Length}, RawHex='{parts[3]}'.");

        Pm5WorkoutMetrics updatedMetrics;
        var changed = false;
        lock (gate)
        {
            updatedMetrics = latestWorkoutMetrics;
            changed = Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(characteristicUuid, payload, ref updatedMetrics);
            if (changed)
            {
                latestWorkoutMetrics = updatedMetrics;
            }
        }

        if (!changed)
        {
            return;
        }

        Log($"Parsed workout metrics. Watts={(updatedMetrics.HasWatts ? updatedMetrics.Watts.ToString("0") : "-")}, HeartRate={(updatedMetrics.HasHeartRateBpm ? updatedMetrics.HeartRateBpm.ToString("0") : "-")}, StrokeRate={(updatedMetrics.HasStrokeRateSpm ? updatedMetrics.StrokeRateSpm.ToString("0") : "-")}, TotalStrokes={(updatedMetrics.HasTotalStrokes ? updatedMetrics.TotalStrokes.ToString() : "-")}.");
        WorkoutDataChanged?.Invoke();
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
[Windows.Devices.Enumeration.DeviceInformationCollection,Windows.Devices.Enumeration,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Enumeration.DeviceAccessInformation,Windows.Devices.Enumeration,ContentType=WindowsRuntime] | Out-Null
[Windows.Foundation.TypedEventHandler`2,Windows.Foundation,ContentType=WindowsRuntime] | Out-Null
[Console]::WriteLine('LOG|Windows BLE scan helper started. Scanning advertisements and BLE device list for 20 seconds.')
$services = @([Guid]'ce060000-43e5-11e4-916c-0800200c9a66', [Guid]'ce060030-43e5-11e4-916c-0800200c9a66', [Guid]'ce060020-43e5-11e4-916c-0800200c9a66', [Guid]'ce060010-43e5-11e4-916c-0800200c9a66')
$seen = [hashtable]::Synchronized(@{})
function Test-Pm5Name($name) {
    if ([string]::IsNullOrWhiteSpace($name)) { return $false }
    return ($name -match 'PM5|CONCEPT\s?2')
}
function Test-Concept2Service($uuid) {
    foreach ($knownService in $services) { if ($uuid -eq $knownService) { return $true } }
    return $false
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
        if (Test-Concept2Service $uuid) { $hasService = $true }
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
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristicsResult,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattClientCharacteristicConfigurationDescriptorValue,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.GenericAttributeProfile.GattReadClientCharacteristicConfigurationDescriptorResult,Windows.Devices.Bluetooth,ContentType=WindowsRuntime] | Out-Null
[Windows.Storage.Streams.DataReader,Windows.Storage.Streams,ContentType=WindowsRuntime] | Out-Null
[Windows.Foundation.TypedEventHandler`2,Windows.Foundation,ContentType=WindowsRuntime] | Out-Null
function Format-WinRtOperationDiagnostics($operation) {
    if ($null -eq $operation) { return 'Operation=null' }

    $typeName = ''
    $status = ''
    $errorText = ''
    try { $typeName = $operation.GetType().FullName } catch { $typeName = ('TypeError={0}' -f $_.Exception.Message) }
    try { $status = [string]$operation.Status } catch { $status = ('StatusError={0}' -f $_.Exception.Message) }
    try {
        $errorCode = $operation.ErrorCode
        if ($null -ne $errorCode) {
            $hresult = ''
            try { $hresult = ('0x{0:X8}' -f ($errorCode.HResult -band 0xffffffff)) } catch {}
            $errorText = ('ErrorCode={0}, HResult={1}, Message={2}' -f $errorCode.GetType().FullName, $hresult, $errorCode.Message)
        }
    } catch {
        $errorText = ('ErrorCodeReadError={0}' -f $_.Exception.Message)
    }

    return ('Type={0}, Status={1}, {2}' -f $typeName, $status, $errorText)
}
function Await-WinRtOperation($operation, [Type]$resultType, [int]$timeoutMs) {
    $method = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { $_.Name -eq 'AsTask' -and $_.IsGenericMethodDefinition -and $_.GetGenericArguments().Length -eq 1 -and $_.GetParameters().Length -eq 1 } | Select-Object -First 1
    $task = $method.MakeGenericMethod($resultType).Invoke($null, @($operation))
    if (-not $task.Wait($timeoutMs)) { throw 'Timed out waiting for Windows BLE operation.' }
    if ($task.IsFaulted) { throw $task.Exception.GetBaseException().Message }
    return $task.Result
}
function Await-WinRtOperationByStatus($operation, [int]$timeoutMs, [string]$label) {
    $deadline = [DateTime]::UtcNow.AddMilliseconds($timeoutMs)
    $lastStatus = ''
    while ([DateTime]::UtcNow -lt $deadline) {
        $status = [string]$operation.Status
        if ($status -ne $lastStatus) {
            [Console]::WriteLine(('LOG|WinRT operation status. Label={0}, Status={1}' -f $label, $status))
            [Console]::Out.Flush()
            $lastStatus = $status
        }

        if ($status -eq 'Completed') {
            return $operation.GetResults()
        }

        if ($status -eq 'Error') {
            throw ('WinRT operation failed. Label={0}, Diagnostics={1}' -f $label, (Format-WinRtOperationDiagnostics $operation))
        }

        if ($status -eq 'Canceled') {
            throw ('WinRT operation canceled. Label={0}' -f $label)
        }

        Start-Sleep -Milliseconds 100
    }

    $finalStatus = [string]$operation.Status
    throw ('Timed out waiting for Windows BLE operation. Label={0}, FinalStatus={1}, Diagnostics={2}' -f $label, $finalStatus, (Format-WinRtOperationDiagnostics $operation))
}
function Convert-BufferToHex($buffer) {
    if ($null -eq $buffer) { return '' }
    $reader = [Windows.Storage.Streams.DataReader]::FromBuffer($buffer)
    $bytes = New-Object byte[] $buffer.Length
    $reader.ReadBytes($bytes)
    return (($bytes | ForEach-Object { $_.ToString('X2') }) -join '')
}
function Get-DeviceAccessStatus([string]$id) {
    if ([string]::IsNullOrWhiteSpace($id)) { return 'Unknown' }
    try {
        $access = [Windows.Devices.Enumeration.DeviceAccessInformation]::CreateFromId($id)
        if ($null -eq $access) { return 'Unknown' }
        return [string]$access.CurrentStatus
    } catch {
        return ('Error: {0}' -f $_.Exception.Message)
    }
}
function Read-Pm5CccdValue($characteristic, [string]$name, [Guid]$uuid, $cacheMode, [string]$phase) {
    try {
        $result = Await-WinRtOperation ($characteristic.ReadClientCharacteristicConfigurationDescriptorAsync()) ([Windows.Devices.Bluetooth.GenericAttributeProfile.GattReadClientCharacteristicConfigurationDescriptorResult]) 8000
        [Console]::WriteLine(('LOG|PM5 CCCD read. Phase={0}, Name={1}, Uuid={2}, CacheMode={3}, Status={4}, Descriptor={5}' -f $phase, $name, $uuid, $cacheMode, $result.Status, $result.ClientCharacteristicConfigurationDescriptor))
        [Console]::Out.Flush()
    } catch {
        [Console]::WriteLine(('ERROR|PM5 CCCD read failed. Phase={0}, Name={1}, Uuid={2}, CacheMode={3}, Error={4}' -f $phase, $name, $uuid, $cacheMode, $_.Exception.Message))
        [Console]::Out.Flush()
    }
}
function Find-Pm5WorkoutServiceDeviceId([Guid]$workoutServiceUuid, [UInt64]$address) {
    $addressHex = ''
    if ($address -ne 0) { $addressHex = ('{0:X12}' -f $address) }

    try {
        $selector = [Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService]::GetDeviceSelectorFromUuid($workoutServiceUuid)
        [Console]::WriteLine(('LOG|Looking up PM5 workout service with WinRT selector: {0}' -f $selector))
        [Console]::Out.Flush()
        $serviceDevices = Await-WinRtOperation ([Windows.Devices.Enumeration.DeviceInformation]::FindAllAsync($selector)) ([Windows.Devices.Enumeration.DeviceInformationCollection]) 20000
        $count = 0
        if ($null -ne $serviceDevices) { $count = $serviceDevices.Count }
        [Console]::WriteLine(('LOG|WinRT workout service lookup returned Count={0}' -f $count))
        [Console]::Out.Flush()
        foreach ($serviceDevice in $serviceDevices) {
            $id = [string]$serviceDevice.Id
            $name = [string]$serviceDevice.Name
            $upperId = $id.ToUpperInvariant()
            $matchesAddress = [string]::IsNullOrWhiteSpace($addressHex) -or $upperId.Contains($addressHex)
            $accessStatus = Get-DeviceAccessStatus $id
            [Console]::WriteLine(('LOG|WinRT workout service candidate. Name=""{0}"", Id=""{1}"", IsEnabled={2}, Access={3}, MatchesAddress={4}' -f $name, $id, $serviceDevice.IsEnabled, $accessStatus, $matchesAddress))
            [Console]::Out.Flush()
            if ($matchesAddress) {
                [Console]::WriteLine(('LOG|Using PM5 workout service WinRT DeviceInformation.Id=""{0}""' -f $id))
                [Console]::Out.Flush()
                return $id
            }
        }
    } catch {
        [Console]::WriteLine(('ERROR|PM5 workout service WinRT lookup failed: {0}' -f $_.Exception.Message))
        [Console]::Out.Flush()
    }

    try {
        foreach ($pnpDevice in Get-PnpDevice -ErrorAction Stop) {
            $id = [string]$pnpDevice.InstanceId
            if ([string]::IsNullOrWhiteSpace($id)) { continue }
            $upperId = $id.ToUpperInvariant()
            $isWorkoutService = $upperId.Contains('{CE060030-43E5-11E4-916C-0800200C9A66}')
            $matchesAddress = [string]::IsNullOrWhiteSpace($addressHex) -or $upperId.Contains($addressHex)
            if ($isWorkoutService -and $matchesAddress) {
                [Console]::WriteLine(('LOG|Found PM5 workout service device id from Windows PnP fallback. Id=""{0}"", Access={1}' -f $id, (Get-DeviceAccessStatus $id)))
                [Console]::Out.Flush()
                return $id
            }
        }
    } catch {
        [Console]::WriteLine(('ERROR|PM5 workout service PnP lookup failed: {0}' -f $_.Exception.Message))
        [Console]::Out.Flush()
    }

    return $null
}
function Start-Pm5WorkoutNotifications($pm5Service, [string]$deviceName) {
    [Console]::WriteLine(('CONNECTED|GATT Concept2 PM5 workout service verified. DeviceName=""{0}""' -f $deviceName))
    [Console]::Out.Flush()
    try {
        $session = $pm5Service.Session
        if ($null -ne $session) {
            $session.MaintainConnection = $true
            [Console]::WriteLine(('LOG|PM5 GATT session maintain connection enabled. MaxPduSize={0}' -f $session.MaxPduSize))
            [Console]::Out.Flush()
        }
    } catch {
        [Console]::WriteLine(('ERROR|PM5 GATT session setup failed: {0}' -f $_.Exception.Message))
        [Console]::Out.Flush()
    }

    $subscriptions = @()
    $subscriptionAttempt = 0
    while ($true) {
        $activeSubscriptions = @($subscriptions | Where-Object { $null -ne $_ })
        if ($activeSubscriptions.Count -eq 0) {
            $subscriptionAttempt++
            [Console]::WriteLine(('LOG|PM5 workout data subscription attempt {0} started.' -f $subscriptionAttempt))
            [Console]::Out.Flush()

            $subscriptions = @()
            $primaryName = 'Multiplexed Information'
            $primarySubscription = Subscribe-Pm5Characteristic $pm5Service 'Multiplexed Information' ([Guid]'ce060080-43e5-11e4-916c-0800200c9a66')
            if ($null -eq $primarySubscription) {
                [Console]::WriteLine('LOG|PM5 multiplexed workout data subscription unavailable. Falling back to direct Rowing Stroke Data.')
                [Console]::Out.Flush()
                $primaryName = 'Rowing Stroke Data'
                $primarySubscription = Subscribe-Pm5Characteristic $pm5Service 'Rowing Stroke Data' ([Guid]'ce060035-43e5-11e4-916c-0800200c9a66')
            }

            if ($null -ne $primarySubscription) {
                $subscriptions += $primarySubscription
                [Console]::WriteLine(('LOG|PM5 primary workout data subscription active. Name={0}' -f $primaryName))
                [Console]::Out.Flush()

                if ($primaryName -eq 'Rowing Stroke Data') {
                    Start-Sleep -Milliseconds 500
                    $optionalSubscriptions = @()
                    $optionalSubscriptions += Subscribe-Pm5Characteristic $pm5Service 'Rowing Additional Stroke Data' ([Guid]'ce060036-43e5-11e4-916c-0800200c9a66')
                    Start-Sleep -Milliseconds 500
                    $optionalSubscriptions += Subscribe-Pm5Characteristic $pm5Service 'Rowing Additional Status 1' ([Guid]'ce060032-43e5-11e4-916c-0800200c9a66')
                    Start-Sleep -Milliseconds 500
                    $optionalSubscriptions += Subscribe-Pm5Characteristic $pm5Service 'Rowing Additional Status 2' ([Guid]'ce060033-43e5-11e4-916c-0800200c9a66')

                    $optionalActiveSubscriptions = @($optionalSubscriptions | Where-Object { $null -ne $_ })
                    if ($optionalActiveSubscriptions.Count -gt 0) {
                        $subscriptions += $optionalActiveSubscriptions
                    }
                    [Console]::WriteLine(('LOG|PM5 optional workout data subscriptions active. Count={0}' -f $optionalActiveSubscriptions.Count))
                } else {
                    [Console]::WriteLine('LOG|PM5 multiplexed workout data subscription active. Direct characteristic subscriptions skipped per Concept2 multiplexing rules.')
                }
                [Console]::Out.Flush()
            } else {
                [Console]::WriteLine('LOG|PM5 primary workout data subscription unavailable. Optional subscriptions deferred.')
                [Console]::Out.Flush()
            }

            $activeSubscriptions = @($subscriptions | Where-Object { $null -ne $_ })
            [Console]::WriteLine(('LOG|PM5 workout data subscriptions active. Count={0}' -f $activeSubscriptions.Count))
            if ($activeSubscriptions.Count -eq 0) {
                [Console]::WriteLine('LOG|PM5 workout data subscriptions unavailable. Retrying while PM5 connection stays alive.')
            }
            [Console]::Out.Flush()
        }

        $sleepSeconds = 1
        if ($activeSubscriptions.Count -eq 0) { $sleepSeconds = 3 }
        Start-Sleep -Seconds $sleepSeconds
    }
}
function Subscribe-Pm5Characteristic($serviceObject, [string]$name, [Guid]$uuid) {
    foreach ($cacheMode in @([Windows.Devices.Bluetooth.BluetoothCacheMode]::Uncached, [Windows.Devices.Bluetooth.BluetoothCacheMode]::Cached)) {
        $characteristic = $null
        try {
            [Console]::WriteLine(('LOG|PM5 characteristic lookup started. Name={0}, Uuid={1}, CacheMode={2}' -f $name, $uuid, $cacheMode))
            [Console]::Out.Flush()
            $result = Await-WinRtOperation ($serviceObject.GetCharacteristicsForUuidAsync($uuid, $cacheMode)) ([Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristicsResult]) 8000
            $count = 0
            if ($null -ne $result.Characteristics) { $count = $result.Characteristics.Count }
            [Console]::WriteLine(('LOG|PM5 characteristic lookup result. Name={0}, Uuid={1}, CacheMode={2}, Status={3}, Count={4}' -f $name, $uuid, $cacheMode, $result.Status, $count))
            [Console]::Out.Flush()
            if ($result.Status -eq [Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus]::Success -and $count -gt 0) {
                $characteristic = $result.Characteristics[0]
            }
        } catch {
            [Console]::WriteLine(('ERROR|PM5 characteristic lookup failed. Name={0}, Uuid={1}, CacheMode={2}, Error={3}' -f $name, $uuid, $cacheMode, $_.Exception.Message))
            [Console]::Out.Flush()
        }

        if ($null -eq $characteristic) {
            continue
        }

        $localName = $name
        $localUuid = $uuid
        $properties = $characteristic.CharacteristicProperties
        $notifyFlag = [Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristicProperties]::Notify
        $indicateFlag = [Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristicProperties]::Indicate
        $descriptorValue = [Windows.Devices.Bluetooth.GenericAttributeProfile.GattClientCharacteristicConfigurationDescriptorValue]::Notify
        if ((([int]$properties) -band ([int]$indicateFlag)) -ne 0 -and (([int]$properties) -band ([int]$notifyFlag)) -eq 0) {
            $descriptorValue = [Windows.Devices.Bluetooth.GenericAttributeProfile.GattClientCharacteristicConfigurationDescriptorValue]::Indicate
        }

        $handler = [Windows.Foundation.TypedEventHandler[Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic,Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs]] {
            param($sender, $args)
            try {
                $hex = Convert-BufferToHex $args.CharacteristicValue
                [Console]::WriteLine(('METRIC_RAW|{0}|{1}|{2}' -f $localName, $localUuid, $hex))
                [Console]::Out.Flush()
            } catch {
                [Console]::WriteLine(('ERROR|PM5 workout notification parse failed for {0}: {1}' -f $localName, $_.Exception.Message))
                [Console]::Out.Flush()
            }
        }
        $token = $null
        try {
            $token = $characteristic.add_ValueChanged($handler)
            [Console]::WriteLine(('LOG|PM5 notification handler attached before CCCD write. Name={0}, Uuid={1}, CacheMode={2}' -f $localName, $localUuid, $cacheMode))
            [Console]::Out.Flush()
        } catch {
            [Console]::WriteLine(('ERROR|PM5 notification handler attach failed. Name={0}, Uuid={1}, CacheMode={2}, Error={3}' -f $localName, $localUuid, $cacheMode, $_.Exception.Message))
            [Console]::Out.Flush()
            continue
        }

        [Console]::WriteLine(('LOG|PM5 notification subscribe started. Name={0}, Uuid={1}, CacheMode={2}, Properties={3}, Descriptor={4}' -f $localName, $localUuid, $cacheMode, $properties, $descriptorValue))
        [Console]::Out.Flush()
        Read-Pm5CccdValue $characteristic $localName $localUuid $cacheMode 'BeforeNotifyWrite'
        $writeOperation = $null
        try {
            $writeOperation = $characteristic.WriteClientCharacteristicConfigurationDescriptorAsync($descriptorValue)
            [Console]::WriteLine(('LOG|PM5 notification write operation created. Name={0}, Uuid={1}, CacheMode={2}, Diagnostics={3}' -f $localName, $localUuid, $cacheMode, (Format-WinRtOperationDiagnostics $writeOperation)))
            [Console]::Out.Flush()
            $status = Await-WinRtOperationByStatus $writeOperation 12000 ('PM5 Notify {0}' -f $localName)
        } catch {
            if ($null -ne $token) {
                try { $characteristic.remove_ValueChanged($token) } catch {}
            }
            $errorMessage = $_.Exception.Message
            if ($null -ne $_.Exception.InnerException) {
                $errorMessage = ('{0} InnerException={1}' -f $errorMessage, $_.Exception.InnerException.Message)
            }
            [Console]::WriteLine(('ERROR|PM5 notification subscribe timed out or failed. Name={0}, Uuid={1}, CacheMode={2}, Descriptor={3}, Error={4}, Diagnostics={5}' -f $localName, $localUuid, $cacheMode, $descriptorValue, $errorMessage, (Format-WinRtOperationDiagnostics $writeOperation)))
            [Console]::Out.Flush()
            Read-Pm5CccdValue $characteristic $localName $localUuid $cacheMode 'AfterNotifyWriteFailure'
            continue
        }

        [Console]::WriteLine(('LOG|PM5 notification subscribe completed. Name={0}, Uuid={1}, CacheMode={2}, Descriptor={3}, Status={4}' -f $localName, $localUuid, $cacheMode, $descriptorValue, $status))
        [Console]::Out.Flush()
        Read-Pm5CccdValue $characteristic $localName $localUuid $cacheMode 'AfterNotifyWrite'

        if ($status -ne [Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus]::Success) {
            if ($null -ne $token) {
                try { $characteristic.remove_ValueChanged($token) } catch {}
            }
            [Console]::WriteLine(('ERROR|PM5 notification subscribe failed. Name={0}, Uuid={1}, CacheMode={2}, Status={3}' -f $localName, $localUuid, $cacheMode, $status))
            [Console]::Out.Flush()
            continue
        }

        [Console]::WriteLine(('LOG|PM5 notification subscribed. Name={0}, Uuid={1}, CacheMode={2}' -f $localName, $localUuid, $cacheMode))
        [Console]::Out.Flush()
        return [pscustomobject]@{ Characteristic = $characteristic; Token = $token; Name = $localName }
    }

    [Console]::WriteLine(('LOG|PM5 characteristic could not be subscribed after uncached/cached attempts. Name={0}, Uuid={1}' -f $name, $uuid))
    [Console]::Out.Flush()
    return $null
}
$deviceId = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + encodedDeviceId + @"'))
$address = [UInt64]" + device.BluetoothAddress + @"
$workoutService = [Guid]'ce060030-43e5-11e4-916c-0800200c9a66'
[Console]::WriteLine(('LOG|Windows BLE connect helper started. DeviceId=""{0}"", Address={1}' -f $deviceId, $address))
$workoutServiceDeviceId = Find-Pm5WorkoutServiceDeviceId $workoutService $address
if (-not [string]::IsNullOrWhiteSpace($workoutServiceDeviceId)) {
    [Console]::WriteLine(('LOG|Connecting directly with GattDeviceService.FromIdAsync for PM5 workout service. Access={0}' -f (Get-DeviceAccessStatus $workoutServiceDeviceId)))
    [Console]::Out.Flush()
    $directService = Await-WinRtOperation ([Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService]::FromIdAsync($workoutServiceDeviceId)) ([Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService]) 20000
    if ($null -ne $directService) {
        Start-Pm5WorkoutNotifications $directService 'Concept2 PM5'
    }

    [Console]::WriteLine('ERROR|GattDeviceService.FromIdAsync returned null for PM5 workout service. Falling back to BluetoothLEDevice service lookup.')
    [Console]::Out.Flush()
}
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
[Console]::WriteLine(('LOG|BluetoothLEDevice resolved. Name=""{0}"", DeviceId=""{1}"". Verifying Concept2 PM5 workout GATT service {2}.' -f $device.Name, $device.DeviceId, $workoutService))
[Console]::Out.Flush()
$lastServiceStatus = 'NotAttempted'
$lastServiceCount = 0
foreach ($serviceCacheMode in @([Windows.Devices.Bluetooth.BluetoothCacheMode]::Cached, [Windows.Devices.Bluetooth.BluetoothCacheMode]::Uncached)) {
    try {
        [Console]::WriteLine(('LOG|PM5 workout service lookup started. CacheMode={0}' -f $serviceCacheMode))
        [Console]::Out.Flush()
        $servicesResult = Await-WinRtOperation ($device.GetGattServicesForUuidAsync($workoutService, $serviceCacheMode)) ([Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceServicesResult]) 20000
        $serviceCount = 0
        if ($null -ne $servicesResult.Services) { $serviceCount = $servicesResult.Services.Count }
        $lastServiceStatus = [string]$servicesResult.Status
        $lastServiceCount = $serviceCount
        [Console]::WriteLine(('LOG|PM5 workout service lookup result. CacheMode={0}, GattStatus={1}, ServiceCount={2}' -f $serviceCacheMode, $servicesResult.Status, $serviceCount))
        [Console]::Out.Flush()
        if ($servicesResult.Status -eq [Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus]::Success -and $serviceCount -gt 0) {
            Start-Pm5WorkoutNotifications $servicesResult.Services[0] $device.Name
        }
    } catch {
        $lastServiceStatus = $_.Exception.Message
        [Console]::WriteLine(('ERROR|PM5 workout service lookup failed. CacheMode={0}, Error={1}' -f $serviceCacheMode, $_.Exception.Message))
        [Console]::Out.Flush()
    }
}
[Console]::WriteLine(('FAILED|GATT workout service verification failed. LastStatus={0}, LastServiceCount={1}' -f $lastServiceStatus, $lastServiceCount))
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
