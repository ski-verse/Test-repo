using NUnit.Framework;

public class Pm5ProbeBridgeMetricsParserTests
{
    [Test]
    public void TryApplyMetricsLine_MapsPm5BridgeValuesToWorkoutMetrics()
    {
        var metrics = new Pm5WorkoutMetrics();
        var line = "PM5_METRICS|Watts=126|SPM=56|StrokeCount=25|Time=00:26.82|Distance=84.7|SpeedKmh=12.8|Pace=02:20.88|AvgPace=02:22.00|SplitAvgWatts=120|DragFactor=113|HeartRate=|WorkoutState=3|RowingState=4|StrokeState=5";

        Assert.IsTrue(Pm5ProbeBridgeMetricsParser.TryApplyMetricsLine(line, ref metrics));

        Assert.IsTrue(metrics.HasWatts);
        Assert.AreEqual(126f, metrics.Watts, 0.001f);
        Assert.IsTrue(metrics.HasStrokeRateSpm);
        Assert.AreEqual(56f, metrics.StrokeRateSpm, 0.001f);
        Assert.IsTrue(metrics.HasTotalStrokes);
        Assert.AreEqual(25, metrics.TotalStrokes);
        Assert.IsTrue(metrics.HasElapsedTimeSeconds);
        Assert.AreEqual(26.82f, metrics.ElapsedTimeSeconds, 0.001f);
        Assert.IsTrue(metrics.HasDistanceMeters);
        Assert.AreEqual(84.7f, metrics.DistanceMeters, 0.001f);
        Assert.IsTrue(metrics.HasSpeedKmh);
        Assert.AreEqual(12.8f, metrics.SpeedKmh, 0.001f);
        Assert.IsTrue(metrics.HasPaceSecondsPer500m);
        Assert.AreEqual(140.88f, metrics.PaceSecondsPer500m, 0.001f);
        Assert.IsTrue(metrics.HasAveragePaceSecondsPer500m);
        Assert.AreEqual(142f, metrics.AveragePaceSecondsPer500m, 0.001f);
        Assert.IsTrue(metrics.HasSplitAverageWatts);
        Assert.AreEqual(120f, metrics.SplitAverageWatts, 0.001f);
        Assert.IsTrue(metrics.HasDragFactor);
        Assert.AreEqual(113, metrics.DragFactor);
        Assert.IsFalse(metrics.HasHeartRateBpm);
        Assert.IsTrue(metrics.HasWorkoutState);
        Assert.AreEqual(3, metrics.WorkoutState);
        Assert.IsTrue(metrics.HasRowingState);
        Assert.AreEqual(4, metrics.RowingState);
        Assert.IsTrue(metrics.HasStrokeState);
        Assert.AreEqual(5, metrics.StrokeState);
    }

    [Test]
    public void TryApplyMetricsLine_IgnoresNonMetricsLines()
    {
        var metrics = new Pm5WorkoutMetrics();

        Assert.IsFalse(Pm5ProbeBridgeMetricsParser.TryApplyMetricsLine("LOG|PM5 connected", ref metrics));
        Assert.IsFalse(metrics.HasAnyMetrics);
    }

    [Test]
    public void TryApplyMetricsLine_BlankHeartRateClearsPreviousHeartRate()
    {
        var metrics = new Pm5WorkoutMetrics
        {
            HasHeartRateBpm = true,
            HeartRateBpm = 146f
        };

        Assert.IsTrue(Pm5ProbeBridgeMetricsParser.TryApplyMetricsLine("PM5_METRICS|Watts=126|HeartRate=", ref metrics));

        Assert.IsFalse(metrics.HasHeartRateBpm);
        Assert.AreEqual(0f, metrics.HeartRateBpm, 0.001f);
    }
}
