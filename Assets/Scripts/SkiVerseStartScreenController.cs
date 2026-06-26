using TMPro;
using System.Collections.Generic;
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
    public const string JamtlandTourLabel = "Jämtland Ski Tour";
    public const string Pm5NotConnectedText = Pm5BleRuntimeConnector.NotConnectedStatusText;
    public const string Pm5SearchingText = Pm5BleRuntimeConnector.SearchingStatusText;
    public const string Pm5FoundText = Pm5BleRuntimeConnector.Pm5FoundStatusText;
    public const string Pm5ConnectingText = Pm5BleRuntimeConnector.ConnectingStatusText;
    public const string Pm5ConnectedText = Pm5BleRuntimeConnector.ConnectedStatusText;
    public const string Pm5ConnectionFailedText = Pm5BleRuntimeConnector.ConnectionFailedStatusText;

    public GameObject startScreenPanel;
    public Button startSessionButton;
    public Button connectPm5Button;
    public TMP_Text selectedCourseText;
    public TMP_Text pm5StatusText;
    public TMP_Text pm5DeviceListText;
    public RectTransform pm5DeviceButtonContainer;
    public Pm5BleRuntimeConnector pm5Connector;
    public Toggle threeKmToggle;
    public Toggle fortyKmToggle;
    public Toggle jamtlandToggle;

    public CourseSelection SelectedCourse { get; private set; } = CourseSelection.ThreeKmCircuit;
    public bool HasStartedSession { get; private set; }
    public bool IsPm5Connected { get; private set; }

    private readonly List<Button> pm5DeviceButtons = new List<Button>();
    private int renderedPm5DeviceCount = -1;
    private int renderedSelectedPm5DeviceIndex = -2;
    private Pm5BleConnectionStatus renderedPm5Status = (Pm5BleConnectionStatus)(-1);
    private bool pm5UiDirty;

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
        EnsurePm5Connector();
    }

    private void Start()
    {
        CreateRuntimeUiIfNeeded();
        SetCourseSelection(CourseSelection.ThreeKmCircuit);
        PauseGameplayBeforeStart();
    }

    private void Update()
    {
        if (startScreenPanel != null && startScreenPanel.activeSelf)
        {
            RefreshPm5Ui(pm5UiDirty);
            pm5UiDirty = false;
        }
    }

    private void OnDestroy()
    {
        if (startSessionButton != null)
        {
            startSessionButton.onClick.RemoveListener(StartSelectedSession);
        }

        if (connectPm5Button != null)
        {
            connectPm5Button.onClick.RemoveListener(ConnectPm5);
        }

        if (pm5Connector != null)
        {
            pm5Connector.StateChanged -= MarkPm5UiDirty;
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

    public void ConnectPm5()
    {
        EnsurePm5Connector();
        Debug.Log($"[Ski-Verse PM5 BLE] Connect PM5 button pressed. CurrentStatus={pm5Connector.Status}, SelectedDeviceIndex={pm5Connector.SelectedDeviceIndex}, DiscoveredDevices={pm5Connector.DiscoveredDevices.Count}.");

        if (pm5Connector.SelectedDevice.HasValue)
        {
            pm5Connector.ConnectSelectedDevice();
        }
        else
        {
            pm5Connector.StartScan();
        }

        RefreshPm5Ui(true);
    }

    public void SelectPm5Device(int deviceIndex)
    {
        EnsurePm5Connector();
        Debug.Log($"[Ski-Verse PM5 BLE] PM5 device button pressed. DeviceIndex={deviceIndex}.");
        pm5Connector.SelectDevice(deviceIndex);
        RefreshPm5Ui(true);
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
            if (connectPm5Button != null)
            {
                connectPm5Button.onClick.RemoveListener(ConnectPm5);
                connectPm5Button.onClick.AddListener(ConnectPm5);
            }

            RefreshPm5Ui(true);
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
        selectedCourseText = CreatePanelText(startScreenPanel.transform, "Selected Course Text", new Vector2(0f, 195f), new Vector2(760f, 44f), 28f, FontStyles.Bold);

        var toggleGroup = startScreenPanel.AddComponent<ToggleGroup>();
        threeKmToggle = CreateCourseToggle(startScreenPanel.transform, toggleGroup, ThreeKmCourseLabel, new Vector2(0f, 120f), true, () => SetCourseSelection(CourseSelection.ThreeKmCircuit));
        fortyKmToggle = CreateCourseToggle(startScreenPanel.transform, toggleGroup, $"{FortyKmCourseLabel} (Coming Soon)", new Vector2(0f, 48f), false, () => SetCourseSelection(CourseSelection.FortyKmLongCourse));
        jamtlandToggle = CreateCourseToggle(startScreenPanel.transform, toggleGroup, $"{JamtlandTourLabel} (Coming Later)", new Vector2(0f, -24f), false, () => SetCourseSelection(CourseSelection.JamtlandSkiTour));

        CreatePm5Section(startScreenPanel.transform);

        startSessionButton = CreateButton(startScreenPanel.transform, "Start Session Button", "Start Session", new Vector2(0f, -320f), new Vector2(430f, 78f), 34f);
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
        rectTransform.sizeDelta = new Vector2(1040f, 780f);

        return panel;
    }

    private static void CreateTitle(Transform parent)
    {
        var title = CreatePanelText(parent, "Ski-Verse Title", new Vector2(0f, 318f), new Vector2(900f, 86f), 78f, FontStyles.Bold);
        title.text = TitleText;
    }

    private static void CreateSubtitle(Transform parent)
    {
        var subtitle = CreatePanelText(parent, "Alpha Menu Subtitle", new Vector2(0f, 250f), new Vector2(820f, 34f), 26f, FontStyles.Normal);
        subtitle.text = "Alpha Training";
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
        rectTransform.sizeDelta = new Vector2(680f, 62f);

        var text = CreatePanelText(toggleObject.transform, "Label", Vector2.zero, new Vector2(620f, 46f), 30f, FontStyles.Bold);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;

        return toggle;
    }

    private void CreatePm5Section(Transform parent)
    {
        var section = new GameObject("PM5 Connection Panel");
        section.transform.SetParent(parent, false);

        var image = section.AddComponent<Image>();
        image.color = new Color(0.055f, 0.075f, 0.085f, 0.96f);

        var rectTransform = section.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -160f);
        rectTransform.sizeDelta = new Vector2(680f, 240f);

        var label = CreatePanelText(section.transform, "PM5 Section Title", new Vector2(0f, 88f), new Vector2(600f, 34f), 26f, FontStyles.Bold);
        label.text = "Concept2 PM5";

        pm5StatusText = CreatePanelText(section.transform, "PM5 Status Text", new Vector2(0f, 50f), new Vector2(600f, 32f), 24f, FontStyles.Normal);

        pm5DeviceListText = CreatePanelText(section.transform, "PM5 Device List Text", new Vector2(0f, 16f), new Vector2(600f, 28f), 20f, FontStyles.Normal);
        pm5DeviceListText.text = "No PM5 devices found";

        var deviceListObject = new GameObject("PM5 Device Button Container");
        deviceListObject.transform.SetParent(section.transform, false);
        pm5DeviceButtonContainer = deviceListObject.AddComponent<RectTransform>();
        pm5DeviceButtonContainer.anchorMin = new Vector2(0.5f, 0.5f);
        pm5DeviceButtonContainer.anchorMax = new Vector2(0.5f, 0.5f);
        pm5DeviceButtonContainer.pivot = new Vector2(0.5f, 0.5f);
        pm5DeviceButtonContainer.anchoredPosition = new Vector2(0f, -26f);
        pm5DeviceButtonContainer.sizeDelta = new Vector2(600f, 58f);

        connectPm5Button = CreateButton(section.transform, "Connect PM5 Button", "Connect PM5", new Vector2(0f, -88f), new Vector2(340f, 54f), 26f);
        connectPm5Button.onClick.AddListener(ConnectPm5);
        RefreshPm5Ui(true);
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2? size = null, float fontSize = 32f)
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
        var buttonSize = size ?? new Vector2(360f, 68f);
        rectTransform.sizeDelta = buttonSize;

        var text = CreatePanelText(buttonObject.transform, "Label", Vector2.zero, new Vector2(buttonSize.x - 40f, buttonSize.y - 18f), fontSize, FontStyles.Bold);
        text.text = label;

        return button;
    }

    private void EnsurePm5Connector()
    {
        if (pm5Connector == null)
        {
            pm5Connector = GetComponent<Pm5BleRuntimeConnector>();
        }

        if (pm5Connector == null)
        {
            pm5Connector = gameObject.AddComponent<Pm5BleRuntimeConnector>();
        }

        pm5Connector.StateChanged -= MarkPm5UiDirty;
        pm5Connector.StateChanged += MarkPm5UiDirty;
    }

    private void MarkPm5UiDirty()
    {
        pm5UiDirty = true;
    }

    private void RefreshPm5Ui()
    {
        RefreshPm5Ui(false);
    }

    private void RefreshPm5Ui(bool force)
    {
        EnsurePm5Connector();
        IsPm5Connected = pm5Connector.Status == Pm5BleConnectionStatus.Connected;

        if (pm5StatusText != null)
        {
            pm5StatusText.text = pm5Connector.GetStatusText();
        }

        RebuildPm5DeviceListIfNeeded(force);
    }

    private void RebuildPm5DeviceListIfNeeded(bool force)
    {
        if (pm5DeviceButtonContainer == null || pm5Connector == null)
        {
            return;
        }

        var devices = pm5Connector.DiscoveredDevices;
        if (!force && devices.Count == renderedPm5DeviceCount && pm5Connector.SelectedDeviceIndex == renderedSelectedPm5DeviceIndex && pm5Connector.Status == renderedPm5Status)
        {
            return;
        }

        renderedPm5DeviceCount = devices.Count;
        renderedSelectedPm5DeviceIndex = pm5Connector.SelectedDeviceIndex;
        renderedPm5Status = pm5Connector.Status;

        for (var i = pm5DeviceButtonContainer.childCount - 1; i >= 0; i--)
        {
            DestroyUiObject(pm5DeviceButtonContainer.GetChild(i).gameObject);
        }

        pm5DeviceButtons.Clear();

        if (pm5DeviceListText != null)
        {
            pm5DeviceListText.text = devices.Count == 0
                ? (pm5Connector.Status == Pm5BleConnectionStatus.Searching ? "Scanning for PM5..." : "No PM5 devices found")
                : "Select PM5";
        }

        var buttonCount = Mathf.Min(3, devices.Count);
        for (var i = 0; i < buttonCount; i++)
        {
            var capturedIndex = i;
            var label = pm5Connector.SelectedDeviceIndex == i ? $"> {devices[i].Name}" : devices[i].Name;
            Debug.Log($"[Ski-Verse PM5 BLE] Rendering PM5 device button. Index={i}, Label='{label}', DeviceId='{devices[i].DeviceId}', Address='{devices[i].BluetoothAddress}'.");
            var button = CreateButton(pm5DeviceButtonContainer, $"PM5 Device {i + 1}", label, new Vector2(0f, 18f - i * 34f), new Vector2(560f, 30f), 18f);
            button.onClick.AddListener(() => SelectPm5Device(capturedIndex));
            pm5DeviceButtons.Add(button);
        }
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

    private static void DestroyUiObject(GameObject uiObject)
    {
        if (Application.isPlaying)
        {
            Destroy(uiObject);
        }
        else
        {
            DestroyImmediate(uiObject);
        }
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
