using UnityEngine;

[DisallowMultipleComponent]
public class Pm5PlayerInputSource : MonoBehaviour, IPlayerInputSource
{
    public KeyCode accelerateKey = KeyCode.W;
    public KeyCode decelerateKey = KeyCode.S;
    public MonoBehaviour workoutMetricsSourceBehaviour;

    private IWorkoutMetricsSource workoutMetricsSource;
    private bool searchedForWorkoutMetricsSource;

    private void Awake()
    {
        CacheWorkoutMetricsSourceIfNeeded();
    }

    public PlayerMovementInput ReadMovementInput()
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

        return new PlayerMovementInput(speedAxis, ReadWatts());
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
            return;
        }

        if (searchedForWorkoutMetricsSource)
        {
            return;
        }

        searchedForWorkoutMetricsSource = true;
        var discoveredSource = Object.FindFirstObjectByType<Pm5WorkoutDataSource>();
        if (discoveredSource != null)
        {
            workoutMetricsSourceBehaviour = discoveredSource;
            workoutMetricsSource = discoveredSource;
        }
    }
}
