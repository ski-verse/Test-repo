using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

internal static class Program
{
    private static readonly Guid Concept2BaseServiceUuid = new("ce060000-43e5-11e4-916c-0800200c9a66");
    private static readonly Guid RowingServiceUuid = new("ce060030-43e5-11e4-916c-0800200c9a66");

    private static readonly ProbeCharacteristic[] Characteristics =
    [
        new("General Status", new Guid("ce060031-43e5-11e4-916c-0800200c9a66")),
        new("Additional Status 1", new Guid("ce060032-43e5-11e4-916c-0800200c9a66")),
        new("Additional Stroke Data", new Guid("ce060036-43e5-11e4-916c-0800200c9a66")),
        new("Multiplexed Information", new Guid("ce060080-43e5-11e4-916c-0800200c9a66")),
    ];

    private static readonly Guid[] KnownConcept2Services =
    [
        Concept2BaseServiceUuid,
        RowingServiceUuid,
        new("ce060010-43e5-11e4-916c-0800200c9a66"),
        new("ce060020-43e5-11e4-916c-0800200c9a66"),
    ];

    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        var options = ProbeOptions.Parse(args);

        Log("PM5 BLE Probe starting.");
        Log($"TargetFramework: net8.0-windows10.0.19041.0");
        Log($"ScanSeconds={options.ScanSeconds}, VerboseAdvertisements={options.VerboseAdvertisements}, Address={options.Address?.ToString() ?? "scan"}");

        try
        {
            var address = options.Address ?? await ScanAndSelectPm5(options);
            if (address == null)
            {
                Warn("No PM5 selected. Put PM5 in Connect/app mode and try again.");
                return 2;
            }

            await ConnectAndSubscribe(address.Value);
            return 0;
        }
        catch (Exception exception)
        {
            Error(exception.ToString());
            return 1;
        }
    }

    private static async Task<ulong?> ScanAndSelectPm5(ProbeOptions options)
    {
        var found = new Dictionary<ulong, DiscoveredPm5>();
        var allAdvertisements = 0;
        var pm5Matches = 0;

        using var scanFinished = new CancellationTokenSource(TimeSpan.FromSeconds(options.ScanSeconds));
        var watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active,
        };

        watcher.Received += (_, eventArgs) =>
        {
            allAdvertisements++;
            var name = eventArgs.Advertisement.LocalName ?? string.Empty;
            var serviceUuids = eventArgs.Advertisement.ServiceUuids.ToArray();
            var matches = IsPm5(name, serviceUuids);

            if (options.VerboseAdvertisements || matches)
            {
                Log($"ADV|Address={eventArgs.BluetoothAddress}|Name=\"{name}\"|Services={FormatServices(serviceUuids)}|Rssi={eventArgs.RawSignalStrengthInDBm}|MatchesPM5={matches}");
            }

            if (!matches)
            {
                return;
            }

            pm5Matches++;
            found[eventArgs.BluetoothAddress] = new DiscoveredPm5(eventArgs.BluetoothAddress, string.IsNullOrWhiteSpace(name) ? "Concept2 PM5" : name, serviceUuids);
        };

        watcher.Stopped += (_, eventArgs) =>
        {
            Log($"SCAN_STOPPED|Status={eventArgs.Error}");
        };

        Log($"SCAN_START|InitialStatus={watcher.Status}|Mode={watcher.ScanningMode}");
        watcher.Start();
        Log($"SCAN_STARTED|Status={watcher.Status}");

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(options.ScanSeconds), scanFinished.Token);
        }
        catch (TaskCanceledException)
        {
        }

        Log($"SCAN_STOPPING|Status={watcher.Status}");
        watcher.Stop();
        await Task.Delay(500);
        Log($"SCAN_SUMMARY|Advertisements={allAdvertisements}|Pm5Matches={pm5Matches}|UniquePm5={found.Count}|FinalStatus={watcher.Status}");

        if (found.Count == 0)
        {
            return null;
        }

        var devices = found.Values.OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        for (var i = 0; i < devices.Length; i++)
        {
            Log($"PM5[{i + 1}] Name=\"{devices[i].Name}\" Address={devices[i].Address} Services={FormatServices(devices[i].ServiceUuids)}");
        }

        if (devices.Length == 1 || !Environment.UserInteractive)
        {
            Log($"SELECT|Using PM5 \"{devices[0].Name}\" Address={devices[0].Address}");
            return devices[0].Address;
        }

        Console.Write("Select PM5 number: ");
        var input = Console.ReadLine();
        if (!int.TryParse(input, out var selected) || selected < 1 || selected > devices.Length)
        {
            Warn("Invalid PM5 selection.");
            return null;
        }

        return devices[selected - 1].Address;
    }

    private static async Task ConnectAndSubscribe(ulong address)
    {
        Log($"CONNECT_START|Address={address}");
        using var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
        if (device == null)
        {
            throw new InvalidOperationException("BluetoothLEDevice.FromBluetoothAddressAsync returned null.");
        }

        Log($"DEVICE|Name=\"{device.Name}\"|DeviceId=\"{device.DeviceId}\"|ConnectionStatus={device.ConnectionStatus}");
        device.ConnectionStatusChanged += (_, _) =>
        {
            Log($"DEVICE_CONNECTION_STATUS|{device.ConnectionStatus}");
        };

        var session = await GattSession.FromDeviceIdAsync(device.BluetoothDeviceId);
        if (session != null)
        {
            Log($"GATT_SESSION|CanMaintainConnection={session.CanMaintainConnection}|InitialStatus={session.SessionStatus}|MaxPduSize={session.MaxPduSize}");
            session.MaintainConnection = true;
            session.SessionStatusChanged += (_, eventArgs) =>
            {
                Log($"GATT_SESSION_STATUS|Status={eventArgs.Status}|Error={eventArgs.Error}");
            };
        }
        else
        {
            Warn("GattSession.FromDeviceIdAsync returned null.");
        }

        var serviceResult = await device.GetGattServicesForUuidAsync(RowingServiceUuid, BluetoothCacheMode.Uncached);
        Log($"SERVICE_LOOKUP|Uuid={RowingServiceUuid}|Status={serviceResult.Status}|Count={serviceResult.Services.Count}");
        if (serviceResult.Status != GattCommunicationStatus.Success || serviceResult.Services.Count == 0)
        {
            serviceResult = await device.GetGattServicesForUuidAsync(RowingServiceUuid, BluetoothCacheMode.Cached);
            Log($"SERVICE_LOOKUP_CACHED|Uuid={RowingServiceUuid}|Status={serviceResult.Status}|Count={serviceResult.Services.Count}");
        }

        if (serviceResult.Status != GattCommunicationStatus.Success || serviceResult.Services.Count == 0)
        {
            throw new InvalidOperationException($"Could not open rowing service {RowingServiceUuid}. Status={serviceResult.Status}, Count={serviceResult.Services.Count}");
        }

        using var service = serviceResult.Services[0];
        Log($"SERVICE_OPEN|DeviceId=\"{service.DeviceId}\"|Uuid={service.Uuid}");

        var activeSubscriptions = new List<GattCharacteristic>();
        foreach (var target in Characteristics)
        {
            var subscribed = await TrySubscribe(service, target);
            if (subscribed != null)
            {
                activeSubscriptions.Add(subscribed);
            }
        }

        Log($"SUBSCRIBE_SUMMARY|ActiveSubscriptions={activeSubscriptions.Count}");
        if (activeSubscriptions.Count == 0)
        {
            Warn("No notifications subscribed. Probe will stay open briefly for diagnostics, but no RAW packets are expected.");
        }

        Log("LISTENING|Press Enter to stop. Start rowing/skiing now and watch for RAW lines.");
        Console.ReadLine();
    }

    private static async Task<GattCharacteristic?> TrySubscribe(GattDeviceService service, ProbeCharacteristic target)
    {
        foreach (var cacheMode in new[] { BluetoothCacheMode.Uncached, BluetoothCacheMode.Cached })
        {
            Log($"CHAR_LOOKUP|Name=\"{target.Name}\"|Uuid={target.Uuid}|CacheMode={cacheMode}");
            var result = await service.GetCharacteristicsForUuidAsync(target.Uuid, cacheMode);
            Log($"CHAR_LOOKUP_RESULT|Name=\"{target.Name}\"|Uuid={target.Uuid}|CacheMode={cacheMode}|Status={result.Status}|Count={result.Characteristics.Count}");

            if (result.Status != GattCommunicationStatus.Success || result.Characteristics.Count == 0)
            {
                continue;
            }

            var characteristic = result.Characteristics[0];
            Log($"CHAR_STATE|Name=\"{target.Name}\"|Properties={characteristic.CharacteristicProperties}|ProtectionLevel={characteristic.ProtectionLevel}");
            characteristic.ValueChanged += (_, eventArgs) =>
            {
                var raw = ReadBuffer(eventArgs.CharacteristicValue);
                Log($"RAW|Name=\"{target.Name}\"|Uuid={target.Uuid}|Bytes={raw.Length}|Hex={Convert.ToHexString(raw)}");
            };

            var descriptorValue = SelectDescriptorValue(characteristic);
            var before = await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
            Log($"CCCD_BEFORE|Name=\"{target.Name}\"|Status={before.Status}|Descriptor={before.ClientCharacteristicConfigurationDescriptor}");

            Log($"CCCD_WRITE_START|Name=\"{target.Name}\"|Descriptor={descriptorValue}");
            var writeStatus = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(descriptorValue);
            Log($"CCCD_WRITE_RESULT|Name=\"{target.Name}\"|Status={writeStatus}");

            var after = await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
            Log($"CCCD_AFTER|Name=\"{target.Name}\"|Status={after.Status}|Descriptor={after.ClientCharacteristicConfigurationDescriptor}");

            if (writeStatus == GattCommunicationStatus.Success)
            {
                return characteristic;
            }
        }

        Warn($"SUBSCRIBE_FAILED|Name=\"{target.Name}\"|Uuid={target.Uuid}");
        return null;
    }

    private static GattClientCharacteristicConfigurationDescriptorValue SelectDescriptorValue(GattCharacteristic characteristic)
    {
        var properties = characteristic.CharacteristicProperties;
        if (properties.HasFlag(GattCharacteristicProperties.Notify))
        {
            return GattClientCharacteristicConfigurationDescriptorValue.Notify;
        }

        if (properties.HasFlag(GattCharacteristicProperties.Indicate))
        {
            return GattClientCharacteristicConfigurationDescriptorValue.Indicate;
        }

        return GattClientCharacteristicConfigurationDescriptorValue.Notify;
    }

    private static bool IsPm5(string localName, IReadOnlyCollection<Guid> serviceUuids)
    {
        if (!string.IsNullOrWhiteSpace(localName))
        {
            var normalized = localName.ToUpperInvariant();
            if (normalized.Contains("PM5", StringComparison.Ordinal) ||
                normalized.Contains("CONCEPT2", StringComparison.Ordinal) ||
                normalized.Contains("CONCEPT 2", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return serviceUuids.Any(serviceUuid => KnownConcept2Services.Contains(serviceUuid));
    }

    private static byte[] ReadBuffer(IBuffer buffer)
    {
        var data = new byte[checked((int)buffer.Length)];
        using var reader = DataReader.FromBuffer(buffer);
        reader.ReadBytes(data);
        return data;
    }

    private static string FormatServices(IEnumerable<Guid> serviceUuids)
    {
        var services = serviceUuids.ToArray();
        return services.Length == 0 ? "none" : string.Join(",", services.Select(service => service.ToString()));
    }

    private static void Log(string message)
    {
        Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} {message}");
    }

    private static void Warn(string message)
    {
        Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} WARN|{message}");
    }

    private static void Error(string message)
    {
        Console.Error.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.fff} ERROR|{message}");
    }

    private readonly record struct ProbeCharacteristic(string Name, Guid Uuid);

    private readonly record struct DiscoveredPm5(ulong Address, string Name, IReadOnlyCollection<Guid> ServiceUuids);

    private sealed class ProbeOptions
    {
        public int ScanSeconds { get; private init; } = 30;
        public bool VerboseAdvertisements { get; private init; }
        public ulong? Address { get; private init; }

        public static ProbeOptions Parse(string[] args)
        {
            var scanSeconds = 30;
            var verboseAdvertisements = false;
            ulong? address = null;

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--scan-seconds" when i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedSeconds):
                        scanSeconds = Math.Clamp(parsedSeconds, 5, 300);
                        i++;
                        break;
                    case "--verbose-advertisements":
                        verboseAdvertisements = true;
                        break;
                    case "--address" when i + 1 < args.Length && ulong.TryParse(args[i + 1], out var parsedAddress):
                        address = parsedAddress;
                        i++;
                        break;
                    case "--help":
                    case "-h":
                        Console.WriteLine("Usage: dotnet run -- [--scan-seconds 30] [--verbose-advertisements] [--address 237390097829415]");
                        break;
                }
            }

            return new ProbeOptions
            {
                ScanSeconds = scanSeconds,
                VerboseAdvertisements = verboseAdvertisements,
                Address = address,
            };
        }
    }
}
