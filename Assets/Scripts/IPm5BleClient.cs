using System;
using System.Collections.Generic;

public interface IPm5BleClient
{
    event Action StateChanged;

    Pm5BleConnectionStatus Status { get; }

    Pm5WorkoutDataStatus DataStatus { get; }

    IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices { get; }

    void StartScan();

    void StopScan();

    void Connect(Pm5BleDeviceInfo device);
}
