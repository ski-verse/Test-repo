public struct Pm5WorkoutMetrics
{
    public bool HasWatts;
    public float Watts;
    public bool HasHeartRateBpm;
    public float HeartRateBpm;
    public bool HasStrokeRateSpm;
    public float StrokeRateSpm;
    public bool HasTotalStrokes;
    public int TotalStrokes;

    public bool HasAnyMetrics => HasWatts || HasHeartRateBpm || HasStrokeRateSpm || HasTotalStrokes;
}

public interface IPm5WorkoutDataClient
{
    event System.Action WorkoutDataChanged;

    bool HasWorkoutData { get; }

    Pm5WorkoutMetrics LatestWorkoutMetrics { get; }
}
