using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SkiVerseStartScreenController : MonoBehaviour
{
    public enum CourseSelection
    {
        ThreeKmCircuit,
        FortyKmLongCourse,
        JamtlandSkiTour
    }

    public const string TitleText = "Ski-Verse";
    public const string ThreeKmCourseLabel = "3 km Circuit";
    public const string FortyKmCourseLabel = "40 km Long Course";
    public const string JamtlandTourLabel = "Jamtland Ski Tour";

    public GameObject startScreenPanel;
    public Button startSessionButton;
    public TMP_Text selectedCourseText;
    public Toggle threeKmToggle;
    public Toggle fortyKmToggle;
    public Toggle jamtlandToggle;

    public CourseSelection SelectedCourse { get; private set; } = CourseSelection.ThreeKmCircuit;
    public bool HasStartedSession { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallStartScreen()
    {
        if (Object.FindFirstObjectByType<SkiVerseStartScreenController>() != null)
        {
            return;
        }

        var startScreenObject = new GameObject("Ski-Verse Alpha Start Screen");
        startScreenObject.AddComponent<SkiVerseStartScreenController>();
    }

    private void Awake()
    {
        Time.timeScale = 0f;
    }

    private void Start()
    {
        CreateRuntimeUiIfNeeded();
        SetCourseSelection(CourseSelection.ThreeKmCircuit);
        PauseGameplayBeforeStart();
    }

    private void OnDestroy()
    {
        if (startSessionButton != null)
        {
            startSessionButton.onClick.RemoveListener(StartSelectedSession);
        }
    }

    public void SetCourseSelection(CourseSelection selection)
    {
        SelectedCourse = selection;

        if (selectedCourseText != null)
        {
            selectedCourseText.text = selection == CourseSelection.ThreeKmCircuit
                ? ThreeKmCourseLabel
                : $"{FormatCourseLabel(selection)} - Coming Soon";
        }
    }

    public void StartSelectedSession()
    {
        if (SelectedCourse != CourseSelection.ThreeKmCircuit)
        {
            SetCourseSelection(CourseSelection.ThreeKmCircuit);
        }

        HasStartedSession = true;
        Time.timeScale = 1f;

        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(false);
        }

        var session = Object.FindFirstObjectByType<WorkoutSessionController>();
        if (session != null)
        {
            session.ReturnToStartSession();
            return;
        }

        var player = Object.FindFirstObjectByType<PlayerSpeedController>();
        if (player != null)
        {
            player.enabled = true;
            player.CurrentSpeed = 4f;
            player.AlignToCourse(0f);
            player.SetStartDistanceZ(0f);
        }
    }

    private void PauseGameplayBeforeStart()
    {
        var player = Object.FindFirstObjectByType<PlayerSpeedController>();
        if (player != null)
        {
            player.CurrentSpeed = 0f;
            player.enabled = false;
        }
    }

    private void CreateRuntimeUiIfNeeded()
    {
        if (startScreenPanel != null && startSessionButton != null)
        {
            EnsureEventSystemExists();
            startSessionButton.onClick.RemoveListener(StartSelectedSession);
            startSessionButton.onClick.AddListener(StartSelectedSession);
            return;
        }

        var canvasObject = new GameObject("Ski-Verse Start Screen Canvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 3000;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystemExists();

        startScreenPanel = CreatePanel(canvasObject.transform);
        CreateTitle(startScreenPanel.transform);
        CreateSubtitle(startScreenPanel.transform);
        selectedCourseText = CreatePanelText(startScreenPanel.transform, "Selected Course Text", new Vector2(0f, 160f), new Vector2(680f, 42f), 26f, FontStyles.Bold);

        var toggleGroup = startScreenPanel.AddComponent<ToggleGroup>();
        threeKmToggle = CreateCourseToggle(startScreenPanel.transform, toggleGroup, ThreeKmCourseLabel, new Vector2(0f, 86f), true, () => SetCourseSelection(CourseSelection.ThreeKmCircuit));
        fortyKmToggle = CreateCourseToggle(startScreenPanel.transform, toggleGroup, $"{FortyKmCourseLabel} (Coming Soon)", new Vector2(0f, 16f), false, () => SetCourseSelection(CourseSelection.FortyKmLongCourse));
        jamtlandToggle = CreateCourseToggle(startScreenPanel.transform, toggleGroup, $"{JamtlandTourLabel} (Coming Later)", new Vector2(0f, -54f), false, () => SetCourseSelection(CourseSelection.JamtlandSkiTour));

        startSessionButton = CreateButton(startScreenPanel.transform, "Start Session Button", "Start Session", new Vector2(0f, -165f));
        startSessionButton.onClick.AddListener(StartSelectedSession);
    }

    private static GameObject CreatePanel(Transform parent)
    {
        var panel = new GameObject("Ski-Verse Alpha Menu Panel");
        panel.transform.SetParent(parent, false);

        var image = panel.AddComponent<Image>();
        image.color = new Color(0.025f, 0.035f, 0.045f, 0.92f);

        var rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(760f, 620f);

        return panel;
    }

    private static void CreateTitle(Transform parent)
    {
        var title = CreatePanelText(parent, "Ski-Verse Title", new Vector2(0f, 245f), new Vector2(680f, 80f), 64f, FontStyles.Bold);
        title.text = TitleText;
    }

    private static void CreateSubtitle(Transform parent)
    {
        var subtitle = CreatePanelText(parent, "Alpha Menu Subtitle", new Vector2(0f, 202f), new Vector2(680f, 36f), 24f, FontStyles.Normal);
        subtitle.text = "Alpha";
        subtitle.color = new Color(0.72f, 0.86f, 1f);
    }

    private static Toggle CreateCourseToggle(Transform parent, ToggleGroup group, string label, Vector2 anchoredPosition, bool isOn, UnityEngine.Events.UnityAction onSelected)
    {
        var toggleObject = new GameObject(label);
        toggleObject.transform.SetParent(parent, false);

        var background = toggleObject.AddComponent<Image>();
        background.color = isOn ? new Color(0.16f, 0.34f, 0.52f, 0.95f) : new Color(0.08f, 0.1f, 0.12f, 0.95f);

        var toggle = toggleObject.AddComponent<Toggle>();
        toggle.group = group;
        toggle.isOn = isOn;
        toggle.transition = Selectable.Transition.ColorTint;
        toggle.targetGraphic = background;
        toggle.onValueChanged.AddListener(isSelected =>
        {
            background.color = isSelected ? new Color(0.16f, 0.34f, 0.52f, 0.95f) : new Color(0.08f, 0.1f, 0.12f, 0.95f);
            if (isSelected)
            {
                onSelected.Invoke();
            }
        });

        var rectTransform = toggleObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(560f, 54f);

        var text = CreatePanelText(toggleObject.transform, "Label", Vector2.zero, new Vector2(500f, 42f), 28f, FontStyles.Bold);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;

        return toggle;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        var buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.62f, 0.36f, 0.98f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        var rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(360f, 68f);

        var text = CreatePanelText(buttonObject.transform, "Label", Vector2.zero, new Vector2(320f, 48f), 32f, FontStyles.Bold);
        text.text = label;

        return button;
    }

    private static TextMeshProUGUI CreatePanelText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
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

    private static string FormatCourseLabel(CourseSelection selection)
    {
        switch (selection)
        {
            case CourseSelection.FortyKmLongCourse:
                return FortyKmCourseLabel;
            case CourseSelection.JamtlandSkiTour:
                return JamtlandTourLabel;
            default:
                return ThreeKmCourseLabel;
        }
    }

    private static void EnsureEventSystemExists()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject("Event System");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
