using UnityEngine;

[DisallowMultipleComponent]
public class Pm5WorkoutDataSource : MonoBehaviour, IWorkoutMetricsSource, IStrokeMetricsSource
{
    public Pm5BleRuntimeConnector pm5Connector;

    private IPm5WorkoutDataClient workoutDataClient;
    private Pm5WorkoutMetrics latestMetrics;

    public bool HasWorkoutMetrics => latestMetrics.HasWatts || latestMetrics.HasHeartRateBpm;

    public float Watts => latestMetrics.HasWatts ? latestMetrics.Watts : 0f;

    public float HeartRateBpm => latestMetrics.HasHeartRateBpm ? latestMetrics.HeartRateBpm : 0f;

    public bool HasStrokeMetrics => latestMetrics.HasStrokeRateSpm && latestMetrics.HasTotalStrokes;

    public float StrokeRateSpm => latestMetrics.HasStrokeRateSpm ? latestMetrics.StrokeRateSpm : 0f;

    public int TotalStrokes => latestMetrics.HasTotalStrokes ? latestMetrics.TotalStrokes : 0;

    private void Awake()
    {
        CacheConnectorIfNeeded();
        RefreshClientSubscription();
    }

    private void Update()
    {
        RefreshClientSubscription();
        ReadLatestMetrics();
    }

    private void OnDestroy()
    {
        if (workoutDataClient != null)
        {
            workoutDataClient.WorkoutDataChanged -= ReadLatestMetrics;
        }
    }

    public void RefreshClientSubscription()
    {
        CacheConnectorIfNeeded();

        var nextClient = pm5Connector != null ? pm5Connector.Client as IPm5WorkoutDataClient : null;
        if (ReferenceEquals(nextClient, workoutDataClient))
        {
            return;
        }

        if (workoutDataClient != null)
        {
            workoutDataClient.WorkoutDataChanged -= ReadLatestMetrics;
        }

        workoutDataClient = nextClient;

        if (workoutDataClient != null)
        {
            workoutDataClient.WorkoutDataChanged += ReadLatestMetrics;
            Debug.Log("[Ski-Verse PM5 BLE] PM5 workout data source connected to BLE client.");
        }

        ReadLatestMetrics();
    }

    private void ReadLatestMetrics()
    {
        if (workoutDataClient == null || !workoutDataClient.HasWorkoutData)
        {
            return;
        }

        latestMetrics = workoutDataClient.LatestWorkoutMetrics;
    }

    private void CacheConnectorIfNeeded()
    {
        if (pm5Connector == null)
        {
            pm5Connector = Object.FindFirstObjectByType<Pm5BleRuntimeConnector>();
        }
    }
}
