using System;
using System.Reflection;
using NUnit.Framework;

public class Pm5BleRuntimeConnectorTests
{
    [Test]
    public void StatusToText_ReturnsRequiredPm5UiStates()
    {
        Assert.AreEqual("PM5: Not connected", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.NotConnected));
        Assert.AreEqual("Searching...", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Searching));
        Assert.AreEqual("PM5 Found - not connected", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Pm5Found));
        Assert.AreEqual("Connecting...", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Connecting));
        Assert.AreEqual("Connected", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Connected));
        Assert.AreEqual("PM5 Found - connection not implemented", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.Pm5FoundConnectionNotImplemented));
        Assert.AreEqual("Connection Failed", Pm5BleRuntimeConnector.StatusToText(Pm5BleConnectionStatus.ConnectionFailed));
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
    public void WindowsPm5BleClient_CSharpConnectHelperUsesAwaitedCccdNotifyWrite()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("[STAThread]", source);
        StringAssert.Contains("WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify)", source);
        StringAssert.Contains("await TimeoutAfter", source);
        StringAssert.Contains("METRIC_RAW|{0}|{1}|{2}", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperLogsCccdOperationDiagnostics()
    {
        var source = BuildCSharpConnectHelperSourceForTest();

        StringAssert.Contains("PM5 notification write operation created", source);
        StringAssert.Contains("FormatWinRtOperationDiagnostics(writeOperation)", source);
        StringAssert.Contains("Diagnostics={4}", source);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperTriesMultiplexedBeforeDirectStrokeFallback()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var multiplexedIndex = source.IndexOf("Multiplexed Information", StringComparison.Ordinal);
        var strokeIndex = source.IndexOf("Rowing Stroke Data", StringComparison.Ordinal);

        Assert.GreaterOrEqual(multiplexedIndex, 0);
        Assert.GreaterOrEqual(strokeIndex, 0);
        Assert.Less(multiplexedIndex, strokeIndex);
    }

    [Test]
    public void WindowsPm5BleClient_CSharpConnectHelperUsesBluetoothDeviceServiceBeforeSelectorFallback()
    {
        var source = BuildCSharpConnectHelperSourceForTest();
        var bluetoothDeviceResolveIndex = source.IndexOf("BluetoothLEDevice device;", StringComparison.Ordinal);
        var selectorFallbackCallIndex = source.IndexOf("TryOpenWorkoutServiceFromSelector(address)", StringComparison.Ordinal);

        Assert.GreaterOrEqual(bluetoothDeviceResolveIndex, 0);
        Assert.GreaterOrEqual(selectorFallbackCallIndex, 0);
        Assert.Less(bluetoothDeviceResolveIndex, selectorFallbackCallIndex);
    }

    private static string BuildConnectScriptForTest()
    {
        var method = typeof(WindowsPm5BleClient).GetMethod("BuildConnectScript", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, new object[] { new Pm5BleDeviceInfo("pm5-1", "PM5 12345", 12345) });
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
