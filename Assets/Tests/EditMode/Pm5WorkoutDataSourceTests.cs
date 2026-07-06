using NUnit.Framework;
using System;
using UnityEngine;

public class Pm5WorkoutDataSourceTests
{
    [Test]
    public void WorkoutDataSource_ExposesLatestPm5MetricsForHud()
    {
        var connectorObject = new GameObject("PM5 Connector");
        var connector = connectorObject.AddComponent<Pm5BleRuntimeConnector>();
        var client = new FakePm5BleClient();
        connector.Client = client;

        var source = connectorObject.AddComponent<Pm5WorkoutDataSource>();
        source.pm5Connector = connector;
        source.RefreshClientSubscription();

        client.SetMetrics(new Pm5WorkoutMetrics
        {
            HasWatts = true,
            Watts = 186f,
            HasHeartRateBpm = true,
            HeartRateBpm = 146f,
            HasStrokeRateSpm = true,
            StrokeRateSpm = 31f,
            HasTotalStrokes = true,
            TotalStrokes = 52
        });

        Assert.IsTrue(source.HasWorkoutMetrics);
        Assert.AreEqual(186f, source.Watts, 0.001f);
        Assert.AreEqual(146f, source.HeartRateBpm, 0.001f);
        Assert.IsTrue(source.HasStrokeMetrics);
        Assert.AreEqual(31f, source.StrokeRateSpm, 0.001f);
        Assert.AreEqual(52, source.TotalStrokes);

        UnityEngine.Object.DestroyImmediate(connectorObject);
    }

    private sealed class FakePm5BleClient : IPm5BleClient, IPm5WorkoutDataClient
    {
        private Pm5WorkoutMetrics metrics;

        public event Action StateChanged;
        public event Action WorkoutDataChanged;

        public Pm5BleConnectionStatus Status => Pm5BleConnectionStatus.Connected;

        public Pm5WorkoutDataStatus DataStatus => Pm5WorkoutDataStatus.WaitingForWorkoutData;

        public System.Collections.Generic.IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices => Array.Empty<Pm5BleDeviceInfo>();

        public bool HasWorkoutData => metrics.HasAnyMetrics;

        public Pm5WorkoutMetrics LatestWorkoutMetrics => metrics;

        public void StartScan()
        {
            StateChanged?.Invoke();
        }

        public void StopScan()
        {
        }

        public void Connect(Pm5BleDeviceInfo device)
        {
            StateChanged?.Invoke();
        }

        public void SetMetrics(Pm5WorkoutMetrics nextMetrics)
        {
            metrics = nextMetrics;
            WorkoutDataChanged?.Invoke();
        }
    }
}
