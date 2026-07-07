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
    public bool HasElapsedTimeSeconds;
    public float ElapsedTimeSeconds;
    public bool HasDistanceMeters;
    public float DistanceMeters;
    public bool HasSpeedKmh;
    public float SpeedKmh;
    public bool HasPaceSecondsPer500m;
    public float PaceSecondsPer500m;
    public bool HasAveragePaceSecondsPer500m;
    public float AveragePaceSecondsPer500m;
    public bool HasSplitAverageWatts;
    public float SplitAverageWatts;
    public bool HasDragFactor;
    public int DragFactor;
    public bool HasWorkoutState;
    public int WorkoutState;
    public bool HasRowingState;
    public int RowingState;
    public bool HasStrokeState;
    public int StrokeState;

    public bool HasAnyMetrics =>
        HasWatts ||
        HasHeartRateBpm ||
        HasStrokeRateSpm ||
        HasTotalStrokes ||
        HasElapsedTimeSeconds ||
        HasDistanceMeters ||
        HasSpeedKmh ||
        HasPaceSecondsPer500m ||
        HasAveragePaceSecondsPer500m ||
        HasSplitAverageWatts ||
        HasDragFactor ||
        HasWorkoutState ||
        HasRowingState ||
        HasStrokeState;
}

public interface IPm5WorkoutDataClient
{
    event System.Action WorkoutDataChanged;

    bool HasWorkoutData { get; }

    Pm5WorkoutMetrics LatestWorkoutMetrics { get; }
}
