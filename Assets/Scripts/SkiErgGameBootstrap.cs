using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkiErgGameBootstrap : MonoBehaviour
{
    private const float RoadLengthMeters = 5000f;
    private const float RoadWidthMeters = 8f;
    private const float GrassWidthMeters = 36f;
    private const float RoadSegmentLength = 12f;

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
        CreateGrass();
        CreateRoadMarkings();
        CreateRoadsidePosts();
        CreateTurnSigns();
        CreateStartFinishMarkers();
        CreateRollingHills();
        CreateDistantMountains();
        CreateTrees();
    }

    private static void CreateRoad()
    {
        var road = new GameObject("Sweeping 5 km Training Road");
        var color = new Color(0.16f, 0.18f, 0.2f);

        for (var z = RoadSegmentLength * 0.5f; z < RoadLengthMeters; z += RoadSegmentLength)
        {
            CreatePathCube(road.transform, "Road Segment", 0f, z, RoadSegmentLength + 0.8f, RoadWidthMeters, 0.1f, color);
        }
    }

    private static void CreateGrass()
    {
        var grass = new GameObject("Open Grass Shoulders");
        var color = new Color(0.18f, 0.55f, 0.18f);
        var lateralOffset = RoadWidthMeters * 0.5f + GrassWidthMeters * 0.5f;

        for (var z = 24f; z < RoadLengthMeters; z += 48f)
        {
            CreatePathCube(grass.transform, "Left Open Grass Segment", -lateralOffset, z, 49f, GrassWidthMeters, 0.08f, color, -0.08f);
            CreatePathCube(grass.transform, "Right Open Grass Segment", lateralOffset, z, 49f, GrassWidthMeters, 0.08f, color, -0.08f);
        }
    }

    private static void CreateRoadMarkings()
    {
        var markings = new GameObject("Road Markings");
        var edgeLeft = -RoadWidthMeters * 0.5f + 0.35f;
        var edgeRight = RoadWidthMeters * 0.5f - 0.35f;

        for (var z = RoadSegmentLength * 0.5f; z < RoadLengthMeters; z += RoadSegmentLength)
        {
            CreatePathCube(markings.transform, "Left Edge Line", edgeLeft, z, RoadSegmentLength + 0.4f, 0.16f, 0.04f, Color.white, 0.025f);
            CreatePathCube(markings.transform, "Right Edge Line", edgeRight, z, RoadSegmentLength + 0.4f, 0.16f, 0.04f, Color.white, 0.025f);
        }

        for (var z = 14f; z < RoadLengthMeters; z += 30f)
        {
            CreatePathCube(markings.transform, "Center Dash", 0f, z, 14f, 0.28f, 0.04f, Color.white, 0.03f);
        }
    }

    private static void CreatePathCube(Transform parent, string name, float lateralOffset, float zPosition, float length, float width, float height, Color color, float yPosition = 0f)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        var position = CoursePath.PointAtDistance(zPosition, lateralOffset);
        position.y += yPosition;
        cube.transform.position = position;
        cube.transform.rotation = CoursePath.RotationAtDistance(zPosition);
        cube.transform.localScale = new Vector3(width, height, length);
        cube.GetComponent<Renderer>().material.color = color;
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

    private static void CreateRoadsidePost(Transform parent, float lateralOffset, float zPosition)
    {
        var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = "Speed Post";
        post.transform.SetParent(parent, false);
        var position = CoursePath.PointAtDistance(zPosition, lateralOffset);
        position.y += 0.52f;
        post.transform.position = position;
        post.transform.rotation = HorizontalRotationAtDistance(zPosition);
        post.transform.localScale = new Vector3(0.22f, 1.04f, 0.22f);
        post.GetComponent<Renderer>().material.color = Color.white;
    }

    private static void CreateTurnSigns()
    {
        var signs = new GameObject("Turn Warning Signs");

        for (var z = 90f; z < RoadLengthMeters; z += 260f)
        {
            var turnDirection = CalculateTurnDirection(z);
            var sideOffset = turnDirection > 0f ? EnvironmentPlacement.TurnSignOffset : -EnvironmentPlacement.TurnSignOffset;
            CreateTurnSign(signs.transform, z, sideOffset, turnDirection);
        }
    }

    private static float CalculateTurnDirection(float zPosition)
    {
        var previousX = CoursePath.CenterXAtDistance(zPosition - 25f);
        var upcomingX = CoursePath.CenterXAtDistance(zPosition + 170f);
        return upcomingX >= previousX ? 1f : -1f;
    }

    private static void CreateTurnSign(Transform parent, float zPosition, float lateralOffset, float turnDirection)
    {
        var sign = new GameObject(turnDirection > 0f ? "Right Turn Sign" : "Left Turn Sign");
        sign.transform.SetParent(parent, false);
        var position = CoursePath.PointAtDistance(zPosition, lateralOffset);
        position.y += 1.25f;
        sign.transform.position = position;
        sign.transform.rotation = HorizontalRotationAtDistance(zPosition);

        AddSignPart(sign.transform, "Sign Post", new Vector3(0f, -0.55f, 0f), new Vector3(0.12f, 1.1f, 0.12f), new Color(0.12f, 0.12f, 0.12f), Vector3.zero);
        AddSignPart(sign.transform, "Warning Board", new Vector3(0f, 0.25f, 0f), new Vector3(0.78f, 0.78f, 0.08f), new Color(0.95f, 0.82f, 0.12f), new Vector3(0f, 0f, 45f));
        AddTurnArrow(sign.transform, turnDirection, 0.065f);
        AddTurnArrow(sign.transform, turnDirection, -0.065f);
    }

    private static void AddTurnArrow(Transform parent, float turnDirection, float zOffset)
    {
        AddSignPart(parent, "Arrow Shaft", new Vector3(0f, 0.25f, zOffset), new Vector3(0.42f, 0.08f, 0.04f), Color.black, Vector3.zero);
        AddSignPart(parent, "Arrow Head Upper", new Vector3(0.18f * turnDirection, 0.34f, zOffset), new Vector3(0.26f, 0.07f, 0.04f), Color.black, new Vector3(0f, 0f, -35f * turnDirection));
        AddSignPart(parent, "Arrow Head Lower", new Vector3(0.18f * turnDirection, 0.16f, zOffset), new Vector3(0.26f, 0.07f, 0.04f), Color.black, new Vector3(0f, 0f, 35f * turnDirection));
    }

    private static void AddSignPart(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color, Vector3 localRotation)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localRotation);
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material.color = color;
    }

    private static void CreateStartFinishMarkers()
    {
        CreateGate("Start Gate", 0f, new Color(0.1f, 0.45f, 0.95f));
        CreateGate("Finish Gate", RoadLengthMeters, new Color(0.95f, 0.15f, 0.12f));
        CreatePathCube(null, "Start Line", 0f, 1f, 0.4f, RoadWidthMeters, 0.05f, Color.white, 0.04f);
        CreatePathCube(null, "Finish Line", 0f, RoadLengthMeters - 1f, 0.4f, RoadWidthMeters, 0.05f, Color.white, 0.04f);
    }

    private static void CreateGate(string name, float zPosition, Color color)
    {
        var gate = new GameObject(name);
        gate.transform.position = CoursePath.CenterPointAtDistance(zPosition);
        gate.transform.rotation = HorizontalRotationAtDistance(zPosition);
        AddGatePart(gate.transform, "Left Post", new Vector3(-RoadWidthMeters * 0.5f - 0.35f, 1.5f, 0f), new Vector3(0.28f, 3f, 0.28f), color);
        AddGatePart(gate.transform, "Right Post", new Vector3(RoadWidthMeters * 0.5f + 0.35f, 1.5f, 0f), new Vector3(0.28f, 3f, 0.28f), color);
        AddGatePart(gate.transform, "Top Bar", new Vector3(0f, 3.05f, 0f), new Vector3(RoadWidthMeters + 1.2f, 0.3f, 0.3f), color);
    }

    private static void AddGatePart(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material.color = color;
    }

    private static void CreateRollingHills()
    {
        var hills = new GameObject("Distant Low Poly Rolling Hills");
        var hillColor = new Color(0.12f, 0.44f, 0.16f);

        for (var z = 130f; z < RoadLengthMeters; z += 240f)
        {
            CreateLowPolyHill(hills.transform, CoursePath.PointAtDistance(z, -EnvironmentPlacement.NearHillOffset), new Vector3(EnvironmentPlacement.NearHillHalfWidth * 2f, 10f, 120f), hillColor);
            CreateLowPolyHill(hills.transform, CoursePath.PointAtDistance(z + 110f, EnvironmentPlacement.FarHillOffset), new Vector3(EnvironmentPlacement.FarHillHalfWidth * 2f, 12f, 140f), hillColor);
        }
    }

    private static void CreateLowPolyHill(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var hill = new GameObject("Low Poly Hill");
        hill.transform.SetParent(parent, false);
        position.y += -0.1f;
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
        var mountains = new GameObject("Distant Mountain Backdrop");
        var color = new Color(0.38f, 0.42f, 0.48f);

        for (var z = EnvironmentPlacement.MountainFirstDistance; z <= RoadLengthMeters; z += EnvironmentPlacement.MountainSpacing)
        {
            CreateLowPolyMountain(mountains.transform, CoursePath.PointAtDistance(z, -EnvironmentPlacement.NearMountainOffset), new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2f, 234f, 354f), color);
            CreateLowPolyMountain(mountains.transform, CoursePath.PointAtDistance(z + 340f, EnvironmentPlacement.FarMountainOffset), new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2f, 288f, 414f), color);
        }
    }

    private static void CreateLowPolyMountain(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var mountain = new GameObject("Low Poly Mountain");
        mountain.transform.SetParent(parent, false);
        position.y += -5f;
        mountain.transform.position = position;
        mountain.transform.localScale = scale;

        var meshFilter = mountain.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMoundMesh();
        mountain.AddComponent<MeshRenderer>().material.color = color;
    }

    private static void CreateTrees()
    {
        var trees = new GameObject("Set Back Roadside Trees");

        for (var z = 18f; z < RoadLengthMeters; z += 28f)
        {
            CreateTree(trees.transform, CoursePath.PointAtDistance(z, -EnvironmentPlacement.NearTreeOffset), 0.85f + Mathf.PingPong(z * 0.013f, 0.5f));
            CreateTree(trees.transform, CoursePath.PointAtDistance(z + 11f, EnvironmentPlacement.NearTreeOffset), 0.85f + Mathf.PingPong(z * 0.017f, 0.55f));
            CreateTree(trees.transform, CoursePath.PointAtDistance(z + 18f, -EnvironmentPlacement.MidTreeOffset), 1f + Mathf.PingPong(z * 0.011f, 0.6f));
            CreateTree(trees.transform, CoursePath.PointAtDistance(z + 25f, EnvironmentPlacement.MidTreeOffset), 0.95f + Mathf.PingPong(z * 0.019f, 0.6f));
            CreateTree(trees.transform, CoursePath.PointAtDistance(z + 34f, -EnvironmentPlacement.FarTreeOffset), 1.2f + Mathf.PingPong(z * 0.009f, 0.55f));
            CreateTree(trees.transform, CoursePath.PointAtDistance(z + 43f, EnvironmentPlacement.FarTreeOffset), 1.15f + Mathf.PingPong(z * 0.015f, 0.65f));
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
        var skier = new GameObject("Low Poly Roller Skier");
        skier.transform.position = CoursePath.CenterPointAtDistance(0f);
        skier.transform.rotation = CoursePath.RotationAtDistance(0f);

        var controller = skier.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 4f;
        controller.SetStartDistanceZ(skier.transform.position.z);

        var animator = skier.AddComponent<RollerSkierAnimator>();
        animator.player = controller;
        var visualRoot = CreateChild(skier.transform, "Roller Skier Visual", Vector3.zero);
        CreateRollerSkierVisual(visualRoot, animator);
        animator.ApplyPose(0.15f);

        return skier;
    }

    private static void CreateRollerSkierVisual(Transform parent, RollerSkierAnimator animator)
    {
        var suitBlue = new Color(0.08f, 0.36f, 0.9f);
        var darkSuit = new Color(0.05f, 0.07f, 0.1f);
        var skin = new Color(0.95f, 0.78f, 0.58f);
        var skiColor = new Color(0.92f, 0.94f, 0.92f);
        var wheelColor = new Color(0.03f, 0.03f, 0.035f);

        animator.leftSki = CreateRollerSki(parent, "Left Parallel Roller Ski", -0.24f, skiColor, wheelColor);
        animator.rightSki = CreateRollerSki(parent, "Right Parallel Roller Ski", 0.24f, skiColor, wheelColor);

        AddBodyPart(parent, "Left Thigh", PrimitiveType.Capsule, new Vector3(-0.16f, 0.67f, 0.12f), new Vector3(0.13f, 0.36f, 0.13f), darkSuit, new Vector3(-20f, 0f, 4f));
        AddBodyPart(parent, "Right Thigh", PrimitiveType.Capsule, new Vector3(0.16f, 0.67f, 0.12f), new Vector3(0.13f, 0.36f, 0.13f), darkSuit, new Vector3(-20f, 0f, -4f));
        AddBodyPart(parent, "Left Shin", PrimitiveType.Capsule, new Vector3(-0.18f, 0.34f, 0.04f), new Vector3(0.105f, 0.34f, 0.105f), darkSuit, new Vector3(8f, 0f, -3f));
        AddBodyPart(parent, "Right Shin", PrimitiveType.Capsule, new Vector3(0.18f, 0.34f, 0.04f), new Vector3(0.105f, 0.34f, 0.105f), darkSuit, new Vector3(8f, 0f, 3f));
        AddBodyPart(parent, "Hips", PrimitiveType.Cube, new Vector3(0f, 0.94f, 0.04f), new Vector3(0.46f, 0.24f, 0.28f), darkSuit, new Vector3(-8f, 0f, 0f));

        var torsoPivot = CreateChild(parent, "Torso Pivot", new Vector3(0f, 1.08f, 0.04f));
        animator.torso = torsoPivot;
        AddBodyPart(torsoPivot, "Forward Leaning Torso", PrimitiveType.Capsule, new Vector3(0f, 0.32f, -0.04f), new Vector3(0.38f, 0.58f, 0.28f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.78f, -0.18f), new Vector3(0.25f, 0.25f, 0.25f), skin, Vector3.zero);
        AddBodyPart(torsoPivot, "Helmet", PrimitiveType.Sphere, new Vector3(0f, 0.89f, -0.18f), new Vector3(0.27f, 0.13f, 0.27f), new Color(0.1f, 0.12f, 0.14f), Vector3.zero);

        animator.leftArm = CreateArm(parent, "Left Double-Poling Arm", new Vector3(-0.31f, 1.46f, -0.04f), -1f, suitBlue, skin);
        animator.rightArm = CreateArm(parent, "Right Double-Poling Arm", new Vector3(0.31f, 1.46f, -0.04f), 1f, suitBlue, skin);
        animator.leftPole = CreatePole(parent, "Left Carbon Pole", new Vector3(-0.48f, 0.9f, 0.16f), -1f);
        animator.rightPole = CreatePole(parent, "Right Carbon Pole", new Vector3(0.48f, 0.9f, 0.16f), 1f);
    }

    private static Transform CreateRollerSki(Transform parent, string name, float xPosition, Color skiColor, Color wheelColor)
    {
        var ski = CreateChild(parent, name, new Vector3(xPosition, 0f, 0.22f));
        AddBodyPart(ski, "Ski Frame", PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0.08f, 0.045f, 1.82f), skiColor, Vector3.zero);
        AddBodyPart(ski, "Front Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0.72f), new Vector3(0.12f, 0.045f, 0.12f), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, -0.66f), new Vector3(0.12f, 0.045f, 0.12f), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Boot", PrimitiveType.Cube, new Vector3(0f, 0.16f, 0.1f), new Vector3(0.13f, 0.13f, 0.32f), new Color(0.04f, 0.04f, 0.045f), Vector3.zero);
        return ski;
    }

    private static Transform CreateArm(Transform parent, string name, Vector3 localPosition, float side, Color suitColor, Color skinColor)
    {
        var armPivot = CreateChild(parent, name, localPosition);
        AddBodyPart(armPivot, "Upper Arm", PrimitiveType.Capsule, new Vector3(0.02f * side, -0.22f, 0.08f), new Vector3(0.095f, 0.29f, 0.095f), suitColor, new Vector3(22f, 0f, 8f * side));
        AddBodyPart(armPivot, "Forearm", PrimitiveType.Capsule, new Vector3(0.08f * side, -0.48f, 0.22f), new Vector3(0.08f, 0.32f, 0.08f), suitColor, new Vector3(36f, 0f, -12f * side));
        AddBodyPart(armPivot, "Hand", PrimitiveType.Sphere, new Vector3(0.13f * side, -0.72f, 0.39f), new Vector3(0.11f, 0.11f, 0.11f), skinColor, Vector3.zero);
        return armPivot;
    }

    private static Transform CreatePole(Transform parent, string name, Vector3 localPosition, float side)
    {
        var polePivot = CreateChild(parent, name, localPosition);
        AddBodyPart(polePivot, "Pole Shaft", PrimitiveType.Cylinder, new Vector3(0.06f * side, -0.48f, 0.25f), new Vector3(0.025f, 0.9f, 0.025f), new Color(0.04f, 0.04f, 0.045f), new Vector3(26f, 0f, -8f * side));
        AddBodyPart(polePivot, "Pole Tip", PrimitiveType.Sphere, new Vector3(0.2f * side, -1.02f, 0.58f), new Vector3(0.055f, 0.055f, 0.055f), new Color(0.02f, 0.02f, 0.025f), Vector3.zero);
        return polePivot;
    }

    private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent, false);
        child.localPosition = localPosition;
        return child;
    }

    private static Transform AddBodyPart(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color, Vector3 localRotation)
    {
        var part = GameObject.CreatePrimitive(primitiveType);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localRotation);
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material.color = color;
        return part.transform;
    }

    private static void CreateCamera(Transform target, PlayerSpeedController player)
    {
        var cameraObject = new GameObject("Follow Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        cameraObject.transform.position = target.position + target.TransformDirection(new Vector3(0f, 3.8f, -8.2f));
        cameraObject.transform.LookAt(target.position + Vector3.up * 1.1f + target.forward * 12f);

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
        text.textWrappingMode = TextWrappingModes.NoWrap;
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

    private static Quaternion HorizontalRotationAtDistance(float zPosition)
    {
        var direction = CoursePath.DirectionAtDistance(zPosition);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
