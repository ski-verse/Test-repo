using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class WorkoutSessionController : MonoBehaviour
{
    public const float DefaultFinishDistanceKm = 5f;

    public PlayerSpeedController player;
    public TMP_Text elapsedTimeText;
    public GameObject finishSummaryPanel;
    public TMP_Text finishSummaryText;
    public Button restartButton;
    public float finishDistanceKm = DefaultFinishDistanceKm;

    public float ElapsedTimeSeconds { get; private set; }
    public float MaxSpeedKmh { get; private set; }
    public bool IsFinished { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWorkoutSessionFlow()
    {
        if (Object.FindObjectOfType<WorkoutSessionController>() != null)
        {
            return;
        }

        var sessionObject = new GameObject("Workout Session Flow");
        sessionObject.AddComponent<WorkoutSessionController>();
    }

    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartSession);
        }
    }

    private void Start()
    {
        CreateRuntimeUiIfNeeded();
        StartSession();
    }

    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartSession);
        }
    }

    private void Update()
    {
        if (player == null)
        {
            player = Object.FindObjectOfType<PlayerSpeedController>();
        }

        if (player == null || IsFinished)
        {
            return;
        }

        AdvanceSession(Time.deltaTime, player.DistanceKm, player.SpeedKmh);
    }

    public void StartSession()
    {
        ElapsedTimeSeconds = 0f;
        MaxSpeedKmh = 0f;
        IsFinished = false;

        if (finishSummaryPanel != null)
        {
            finishSummaryPanel.SetActive(false);
        }

        if (player != null)
        {
            player.enabled = true;
        }

        RefreshElapsedTimeText();
    }

    public void AdvanceSession(float deltaTime, float distanceKm, float speedKmh)
    {
        if (IsFinished)
        {
            return;
        }

        ElapsedTimeSeconds += Mathf.Max(0f, deltaTime);
        MaxSpeedKmh = Mathf.Max(MaxSpeedKmh, speedKmh);
        RefreshElapsedTimeText();

        if (distanceKm >= finishDistanceKm)
        {
            FinishSession(distanceKm);
        }
    }

    public void FinishSession(float distanceKm)
    {
        IsFinished = true;
        var summaryDistanceKm = Mathf.Max(distanceKm, finishDistanceKm);
        var averageSpeedKmh = CalculateAverageSpeedKmh(summaryDistanceKm, ElapsedTimeSeconds);

        if (player != null)
        {
            player.CurrentSpeed = 0f;
            player.enabled = false;
        }

        if (finishSummaryText != null)
        {
            finishSummaryText.text = BuildFinishSummary(ElapsedTimeSeconds, summaryDistanceKm, averageSpeedKmh, MaxSpeedKmh);
        }

        if (finishSummaryPanel != null)
        {
            finishSummaryPanel.SetActive(true);
        }
    }

    public void RestartSession()
    {
        Time.timeScale = 1f;

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            return;
        }

        if (activeScene.IsValid() && !string.IsNullOrEmpty(activeScene.name))
        {
            SceneManager.LoadScene(activeScene.name);
            return;
        }

        ResetPlayerToStart();
        StartSession();
    }

    public static float CalculateAverageSpeedKmh(float distanceKm, float elapsedTimeSeconds)
    {
        if (elapsedTimeSeconds <= 0f)
        {
            return 0f;
        }

        return distanceKm / (elapsedTimeSeconds / 3600f);
    }

    public static string FormatElapsedTime(float elapsedTimeSeconds)
    {
        var totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, elapsedTimeSeconds));
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds / 60 % 60;
        var seconds = totalSeconds % 60;

        if (hours > 0)
        {
            return $"{hours}:{minutes:00}:{seconds:00}";
        }

        return $"{minutes:00}:{seconds:00}";
    }

    public static string BuildFinishSummary(float elapsedTimeSeconds, float distanceKm, float averageSpeedKmh, float maxSpeedKmh)
    {
        return "Finish\n" +
               $"Time: {FormatElapsedTime(elapsedTimeSeconds)}\n" +
               $"Distance: {distanceKm:0.00} km\n" +
               $"Average speed: {averageSpeedKmh:0.0} km/h\n" +
               $"Max speed: {maxSpeedKmh:0.0} km/h";
    }

    private void RefreshElapsedTimeText()
    {
        if (elapsedTimeText != null)
        {
            elapsedTimeText.text = $"Time: {FormatElapsedTime(ElapsedTimeSeconds)}";
        }
    }

    private void ResetPlayerToStart()
    {
        if (player == null)
        {
            player = Object.FindObjectOfType<PlayerSpeedController>();
        }

        if (player == null)
        {
            return;
        }

        player.enabled = true;
        player.CurrentSpeed = 4f;
        player.AlignToCourse(0f);
        player.SetStartDistanceZ(player.transform.position.z);
    }

    private void CreateRuntimeUiIfNeeded()
    {
        if (elapsedTimeText != null && finishSummaryPanel != null && finishSummaryText != null && restartButton != null)
        {
            return;
        }

        var canvasObject = new GameObject("Workout HUD");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        elapsedTimeText = CreateHudText(canvasObject.transform, "Elapsed Time Text", new Vector2(28f, -128f));
        finishSummaryPanel = CreateFinishSummaryPanel(canvasObject.transform, out var summaryText, out var button);
        finishSummaryText = summaryText;
        restartButton = button;
        restartButton.onClick.AddListener(RestartSession);
        finishSummaryPanel.SetActive(false);
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
        rectTransform.sizeDelta = new Vector2(520f, 48f);

        return text;
    }

    private static GameObject CreateFinishSummaryPanel(Transform parent, out TextMeshProUGUI summaryText, out Button restartButton)
    {
        var panel = new GameObject("Finish Summary Panel");
        panel.transform.SetParent(parent, false);

        var image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.78f);

        var rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(560f, 420f);

        summaryText = CreatePanelText(panel.transform, "Finish Summary Text", new Vector2(0f, 70f), new Vector2(480f, 230f), 34f);
        restartButton = CreateRestartButton(panel.transform);
        return panel;
    }

    private static TextMeshProUGUI CreatePanelText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float fontSize)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        return text;
    }

    private static Button CreateRestartButton(Transform parent)
    {
        var buttonObject = new GameObject("Restart Button");
        buttonObject.transform.SetParent(parent, false);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.45f, 0.95f, 0.95f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        var rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -145f);
        rectTransform.sizeDelta = new Vector2(240f, 64f);

        var label = CreatePanelText(buttonObject.transform, "Restart Button Text", Vector2.zero, new Vector2(220f, 48f), 28f);
        label.text = "Restart";

        return button;
    }
}
