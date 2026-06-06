using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CourseMinimapDisplay : MonoBehaviour
{
    public const int CourseSampleCount = 96;
    public static readonly Vector2 DefaultMapSize = new Vector2(240f, 240f);
    public static readonly Vector2 DefaultMapPosition = new Vector2(-28f, -28f);
    public const float MapPadding = 18f;

    public PlayerSpeedController player;
    public RectTransform playerDot;
    public RectTransform courseShapeRoot;
    public Vector2 mapSize = DefaultMapSize;

    private Rect courseBounds;

    private void Awake()
    {
        courseBounds = CalculateCourseBounds(CourseSampleCount);
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (player == null || playerDot == null)
        {
            return;
        }

        playerDot.anchoredPosition = CalculateMapPosition(player.CurrentLapProgressMeters, mapSize, courseBounds);
    }

    public static CourseMinimapDisplay CreateRuntimeMinimap(Transform parent, PlayerSpeedController player)
    {
        var panelObject = new GameObject("Course Minimap", typeof(RectTransform));
        panelObject.transform.SetParent(parent, false);

        var panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.58f);
        panelImage.raycastTarget = false;

        var panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = DefaultMapPosition;
        panelRect.sizeDelta = DefaultMapSize;

        var display = panelObject.AddComponent<CourseMinimapDisplay>();
        display.player = player;
        display.mapSize = DefaultMapSize;
        display.courseBounds = CalculateCourseBounds(CourseSampleCount);

        var shapeObject = new GameObject("Course Shape", typeof(RectTransform));
        var shapeRoot = shapeObject.GetComponent<RectTransform>();
        shapeRoot.SetParent(panelRect, false);
        shapeRoot.anchorMin = new Vector2(0.5f, 0.5f);
        shapeRoot.anchorMax = new Vector2(0.5f, 0.5f);
        shapeRoot.pivot = new Vector2(0.5f, 0.5f);
        shapeRoot.anchoredPosition = Vector2.zero;
        shapeRoot.sizeDelta = DefaultMapSize;
        display.courseShapeRoot = shapeRoot;

        CreateCourseDots(shapeRoot, display.courseBounds, DefaultMapSize);
        display.playerDot = CreateDot(panelRect, "Player Dot", new Vector2(12f, 12f), new Color(0.25f, 1f, 0.25f, 1f));
        display.Refresh();
        return display;
    }

    public static Rect CalculateCourseBounds(int sampleCount)
    {
        var safeSampleCount = Mathf.Max(8, sampleCount);
        var firstPoint = CoursePath.CenterPointAtDistance(0f);
        var minX = firstPoint.x;
        var maxX = firstPoint.x;
        var minZ = firstPoint.z;
        var maxZ = firstPoint.z;

        for (var i = 1; i < safeSampleCount; i++)
        {
            var distance = CoursePath.CourseLengthMeters * (i / (float)safeSampleCount);
            var point = CoursePath.CenterPointAtDistance(distance);
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minZ = Mathf.Min(minZ, point.z);
            maxZ = Mathf.Max(maxZ, point.z);
        }

        return Rect.MinMaxRect(minX, minZ, maxX, maxZ);
    }

    public static Vector2 CalculateMapPosition(float distanceMeters, Vector2 mapSize, Rect bounds)
    {
        var point = CoursePath.CenterPointAtDistance(distanceMeters);
        var usableWidth = Mathf.Max(1f, mapSize.x - MapPadding * 2f);
        var usableHeight = Mathf.Max(1f, mapSize.y - MapPadding * 2f);
        var normalizedX = Mathf.InverseLerp(bounds.xMin, bounds.xMax, point.x);
        var normalizedY = Mathf.InverseLerp(bounds.yMin, bounds.yMax, point.z);

        return new Vector2(
            (normalizedX - 0.5f) * usableWidth,
            (normalizedY - 0.5f) * usableHeight);
    }

    private static void CreateCourseDots(RectTransform parent, Rect bounds, Vector2 mapSize)
    {
        for (var i = 0; i < CourseSampleCount; i++)
        {
            var distance = CoursePath.CourseLengthMeters * (i / (float)CourseSampleCount);
            var dot = CreateDot(parent, "Course Dot", new Vector2(4f, 4f), new Color(1f, 1f, 1f, 0.88f));
            dot.anchoredPosition = CalculateMapPosition(distance, mapSize, bounds);
        }
    }

    private static RectTransform CreateDot(Transform parent, string name, Vector2 size, Color color)
    {
        var dotObject = new GameObject(name, typeof(RectTransform));
        dotObject.transform.SetParent(parent, false);

        var image = dotObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        var rect = dotObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        return rect;
    }
}
