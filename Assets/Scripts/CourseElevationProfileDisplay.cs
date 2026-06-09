using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CourseElevationProfileDisplay : MonoBehaviour
{
    public const int ProfileSampleCount = 96;
    public static readonly Vector2 DefaultProfileSize = new Vector2(240f, 132f);
    public static readonly Vector2 DefaultProfilePosition = new Vector2(-28f, -286f);
    public const float ProfilePadding = 16f;

    public PlayerSpeedController player;
    public RectTransform playerMarker;
    public RectTransform profileShapeRoot;
    public TextMeshProUGUI gradientText;
    public Vector2 profileSize = DefaultProfileSize;

    private Vector2 elevationBounds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeProfileUpdater()
    {
        if (Object.FindFirstObjectByType<CourseElevationProfileRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Elevation Profile HUD Runtime Updater");
        updater.AddComponent<CourseElevationProfileRuntimeUpdater>();
    }

    private void Awake()
    {
        elevationBounds = CalculateElevationBounds(ProfileSampleCount);
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (player == null || playerMarker == null)
        {
            return;
        }

        playerMarker.anchoredPosition = CalculateProfilePosition(player.CurrentLapProgressMeters, profileSize, elevationBounds);

        if (gradientText != null)
        {
            gradientText.text = $"Gradient: {player.CurrentGradientPercent:0.0}%";
        }
    }

    public static CourseElevationProfileDisplay EnsureRuntimeProfile(PlayerSpeedController player)
    {
        var existing = Object.FindFirstObjectByType<CourseElevationProfileDisplay>();
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

        return CreateRuntimeProfile(canvas.transform, player);
    }

    public static CourseElevationProfileDisplay CreateRuntimeProfile(Transform parent, PlayerSpeedController player)
    {
        var panelObject = new GameObject("Course Elevation Profile", typeof(RectTransform));
        panelObject.transform.SetParent(parent, false);

        var panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.58f);
        panelImage.raycastTarget = false;

        var panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = DefaultProfilePosition;
        panelRect.sizeDelta = DefaultProfileSize;

        var display = panelObject.AddComponent<CourseElevationProfileDisplay>();
        display.player = player;
        display.profileSize = DefaultProfileSize;
        display.elevationBounds = CalculateElevationBounds(ProfileSampleCount);

        var shapeObject = new GameObject("Elevation Shape", typeof(RectTransform));
        var shapeRoot = shapeObject.GetComponent<RectTransform>();
        shapeRoot.SetParent(panelRect, false);
        shapeRoot.anchorMin = new Vector2(0.5f, 0.5f);
        shapeRoot.anchorMax = new Vector2(0.5f, 0.5f);
        shapeRoot.pivot = new Vector2(0.5f, 0.5f);
        shapeRoot.anchoredPosition = Vector2.zero;
        shapeRoot.sizeDelta = DefaultProfileSize;
        display.profileShapeRoot = shapeRoot;

        CreateProfileLine(shapeRoot, display.elevationBounds, DefaultProfileSize);
        display.playerMarker = CreateMarker(panelRect);
        display.gradientText = CreateGradientText(panelRect);
        display.Refresh();
        return display;
    }

    public static Vector2 CalculateElevationBounds(int sampleCount)
    {
        var safeSampleCount = Mathf.Max(8, sampleCount);
        var minHeight = CoursePath.HeightAtDistance(0f);
        var maxHeight = minHeight;

        for (var i = 1; i <= safeSampleCount; i++)
        {
            var distance = CoursePath.CourseLengthMeters * (i / (float)safeSampleCount);
            var height = CoursePath.HeightAtDistance(distance);
            minHeight = Mathf.Min(minHeight, height);
            maxHeight = Mathf.Max(maxHeight, height);
        }

        if (maxHeight - minHeight < 0.001f)
        {
            maxHeight = minHeight + 1f;
        }

        return new Vector2(minHeight, maxHeight);
    }

    public static Vector2 CalculateProfilePosition(float distanceMeters, Vector2 profileSize, Vector2 elevationBounds)
    {
        var normalizedDistance = CoursePath.Progress01AtDistance(distanceMeters);
        var height = CoursePath.HeightAtDistance(distanceMeters);
        var usableWidth = Mathf.Max(1f, profileSize.x - ProfilePadding * 2f);
        var usableHeight = Mathf.Max(1f, profileSize.y - ProfilePadding * 2f - 24f);
        var normalizedHeight = Mathf.InverseLerp(elevationBounds.x, elevationBounds.y, height);

        return new Vector2(
            (normalizedDistance - 0.5f) * usableWidth,
            (normalizedHeight - 0.5f) * usableHeight + 8f);
    }

    private static void CreateProfileLine(RectTransform parent, Vector2 elevationBounds, Vector2 profileSize)
    {
        var previous = CalculateProfilePosition(0f, profileSize, elevationBounds);

        for (var i = 1; i <= ProfileSampleCount; i++)
        {
            var distance = CoursePath.CourseLengthMeters * (i / (float)ProfileSampleCount);
            var current = CalculateProfilePosition(distance, profileSize, elevationBounds);
            CreateLineSegment(parent, previous, current, new Color(1f, 1f, 1f, 0.88f), 3f);
            previous = current;
        }
    }

    private static void CreateLineSegment(Transform parent, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var lineObject = new GameObject("Elevation Segment", typeof(RectTransform));
        lineObject.transform.SetParent(parent, false);

        var image = lineObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        var rect = lineObject.GetComponent<RectTransform>();
        var delta = end - start;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = (start + end) * 0.5f;
        rect.sizeDelta = new Vector2(Mathf.Max(1f, delta.magnitude), thickness);
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private static RectTransform CreateMarker(Transform parent)
    {
        var markerObject = new GameObject("Elevation Player Marker", typeof(RectTransform));
        markerObject.transform.SetParent(parent, false);

        var image = markerObject.AddComponent<Image>();
        image.color = new Color(0.25f, 1f, 0.25f, 1f);
        image.raycastTarget = false;

        var rect = markerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(12f, 12f);
        return rect;
    }

    private static TextMeshProUGUI CreateGradientText(Transform parent)
    {
        var textObject = new GameObject("Elevation Gradient Text");
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.BottomLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(12f, 8f);
        rect.sizeDelta = new Vector2(210f, 26f);
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

public class CourseElevationProfileRuntimeUpdater : MonoBehaviour
{
    private const int MaxInstallFrames = 120;
    private int installFrames;

    private void Update()
    {
        var player = Object.FindFirstObjectByType<PlayerSpeedController>();
        if (CourseElevationProfileDisplay.EnsureRuntimeProfile(player) != null || installFrames >= MaxInstallFrames)
        {
            Destroy(gameObject);
            return;
        }

        installFrames++;
    }
}
