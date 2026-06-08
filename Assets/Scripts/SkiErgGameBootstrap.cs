using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkiErgGameBootstrap : MonoBehaviour
{
    private const float RoadLengthMeters = CoursePath.CourseLengthMeters;
    private const float RoadWidthMeters = 8f;
    private const float GrassWidthMeters = 18f;
    private const float RoadSegmentLength = 12f;
    private const float SkierVisualScale = 1.18f;

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
        CreateRoadShoulders();
        CreateGrass();
        CreateRoadMarkings();
        CreateRoadsidePosts();
        CreateTurnSigns();
        CreateStartFinishMarkers();
        CreateRollingHills();
        CreateDistantForests();
        CreateRockClusters();
        CreateDistantMountains();
        CreateTrees();
    }

    private static void CreateRoad()
    {
        var road = new GameObject("Sweeping 3 km Loop Road");
        var color = new Color(0.16f, 0.18f, 0.2f);
        var meshFilter = road.AddComponent<MeshFilter>();
        meshFilter.mesh = LoopRoadMeshBuilder.CreateRoadMesh(RoadWidthMeters);
        road.AddComponent<MeshRenderer>().material.color = color;
    }

    private static void CreateRoadShoulders()
    {
        var shoulders = new GameObject("Roadside Embankment Shoulders");
        var left = CreateRoadShoulder(shoulders.transform, "Left Road Shoulder", -1f);
        var right = CreateRoadShoulder(shoulders.transform, "Right Road Shoulder", 1f);
        var color = new Color(0.2f, 0.48f, 0.18f);
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

    private static void CreateGrass()
    {
        var grass = new GameObject("Open Grass Shoulders");
        var color = new Color(0.18f, 0.55f, 0.18f);
        var lateralOffset = RoadWidthMeters * 0.5f + EnvironmentPlacement.OpenTerrainMargin + GrassWidthMeters * 0.5f;

        for (var z = 16f; z < RoadLengthMeters; z += 32f)
        {
            CreateEnvironmentPathCube(grass.transform, "Left Open Grass Segment", -lateralOffset, z, 33f, GrassWidthMeters, 0.08f, color, -0.08f);
            CreateEnvironmentPathCube(grass.transform, "Right Open Grass Segment", lateralOffset, z, 33f, GrassWidthMeters, 0.08f, color, -0.08f);
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

    private static void CreateRoadsidePosts()
    {
        var posts = new GameObject("Roadside Speed Posts");
        var leftX = -(RoadWidthMeters * 0.5f + EnvironmentPlacement.OpenTerrainMargin + 0.9f);
        var rightX = RoadWidthMeters * 0.5f + EnvironmentPlacement.OpenTerrainMargin + 0.9f;

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
        var position = EnvironmentPlacement.SafePointAtDistance(zPosition, lateralOffset, 0.18f);
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
        CreateGate("Start Finish Gate", 0f, new Color(0.1f, 0.45f, 0.95f));
        CreatePathCube(null, "Start Finish Line", 0f, 1f, 0.4f, RoadWidthMeters, 0.05f, Color.white, 0.04f);
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
        var hills = new GameObject("Open Jamtland Rolling Hills");
        var nearHillColor = new Color(0.2f, 0.5f, 0.18f);
        var farHillColor = new Color(0.15f, 0.4f, 0.17f);

        for (var z = 110f; z < RoadLengthMeters; z += 185f)
        {
            CreateLowPolyHill(hills.transform, EnvironmentPlacement.SafePointAtDistance(z, -EnvironmentPlacement.NearHillOffset, CalculateFootprintRadius(EnvironmentPlacement.NearHillHalfWidth * 2f, 150f)), new Vector3(EnvironmentPlacement.NearHillHalfWidth * 2f, 8f, 150f), nearHillColor);
            CreateLowPolyHill(hills.transform, EnvironmentPlacement.SafePointAtDistance(z + 62f, EnvironmentPlacement.NearHillOffset, CalculateFootprintRadius(EnvironmentPlacement.NearHillHalfWidth * 2f, 145f)), new Vector3(EnvironmentPlacement.NearHillHalfWidth * 2f, 7.5f, 145f), nearHillColor);
            CreateLowPolyHill(hills.transform, EnvironmentPlacement.SafePointAtDistance(z + 116f, -EnvironmentPlacement.FarHillOffset, CalculateFootprintRadius(EnvironmentPlacement.FarHillHalfWidth * 2f, 190f)), new Vector3(EnvironmentPlacement.FarHillHalfWidth * 2f, 11f, 190f), farHillColor);
            CreateLowPolyHill(hills.transform, EnvironmentPlacement.SafePointAtDistance(z + 155f, EnvironmentPlacement.FarHillOffset, CalculateFootprintRadius(EnvironmentPlacement.FarHillHalfWidth * 2f, 200f)), new Vector3(EnvironmentPlacement.FarHillHalfWidth * 2f, 12f, 200f), farHillColor);
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

    private static void CreateDistantForests()
    {
        var forests = new GameObject("Nordic Distant Forest Bands");
        var trunkColor = new Color(0.23f, 0.14f, 0.08f);
        var crownColor = new Color(0.035f, 0.19f, 0.09f);

        for (var z = 38f; z < RoadLengthMeters; z += 48f)
        {
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z, -EnvironmentPlacement.NearForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.25f + Mathf.PingPong(z * 0.011f, 0.45f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 17f, EnvironmentPlacement.NearForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.2f + Mathf.PingPong(z * 0.013f, 0.45f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 31f, -EnvironmentPlacement.FarForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.45f + Mathf.PingPong(z * 0.009f, 0.5f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 48f, EnvironmentPlacement.FarForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.4f + Mathf.PingPong(z * 0.015f, 0.5f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 21f, -EnvironmentPlacement.MidForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.55f + Mathf.PingPong(z * 0.012f, 0.5f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 29f, EnvironmentPlacement.MidForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.5f + Mathf.PingPong(z * 0.014f, 0.45f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 8f, -EnvironmentPlacement.HighForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.7f + Mathf.PingPong(z * 0.01f, 0.55f), trunkColor, crownColor);
            CreateConifer(forests.transform, EnvironmentPlacement.SafePointAtDistance(z + 39f, EnvironmentPlacement.HighForestOffset, EnvironmentPlacement.MaxForestTreeRadius), 1.65f + Mathf.PingPong(z * 0.016f, 0.5f), trunkColor, crownColor);
        }
    }

    private static void CreateConifer(Transform parent, Vector3 position, float scale, Color trunkColor, Color crownColor)
    {
        if (StarterPackEnvironmentAssets.TryCreatePine(parent, position, scale * 1.35f, position.x + position.z, out _))
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

    private static void CreateRockClusters()
    {
        var rocks = new GameObject("Nordic Starter Pack Rocks");

        for (var z = 70f; z < RoadLengthMeters; z += 92f)
        {
            CreateRockCluster(rocks.transform, EnvironmentPlacement.SafePointAtDistance(z, -EnvironmentPlacement.MidTreeOffset - 6f, 2.8f), 0.8f + Mathf.PingPong(z * 0.021f, 0.45f));
            CreateRockCluster(rocks.transform, EnvironmentPlacement.SafePointAtDistance(z + 47f, EnvironmentPlacement.FarTreeOffset + 5f, 3.2f), 0.9f + Mathf.PingPong(z * 0.017f, 0.5f));
        }
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

    private static void CreateDistantMountains()
    {
        var mountains = new GameObject("Nordic Mountain Ranges");
        var nearColor = new Color(0.34f, 0.4f, 0.46f);
        var farColor = new Color(0.48f, 0.53f, 0.58f);

        for (var z = EnvironmentPlacement.MountainFirstDistance; z <= RoadLengthMeters; z += EnvironmentPlacement.MountainSpacing)
        {
            CreateLowPolyMountain(mountains.transform, EnvironmentPlacement.SafePointAtDistance(z, -EnvironmentPlacement.NearMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2f, 310f)), new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2f, 170f * EnvironmentPlacement.MountainHeightScale, 310f), nearColor);
            CreateLowPolyMountain(mountains.transform, EnvironmentPlacement.SafePointAtDistance(z + 120f, EnvironmentPlacement.NearMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2.1f, 320f)), new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2.1f, 185f * EnvironmentPlacement.MountainHeightScale, 320f), nearColor);
            CreateLowPolyMountain(mountains.transform, EnvironmentPlacement.SafePointAtDistance(z + 240f, -EnvironmentPlacement.FarMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2f, 390f)), new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2f, 220f * EnvironmentPlacement.MountainHeightScale, 390f), farColor);
            CreateLowPolyMountain(mountains.transform, EnvironmentPlacement.SafePointAtDistance(z + 360f, EnvironmentPlacement.FarMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2.05f, 405f)), new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2.05f, 235f * EnvironmentPlacement.MountainHeightScale, 405f), farColor);
        }
    }

    private static void CreateLowPolyMountain(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var mountain = new GameObject("Low Poly Mountain");
        mountain.transform.SetParent(parent, false);
        position.y += -8f;
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
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z, -EnvironmentPlacement.NearTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.85f + Mathf.PingPong(z * 0.013f, 0.5f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 11f, EnvironmentPlacement.NearTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.85f + Mathf.PingPong(z * 0.017f, 0.55f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 18f, -EnvironmentPlacement.MidTreeOffset, EnvironmentPlacement.MaxTreeRadius), 1f + Mathf.PingPong(z * 0.011f, 0.6f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 25f, EnvironmentPlacement.MidTreeOffset, EnvironmentPlacement.MaxTreeRadius), 0.95f + Mathf.PingPong(z * 0.019f, 0.6f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 34f, -EnvironmentPlacement.FarTreeOffset, EnvironmentPlacement.MaxTreeRadius), 1.2f + Mathf.PingPong(z * 0.009f, 0.55f));
            CreateTree(trees.transform, EnvironmentPlacement.SafePointAtDistance(z + 43f, EnvironmentPlacement.FarTreeOffset, EnvironmentPlacement.MaxTreeRadius), 1.15f + Mathf.PingPong(z * 0.015f, 0.65f));
        }
    }

    private static void CreateTree(Transform parent, Vector3 position, float scale)
    {
        if (StarterPackEnvironmentAssets.TryCreateMixedTree(parent, position, scale * 1.15f, position.x * 0.23f + position.z * 0.41f, out _))
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

        CourseMinimapDisplay.CreateRuntimeMinimap(canvasObject.transform, player);
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
