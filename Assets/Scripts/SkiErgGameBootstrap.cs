using UnityEngine;
using UnityEngine.UI;

public class SkiErgGameBootstrap : MonoBehaviour
{
    private const float RoadLengthMeters = 5000f;
    private const float RoadWidthMeters = 8f;
    private const float GrassWidthMeters = 36f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildPrototypeScene()
    {
        if (Object.FindObjectOfType<PlayerSpeedController>() != null)
        {
            return;
        }

        CreateEnvironment();
        var player = CreateSkier();
        CreateCamera(player.transform);
        CreateLight();
        CreateHud(player.GetComponent<PlayerSpeedController>());
    }

    private static void CreateEnvironment()
    {
        CreateRoad();
        CreateGrassStrip("Left Grass", -RoadWidthMeters * 0.5f - GrassWidthMeters * 0.5f);
        CreateGrassStrip("Right Grass", RoadWidthMeters * 0.5f + GrassWidthMeters * 0.5f);
    }

    private static void CreateRoad()
    {
        var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "5 km Training Road";
        road.transform.position = new Vector3(0f, -0.05f, RoadLengthMeters * 0.5f);
        road.transform.localScale = new Vector3(RoadWidthMeters, 0.1f, RoadLengthMeters);

        var renderer = road.GetComponent<Renderer>();
        renderer.material.color = new Color(0.16f, 0.18f, 0.2f);
    }

    private static void CreateGrassStrip(string name, float xPosition)
    {
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = name;
        grass.transform.position = new Vector3(xPosition, -0.08f, RoadLengthMeters * 0.5f);
        grass.transform.localScale = new Vector3(GrassWidthMeters, 0.08f, RoadLengthMeters);

        var renderer = grass.GetComponent<Renderer>();
        renderer.material.color = new Color(0.18f, 0.55f, 0.18f);
    }

    private static GameObject CreateSkier()
    {
        var skier = new GameObject("Placeholder Skier");
        skier.transform.position = new Vector3(0f, 0f, 0f);

        var controller = skier.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 4f;
        controller.SetStartDistanceZ(skier.transform.position.z);

        AddBodyPart(skier.transform, "Body", PrimitiveType.Capsule, new Vector3(0f, 1.25f, 0f), new Vector3(0.55f, 0.95f, 0.35f), new Color(0.1f, 0.45f, 0.95f));
        AddBodyPart(skier.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 2.1f, 0f), new Vector3(0.35f, 0.35f, 0.35f), new Color(0.95f, 0.78f, 0.58f));
        AddBodyPart(skier.transform, "Left Ski", PrimitiveType.Cube, new Vector3(-0.28f, 0.05f, 0.28f), new Vector3(0.12f, 0.08f, 1.7f), Color.white);
        AddBodyPart(skier.transform, "Right Ski", PrimitiveType.Cube, new Vector3(0.28f, 0.05f, 0.28f), new Vector3(0.12f, 0.08f, 1.7f), Color.white);
        AddPole(skier.transform, "Left Pole", new Vector3(-0.65f, 0.85f, 0.2f), 18f);
        AddPole(skier.transform, "Right Pole", new Vector3(0.65f, 0.85f, 0.2f), -18f);

        return skier;
    }

    private static void AddBodyPart(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
    {
        var part = GameObject.CreatePrimitive(primitiveType);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        var renderer = part.GetComponent<Renderer>();
        renderer.material.color = color;
    }

    private static void AddPole(Transform parent, string name, Vector3 localPosition, float zRotation)
    {
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = name;
        pole.transform.SetParent(parent, false);
        pole.transform.localPosition = localPosition;
        pole.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        pole.transform.localScale = new Vector3(0.035f, 0.9f, 0.035f);

        var renderer = pole.GetComponent<Renderer>();
        renderer.material.color = new Color(0.08f, 0.08f, 0.08f);
    }

    private static void CreateCamera(Transform target)
    {
        var cameraObject = new GameObject("Follow Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        cameraObject.transform.position = target.position + new Vector3(0f, 4f, -8f);
        cameraObject.transform.LookAt(target.position + Vector3.up * 1.2f);

        var followCamera = cameraObject.AddComponent<FollowCamera>();
        followCamera.target = target;

        if (Camera.main != null)
        {
            Camera.main.gameObject.SetActive(false);
        }
    }

    private static void CreateLight()
    {
        var lightObject = new GameObject("Sun Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateHud(PlayerSpeedController player)
    {
        var canvasObject = new GameObject("Race HUD");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        var speedText = CreateHudText(canvasObject.transform, "Speed Text", new Vector2(24f, -24f));
        var distanceText = CreateHudText(canvasObject.transform, "Distance Text", new Vector2(24f, -58f));

        var display = canvasObject.AddComponent<SpeedDistanceDisplay>();
        display.player = player;
        display.speedText = speedText;
        display.distanceText = distanceText;
    }

    private static Text CreateHudText(Transform parent, string name, Vector2 anchoredPosition)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.text = string.Empty;

        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(320f, 32f);

        return text;
    }
}
