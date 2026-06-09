using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class StartFinishPortalRuntimeUpdater : MonoBehaviour
{
    public const string PortalRootName = "Combined START FINISH Race Portal";
    public const float PortalSpanWidth = 13.6f;
    public const float TowerLateralOffset = 6.25f;
    public const float PortalHeight = 5.5f;
    public static readonly Color NordicBlue = new Color(0.08f, 0.34f, 0.78f, 1f);
    public static readonly Color RaceRed = new Color(0.86f, 0.08f, 0.08f, 1f);
    public static readonly Color SnowWhite = new Color(0.96f, 0.97f, 0.94f, 1f);
    public static readonly Color DarkText = new Color(0.02f, 0.025f, 0.03f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallStartFinishPortal()
    {
        EnsureStartFinishPortal();
    }

    public static Vector3 CalculatePortalPosition()
    {
        return CoursePath.CenterPointAtDistance(0f);
    }

    public static GameObject EnsureStartFinishPortal()
    {
        DisableOldSmallGate();

        var existing = GameObject.Find(PortalRootName);
        if (existing != null)
        {
            if (existing.GetComponent<StartFinishPortalRuntimeUpdater>() == null)
            {
                existing.AddComponent<StartFinishPortalRuntimeUpdater>();
            }

            return existing;
        }

        var portal = new GameObject(PortalRootName);
        portal.transform.position = CalculatePortalPosition();
        portal.transform.rotation = HorizontalRotationAtDistance(0f);
        portal.AddComponent<StartFinishPortalRuntimeUpdater>();

        CreateTowers(portal.transform);
        CreateTopBanner(portal.transform);
        CreateBrandPanels(portal.transform);
        CreatePennants(portal.transform);
        CreatePaintedFinishLine(portal.transform);
        return portal;
    }

    private static void DisableOldSmallGate()
    {
        var oldGate = GameObject.Find("Start Finish Gate");
        if (oldGate != null)
        {
            oldGate.SetActive(false);
        }

        var oldLine = GameObject.Find("Start Finish Line");
        if (oldLine != null)
        {
            oldLine.SetActive(false);
        }
    }

    private static void CreateTowers(Transform parent)
    {
        CreateTower(parent, "Left Race Portal Tower", -TowerLateralOffset);
        CreateTower(parent, "Right Race Portal Tower", TowerLateralOffset);
    }

    private static void CreateTower(Transform parent, string name, float lateralOffset)
    {
        AddPart(parent, name, new Vector3(lateralOffset, PortalHeight * 0.5f, 0f), new Vector3(0.74f, PortalHeight, 0.74f), NordicBlue);
        AddPart(parent, name + " White Cap", new Vector3(lateralOffset, PortalHeight + 0.18f, 0f), new Vector3(1f, 0.36f, 1f), SnowWhite);
        AddPart(parent, name + " Red Base", new Vector3(lateralOffset, 0.22f, 0f), new Vector3(1.05f, 0.44f, 1.05f), RaceRed);
        AddPart(parent, name + " Side Stripe", new Vector3(lateralOffset, PortalHeight * 0.5f, -0.39f), new Vector3(0.82f, PortalHeight * 0.9f, 0.08f), SnowWhite);
    }

    private static void CreateTopBanner(Transform parent)
    {
        AddPart(parent, "START / FINISH Banner", new Vector3(0f, PortalHeight, 0f), new Vector3(PortalSpanWidth, 1.05f, 0.42f), SnowWhite);
        AddPart(parent, "Banner Top Blue Rail", new Vector3(0f, PortalHeight + 0.65f, 0f), new Vector3(PortalSpanWidth + 0.7f, 0.28f, 0.52f), NordicBlue);
        AddPart(parent, "Banner Bottom Red Rail", new Vector3(0f, PortalHeight - 0.65f, 0f), new Vector3(PortalSpanWidth + 0.4f, 0.22f, 0.5f), RaceRed);
        AddWorldText(parent, "START / FINISH Text Road Face", "START / FINISH", new Vector3(0f, PortalHeight, -0.235f), 0f, 0.36f, DarkText);
        AddWorldText(parent, "START / FINISH Text Reverse Face", "START / FINISH", new Vector3(0f, PortalHeight, 0.235f), 180f, 0.36f, DarkText);
    }

    private static void CreateBrandPanels(Transform parent)
    {
        AddBrandPanel(parent, "Left Race Brand Panel", -TowerLateralOffset, "SKI-VERSE");
        AddBrandPanel(parent, "Right Race Brand Panel", TowerLateralOffset, "NORDIC RACE");
    }

    private static void AddBrandPanel(Transform parent, string name, float lateralOffset, string label)
    {
        AddPart(parent, name, new Vector3(lateralOffset, 3f, -0.43f), new Vector3(1.25f, 1.15f, 0.08f), SnowWhite);
        AddWorldText(parent, name + " Text", label, new Vector3(lateralOffset, 3f, -0.485f), 0f, 0.13f, DarkText);
    }

    private static void CreatePennants(Transform parent)
    {
        for (var index = 0; index < 12; index++)
        {
            var x = Mathf.Lerp(-5.4f, 5.4f, index / 11f);
            var color = index % 2 == 0 ? RaceRed : NordicBlue;
            AddPart(parent, "Race Pennant", new Vector3(x, PortalHeight - 1.05f, -0.52f), new Vector3(0.42f, 0.28f, 0.08f), color);
        }

        AddPart(parent, "Left Flag", new Vector3(-TowerLateralOffset - 0.55f, PortalHeight + 0.9f, -0.1f), new Vector3(0.76f, 0.46f, 0.08f), RaceRed);
        AddPart(parent, "Right Flag", new Vector3(TowerLateralOffset + 0.55f, PortalHeight + 0.9f, -0.1f), new Vector3(0.76f, 0.46f, 0.08f), NordicBlue);
    }

    private static void CreatePaintedFinishLine(Transform parent)
    {
        AddPart(parent, "Start Finish Painted Line", new Vector3(0f, 0.045f, 1f), new Vector3(8.7f, 0.055f, 0.42f), SnowWhite);
        AddPart(parent, "Start Finish Blue Edge", new Vector3(0f, 0.052f, 0.55f), new Vector3(8.7f, 0.055f, 0.12f), NordicBlue);
        AddPart(parent, "Start Finish Red Edge", new Vector3(0f, 0.052f, 1.45f), new Vector3(8.7f, 0.055f, 0.12f), RaceRed);
    }

    private static void AddPart(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material.color = color;
    }

    private static void AddWorldText(Transform parent, string name, string label, Vector3 localPosition, float yRotation, float scale, Color color)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        textObject.transform.localScale = new Vector3(scale, scale, scale);

        var text = textObject.AddComponent<TextMeshPro>();
        text.text = label;
        text.fontSize = 4f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.enableAutoSizing = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;

        var rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(24f, 4f);
    }

    private static Quaternion HorizontalRotationAtDistance(float distanceMeters)
    {
        var direction = CoursePath.DirectionAtDistance(distanceMeters);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
