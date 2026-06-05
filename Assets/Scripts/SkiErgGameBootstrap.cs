using TMPro;
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
        CreateCamera(player.transform, player.GetComponent<PlayerSpeedController>());
        CreateLight();
        CreateHud(player.GetComponent<PlayerSpeedController>());
    }

    private static void CreateEnvironment()
    {
        CreateRoad();
        CreateGrassStrip("Left Grass", -RoadWidthMeters * 0.5f - GrassWidthMeters * 0.5f);
        CreateGrassStrip("Right Grass", RoadWidthMeters * 0.5f + GrassWidthMeters * 0.5f);
        CreateRoadMarkings();
        CreateStartFinishMarkers();
        CreateTrees();
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

    private static void CreateRoadMarkings()
    {
        var markings = new GameObject("Road Markings");
        CreateRoadLine(markings.transform, "Left Edge Line", -RoadWidthMeters * 0.5f + 0.35f, RoadLengthMeters * 0.5f, RoadLengthMeters, 0.14f);
        CreateRoadLine(markings.transform, "Right Edge Line", RoadWidthMeters * 0.5f - 0.35f, RoadLengthMeters * 0.5f, RoadLengthMeters, 0.14f);

        for (var z = 18f; z < RoadLengthMeters; z += 42f)
        {
            CreateRoadLine(markings.transform, "Center Dash", 0f, z, 13f, 0.24f);
        }
    }

    private static void CreateRoadLine(Transform parent, string name, float xPosition, float zPosition, float length, float width)
    {
        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = name;
        line.transform.SetParent(parent, false);
        line.transform.position = new Vector3(xPosition, 0.025f, zPosition);
        line.transform.localScale = new Vector3(width, 0.035f, length);

        var renderer = line.GetComponent<Renderer>();
        renderer.material.color = Color.white;
    }

    private static void CreateStartFinishMarkers()
    {
        CreateGate("Start Gate", 0f, new Color(0.1f, 0.45f, 0.95f));
        CreateGate("Finish Gate", RoadLengthMeters, new Color(0.95f, 0.15f, 0.12f));
        CreateRoadLine(null, "Start Line", 0f, 1f, 0.35f, RoadWidthMeters);
        CreateRoadLine(null, "Finish Line", 0f, RoadLengthMeters - 1f, 0.35f, RoadWidthMeters);
    }

    private static void CreateGate(string name, float zPosition, Color color)
    {
        var gate = new GameObject(name);
        AddGatePart(gate.transform, "Left Post", new Vector3(-RoadWidthMeters * 0.5f - 0.35f, 1.5f, zPosition), new Vector3(0.28f, 3f, 0.28f), color);
        AddGatePart(gate.transform, "Right Post", new Vector3(RoadWidthMeters * 0.5f + 0.35f, 1.5f, zPosition), new Vector3(0.28f, 3f, 0.28f), color);
        AddGatePart(gate.transform, "Top Bar", new Vector3(0f, 3.05f, zPosition), new Vector3(RoadWidthMeters + 1.2f, 0.3f, 0.3f), color);
    }

    private static void AddGatePart(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.position = position;
        part.transform.localScale = scale;

        var renderer = part.GetComponent<Renderer>();
        renderer.material.color = color;
    }

    private static void CreateTrees()
    {
        var trees = new GameObject("Roadside Trees");

        for (var z = 35f; z < RoadLengthMeters; z += 85f)
        {
            CreateTree(trees.transform, new Vector3(-RoadWidthMeters * 0.5f - 7f, 0f, z), 1f + Mathf.PingPong(z * 0.013f, 0.45f));
            CreateTree(trees.transform, new Vector3(RoadWidthMeters * 0.5f + 7f, 0f, z + 28f), 0.9f + Mathf.PingPong(z * 0.017f, 0.5f));
        }
    }

    private static void CreateTree(Transform parent, Vector3 position, float scale)
    {
        var tree = new GameObject("Tree");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 0.65f * scale, 0f);
        trunk.transform.localScale = new Vector3(0.22f * scale, 0.65f * scale, 0.22f * scale);
        trunk.GetComponent<Renderer>().material.color = new Color(0.36f, 0.22f, 0.11f);

        var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform, false);
        crown.transform.localPosition = new Vector3(0f, 1.65f * scale, 0f);
        crown.transform.localScale = new Vector3(1.4f * scale, 1.65f * scale, 1.4f * scale);
        crown.GetComponent<Renderer>().material.color = new Color(0.08f, 0.35f, 0.12f);
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

    private static void CreateCamera(Transform target, PlayerSpeedController player)
    {
        var cameraObject = new GameObject("Follow Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        cameraObject.transform.position = target.position + new Vector3(0f, 3.2f, -7f);
        cameraObject.transform.LookAt(target.position + Vector3.up * 1.15f + target.forward * 5f);

        var followCamera = cameraObject.AddComponent<FollowCamera>();
        followCamera.target = target;
        followCamera.player = player;

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
        canvas.sortingOrder = 1000;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var speedText = CreateHudText(canvasObject.transform, "Speed Text", new Vector2(28f, -28f));
        var distanceText = CreateHudText(canvasObject.transform, "Distance Text", new Vector2(28f, -78f));

        var display = canvasObject.AddComponent<SpeedDistanceDisplay>();
        display.player = player;
        display.speedText = speedText;
        display.distanceText = distanceText;
        display.Refresh();
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
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.text = string.Empty;

        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(520f, 48f);

        return text;
    }
}
