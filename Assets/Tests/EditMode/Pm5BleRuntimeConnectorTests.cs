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
    public void WindowsPm5BleClient_ConnectScriptSubscribesPrimaryStrokeDataBeforeOptionalMetrics()
    {
        var script = BuildConnectScriptForTest();
        var primaryStrokeSubscribe = "$primarySubscription = Subscribe-Pm5Characteristic $pm5Service 'Rowing Stroke Data'";
        var optionalStatusSubscribe = "$optionalSubscriptions += Subscribe-Pm5Characteristic $pm5Service 'Rowing Additional Status 1'";

        StringAssert.Contains(primaryStrokeSubscribe, script);
        StringAssert.Contains(optionalStatusSubscribe, script);
        Assert.Less(script.IndexOf(primaryStrokeSubscribe, StringComparison.Ordinal), script.IndexOf(optionalStatusSubscribe, StringComparison.Ordinal));
    }

    private static string BuildConnectScriptForTest()
    {
        var method = typeof(WindowsPm5BleClient).GetMethod("BuildConnectScript", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, new object[] { new Pm5BleDeviceInfo("pm5-1", "PM5 12345", 12345) });
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
