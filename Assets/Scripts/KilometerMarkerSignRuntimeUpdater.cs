using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class KilometerMarkerSignRuntimeUpdater : MonoBehaviour
{
    public const string MarkerRootName = "Kilometer Marker Signs";
    public const float MarkerSpacingMeters = 1000f;
    public const float MarkerLateralOffset = EnvironmentPlacement.RoadHalfWidth + 1.45f;
    public const float MarkerFootprintRadius = 0.45f;
    public static readonly Color BoardColor = Color.white;
    public static readonly Color TextColor = Color.black;
    public static readonly Color PostColor = new Color(0.12f, 0.12f, 0.12f, 1f);
    public static readonly Color BorderColor = Color.black;

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
        position.y += 1.8f;
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

            RebuildKilometerMarkers(existing.transform);
            return existing;
        }

        var root = new GameObject(MarkerRootName);
        root.AddComponent<KilometerMarkerSignRuntimeUpdater>();
        RebuildKilometerMarkers(root.transform);
        return root;
    }

    private static void RebuildKilometerMarkers(Transform root)
    {
        ClearChildren(root);

        var markerCount = CalculateMarkerCount(CoursePath.CourseLengthMeters);
        for (var kilometer = 1; kilometer <= markerCount; kilometer++)
        {
            var distanceMeters = kilometer * MarkerSpacingMeters;
            CreateMarkerSign(root, kilometer, distanceMeters, -1f);
            CreateMarkerSign(root, kilometer, distanceMeters, 1f);
        }
    }

    private static void CreateMarkerSign(Transform parent, int kilometer, float distanceMeters, float side)
    {
        var sideName = side < 0f ? "Left" : "Right";
        var sign = new GameObject($"{sideName} {kilometer} km Marker");
        sign.transform.SetParent(parent, false);
        sign.transform.position = CalculateMarkerPosition(distanceMeters, side);
        sign.transform.rotation = CalculateApproachFacingRotation(distanceMeters);

        AddSignPart(sign.transform, "Marker Post", new Vector3(0f, -1.05f, 0f), new Vector3(0.2f, 2.1f, 0.2f), PostColor);
        AddSignPart(sign.transform, "Marker Board", new Vector3(0f, 0.55f, 0f), new Vector3(3.35f, 2.75f, 0.16f), BoardColor);
        AddSignPart(sign.transform, "Marker Board Border Top", new Vector3(0f, 1.98f, -0.01f), new Vector3(3.62f, 0.18f, 0.18f), BorderColor);
        AddSignPart(sign.transform, "Marker Board Border Bottom", new Vector3(0f, -0.88f, -0.01f), new Vector3(3.62f, 0.18f, 0.18f), BorderColor);
        AddSignPart(sign.transform, "Marker Board Border Left", new Vector3(-1.74f, 0.55f, -0.01f), new Vector3(0.18f, 2.9f, 0.18f), BorderColor);
        AddSignPart(sign.transform, "Marker Board Border Right", new Vector3(1.74f, 0.55f, -0.01f), new Vector3(0.18f, 2.9f, 0.18f), BorderColor);
        AddMarkerText(sign.transform, $"{kilometer} km");
    }

    public static Quaternion CalculateApproachFacingRotation(float distanceMeters)
    {
        var direction = CoursePath.DirectionAtDistance(distanceMeters);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(-direction.normalized, Vector3.up);
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
        AddMarkerTextFace(parent, "Marker Text Approach Face", label, 0.125f, 0f);
        AddMarkerTextFace(parent, "Marker Text Reverse Face", label, -0.125f, 180f);
    }

    private static void AddMarkerTextFace(Transform parent, string name, string label, float localZ, float yRotation)
    {
        var textObject = new GameObject("Marker Text");
        textObject.name = name;
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = new Vector3(0f, 0.55f, localZ);
        textObject.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        textObject.transform.localScale = new Vector3(0.82f, 0.82f, 0.82f);

        var text = textObject.AddComponent<TextMeshPro>();
        text.text = label;
        text.fontSize = 4.8f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TextColor;
        text.enableAutoSizing = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;

        var rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(4.4f, 1.7f);
    }

    private static void ClearChildren(Transform root)
    {
        for (var index = root.childCount - 1; index >= 0; index--)
        {
            var child = root.GetChild(index).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }
}
