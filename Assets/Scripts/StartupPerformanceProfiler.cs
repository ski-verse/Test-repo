using System.Diagnostics;
using UnityEngine;

public class StartupPerformanceProfiler : MonoBehaviour
{
    public const float StartupProfileDurationSeconds = 20f;
    public const float HitchThresholdSeconds = 0.05f;
    private const string RuntimeProfilerName = "Ski-Verse Startup Performance Profiler";

    private float elapsedSeconds;
    private int frameCount;
    private int hitchCount;
    private float worstFrameSeconds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallStartupProfiler()
    {
        if (Object.FindFirstObjectByType<StartupPerformanceProfiler>() != null)
        {
            return;
        }

        var profiler = new GameObject(RuntimeProfilerName);
        profiler.AddComponent<StartupPerformanceProfiler>();
        Log("first 20 second frame profiler started");
    }

    private void Update()
    {
        var delta = Time.unscaledDeltaTime;
        elapsedSeconds += delta;
        frameCount++;
        worstFrameSeconds = Mathf.Max(worstFrameSeconds, delta);

        if (delta >= HitchThresholdSeconds)
        {
            hitchCount++;
            Log($"startup hitch frame {hitchCount}: {delta * 1000f:0.0} ms at {elapsedSeconds:0.00}s");
        }

        if (elapsedSeconds >= StartupProfileDurationSeconds)
        {
            var averageFrameMs = frameCount > 0 ? elapsedSeconds / frameCount * 1000f : 0f;
            Log($"first 20 second frame profiler finished: frames={frameCount}, hitches={hitchCount}, avg={averageFrameMs:0.0} ms, worst={worstFrameSeconds * 1000f:0.0} ms");
            Destroy(gameObject);
        }
    }

    public static StartupTimingScope Measure(string systemName)
    {
        return new StartupTimingScope(systemName);
    }

    public static void Log(string message)
    {
        UnityEngine.Debug.Log($"[Ski-Verse Startup] {message}");
    }
}

public readonly struct StartupTimingScope : System.IDisposable
{
    private readonly string systemName;
    private readonly Stopwatch stopwatch;

    public StartupTimingScope(string systemName)
    {
        this.systemName = systemName;
        stopwatch = Stopwatch.StartNew();
        StartupPerformanceProfiler.Log($"{systemName} start");
    }

    public void Dispose()
    {
        stopwatch.Stop();
        StartupPerformanceProfiler.Log($"{systemName} end ({stopwatch.Elapsed.TotalMilliseconds:0.0} ms)");
    }
}
