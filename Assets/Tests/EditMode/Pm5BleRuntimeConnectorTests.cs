using System;
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
        Assert.IsFalse(WindowsPm5BleClient.IsConcept2Pm5Advertisement("Bluetooth Headphones", new Guid[0]));
    }
}
