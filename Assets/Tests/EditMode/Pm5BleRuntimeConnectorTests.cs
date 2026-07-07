using System;
using System.Reflection;
using NUnit.Framework;

public class Pm5BleRuntimeConnectorTests
{
    [Test]
    public void StatusToText_ReturnsRequiredPm5UiStates()
    {
        Assert.AreEqual("PM5: Not connected", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.NotConnected));
        Assert.AreEqual("PM5: Searching", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Searching));
        Assert.AreEqual("PM5: Found", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Pm5Found));
        Assert.AreEqual("PM5: Connecting", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Connecting));
        Assert.AreEqual("PM5: Connected", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Connected));
        Assert.AreEqual("PM5 Found - connection not implemented", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Pm5FoundConnectionNotImplemented));
        Assert.AreEqual("Connection Failed", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.ConnectionFailed));
    }

    [Test]
    public void DataStatusToText_ReturnsRequiredPm5WorkoutDataUiStates()
    {
        Assert.AreEqual("Data: Waiting for workout data", Pm5BleRuntimeConnector.DataStatusToText(Pm5WorkoutDataStatus.WaitingForWorkoutData));
        Assert.AreEqual("Data: Subscribing to workout notifications", Pm5BleRuntimeConnector.DataStatusToText(Pm5WorkoutDataStatus.SubscribingToWorkoutNotifications));
        Assert.AreEqual("Data: Receiving live data", Pm5BleRuntimeConnector.DataStatusToText(Pm5WorkoutDataStatus.ReceivingLiveData));
        Assert.AreEqual("Data: Notification subscription failed", Pm5BleRuntimeConnector.DataStatusToText(Pm5WorkoutDataStatus.NotificationSubscriptionFailed));
    }

    [Test]
    public void WindowsPm5BleClient_DetectsConcept2Pm5Advertisements()
    {
        Assert.IsTrue(WindowsPm5BleClient.IsConcept2Pm5Advertisement("PM5 12345", new Guid[0]));
        Assert.IsTrue(WindowsPm5BleClient.IsConcept2Pm5Advertisement("Concept2 PM5", new Guid[0]));
        Assert.IsTrue(WindowsPm5BleClient.IsConcept2Pm5Advertisement(string.Empty, new[] { WindowsPm5BleClient.Concept2ServiceUuid }));
        Assert.IsTrue(WindowsPm5BleClient.IsConcept2Pm5Advertisement(string.Empty, new[] { WindowsPm5BleClient.Concept2WorkoutDataServiceUuid }));
        Assert.IsFalse(WindowsPm5BleClient.IsConcept2Pm5Advertisement("Bluetooth Headphones", new Guid[0]));
    }

    [Test]
    public void WindowsPm5BleClient_ProductionScanScriptDoesNotUseWindowsKnownDeviceFallback()
    {
        var script = BuildScanScriptForTest(Pm5BleDiscoveryMode.ProductionBleAdvertisementsOnly);

        StringAssert.Contains("Production BLE advertisement scan started", script);
        StringAssert.Contains("Development Windows known-device fallback disabled", script);
        StringAssert.DoesNotContain("Development fallback checking Windows PnP", script);
        StringAssert.DoesNotContain("Report-Pm5 'DevelopmentWindowsPnP'", script);
        StringAssert.DoesNotContain("CreateWatcher($selector)", script);
    }

    [Test]
    public void WindowsPm5BleClient_DevelopmentScanScriptSeparatesAdvertisementScanFromWindowsFallback()
    {
        var script = BuildScanScriptForTest(Pm5BleDiscoveryMode.DevelopmentWindowsKnownDevicesFallback);
        var advertisementIndex = script.IndexOf("Production BLE advertisement scan started", StringComparison.Ordinal);
        var fallbackIndex = script.IndexOf("Development Windows known-device fallback enabled", StringComparison.Ordinal);

        Assert.That(advertisementIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(fallbackIndex, Is.GreaterThan(advertisementIndex));
        StringAssert.Contains("Report-Pm5 'Advertisement'", script);
        StringAssert.Contains("Report-Pm5 'DevelopmentDeviceWatcher'", script);
        StringAssert.Contains("Report-Pm5 'DevelopmentWindowsPnP'", script);
    }

    [Test]
    public void WindowsPm5BleClient_ScanScriptLogsDiscoveryCounters()
    {
        var script = BuildScanScriptForTest(Pm5BleDiscoveryMode.DevelopmentWindowsKnownDevicesFallback);

        StringAssert.Contains("$advertisementCount = 0", script);
        StringAssert.Contains("$pm5AdvertisementMatchCount = 0", script);
        StringAssert.Contains("Production BLE advertisement scan summary", script);
        StringAssert.Contains("$developmentDeviceWatcherCount = 0", script);
        StringAssert.Contains("$developmentPnpCount = 0", script);
        StringAssert.Contains("Development fallback scan summary", script);
    }

    [Test]
    public void WindowsPm5BleClient_ScanScriptLogsBluetoothAdapterAndWatcherStatus()
    {
        var script = BuildScanScriptForTest(Pm5BleDiscoveryMode.DevelopmentWindowsKnownDevicesFallback);

        StringAssert.Contains("Bluetooth support service status", script);
        StringAssert.Contains("Bluetooth PnP adapter", script);
        StringAssert.Contains("Advertisement watcher initial status", script);
        StringAssert.Contains("Advertisement watcher start requested", script);
        StringAssert.Contains("Advertisement watcher status after start", script);
        StringAssert.Contains("Advertisement watcher status before stop", script);
        StringAssert.Contains("Advertisement watcher status after stop", script);
    }

    [Test]
    public void RuntimeConnector_StopsPm5ClientWhenDisabled()
    {
        var gameObject = new UnityEngine.GameObject("PM5 Runtime Connector");
        var connector = gameObject.AddComponent<Pm5BleRuntimeConnector>();
        var client = new FakePm5BleClient();

        connector.Client = client;
        gameObject.SetActive(false);

        Assert.AreEqual(1, client.StopScanCount);

        UnityEngine.Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void RuntimeConnector_UsesProbeBridgeByDefaultAndKeepsLegacyWindowsFallback()
    {
        var bridgeObject = new UnityEngine.GameObject("PM5 Bridge Connector");
        var bridgeConnector = bridgeObject.AddComponent<Pm5BleRuntimeConnector>();

        Assert.AreEqual(Pm5BleClientMode.ProbeBridge, bridgeConnector.clientMode);
        Assert.IsInstanceOf<Pm5ProbeBridgeClient>(bridgeConnector.Client);

        var legacyObject = new UnityEngine.GameObject("PM5 Legacy Connector");
        var legacyConnector = legacyObject.AddComponent<Pm5BleRuntimeConnector>();
        legacyConnector.clientMode = Pm5BleClientMode.LegacyWindowsBle;

        Assert.IsInstanceOf<WindowsPm5BleClient>(legacyConnector.Client);

        UnityEngine.Object.DestroyImmediate(bridgeObject);
        UnityEngine.Object.DestroyImmediate(legacyObject);
    }

    [Test]
    public void Pm5ProbeBridgeClient_BuildsMachineReadableBridgeArguments()
    {
        var arguments = Pm5ProbeBridgeClient.BuildProbeArguments("C:\\Probe\\Pm5BleProbe.csproj", 30);

        StringAssert.Contains("run --project \"C:\\Probe\\Pm5BleProbe.csproj\"", arguments);
        StringAssert.Contains("-- --bridge --scan-seconds 30", arguments);
    }

    [Test]
    public void Pm5ProbeBridgeClient_UsesConfiguredDotnetExecutableWhenProvided()
    {
        const string configuredDotnet = "C:\\Tools\\dotnet\\dotnet.exe";

        Assert.AreEqual(configuredDotnet, Pm5ProbeBridgeClient.ResolveDotnetExecutable(configuredDotnet));
    }

    [Test]
    public void Pm5ProbeBridgeClient_ResolvesProgramFilesDotnetBeforePathFallback()
    {
        var programFilesDotnet = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "dotnet",
            "dotnet.exe");

        var expected = System.IO.File.Exists(programFilesDotnet)
            ? programFilesDotnet
            : "dotnet";

        Assert.AreEqual(expected, Pm5ProbeBridgeClient.ResolveDotnetExecutable(null));
        Assert.AreEqual(expected, Pm5ProbeBridgeClient.ResolveDotnetExecutable("dotnet"));
    }

    [Test]
    public void WindowsPm5BleClient_ConnectScriptSubscribesMultiplexedDataBeforeDirectStrokeFallback()
    {
        var script = BuildConnectScriptForTest();
        var multiplexedSubscribe = "$primarySubscription = Subscribe-Pm5Characteristic $pm5Service 'Multiplexed Information'";
        var directStrokeFallbackSubscribe = "$primarySubscription = Subscribe-Pm5Characteristic $pm5Service 'Rowing Stroke Data'";

        StringAssert.Contains(multiplexedSubscribe, script);
        StringAssert.Contains(directStrokeFallbackSubscribe, script);
        Assert.Less(script.IndexOf(multiplexedSubscribe, StringComparison.Ordinal), script.IndexOf(directStrokeFallbackSubscribe, StringComparison.Ordinal));
    }

    [Test]
    public void WindowsPm5BleClient_ConnectScriptUsesNativeStatusPollingForPm5NotifyWrite()
    {
        var script = BuildConnectScriptForTest();

        StringAssert.Contains("function Await-WinRtOperationByStatus", script);
        StringAssert.Contains("Await-WinRtOperationByStatus ($characteristic.WriteClientCharacteristicConfigurationDescriptorAsync($descriptorValue))", script);
    }

    [Test]
    public void WindowsPm5BleClient_ConnectScriptAttachesNotificationHandlerBeforeNotifyWrite()
    {
        var script = BuildConnectScriptForTest();
        var handlerIndex = script.IndexOf("$token = $characteristic.add_ValueChanged($handler)", StringComparison.Ordinal);
        var notifyWriteIndex = script.IndexOf("WriteClientCharacteristicConfigurationDescriptorAsync($descriptorValue)", StringComparison.Ordinal);

        Assert.GreaterOrEqual(handlerIndex, 0);
        Assert.GreaterOrEqual(notifyWriteIndex, 0);
        Assert.Less(handlerIndex, notifyWriteIndex);
    }

    [Test]
    public void WindowsPm5BleClient_ConnectScriptReadsCccdBeforeAndAfterNotifyWrite()
    {
        var script = BuildConnectScriptForTest();

        StringAssert.Contains("function Read-Pm5CccdValue", script);
        StringAssert.Contains("Read-Pm5CccdValue $characteristic $localName $localUuid $cacheMode 'BeforeNotifyWrite'", script);
        StringAssert.Contains("Read-Pm5CccdValue $characteristic $localName $localUuid $cacheMode 'AfterNotifyWrite'", script);
    }

    [Test]
    public void WindowsPm5BleClient_ConnectScriptLogsWinRtNotifyOperationDiagnostics()
    {
        var script = BuildConnectScriptForTest();

        StringAssert.Contains("function Format-WinRtOperationDiagnostics", script);
        StringAssert.Contains("PM5 notification write operation created", script);
        StringAssert.Contains("Diagnostics={5}", script);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperUsesDirectCccdNotifyWrite()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("[STAThread]", source);
        StringAssert.Contains("private static async Task<GattCommunicationStatus> WriteNotifyCccdStandard", source);
        StringAssert.Contains("private static async Task<GattCommunicationStatus> WriteNotifyCccdDirect", source);
        StringAssert.Contains("WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify)", source);
        StringAssert.Contains("writer.WriteBytes(new byte[] { 0x01, 0x00 })", source);
        StringAssert.Contains("descriptor.WriteValueAsync(payload)", source);
        StringAssert.Contains("await TimeoutAfter", source);
        StringAssert.Contains("METRIC_RAW|{0}|{1}|{2}", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperTriesStandardNotifyBeforeDirectCccdFallback()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var standardIndex = source.IndexOf("var status = await WriteNotifyCccdStandard(characteristic, name, uuid, cacheMode);", StringComparison.Ordinal);
        var fallbackIndex = source.IndexOf("PM5 standard notify write failed. Trying direct CCCD payload.", StringComparison.Ordinal);
        var directIndex = source.IndexOf("status = await WriteNotifyCccdDirect(characteristic, name, uuid, cacheMode);", StringComparison.Ordinal);

        Assert.That(standardIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(fallbackIndex, Is.GreaterThan(standardIndex));
        Assert.That(directIndex, Is.GreaterThan(fallbackIndex));
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperCancelsTimedOutWinRtOperations()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("operation.Cancel();", source);
        StringAssert.Contains("WinRT operation cancel requested after timeout", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperLogsCccdOperationDiagnostics()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("PM5 direct CCCD notify write operation created", source);
        StringAssert.Contains("FormatWinRtOperationDiagnostics(writeOperation)", source);
        StringAssert.Contains("Diagnostics={3}", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperLogsGattSessionAndCccdDescriptorDiagnostics()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("PM5 Bluetooth device state. Phase=AfterResolve", source);
        StringAssert.Contains("PM5 device access and pairing state. Phase=AfterResolve", source);
        StringAssert.Contains("IsPaired", source);
        StringAssert.Contains("CanPair", source);
        StringAssert.Contains("DeviceAccess", source);
        StringAssert.Contains("PM5 GATT session state. Phase=BeforeMaintainConnection", source);
        StringAssert.Contains("CanMaintainConnection", source);
        StringAssert.Contains("SessionStatus", source);
        StringAssert.Contains("PM5 characteristic state before notify", source);
        StringAssert.Contains("PM5 CCCD descriptor lookup result", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperWaitsForActiveGattSessionBeforeNotify()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var waitMethodIndex = source.IndexOf("private static async Task WaitForActiveGattSession", StringComparison.Ordinal);
        var waitCallIndex = source.IndexOf("await WaitForActiveGattSession(session)", StringComparison.Ordinal);
        var subscribeIndex = source.IndexOf("SubscribeCharacteristic(service, \"Rowing Stroke Data\", RowingStrokeDataUuid)", StringComparison.Ordinal);

        Assert.That(waitMethodIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(waitCallIndex, Is.GreaterThan(waitMethodIndex));
        Assert.That(waitCallIndex, Is.LessThan(subscribeIndex));
        StringAssert.Contains("PM5 GATT session wait completed", source);
        StringAssert.Contains("PM5 GATT session wait timed out", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperUsesSingleRowingStrokeDiagnosticSubscription()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("PM5 single Rowing Stroke Data subscription diagnostic started.", source);
        StringAssert.Contains("SubscribeCharacteristic(service, \"Rowing Stroke Data\", RowingStrokeDataUuid)", source);
        StringAssert.DoesNotContain("PM5 workout data subscription attempt", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperFallsBackToMultiplexedWhenRowingStrokeNotifyFails()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var rowingSubscribeIndex = source.IndexOf("SubscribeCharacteristic(service, \"Rowing Stroke Data\", RowingStrokeDataUuid)", StringComparison.Ordinal);
        var multiplexedLogIndex = source.IndexOf("PM5 Rowing Stroke Data subscription failed. Trying Multiplexed Information.", StringComparison.Ordinal);
        var multiplexedSubscribeIndex = source.IndexOf("SubscribeCharacteristic(service, \"Multiplexed Information\", MultiplexedInformationUuid)", StringComparison.Ordinal);
        var dataFailureIndex = source.IndexOf("DATA|NotificationSubscriptionFailed|PM5 Rowing Stroke Data and Multiplexed Information subscriptions failed.", StringComparison.Ordinal);

        Assert.That(rowingSubscribeIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(multiplexedLogIndex, Is.GreaterThan(rowingSubscribeIndex));
        Assert.That(multiplexedSubscribeIndex, Is.GreaterThan(multiplexedLogIndex));
        Assert.That(dataFailureIndex, Is.GreaterThan(multiplexedSubscribeIndex));
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperUsesCachedCharacteristicLookupOnlyForSubscription()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var subscribeStart = source.IndexOf("private static async Task<Subscription> SubscribeCharacteristic", StringComparison.Ordinal);
        var subscribeEnd = source.IndexOf("private static async Task ReadCccd", StringComparison.Ordinal);

        Assert.GreaterOrEqual(subscribeStart, 0);
        Assert.Greater(subscribeEnd, subscribeStart);

        var subscribeMethod = source.Substring(subscribeStart, subscribeEnd - subscribeStart);
        StringAssert.Contains("new[] { BluetoothCacheMode.Cached }", subscribeMethod);
        StringAssert.DoesNotContain("BluetoothCacheMode.Uncached", subscribeMethod);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperKeepsConnectionAliveWhenDiagnosticSubscriptionFails()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var failureIndex = source.IndexOf("FAILED|PM5 Rowing Stroke Data and Multiplexed Information subscriptions failed.", StringComparison.Ordinal);
        var dataFailureIndex = source.IndexOf("DATA|NotificationSubscriptionFailed", StringComparison.Ordinal);
        var keepAliveIndex = source.IndexOf("while (true)", dataFailureIndex, StringComparison.Ordinal);

        Assert.AreEqual(-1, failureIndex);
        Assert.That(dataFailureIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(keepAliveIndex, Is.GreaterThan(dataFailureIndex));
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperUsesServiceSelectorBeforeBluetoothDeviceFallback()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var selectorPrimaryCallIndex = source.IndexOf("TryOpenWorkoutServiceFromSelector(address)", StringComparison.Ordinal);
        var bluetoothDeviceResolveIndex = source.IndexOf("BluetoothLEDevice device;", StringComparison.Ordinal);

        Assert.GreaterOrEqual(selectorPrimaryCallIndex, 0);
        Assert.GreaterOrEqual(bluetoothDeviceResolveIndex, 0);
        Assert.Less(selectorPrimaryCallIndex, bluetoothDeviceResolveIndex);
        StringAssert.Contains("Trying PM5 workout service selector before BluetoothLEDevice resolve.", source);
        StringAssert.Contains("PM5 workout service selector did not open a direct service. Falling back to BluetoothLEDevice service lookup.", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperFallsBackToBluetoothDeviceLookupWhenDirectServiceNotifyFails()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var directServiceIndex = source.IndexOf("var directService = await TryOpenWorkoutServiceFromSelector(address);", StringComparison.Ordinal);
        var directNotifyResultIndex = source.IndexOf("var directNotificationsStarted = await StartWorkoutNotifications(directService, \"Concept2 PM5\", false);", StringComparison.Ordinal);
        var fallbackLogIndex = source.IndexOf("Direct PM5 service opened but notification subscription failed. Retrying through BluetoothLEDevice service lookup.", StringComparison.Ordinal);
        var bluetoothDeviceResolveIndex = source.IndexOf("BluetoothLEDevice device;", StringComparison.Ordinal);

        Assert.That(directServiceIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(directNotifyResultIndex, Is.GreaterThan(directServiceIndex));
        Assert.That(fallbackLogIndex, Is.GreaterThan(directNotifyResultIndex));
        Assert.That(bluetoothDeviceResolveIndex, Is.GreaterThan(fallbackLogIndex));
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperSeparatesConnectionStatusFromDataStatus()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var oldServiceVerifiedConnectedIndex = source.IndexOf("CONNECTED|GATT Concept2 PM5 workout service verified", StringComparison.Ordinal);
        var connectedIndex = source.IndexOf("CONNECTED|PM5 GATT connected", StringComparison.Ordinal);
        var subscribingIndex = source.IndexOf("DATA|SubscribingToWorkoutNotifications", StringComparison.Ordinal);
        var subscriptionFailureIndex = source.IndexOf("FAILED|PM5 Rowing Stroke Data and Multiplexed Information subscriptions failed.", StringComparison.Ordinal);
        var receivingIndex = source.IndexOf("DATA|ReceivingLiveData", StringComparison.Ordinal);
        var dataFailureIndex = source.IndexOf("DATA|NotificationSubscriptionFailed", StringComparison.Ordinal);

        Assert.AreEqual(-1, oldServiceVerifiedConnectedIndex);
        Assert.AreEqual(-1, subscriptionFailureIndex);
        Assert.GreaterOrEqual(connectedIndex, 0);
        Assert.GreaterOrEqual(subscribingIndex, 0);
        Assert.GreaterOrEqual(receivingIndex, 0);
        Assert.GreaterOrEqual(dataFailureIndex, 0);
        Assert.Less(connectedIndex, subscribingIndex);
        Assert.Less(subscribingIndex, dataFailureIndex);
        Assert.Less(subscribingIndex, receivingIndex);
    }

    private static string BuildConnectScriptForTest()
    {
        var method = typeof(WindowsPm5BleClient).GetMethod("BuildConnectScript", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, new object[] { new Pm5BleDeviceInfo("pm5-1", "PM5 12345", 12345) });
    }

    private static string BuildScanScriptForTest(Pm5BleDiscoveryMode mode)
    {
        var method = typeof(WindowsPm5BleClient).GetMethod("BuildScanScript", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, new object[] { mode });
    }

    private static string BuildCSharpConnectHelperSourceForTest()
    {
        var method = typeof(WindowsPm5BleClient).GetMethod("BuildCSharpConnectHelperSource", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, null);
    }

    private sealed class FakePm5BleClient : IPm5BleClient
    {
        public event Action StateChanged;

        public Pm5BleConnectionStatus Status => Pm5BleConnectionStatus.NotConnected;

        public Pm5WorkoutDataStatus DataStatus => Pm5WorkoutDataStatus.WaitingForWorkoutData;

        public System.Collections.Generic.IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices => Array.Empty<Pm5BleDeviceInfo>();

        public int StopScanCount { get; private set; }

        public void StartScan()
        {
            StateChanged?.Invoke();
        }

        public void StopScan()
        {
            StopScanCount++;
        }

        public void Connect(Pm5BleDeviceInfo device)
        {
            StateChanged?.Invoke();
        }
    }
}
