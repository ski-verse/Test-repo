using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Pm5BleRuntimeConnector : MonoBehaviour
{
    public const string NotConnectedStatusText = "PM5: Not connected";
    public const string SearchingStatusText = "Searching...";
    public const string Pm5FoundStatusText = "PM5 Found";
    public const string ConnectingStatusText = "Connecting...";
    public const string ConnectedStatusText = "Connected";
    public const string ConnectionFailedStatusText = "Connection Failed";

    private IPm5BleClient client;
    private int selectedDeviceIndex = -1;

    public event Action StateChanged;

    public IPm5BleClient Client
    {
        get
        {
            EnsureClient();
            return client;
        }
        set
        {
            if (client != null)
            {
                client.StateChanged -= NotifyStateChanged;
            }

            client = value;
            selectedDeviceIndex = -1;

            if (client != null)
            {
                client.StateChanged += NotifyStateChanged;
            }

            NotifyStateChanged();
        }
    }

    public Pm5BleConnectionStatus Status => Client.Status;

    public IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices => Client.DiscoveredDevices;

    public int SelectedDeviceIndex => selectedDeviceIndex;

    public Pm5BleDeviceInfo? SelectedDevice
    {
        get
        {
            var devices = DiscoveredDevices;
            if (selectedDeviceIndex < 0 || selectedDeviceIndex >= devices.Count)
            {
                return null;
            }

            return devices[selectedDeviceIndex];
        }
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.StateChanged -= NotifyStateChanged;
            client.StopScan();
        }
    }

    public void StartScan()
    {
        selectedDeviceIndex = -1;
        Client.StartScan();
        NotifyStateChanged();
    }

    public void SelectDevice(int deviceIndex)
    {
        var devices = DiscoveredDevices;
        selectedDeviceIndex = deviceIndex >= 0 && deviceIndex < devices.Count ? deviceIndex : -1;
        NotifyStateChanged();
    }

    public void ConnectSelectedDevice()
    {
        var selected = SelectedDevice;
        if (!selected.HasValue)
        {
            StartScan();
            return;
        }

        Client.Connect(selected.Value);
        NotifyStateChanged();
    }

    public string GetStatusText()
    {
        return StatusToText(Status);
    }

    public static string StatusToText(Pm5BleConnectionStatus status)
    {
        switch (status)
        {
            case Pm5BleConnectionStatus.Searching:
                return SearchingStatusText;
            case Pm5BleConnectionStatus.Pm5Found:
                return Pm5FoundStatusText;
            case Pm5BleConnectionStatus.Connecting:
                return ConnectingStatusText;
            case Pm5BleConnectionStatus.Connected:
                return ConnectedStatusText;
            case Pm5BleConnectionStatus.ConnectionFailed:
                return ConnectionFailedStatusText;
            default:
                return NotConnectedStatusText;
        }
    }

    private void EnsureClient()
    {
        if (client != null)
        {
            return;
        }

        Client = new WindowsPm5BleClient();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
