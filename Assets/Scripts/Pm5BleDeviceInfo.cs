using System;

[Serializable]
public readonly struct Pm5BleDeviceInfo
{
    public Pm5BleDeviceInfo(string deviceId, string name, ulong bluetoothAddress = 0)
    {
        DeviceId = deviceId ?? string.Empty;
        Name = string.IsNullOrWhiteSpace(name) ? "Concept2 PM5" : name;
        BluetoothAddress = bluetoothAddress;
    }

    public string DeviceId { get; }

    public string Name { get; }

    public ulong BluetoothAddress { get; }

    public bool IsValid => !string.IsNullOrEmpty(DeviceId) || BluetoothAddress != 0;
}
