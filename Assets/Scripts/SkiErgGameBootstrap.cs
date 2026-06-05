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
        CreateRoadsidePosts();
        CreateStartFinishMarkers();
        CreateRollingHills();
        CreateDistantMountains();
        CreateTrees();
    }

    private static void CreateRoad()
    {
        var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "5 km Training Road";
        road.transform.position = new Vector3(0f, -0.05f, RoadLengthMeters * 0.5f);
        road.transform.localScale = new Vector3(RoadWidthMeters, 0.1f, RoadLengthMeters);
        road.GetComponent<Renderer>().material.color = new Color(0.16f, 0.18f, 0.2f);
    }

    private static void CreateGrassStrip(string name, float xPosition)
    {
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = name;
        grass.transform.position = new Vector3(xPosition, -0.08f, RoadLengthMeters * 0.5f);
        grass.transform.localScale = new Vector3(GrassWidthMeters, 0.08f, RoadLengthMeters);
        grass.GetComponent<Renderer>().material.color = new Color(0.18f, 0.55f, 0.18f);
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
        line.GetComponent<Renderer>().material.color = Color.white;
    }

    private static void CreateRoadsidePosts()
    {
        var posts = new GameObject("Roadside Speed Posts");
        var leftX = -RoadWidthMeters * 0.5f - 0.9f;
        var rightX = RoadWidthMeters * 0.5f + 0.9f;

        for (var z = 25f; z < RoadLengthMeters; z += 25f)
        {
            CreateRoadsidePost(posts.transform, leftX, z);
            CreateRoadsidePost(posts.transform, rightX, z);
        }
    }

    private static void CreateRoadsidePost(Transform parent, float xPosition, float zPosition)
    {
        var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = "Speed Post";
        post.transform.SetParent(parent, false);
        post.transform.position = new Vector3(xPosition, 0.42f, zPosition);
        post.transform.localScale = new Vector3(0.18f, 0.85f, 0.18f);
        post.GetComponent<Renderer>().material.color = Color.white;
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
        part.GetComponent<Renderer>().material.color = color;
    }

    private static void CreateRollingHills()
    {
        var hills = new GameObject("Low Poly Rolling Hills");
        var hillColor = new Color(0.13f, 0.42f, 0.16f);

        for (var z = 180f; z < RoadLengthMeters; z += 360f)
        {
            CreateLowPolyHill(hills.transform, new Vector3(-30f, -0.05f, z), new Vector3(18f, 2.6f, 70f), hillColor);
            CreateLowPolyHill(hills.transform, new Vector3(30f, -0.05f, z + 150f), new Vector3(22f, 3.2f, 85f), hillColor);
        }
    }

    private static void CreateLowPolyHill(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var hill = new GameObject("Low Poly Hill");
        hill.transform.SetParent(parent, false);
        hill.transform.position = position;
        hill.transform.localScale = scale;

        var meshFilter = hill.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMoundMesh();
        hill.AddComponent<MeshRenderer>().material.color = color;
    }

    private static Mesh CreateMoundMesh()
    {
        var vertices = new[]
        {
            new Vector3(0f, 1f, 0f),
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3(0.5f, 0f, -0.5f),
            new Vector3(0.5f, 0f, 0.5f),
            new Vector3(-0.5f, 0f, 0.5f)
        };
        var triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1, 1, 4, 3, 1, 3, 2 };
        var mesh = new Mesh { vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void CreateDistantMountains()
    {
        var mountains = new GameObject("Distant Low Poly Mountains");
        var color = new Color(0.42f, 0.46f, 0.5f);

        for (var z = 500f; z <= RoadLengthMeters; z += 850f)
        {
            CreateLowPolyMountain(mountains.transform, new Vector3(-70f, -0.1f, z), new Vector3(30f, 22f, 42f), color);
            CreateLowPolyMountain(mountains.transform, new Vector3(72f, -0.1f, z + 320f), new Vector3(38f, 28f, 48f), color);
        }
    }

    private static void CreateLowPolyMountain(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var mountain = new GameObject("Low Poly Mountain");
        mountain.transform.SetParent(parent, false);
        mountain.transform.position = position;
        mountain.transform.localScale = scale;

        var meshFilter = mountain.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMoundMesh();
        mountain.AddComponent<MeshRenderer>().material.color = color;
    }

    private static void CreateTrees()
    {
        var trees = new GameObject("Roadside Trees");

        for (var z = 25f; z < RoadLengthMeters; z += 55f)
        {
            CreateTree(trees.transform, new Vector3(-RoadWidthMeters * 0.5f - 6.5f, 0f, z), 0.9f + Mathf.PingPong(z * 0.013f, 0.45f));
            CreateTree(trees.transform, new Vector3(RoadWidthMeters * 0.5f + 6.5f, 0f, z + 18f), 0.9f + Mathf.PingPong(z * 0.017f, 0.5f));
            CreateTree(trees.transform, new Vector3(-RoadWidthMeters * 0.5f - 12f, 0f, z + 30f), 1.1f + Mathf.PingPong(z * 0.011f, 0.5f));
            CreateTree(trees.transform, new Vector3(RoadWidthMeters * 0.5f + 12f, 0f, z + 43f), 1f + Mathf.PingPong(z * 0.019f, 0.5f));
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
        trunk.transform.localScale = new Vector3(0.18f * scale, 0.65f * scale, 0.18f * scale);
        trunk.GetComponent<Renderer>().material.color = new Color(0.36f, 0.22f, 0.11f);

        var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform, false);
        crown.transform.localPosition = new Vector3(0f, 1.65f * scale, 0f);
        crown.transform.localScale = new Vector3(1.15f * scale, 1.35f * scale, 1.15f * scale);
        crown.GetComponent<Renderer>().material.color = new Color(0.08f, 0.35f, 0.12f);
    }

    private static GameObject CreateSkier()
    {
        var skier = new GameObject("Placeholder Skier");
        skier.transform.position = new Vector3(0f, 0f, 0f);

        var controller = skier.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 4f;
        controller.SetStartDistanceZ(skier.transform.position.z);

        AddBodyPart(skier.transform, "Torso", PrimitiveType.Capsule, new Vector3(0f, 1.28f, -0.06f), new Vector3(0.42f, 0.78f, 0.3f), new Color(0.1f, 0.45f, 0.95f), new Vector3(12f, 0f, 0f));
        AddBodyPart(skier.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.95f, -0.18f), new Vector3(0.3f, 0.3f, 0.3f), new Color(0.95f, 0.78f, 0.58f), Vector3.zero);
        AddBodyPart(skier.transform, "Left Leg", PrimitiveType.Capsule, new Vector3(-0.18f, 0.62f, 0.08f), new Vector3(0.16f, 0.55f, 0.16f), new Color(0.08f, 0.1f, 0.16f), new Vector3(-8f, 0f, 0f));
        AddBodyPart(skier.transform, "Right Leg", PrimitiveType.Capsule, new Vector3(0.18f, 0.62f, 0.08f), new Vector3(0.16f, 0.55f, 0.16f), new Color(0.08f, 0.1f, 0.16f), new Vector3(-8f, 0f, 0f));
        AddBodyPart(skier.transform, "Left Ski", PrimitiveType.Cube, new Vector3(-0.28f, 0.05f, 0.33f), new Vector3(0.1f, 0.06f, 1.9f), Color.white, Vector3.zero);
        AddBodyPart(skier.transform, "Right Ski", PrimitiveType.Cube, new Vector3(0.28f, 0.05f, 0.33f), new Vector3(0.1f, 0.06f, 1.9f), Color.white, Vector3.zero);
        AddPole(skier.transform, "Left Pole", new Vector3(-0.58f, 0.82f, 0.15f), 20f);
        AddPole(skier.transform, "Right Pole", new Vector3(0.58f, 0.82f, 0.15f), -20f);

        return skier;
    }

    private static void AddBodyPart(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color, Vector3 localRotation)
    {
        var part = GameObject.CreatePrimitive(primitiveType);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localRotation);
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material.color = color;
    }

    private static void AddPole(Transform parent, string name, Vector3 localPosition, float zRotation)
    {
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = name;
        pole.transform.SetParent(parent, false);
        pole.transform.localPosition = localPosition;
        pole.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        pole.transform.localScale = new Vector3(0.03f, 0.95f, 0.03f);
        pole.GetComponent<Renderer>().material.color = new Color(0.08f, 0.08f, 0.08f);
    }

    private static void CreateCamera(Transform target, PlayerSpeedController player)
    {
        var cameraObject = new GameObject("Follow Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 64f;
        cameraObject.transform.position = target.position + new Vector3(0f, 3f, -6.4f);
        cameraObject.transform.LookAt(target.position + Vector3.up * 1.1f + target.forward * 7f);

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
