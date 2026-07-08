using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Pm5BleRuntimeConnector : MonoBehaviour
{
    public const string NotConnectedStatusText = "PM5: Not connected";
    public const string SearchingStatusText = "PM5: Searching";
    public const string Pm5FoundStatusText = "PM5: Found";
    public const string ConnectingStatusText = "PM5: Connecting";
    public const string ConnectedStatusText = "PM5: Connected";
    public const string Pm5FoundConnectionNotImplementedStatusText = "PM5 Found - connection not implemented";
    public const string ConnectionFailedStatusText = "Connection Failed";
    public const string WaitingForWorkoutDataText = "Data: Waiting for workout data";
    public const string SubscribingToWorkoutNotificationsText = "Data: Subscribing to workout notifications";
    public const string ReceivingLiveDataText = "Data: Receiving live data";
    public const string NotificationSubscriptionFailedText = "Data: Notification subscription failed";

    [Tooltip("Production mode uses BLE advertisements only. Development fallback can use Windows known/paired/PnP BLE devices to continue debugging on Windows.")]
    public Pm5BleDiscoveryMode discoveryMode = Pm5BleDiscoveryMode.DevelopmentWindowsKnownDevicesFallback;
    [Tooltip("Probe Bridge starts the standalone .NET Pm5BleProbe process and reads parsed PM5 metrics from stdout. Legacy Windows BLE keeps the previous Unity helper path as fallback.")]
    public Pm5BleClientMode clientMode = Pm5BleClientMode.ProbeBridge;

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

    public Pm5WorkoutDataStatus DataStatus => Client.DataStatus;

    public IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices => Client.DiscoveredDevices;

    public IPm5WorkoutDataClient WorkoutDataClient => Client as IPm5WorkoutDataClient;

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

    private void OnDisable()
    {
        StopClient();
    }

    private void OnApplicationQuit()
    {
        StopClient();
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.StateChanged -= NotifyStateChanged;
            StopClient();
        }
    }

    public void StartScan()
    {
        PumpClientIfNeeded();
        Debug.Log("[Ski-Verse PM5 BLE] Runtime connector starts scan.");
        selectedDeviceIndex = -1;
        Client.StartScan();
        NotifyStateChanged();
    }

    public void SelectDevice(int deviceIndex)
    {
        PumpClientIfNeeded();
        var devices = DiscoveredDevices;
        selectedDeviceIndex = deviceIndex >= 0 && deviceIndex < devices.Count ? deviceIndex : -1;

        if (selectedDeviceIndex >= 0)
        {
            var device = devices[selectedDeviceIndex];
            Debug.Log($"[Ski-Verse PM5 BLE] Selected PM5 device. Index={selectedDeviceIndex}, Name='{device.Name}', DeviceId='{device.DeviceId}', Address='{device.BluetoothAddress}'.");
        }
        else
        {
            Debug.LogWarning($"[Ski-Verse PM5 BLE] PM5 selection ignored. RequestedIndex={deviceIndex}, DeviceCount={devices.Count}.");
        }

        NotifyStateChanged();
    }

    public void ConnectSelectedDevice()
    {
        PumpClientIfNeeded();
        var selected = SelectedDevice;
        if (!selected.HasValue)
        {
            Debug.LogWarning("[Ski-Verse PM5 BLE] Connect requested without selected PM5. Restarting scan.");
            StartScan();
            return;
        }

        Debug.Log($"[Ski-Verse PM5 BLE] Runtime connector connects selected PM5 '{selected.Value.Name}'.");
        Client.Connect(selected.Value);
        NotifyStateChanged();
    }

    public string GetStatusText()
    {
        PumpClientIfNeeded();
        return StatusToText(Status);
    }

    public string GetDataStatusText()
    {
        PumpClientIfNeeded();
        return DataStatusToText(DataStatus);
    }

    private void Update()
    {
        PumpClientIfNeeded();
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
            case Pm5BleConnectionStatus.Pm5FoundConnectionNotImplemented:
                return Pm5FoundConnectionNotImplementedStatusText;
            case Pm5BleConnectionStatus.ConnectionFailed:
                return ConnectionFailedStatusText;
            default:
                return NotConnectedStatusText;
        }
    }

    public static string DataStatusToText(Pm5WorkoutDataStatus status)
    {
        switch (status)
        {
            case Pm5WorkoutDataStatus.SubscribingToWorkoutNotifications:
                return SubscribingToWorkoutNotificationsText;
            case Pm5WorkoutDataStatus.ReceivingLiveData:
                return ReceivingLiveDataText;
            case Pm5WorkoutDataStatus.NotificationSubscriptionFailed:
                return NotificationSubscriptionFailedText;
            default:
                return WaitingForWorkoutDataText;
        }
    }

    private void EnsureClient()
    {
        if (client != null)
        {
            return;
        }

        Client = clientMode == Pm5BleClientMode.ProbeBridge
            ? new Pm5ProbeBridgeClient()
            : new WindowsPm5BleClient(discoveryMode);
    }

    private void PumpClientIfNeeded()
    {
        if (client is IPm5BleClientPump pump)
        {
            pump.Pump();
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private void StopClient()
    {
        if (client == null)
        {
            return;
        }

        Debug.Log("[Ski-Verse PM5 BLE] Runtime connector stops PM5 BLE client.");
        client.StopScan();
    }
}
