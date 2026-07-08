using UnityEngine;

[DisallowMultipleComponent]
public class Pm5PlayerInputSource : MonoBehaviour, IPlayerInputSource
{
    public KeyCode accelerateKey = KeyCode.W;
    public KeyCode decelerateKey = KeyCode.S;
    public MonoBehaviour workoutMetricsSourceBehaviour;
    public float strokePolingTimeoutSeconds = 1.75f;

    private IWorkoutMetricsSource workoutMetricsSource;
    private IStrokeMetricsSource strokeMetricsSource;
    private bool hasLastStrokeCount;
    private int lastStrokeCount;
    private float lastStrokeReceivedTime = float.NegativeInfinity;

    private void Awake()
    {
        CacheWorkoutMetricsSourceIfNeeded();
    }

    public PlayerMovementInput ReadMovementInput()
    {
        return ReadMovementInput(Time.time);
    }

    public PlayerMovementInput ReadMovementInput(float currentTimeSeconds)
    {
        CacheWorkoutMetricsSourceIfNeeded();
        var speedAxis = 0f;

        if (Input.GetKey(accelerateKey))
        {
            speedAxis += 1f;
        }

        if (Input.GetKey(decelerateKey))
        {
            speedAxis -= 1f;
        }

        var isActivelyPoling = speedAxis > 0f || ReadStrokePolingState(currentTimeSeconds);
        return new PlayerMovementInput(speedAxis, ReadWatts(), isActivelyPoling);
    }

    private float ReadWatts()
    {
        if (workoutMetricsSource == null || !workoutMetricsSource.HasWorkoutMetrics)
        {
            return 0f;
        }

        return Mathf.Max(0f, workoutMetricsSource.Watts);
    }

    private void CacheWorkoutMetricsSourceIfNeeded()
    {
        if (workoutMetricsSource != null)
        {
            return;
        }

        if (workoutMetricsSourceBehaviour is IWorkoutMetricsSource configuredSource)
        {
            workoutMetricsSource = configuredSource;
            strokeMetricsSource = workoutMetricsSourceBehaviour as IStrokeMetricsSource;
            return;
        }

        var discoveredSource = Object.FindFirstObjectByType<Pm5WorkoutDataSource>();
        if (discoveredSource != null)
        {
            workoutMetricsSourceBehaviour = discoveredSource;
            workoutMetricsSource = discoveredSource;
            strokeMetricsSource = discoveredSource;
        }
    }

    private bool ReadStrokePolingState(float currentTimeSeconds)
    {
        if (strokeMetricsSource == null || !strokeMetricsSource.HasStrokeMetrics)
        {
            return false;
        }

        var currentStrokeCount = Mathf.Max(0, strokeMetricsSource.TotalStrokes);
        if (hasLastStrokeCount && currentStrokeCount > lastStrokeCount)
        {
            lastStrokeReceivedTime = currentTimeSeconds;
        }

        hasLastStrokeCount = true;
        lastStrokeCount = currentStrokeCount;
        return currentTimeSeconds - lastStrokeReceivedTime <= Mathf.Max(0f, strokePolingTimeoutSeconds);
    }
}
