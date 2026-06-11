using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class WorkoutMetricsHudDisplay : MonoBehaviour
{
    private static readonly string[] LegacyHudTextNames =
    {
        "Speed Text",
        "Distance Text",
        "Elapsed Time Text",
        "Lap Text",
        "Gradient Text",
        "Stroke Rate Text",
        "Total Strokes Text"
    };

    public PlayerSpeedController player;
    public WorkoutSessionController session;
    public StrokeMetricsDisplay strokeMetricsDisplay;
    public MonoBehaviour workoutMetricsSourceBehaviour;

    public GameObject panelRoot;
    public TMP_Text speedValueText;
    public TMP_Text speedLabelText;
    public TMP_Text wattsValueText;
    public TMP_Text wattsLabelText;
    public TMP_Text heartRateValueText;
    public TMP_Text heartRateLabelText;
    public TMP_Text strokeRateValueText;
    public TMP_Text strokeRateLabelText;
    public TMP_Text timeValueText;
    public TMP_Text timeLabelText;
    public TMP_Text distanceValueText;
    public TMP_Text distanceLabelText;
    public TMP_Text gradientValueText;
    public TMP_Text gradientLabelText;
    public TMP_Text lapValueText;
    public TMP_Text lapLabelText;
    public TMP_Text totalStrokesValueText;
    public TMP_Text totalStrokesLabelText;

    public int CurrentWatts { get; private set; }
    public int CurrentHeartRateBpm { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeMetricsHudUpdater()
    {
        if (Object.FindFirstObjectByType<WorkoutMetricsHudRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Workout Metrics HUD Runtime Updater");
        updater.AddComponent<WorkoutMetricsHudRuntimeUpdater>();
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        CacheSourcesIfNeeded();
        HideLegacyHudTexts();

        var speedKmh = player != null ? player.SpeedKmh : 0f;
        var distanceKm = player != null ? player.DistanceKm : 0f;
        var gradientPercent = player != null ? player.CurrentGradientPercent : 0f;
        var lap = session != null ? session.CurrentLapNumber : player != null ? player.CurrentLapNumber : 1;
        var elapsedTime = session != null ? session.ElapsedTimeSeconds : 0f;
        var movementInput = player != null ? player.LastMovementInput : PlayerMovementInput.None;

        if (TryReadExternalWorkoutMetrics(out var watts, out var heartRateBpm))
        {
            CurrentWatts = watts;
            CurrentHeartRateBpm = heartRateBpm;
        }
        else
        {
            CurrentWatts = CalculateDisplayWatts(movementInput, speedKmh);
            CurrentHeartRateBpm = CalculateDisplayHeartRateBpm(speedKmh);
        }

        var strokeRate = strokeMetricsDisplay != null ? strokeMetricsDisplay.CurrentStrokeRateSpm : 0f;
        var totalStrokes = strokeMetricsDisplay != null ? strokeMetricsDisplay.TotalStrokes : 0;

        SetText(speedValueText, $"{speedKmh:0.0}");
        SetText(wattsValueText, $"{CurrentWatts:0}");
        SetText(heartRateValueText, $"{CurrentHeartRateBpm:0}");
        SetText(strokeRateValueText, $"{strokeRate:0}");
        SetText(timeValueText, WorkoutSessionController.FormatElapsedTime(elapsedTime));
        SetText(distanceValueText, $"{distanceKm:0.00} km");
        SetText(gradientValueText, $"{gradientPercent:0.0}%");
        SetText(lapValueText, $"{lap}");
        SetText(totalStrokesValueText, $"{totalStrokes}");
    }

    public static int CalculateDisplayWatts(PlayerMovementInput movementInput, float speedKmh)
    {
        if (movementInput.PropulsionWatts > PlayerSpeedController.MinimumPropulsionWatts)
        {
            return Mathf.RoundToInt(movementInput.PropulsionWatts);
        }

        if (movementInput.SpeedAxis <= 0f)
        {
            return 0;
        }

        return Mathf.RoundToInt(Mathf.Clamp(95f + Mathf.Max(0f, speedKmh) * 6.5f, 80f, 420f));
    }

    public static int CalculateDisplayHeartRateBpm(float speedKmh)
    {
        if (speedKmh <= 0.1f)
        {
            return 0;
        }

        return Mathf.RoundToInt(Mathf.Clamp(112f + speedKmh * 1.35f, 95f, 188f));
    }

    public static WorkoutMetricsHudDisplay EnsureRuntimeHud(PlayerSpeedController player)
    {
        var existing = Object.FindFirstObjectByType<WorkoutMetricsHudDisplay>();
        if (existing != null)
        {
            if (existing.player == null)
            {
                existing.player = player;
            }

            existing.Refresh();
            return existing;
        }

        var canvas = FindHudCanvas();
        if (canvas == null || player == null)
        {
            return null;
        }

        return CreateRuntimeHud(canvas.transform, player);
    }

    public static WorkoutMetricsHudDisplay CreateRuntimeHud(Transform parent, PlayerSpeedController player)
    {
        var displayObject = new GameObject("Workout Metrics HUD Display");
        displayObject.transform.SetParent(parent, false);

        var display = displayObject.AddComponent<WorkoutMetricsHudDisplay>();
        display.player = player;
        display.session = Object.FindFirstObjectByType<WorkoutSessionController>();
        display.strokeMetricsDisplay = Object.FindFirstObjectByType<StrokeMetricsDisplay>();
        display.panelRoot = CreatePanel(displayObject.transform);
        display.CreateMetricGrid(display.panelRoot.transform);
        HideLegacyHudTexts();
        display.Refresh();
        return display;
    }

    private void CreateMetricGrid(Transform parent)
    {
        CreateMetricTile(parent, "Speed Metric", new Vector2(16f, -16f), "km/h", out speedValueText, out speedLabelText, true);
        CreateMetricTile(parent, "Watts Metric", new Vector2(206f, -16f), "WATTS", out wattsValueText, out wattsLabelText, false);
        CreateMetricTile(parent, "Heart Rate Metric", new Vector2(396f, -16f), "BPM", out heartRateValueText, out heartRateLabelText, false);
        CreateMetricTile(parent, "Stroke Rate Metric", new Vector2(586f, -16f), "SPM", out strokeRateValueText, out strokeRateLabelText, false);

        CreateMetricTile(parent, "Time Metric", new Vector2(16f, -136f), "TIME", out timeValueText, out timeLabelText, false);
        CreateMetricTile(parent, "Distance Metric", new Vector2(206f, -136f), "DISTANCE", out distanceValueText, out distanceLabelText, false);
        CreateMetricTile(parent, "Gradient Metric", new Vector2(396f, -136f), "GRADIENT", out gradientValueText, out gradientLabelText, false);
        CreateMetricTile(parent, "Lap Metric", new Vector2(586f, -136f), "LAP", out lapValueText, out lapLabelText, false);

        var totalStrokesPanel = new GameObject("Total Strokes Metric");
        totalStrokesPanel.transform.SetParent(parent, false);
        var background = totalStrokesPanel.AddComponent<Image>();
        background.color = new Color(0.07f, 0.09f, 0.1f, 0.88f);
        var rect = totalStrokesPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, -256f);
        rect.sizeDelta = new Vector2(760f, 50f);

        totalStrokesLabelText = CreateText(totalStrokesPanel.transform, "Total Strokes Label", new Vector2(18f, -9f), new Vector2(220f, 28f), 18f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.74f, 0.86f, 0.92f, 1f));
        totalStrokesLabelText.text = "TOTAL STROKES";
        totalStrokesValueText = CreateText(totalStrokesPanel.transform, "Total Strokes Value", new Vector2(250f, -5f), new Vector2(480f, 36f), 30f, FontStyles.Bold, TextAlignmentOptions.Right, Color.white);
    }

    private static GameObject CreatePanel(Transform parent)
    {
        var panel = new GameObject("Workout Metrics Panel");
        panel.transform.SetParent(parent, false);

        var image = panel.AddComponent<Image>();
        image.color = new Color(0.015f, 0.02f, 0.025f, 0.76f);

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(800f, 324f);
        return panel;
    }

    private static void CreateMetricTile(Transform parent, string name, Vector2 anchoredPosition, string label, out TMP_Text valueText, out TMP_Text labelText, bool accent)
    {
        var tile = new GameObject(name);
        tile.transform.SetParent(parent, false);

        var image = tile.AddComponent<Image>();
        image.color = accent ? new Color(0.02f, 0.22f, 0.18f, 0.92f) : new Color(0.055f, 0.07f, 0.08f, 0.9f);

        var rect = tile.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(174f, 104f);

        valueText = CreateText(tile.transform, "Value", new Vector2(12f, -9f), new Vector2(150f, 56f), 36f, FontStyles.Bold, TextAlignmentOptions.Left, accent ? new Color(0.52f, 1f, 0.78f, 1f) : Color.white);
        labelText = CreateText(tile.transform, "Label", new Vector2(12f, -64f), new Vector2(150f, 28f), 18f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.74f, 0.86f, 0.92f, 1f));
        labelText.text = label;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return text;
    }

    private void CacheSourcesIfNeeded()
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerSpeedController>();
        }

        if (session == null)
        {
            session = Object.FindFirstObjectByType<WorkoutSessionController>();
        }

        if (strokeMetricsDisplay == null)
        {
            strokeMetricsDisplay = Object.FindFirstObjectByType<StrokeMetricsDisplay>();
        }
    }

    private bool TryReadExternalWorkoutMetrics(out int watts, out int heartRateBpm)
    {
        if (workoutMetricsSourceBehaviour is IWorkoutMetricsSource source && source.HasWorkoutMetrics)
        {
            watts = Mathf.Max(0, Mathf.RoundToInt(source.Watts));
            heartRateBpm = Mathf.Max(0, Mathf.RoundToInt(source.HeartRateBpm));
            return true;
        }

        watts = 0;
        heartRateBpm = 0;
        return false;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void HideLegacyHudTexts()
    {
        for (var index = 0; index < LegacyHudTextNames.Length; index++)
        {
            var legacy = GameObject.Find(LegacyHudTextNames[index]);
            if (legacy != null)
            {
                legacy.SetActive(false);
            }
        }
    }

    private static Canvas FindHudCanvas()
    {
        var raceHud = GameObject.Find("Race HUD");
        if (raceHud != null && raceHud.TryGetComponent<Canvas>(out var raceCanvas))
        {
            return raceCanvas;
        }

        return null;
    }
}

public interface IWorkoutMetricsSource
{
    bool HasWorkoutMetrics { get; }

    float Watts { get; }

    float HeartRateBpm { get; }
}

public class WorkoutMetricsHudRuntimeUpdater : MonoBehaviour
{
    private const int MaxInstallFrames = 120;
    private int installFrames;

    private void Update()
    {
        var player = Object.FindFirstObjectByType<PlayerSpeedController>();
        if (WorkoutMetricsHudDisplay.EnsureRuntimeHud(player) != null || installFrames >= MaxInstallFrames)
        {
            Destroy(gameObject);
            return;
        }

        installFrames++;
    }
}
