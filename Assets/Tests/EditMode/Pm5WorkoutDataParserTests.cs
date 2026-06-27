using NUnit.Framework;
using System;

public class Pm5WorkoutDataParserTests
{
    [Test]
    public void ParseHexPayload_ConvertsHelperMetricLinesToBytes()
    {
        Assert.IsTrue(Pm5WorkoutDataParser.TryParseHexPayload("0102FF", out var payload));

        Assert.AreEqual(new byte[] { 1, 2, 255 }, payload);
    }

    [Test]
    public void AdditionalStatus1_ParsesStrokeRateAndHeartRate()
    {
        var metrics = new Pm5WorkoutMetrics();
        var payload = new byte[] { 0, 0, 0, 0, 0, 32, 146 };

        Assert.IsTrue(Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(Pm5WorkoutDataParser.RowingAdditionalStatus1Uuid, payload, ref metrics));

        Assert.IsTrue(metrics.HasStrokeRateSpm);
        Assert.AreEqual(32f, metrics.StrokeRateSpm, 0.001f);
        Assert.IsTrue(metrics.HasHeartRateBpm);
        Assert.AreEqual(146f, metrics.HeartRateBpm, 0.001f);
    }

    [Test]
    public void AdditionalStrokeData_ParsesWatts()
    {
        var metrics = new Pm5WorkoutMetrics();
        var payload = new byte[] { 0, 0, 0, 200, 0 };

        Assert.IsTrue(Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(Pm5WorkoutDataParser.RowingAdditionalStrokeDataUuid, payload, ref metrics));

        Assert.IsTrue(metrics.HasWatts);
        Assert.AreEqual(200f, metrics.Watts, 0.001f);
    }

    [Test]
    public void StrokeData_ParsesTotalStrokes()
    {
        var metrics = new Pm5WorkoutMetrics();
        var payload = new byte[20];
        payload[16] = 200;
        payload[17] = 1;
        payload[18] = 123;
        payload[19] = 0;

        Assert.IsTrue(Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(Pm5WorkoutDataParser.RowingStrokeDataUuid, payload, ref metrics));

        Assert.IsTrue(metrics.HasTotalStrokes);
        Assert.AreEqual(123, metrics.TotalStrokes);
    }

    [Test]
    public void MultiplexedAdditionalStrokeData_ParsesWattsAndTotalStrokes()
    {
        var metrics = new Pm5WorkoutMetrics();
        var payload = new byte[18];
        payload[0] = 0x36;
        payload[4] = 210;
        payload[5] = 0;
        payload[8] = 17;
        payload[9] = 0;

        Assert.IsTrue(Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(Pm5WorkoutDataParser.MultiplexedInformationUuid, payload, ref metrics));

        Assert.IsTrue(metrics.HasWatts);
        Assert.AreEqual(210f, metrics.Watts, 0.001f);
        Assert.IsTrue(metrics.HasTotalStrokes);
        Assert.AreEqual(17, metrics.TotalStrokes);
    }

    [Test]
    public void MultiplexedStrokeData_ParsesTotalStrokesFromMultiplexedOffset()
    {
        var metrics = new Pm5WorkoutMetrics();
        var payload = new byte[19];
        payload[0] = 0x35;
        payload[17] = 42;
        payload[18] = 0;

        Assert.IsTrue(Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(Pm5WorkoutDataParser.MultiplexedInformationUuid, payload, ref metrics));

        Assert.IsTrue(metrics.HasTotalStrokes);
        Assert.AreEqual(42, metrics.TotalStrokes);
    }

    [Test]
    public void UnknownCharacteristic_DoesNotChangeMetrics()
    {
        var metrics = new Pm5WorkoutMetrics();

        Assert.IsFalse(Pm5WorkoutDataParser.TryApplyCharacteristicUpdate(Guid.NewGuid(), new byte[] { 1, 2, 3 }, ref metrics));
        Assert.IsFalse(metrics.HasAnyMetrics);
    }
}
