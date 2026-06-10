using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkiErgGameBootstrap : MonoBehaviour
{
    public const float DefaultDistantForestSpacingMeters = 48f;
    public const float DefaultRockClusterSpacingMeters = 92f;
    public const float DefaultTreeSpacingMeters = 28f;
    public const float CountryRoadCenterDashLengthMeters = 9f;
    public const float CountryRoadCenterDashWidthMeters = 0.16f;
    public const float CountryRoadCenterDashGapMeters = 23f;
    public const float CountryRoadEdgeLineWidthMeters = 0.12f;

    private const float RoadLengthMeters = CoursePath.CourseLengthMeters;
    private const float RoadWidthMeters = 8f;
    private const float SkierVisualScale = 1.18f;
    public static readonly Color RoadAsphaltColor = new Color(0.105f, 0.12f, 0.13f);
    public static readonly Color RoadMarkingColor = Color.white;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildPrototypeScene()
    {
        using (StartupPerformanceProfiler.Measure("SkiErgGameBootstrap.BuildPrototypeScene"))
        {
            if (Object.FindObjectOfType<PlayerSpeedController>() != null)
            {
                StartupPerformanceProfiler.Log("SkiErgGameBootstrap skipped because player already exists");
                return;
            }

            CreateEnvironment();
            GameObject player;
            using (StartupPerformanceProfiler.Measure("CreateSkier"))
            {
                player = CreateSkier();
            }

            using (StartupPerformanceProfiler.Measure("CreateCamera"))
            {
                CreateCamera(player.transform, player.GetComponent<PlayerSpeedController>());
            }

            using (StartupPerformanceProfiler.Measure("CreateLight"))
            {
                CreateLight();
            }

            using (StartupPerformanceProfiler.Measure("CreateHud"))
            {
                CreateHud(player.GetComponent<PlayerSpeedController>());
            }
        }
    }

    private static void CreateEnvironment()
    {
        using (StartupPerformanceProfiler.Measure("CreateEnvironment"))
        {
            var environmentSettings = NordicEnvironmentSettings.GetOrCreateRuntimeSettings();
            var hasBakedEnvironment = BakedNordicEnvironmentMarker.HasBakedEnvironment();

            using (StartupPerformanceProfiler.Measure("CreateRoad")) CreateRoad();
            using (StartupPerformanceProfiler.Measure("CreateRoadShoulders")) CreateRoadShoulders();
            using (StartupPerformanceProfiler.Measure("CreateRoadMarkings")) CreateRoadMarkings();
            using (StartupPerformanceProfiler.Measure("KilometerMarkerSignRuntimeUpdater.EnsureKilometerMarkers")) KilometerMarkerSignRuntimeUpdater.EnsureKilometerMarkers();
            using (StartupPerformanceProfiler.Measure("CreateTurnSigns")) CreateTurnSigns();
            using (StartupPerformanceProfiler.Measure("CreateStartFinishMarkers")) CreateStartFinishMarkers();

            if (hasBakedEnvironment)
            {
                StartupPerformanceProfiler.Log("Skipping procedural Nordic environment decorations because baked environment exists");
                return;
            }

            using (StartupPerformanceProfiler.Measure("CreateNordicEnvironmentDecorations")) CreateNordicEnvironmentDecorations(environmentSettings, null);
        }
    }

    public static GameObject CreateNordicEnvironmentDecorations(NordicEnvironmentSettings settings, Transform parent)
    {
        var root = parent != null ? parent.gameObject : null;
        ParentIfNeeded(CreateGrass(), parent);
        ParentIfNeeded(CreateDistantForests(settings), parent);
        ParentIfNeeded(CreateRockClusters(settings), parent);
        ParentIfNeeded(CreateDistantMountains(settings), parent);
        ParentIfNeeded(CreateTrees(settings), parent);
        var nordicLandscape = parent != null
            ? NordicLandscapeRuntimeUpdater.CreateNordicLandscapeAtmosphere(parent, settings)
            : NordicLandscapeRuntimeUpdater.EnsureNordicLandscapeAtmosphere(settings);
        ParentIfNeeded(nordicLandscape, parent);
        return root;
    }

    private static void CreateRoad()
    {
        var road = new GameObject("Sweeping 3 km Loop Road");
        var meshFilter = road.AddComponent<MeshFilter>();
        meshFilter.mesh = LoopRoadMeshBuilder.CreateRoadMesh(RoadWidthMeters);
        road.AddComponent<MeshRenderer>().material.color = RoadAsphaltColor;
    }

    private static void CreateRoadShoulders()
    {
        var shoulders = new GameObject("Roadside Embankment Shoulders");
        var left = CreateRoadShoulder(shoulders.transform, "Left Road Shoulder", -1f);
        var right = CreateRoadShoulder(shoulders.transform, "Right Road Shoulder", 1f);
        var color = EnvironmentGroundRenderingCleanup.RoadShoulderGrassColor;
        left.GetComponent<MeshRenderer>().material.color = color;
        right.GetComponent<MeshRenderer>().material.color = color;
    }

    private static GameObject CreateRoadShoulder(Transform parent, string name, float side)
    {
        var shoulder = new GameObject(name);
        shoulder.transform.SetParent(parent, false);
        var meshFilter = shoulder.AddComponent<MeshFilter>();
        meshFilter.mesh = RoadShoulderMeshBuilder.CreateShoulderMesh(side);
        shoulder.AddComponent<MeshRenderer>();
        return shoulder;
    }

    private static GameObject CreateGrass()
    {
        var grass = new GameObject("Continuous Green Roadside Ground");
        var color = EnvironmentGroundRenderingCleanup.OpenTerrainGrassColor;
        var coverage = CreateCourseGroundCoverage(grass.transform);
        var left = CreateRoadsideGround(grass.transform, "Left Continuous Grass Terrain", -1f);
        var right = CreateRoadsideGround(grass.transform, "Right Continuous Grass Terrain", 1f);
        coverage.GetComponent<MeshRenderer>().material.color = color;
        left.GetComponent<MeshRenderer>().material.color = color;
        right.GetComponent<MeshRenderer>().material.color = color;
        return grass;
    }

    private static GameObject CreateCourseGroundCoverage(Transform parent)
    {
        var coverage = new GameObject("Full Course Green Ground Coverage");
        coverage.transform.SetParent(parent, false);
        var meshFilter = coverage.AddComponent<MeshFilter>();
        meshFilter.mesh = CourseGroundCoverageMeshBuilder.CreateCoverageMesh();
        coverage.AddComponent<MeshRenderer>();
        return coverage;
    }

    private static GameObject CreateRoadsideGround(Transform parent, string name, float side)
    {
        var ground = new GameObject(name);
        ground.transform.SetParent(parent, false);
        var meshFilter = ground.AddComponent<MeshFilter>();
        meshFilter.mesh = RoadsideGroundMeshBuilder.CreateGroundMesh(side);
        ground.AddComponent<MeshRenderer>();
        return ground;
    }

    private static void CreateRoadMarkings()
    {
        RemoveExistingObjectsNamed("Road Markings");

        var markings = new GameObject("Road Markings");
        var edgeLeft = -RoadWidthMeters * 0.5f + 0.35f;
        var edgeRight = RoadWidthMeters * 0.5f - 0.35f;

        CreateRoadMarkingMesh(
            markings.transform,
            "Left Curved Edge Line",
            RoadMarkingMeshBuilder.CreateSolidLineMesh(edgeLeft, CountryRoadEdgeLineWidthMeters));
        CreateRoadMarkingMesh(
            markings.transform,
            "Right Curved Edge Line",
            RoadMarkingMeshBuilder.CreateSolidLineMesh(edgeRight, CountryRoadEdgeLineWidthMeters));
        CreateRoadMarkingMesh(
            markings.transform,
            "Curved Center Dashes",
            RoadMarkingMeshBuilder.CreateDashedLineMesh(
                0f,
                CountryRoadCenterDashWidthMeters,
                CountryRoadCenterDashLengthMeters,
                CountryRoadCenterDashGapMeters,
                14f));
    }

    private static void CreateRoadMarkingMesh(Transform parent, string name, Mesh mesh)
    {
        var marking = new GameObject(name);
        marking.transform.SetParent(parent, false);
        marking.AddComponent<MeshFilter>().mesh = mesh;
        marking.AddComponent<MeshRenderer>().material.color = RoadMarkingColor;
    }

    private static void RemoveExistingObjectsNamed(string objectName)
    {
        var existingObjects = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

        for (var index = 0; index < existingObjects.Length; index++)
        {
            var existing = existingObjects[index];

            if (existing != null && existing.name == objectName)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(existing.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(existing.gameObject);
                }
            }
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

    private static void CreateEnvironmentPathCube(Transform parent, string name, float lateralOffset, float zPosition, float length, float width, float height, Color color, float yPosition = 0f)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        var position = EnvironmentPlacement.SafePointAtDistance(zPosition, lateralOffset, CalculateFootprintRadius(width, length));
        position.y += yPosition;
        cube.transform.position = position;
        cube.transform.rotation = CoursePath.RotationAtDistance(zPosition);
        cube.transform.localScale = new Vector3(width, height, length);
        cube.GetComponent<Renderer>().material.color = color;
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
        var position = EnvironmentPlacement.SafePointAtDistance(zPosition, lateralOffset, 0.6f);
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
        StartFinishPortalRuntimeUpdater.EnsureStartFinishPortal();
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
        var frontTriangles = new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1, 1, 4, 3, 1, 3, 2 };
        var mesh = new Mesh
        {
            name = "Solid Low Poly Mound Mesh",
            vertices = BuildDoubleSidedVertices(vertices),
            triangles = BuildDoubleSidedTriangles(frontTriangles, vertices.Length)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Vector3[] BuildDoubleSidedVertices(Vector3[] frontVertices)
    {
        var vertices = new Vector3[frontVertices.Length * 2];
        frontVertices.CopyTo(vertices, 0);
        frontVertices.CopyTo(vertices, frontVertices.Length);
        return vertices;
    }

    private static int[] BuildDoubleSidedTriangles(int[] frontTriangles, int backfaceVertexOffset)
    {
        var triangles = new int[frontTriangles.Length * 2];
        frontTriangles.CopyTo(triangles, 0);

        for (var index = 0; index < frontTriangles.Length; index += 3)
        {
            var reverseIndex = frontTriangles.Length + index;
            triangles[reverseIndex] = frontTriangles[index] + backfaceVertexOffset;
            triangles[reverseIndex + 1] = frontTriangles[index + 2] + backfaceVertexOffset;
            triangles[reverseIndex + 2] = frontTriangles[index + 1] + backfaceVertexOffset;
        }

        return triangles;
    }

    private static GameObject CreateDistantForests(NordicEnvironmentSettings settings)
    {
        var forests = new GameObject("Nordic Distant Forest Bands");
        var trunkColor = new Color(0.23f, 0.14f, 0.08f);
        var crownColor = new Color(0.035f, 0.19f, 0.09f);
        var forestSpacing = settings != null ? settings.EffectiveDistantForestSpacingMeters : DefaultDistantForestSpacingMeters;
        var nearForestOffset = settings != null ? settings.EffectiveNearForestOffset : EnvironmentPlacement.NearForestOffset;
        var farForestOffset = settings != null ? settings.EffectiveFarForestOffset : EnvironmentPlacement.FarForestOffset;
        var midForestOffset = settings != null ? settings.EffectiveMidForestOffset : EnvironmentPlacement.MidForestOffset;
        var highForestOffset = settings != null ? settings.EffectiveHighForestOffset : EnvironmentPlacement.HighForestOffset;

        for (var z = 38f; z < RoadLengthMeters; z += forestSpacing)
        {
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z, -nearForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 0.92f + Mathf.PingPong(z * 0.011f, 0.28f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 17f, nearForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 0.88f + Mathf.PingPong(z * 0.013f, 0.28f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 31f, -farForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.12f + Mathf.PingPong(z * 0.009f, 0.34f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 48f, farForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.08f + Mathf.PingPong(z * 0.015f, 0.34f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 21f, -midForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.18f + Mathf.PingPong(z * 0.012f, 0.34f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 29f, midForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.14f + Mathf.PingPong(z * 0.014f, 0.32f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 8f, -highForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.28f + Mathf.PingPong(z * 0.01f, 0.36f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 39f, highForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.24f + Mathf.PingPong(z * 0.016f, 0.34f), trunkColor, crownColor);
        }

        return forests;
    }

    private static void CreateConifer(Transform parent, Vector3 position, float scale, Color trunkColor, Color crownColor)
    {
        if (StarterPackEnvironmentAssets.TryCreatePine(parent, position, scale, position.x + position.z, out _))
        {
            return;
        }

        var tree = new GameObject("Distant Conifer");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 0.48f * scale, 0f);
        trunk.transform.localScale = new Vector3(0.12f * scale, 0.48f * scale, 0.12f * scale);
        trunk.GetComponent<Renderer>().material.color = trunkColor;

        var crown = new GameObject("Low Poly Crown");
        crown.transform.SetParent(tree.transform, false);
        crown.transform.localPosition = new Vector3(0f, 1.3f * scale, 0f);
        crown.transform.localScale = new Vector3(1.15f * scale, 1.55f * scale, 1.15f * scale);
        var meshFilter = crown.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMoundMesh();
        crown.AddComponent<MeshRenderer>().material.color = crownColor;
    }

    private static GameObject CreateRockClusters(NordicEnvironmentSettings settings)
    {
        var rocks = new GameObject("Nordic Starter Pack Rocks");
        var rockSpacing = settings != null ? settings.EffectiveRockSpacingMeters : DefaultRockClusterSpacingMeters;
        var midTreeOffset = settings != null ? settings.EffectiveMidTreeOffset : EnvironmentPlacement.MidTreeOffset;
        var farTreeOffset = settings != null ? settings.EffectiveFarTreeOffset : EnvironmentPlacement.FarTreeOffset;

        for (var z = 70f; z < RoadLengthMeters; z += rockSpacing)
        {
            CreateRockCluster(rocks.transform, EnvironmentPlacement.SafePointAtDistance(z, -midTreeOffset - 6f, 2.8f), 0.8f + Mathf.PingPong(z * 0.021f, 0.45f));
            CreateRockCluster(rocks.transform, EnvironmentPlacement.SafePointAtDistance(z + 47f, farTreeOffset + 5f, 3.2f), 0.9f + Mathf.PingPong(z * 0.017f, 0.5f));
        }

        return rocks;
    }

    private static void CreateRockCluster(Transform parent, Vector3 position, float scale)
    {
        if (StarterPackEnvironmentAssets.TryCreateRock(parent, position, scale, position.x * 0.37f + position.z * 0.19f, out _))
        {
            return;
        }

        var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Fallback Low Poly Rock";
        rock.transform.SetParent(parent, false);
        rock.transform.position = position;
        rock.transform.localScale = new Vector3(0.8f * scale, 0.38f * scale, 0.6f * scale);
        rock.GetComponent<Renderer>().material.color = new Color(0.28f, 0.3f, 0.31f);
    }

    private static GameObject CreateDistantMountains(NordicEnvironmentSettings settings)
    {
        var mountains = new GameObject("Nordic Mountain Ranges");
        MountainRangeSceneUpdater.BuildMountainRanges(mountains.transform, settings);
        return mountains;
    }

    private static GameObject CreateTrees(NordicEnvironmentSettings settings)
    {
        var trees = new GameObject("Set Back Roadside Trees");
        var treeSpacing = settings != null ? settings.EffectiveBootstrapTreeSpacingMeters : DefaultTreeSpacingMeters;
        var nearTreeOffset = settings != null ? settings.EffectiveNearTreeOffset : EnvironmentPlacement.NearTreeOffset;
        var midTreeOffset = settings != null ? settings.EffectiveMidTreeOffset : EnvironmentPlacement.MidTreeOffset;
        var farTreeOffset = settings != null ? settings.EffectiveFarTreeOffset : EnvironmentPlacement.FarTreeOffset;

        for (var z = 18f; z < RoadLengthMeters; z += treeSpacing)
        {
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z, -nearTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.66f + Mathf.PingPong(z * 0.013f, 0.28f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 11f, nearTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.66f + Mathf.PingPong(z * 0.017f, 0.3f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 18f, -midTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.76f + Mathf.PingPong(z * 0.011f, 0.34f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 25f, midTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.74f + Mathf.PingPong(z * 0.019f, 0.34f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 34f, -farTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.88f + Mathf.PingPong(z * 0.009f, 0.36f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 43f, farTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.86f + Mathf.PingPong(z * 0.015f, 0.38f));
        }

        return trees;
    }

    private static void CreateTree(Transform parent, Vector3 position, float scale)
    {
        if (StarterPackEnvironmentAssets.TryCreateMixedTree(parent, position, scale * 0.9f, position.x * 0.23f + position.z * 0.41f, out _))
        {
            return;
        }

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
        controller.SetStartDistanceZ(0f);

        var animator = skier.AddComponent<RollerSkierAnimator>();
        animator.player = controller;
        var visualRoot = CreateChild(skier.transform, "Roller Skier Visual", Vector3.zero);
        visualRoot.localScale = Vector3.one * SkierVisualScale;
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
        cameraObject.transform.position = target.position + target.TransformDirection(FollowCamera.FocusedPlayerOffset);
        cameraObject.transform.LookAt(target.position + Vector3.up * FollowCamera.FocusedLookTargetHeight + target.forward * 14f);

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
        var lapText = CreateHudText(canvasObject.transform, "Lap Text", new Vector2(28f, -228f));

        var display = canvasObject.AddComponent<SpeedDistanceDisplay>();
        display.player = player;
        display.speedText = speedText;
        display.distanceText = distanceText;
        display.lapText = lapText;
        display.Refresh();

        using (StartupPerformanceProfiler.Measure("CourseMinimapDisplay.CreateRuntimeMinimap")) CourseMinimapDisplay.CreateRuntimeMinimap(canvasObject.transform, player);
        using (StartupPerformanceProfiler.Measure("CourseElevationProfileDisplay.CreateRuntimeProfile")) CourseElevationProfileDisplay.CreateRuntimeProfile(canvasObject.transform, player);
        using (StartupPerformanceProfiler.Measure("StrokeMetricsDisplay.CreateRuntimeStrokeHud")) StrokeMetricsDisplay.CreateRuntimeStrokeHud(canvasObject.transform, player);
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

    private static float CalculateFootprintRadius(float width, float length)
    {
        return Mathf.Sqrt(width * width + length * length) * 0.5f;
    }

    private static void ParentIfNeeded(GameObject child, Transform parent)
    {
        if (child != null && parent != null && child.transform.parent != parent)
        {
            child.transform.SetParent(parent, true);
        }
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
