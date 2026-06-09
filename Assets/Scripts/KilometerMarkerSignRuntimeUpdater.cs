using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class KilometerMarkerSignRuntimeUpdater : MonoBehaviour
{
    public const string MarkerRootName = "Kilometer Marker Signs";
    public const float MarkerSpacingMeters = 1000f;
    public const float MarkerLateralOffset = EnvironmentPlacement.RoadHalfWidth + 1.45f;
    public const float MarkerFootprintRadius = 0.45f;
    public static readonly Color BoardColor = new Color(0.95f, 0.95f, 0.9f, 1f);
    public static readonly Color TextColor = new Color(0.02f, 0.02f, 0.02f, 1f);
    public static readonly Color PostColor = new Color(0.12f, 0.12f, 0.12f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallKilometerMarkers()
    {
        EnsureKilometerMarkers();
    }

    public static int CalculateMarkerCount(float courseLengthMeters)
    {
        return Mathf.FloorToInt(Mathf.Max(0f, courseLengthMeters) / MarkerSpacingMeters);
    }

    public static Vector3 CalculateMarkerPosition(float distanceMeters, float side)
    {
        var sideSign = side < 0f ? -1f : 1f;
        var position = CoursePath.PointAtDistance(distanceMeters, sideSign * MarkerLateralOffset);
        position.y += 0.95f;
        return position;
    }

    public static GameObject EnsureKilometerMarkers()
    {
        var existing = GameObject.Find(MarkerRootName);
        if (existing != null)
        {
            if (existing.GetComponent<KilometerMarkerSignRuntimeUpdater>() == null)
            {
                existing.AddComponent<KilometerMarkerSignRuntimeUpdater>();
            }

            return existing;
        }

        var root = new GameObject(MarkerRootName);
        root.AddComponent<KilometerMarkerSignRuntimeUpdater>();

        var markerCount = CalculateMarkerCount(CoursePath.CourseLengthMeters);
        for (var kilometer = 1; kilometer <= markerCount; kilometer++)
        {
            var distanceMeters = kilometer * MarkerSpacingMeters;
            CreateMarkerSign(root.transform, kilometer, distanceMeters, -1f);
            CreateMarkerSign(root.transform, kilometer, distanceMeters, 1f);
        }

        return root;
    }

    private static void CreateMarkerSign(Transform parent, int kilometer, float distanceMeters, float side)
    {
        var sideName = side < 0f ? "Left" : "Right";
        var sign = new GameObject($"{sideName} {kilometer} km Marker");
        sign.transform.SetParent(parent, false);
        sign.transform.position = CalculateMarkerPosition(distanceMeters, side);
        sign.transform.rotation = CalculateInwardFacingRotation(distanceMeters, side);

        AddSignPart(sign.transform, "Marker Post", new Vector3(0f, -0.42f, 0f), new Vector3(0.12f, 0.9f, 0.12f), PostColor);
        AddSignPart(sign.transform, "Marker Board", new Vector3(0f, 0.18f, 0f), new Vector3(1.08f, 0.48f, 0.08f), BoardColor);
        AddMarkerText(sign.transform, $"{kilometer} km");
    }

    private static Quaternion CalculateInwardFacingRotation(float distanceMeters, float side)
    {
        var right = CoursePath.RightAtDistance(distanceMeters);
        var inward = side < 0f ? right : -right;
        inward.y = 0f;

        if (inward.sqrMagnitude <= 0.001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(inward.normalized, Vector3.up);
    }

    private static void AddSignPart(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material.color = color;
    }

    private static void AddMarkerText(Transform parent, string label)
    {
        AddMarkerTextFace(parent, "Marker Text Road Face", label, -0.046f, 180f);
        AddMarkerTextFace(parent, "Marker Text Outer Face", label, 0.046f, 0f);
    }

    private static void AddMarkerTextFace(Transform parent, string name, string label, float localZ, float yRotation)
    {
        var textObject = new GameObject("Marker Text");
        textObject.name = name;
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = new Vector3(0f, 0.18f, localZ);
        textObject.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        textObject.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        var text = textObject.AddComponent<TextMeshPro>();
        text.text = label;
        text.fontSize = 4.2f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TextColor;
        text.enableAutoSizing = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;

        var rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(9f, 3.2f);
    }
}
