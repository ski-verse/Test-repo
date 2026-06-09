using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class StrokeMetricsDisplay : MonoBehaviour
{
    public static readonly Vector2 StrokeRateTextPosition = new Vector2(28f, -278f);
    public static readonly Vector2 TotalStrokesTextPosition = new Vector2(28f, -328f);
    public const float MinimumSimulatedStrokeRateSpm = 24f;
    public const float MaximumSimulatedStrokeRateSpm = 54f;
    public const float SimulatedStrokeRateBaseSpm = 28f;
    public const float SimulatedStrokeRatePerKmh = 0.42f;

    public PlayerSpeedController player;
    public MonoBehaviour strokeMetricsSourceBehaviour;
    public TMP_Text strokeRateText;
    public TMP_Text totalStrokesText;

    private float accumulatedStrokeCount;

    public float CurrentStrokeRateSpm { get; private set; }

    public int TotalStrokes => Mathf.FloorToInt(accumulatedStrokeCount);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeStrokeHudUpdater()
    {
        if (Object.FindFirstObjectByType<StrokeMetricsRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Stroke Metrics HUD Runtime Updater");
        updater.AddComponent<StrokeMetricsRuntimeUpdater>();
    }

    private void Update()
    {
        Refresh(Time.deltaTime);
    }

    public void Refresh(float deltaTime)
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerSpeedController>();
        }

        if (player != null)
        {
            if (TryReadExternalStrokeMetrics(out var externalStrokeRateSpm, out var externalTotalStrokes))
            {
                CurrentStrokeRateSpm = externalStrokeRateSpm;
                accumulatedStrokeCount = externalTotalStrokes;
            }
            else
            {
                CurrentStrokeRateSpm = CalculateSimulatedStrokeRateSpm(player.LastMovementInput, player.SpeedKmh);
                AdvanceSimulatedStrokeSession(deltaTime, CurrentStrokeRateSpm);
            }
        }

        RefreshText();
    }

    public void ResetStrokeSession()
    {
        accumulatedStrokeCount = 0f;
        CurrentStrokeRateSpm = 0f;
        RefreshText();
    }

    public void AdvanceSimulatedStrokeSession(float deltaTime, float strokeRateSpm)
    {
        var safeDeltaTime = Mathf.Max(0f, deltaTime);
        var safeStrokeRate = Mathf.Max(0f, strokeRateSpm);
        accumulatedStrokeCount += safeStrokeRate / 60f * safeDeltaTime;
    }

    public static float CalculateSimulatedStrokeRateSpm(PlayerMovementInput movementInput, float speedKmh)
    {
        if (movementInput.SpeedAxis <= 0f && movementInput.PropulsionWatts <= PlayerSpeedController.MinimumPropulsionWatts)
        {
            return 0f;
        }

        var simulatedRate = SimulatedStrokeRateBaseSpm + Mathf.Max(0f, speedKmh) * SimulatedStrokeRatePerKmh;
        return Mathf.Clamp(simulatedRate, MinimumSimulatedStrokeRateSpm, MaximumSimulatedStrokeRateSpm);
    }

    public static StrokeMetricsDisplay EnsureRuntimeStrokeHud(PlayerSpeedController player)
    {
        var existing = Object.FindFirstObjectByType<StrokeMetricsDisplay>();
        if (existing != null)
        {
            if (existing.player == null)
            {
                existing.player = player;
            }

            existing.RefreshText();
            return existing;
        }

        var canvas = FindHudCanvas();
        if (canvas == null || player == null)
        {
            return null;
        }

        return CreateRuntimeStrokeHud(canvas.transform, player);
    }

    public static StrokeMetricsDisplay CreateRuntimeStrokeHud(Transform parent, PlayerSpeedController player)
    {
        var displayObject = new GameObject("Stroke Metrics Display");
        displayObject.transform.SetParent(parent, false);

        var display = displayObject.AddComponent<StrokeMetricsDisplay>();
        display.player = player;
        display.strokeRateText = CreateHudText(displayObject.transform, "Stroke Rate Text", StrokeRateTextPosition);
        display.totalStrokesText = CreateHudText(displayObject.transform, "Total Strokes Text", TotalStrokesTextPosition);
        display.RefreshText();
        return display;
    }

    private void RefreshText()
    {
        if (strokeRateText != null)
        {
            strokeRateText.text = $"Stroke Rate: {CurrentStrokeRateSpm:0} SPM";
        }

        if (totalStrokesText != null)
        {
            totalStrokesText.text = $"Total Strokes: {TotalStrokes}";
        }
    }

    private bool TryReadExternalStrokeMetrics(out float strokeRateSpm, out int totalStrokes)
    {
        if (strokeMetricsSourceBehaviour is IStrokeMetricsSource source && source.HasStrokeMetrics)
        {
            strokeRateSpm = Mathf.Max(0f, source.StrokeRateSpm);
            totalStrokes = Mathf.Max(0, source.TotalStrokes);
            return true;
        }

        strokeRateSpm = 0f;
        totalStrokes = 0;
        return false;
    }

    private static TextMeshProUGUI CreateHudText(Transform parent, string name, Vector2 anchoredPosition)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 36f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(560f, 48f);
        return text;
    }

    private static Canvas FindHudCanvas()
    {
        var raceHud = GameObject.Find("Race HUD");
        if (raceHud != null && raceHud.TryGetComponent<Canvas>(out var raceCanvas))
        {
            return raceCanvas;
        }

        return Object.FindFirstObjectByType<Canvas>();
    }
}

public interface IStrokeMetricsSource
{
    bool HasStrokeMetrics { get; }

    float StrokeRateSpm { get; }

    int TotalStrokes { get; }
}

public class StrokeMetricsRuntimeUpdater : MonoBehaviour
{
    private const int MaxInstallFrames = 120;
    private int installFrames;

    private void Update()
    {
        var player = Object.FindFirstObjectByType<PlayerSpeedController>();
        if (StrokeMetricsDisplay.EnsureRuntimeStrokeHud(player) != null || installFrames >= MaxInstallFrames)
        {
            Destroy(gameObject);
            return;
        }

        installFrames++;
    }
}
