using UnityEngine;

[DisallowMultipleComponent]
public class NordicLandscapeRuntimeUpdater : MonoBehaviour
{
    public const string LandscapeRootName = "Nordic Landscape Atmosphere";
    public const float NearTreeLineOffset = EnvironmentPlacement.RoadHalfWidth + 4.35f;
    public const float MidTreeLineOffset = 15.5f;
    public const float ForestTreeSpacingMeters = 24f;
    public const float RoadsideGrassInnerOffset = EnvironmentPlacement.ShoulderOuterOffset + 0.35f;
    public const float RoadsideGrassOuterOffset = EnvironmentPlacement.RoadHalfWidth + 4.05f;
    public const float RoadsideGrassFootprintRadius = 0.22f;
    public const float RoadsideGrassClusterSpacingMeters = 14f;
    public const float SignVisibilityClearanceMeters = 42f;
    public const float LakeNearOffset = 42f;
    public const float LakeFarOffset = 70f;
    public const float LakeFootprintRadius = 34f;
    public const float StreamFootprintRadius = 0.6f;
    public const float BridgeVisualHeight = 0.13f;
    private static readonly Color WaterColor = new Color(0.13f, 0.52f, 0.78f, 0.94f);
    private static readonly Color StreamColor = new Color(0.11f, 0.44f, 0.68f, 0.96f);
    private static readonly Color ShoreColor = new Color(0.22f, 0.58f, 0.28f, 1f);
    private static readonly Color WetlandGrassColor = new Color(0.12f, 0.42f, 0.12f, 1f);
    private static readonly Color BridgeWoodColor = new Color(0.31f, 0.19f, 0.1f, 1f);
    private static readonly Color FallbackTrunkColor = new Color(0.33f, 0.2f, 0.1f, 1f);
    private static readonly Color FallbackCrownColor = new Color(0.07f, 0.35f, 0.12f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallNordicLandscapeAtmosphere()
    {
        EnsureNordicLandscapeAtmosphere();
    }

    public static GameObject EnsureNordicLandscapeAtmosphere()
    {
        var existing = GameObject.Find(LandscapeRootName);
        if (existing != null)
        {
            if (existing.GetComponent<NordicLandscapeRuntimeUpdater>() == null)
            {
                existing.AddComponent<NordicLandscapeRuntimeUpdater>();
            }

            EnsureWaterFeatureLayers(existing.transform);
            EnsureRoadsideGrassLayer(existing.transform);
            return existing;
        }

        var root = new GameObject(LandscapeRootName);
        root.AddComponent<NordicLandscapeRuntimeUpdater>();
        EnsureWaterFeatureLayers(root.transform);
        CreateRoadsideGrassClusters(root.transform);
        CreateCloserForestBands(root.transform);
        return root;
    }

    public static Vector3 CalculateLakePosition(float distanceMeters, float side, float footprintRadius)
    {
        var sideSign = side < 0f ? -1f : 1f;
        var offset = sideSign * (side < 0f ? LakeNearOffset : LakeFarOffset);
        return EnvironmentPlacement.SafePointAtDistance(distanceMeters, offset, footprintRadius);
    }

    public static bool IsForestOpening(float distanceMeters)
    {
        var distance = CoursePath.NormalizeDistance(distanceMeters);
        return IsWithin(distance, 720f, 860f)
            || IsWithin(distance, 1420f, 1570f)
            || IsWithin(distance, 2220f, 2360f);
    }

    private static bool IsWithin(float value, float start, float end)
    {
        return value >= start && value <= end;
    }

    private static void CreateLakes(Transform parent)
    {
        var lakes = new GameObject("Jamtland Lakes");
        lakes.transform.SetParent(parent, false);

        CreateLake(lakes.transform, "Left Roadside Lake", 640f, -1f, new Vector2(58f, 34f), 8f);
        CreateLake(lakes.transform, "Right Forest Lake", 1660f, 1f, new Vector2(68f, 38f), -12f);
        CreateLake(lakes.transform, "Left Open Glade Lake", 2480f, -1f, new Vector2(52f, 30f), 18f);
    }

    private static void CreateLake(Transform parent, string name, float distanceMeters, float side, Vector2 size, float forwardOffset)
    {
        var lake = new GameObject(name);
        lake.transform.SetParent(parent, false);
        lake.transform.position = CalculateLakePosition(distanceMeters, side, Mathf.Max(size.x, size.y) * 0.5f);
        lake.transform.rotation = CoursePath.RotationAtDistance(distanceMeters);

        AddLakeDisc(lake.transform, "Grass Shore", new Vector3(0f, -0.02f, forwardOffset), new Vector3(size.x * 1.15f, 0.04f, size.y * 1.15f), ShoreColor);
        AddLakeDisc(lake.transform, "Calm Blue Lake", new Vector3(0f, 0.015f, forwardOffset), new Vector3(size.x, 0.035f, size.y), WaterColor);
        AddLakeDisc(lake.transform, "Irregular Shore North", new Vector3(size.x * 0.14f, -0.01f, forwardOffset + size.y * 0.36f), new Vector3(size.x * 0.38f, 0.035f, size.y * 0.22f), ShoreColor);
        AddLakeDisc(lake.transform, "Irregular Shore South", new Vector3(-size.x * 0.18f, -0.01f, forwardOffset - size.y * 0.34f), new Vector3(size.x * 0.32f, 0.035f, size.y * 0.2f), ShoreColor);
        AddLakeDisc(lake.transform, "Visible Water Bay", new Vector3(-size.x * 0.22f, 0.018f, forwardOffset + size.y * 0.2f), new Vector3(size.x * 0.38f, 0.03f, size.y * 0.24f), WaterColor);
    }

    private static void AddLakeDisc(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = name;
        disc.transform.SetParent(parent, false);
        disc.transform.localPosition = localPosition;
        disc.transform.localScale = scale;
        disc.GetComponent<Renderer>().material.color = color;
        DisableCollider(disc);
    }

    private static void EnsureWaterFeatureLayers(Transform parent)
    {
        RebuildLayer(parent, "Jamtland Lakes", CreateLakes);
        RebuildLayer(parent, "Nordic Streams And Creeks", CreateStreamsAndCreeks);
        RebuildLayer(parent, "Water Feature Rocks And Vegetation", CreateWaterFeatureDetails);
    }

    private static void RebuildLayer(Transform parent, string layerName, System.Action<Transform> createLayer)
    {
        var existing = parent.Find(layerName);
        if (existing != null)
        {
            DestroyObject(existing.gameObject);
        }

        createLayer(parent);
    }

    private static void DestroyObject(GameObject gameObject)
    {
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public static Vector3 CalculateStreamPoint(float distanceMeters, float lateralOffset)
    {
        return EnvironmentPlacement.SafePointAtDistance(distanceMeters, lateralOffset, StreamFootprintRadius);
    }

    private static void CreateStreamsAndCreeks(Transform parent)
    {
        var streams = new GameObject("Nordic Streams And Creeks");
        streams.transform.SetParent(parent, false);

        CreateCrossRoadCreek(streams.transform, "Left Lake Inlet Creek", 610f, -LakeNearOffset, 24f);
        CreateForestCreek(streams.transform, "Right Forest Creek", 1600f, 1f, LakeFarOffset, 28f);
        CreateForestCreek(streams.transform, "Open Glade Creek", 2440f, -1f, LakeNearOffset, 22f);
    }

    private static void CreateCrossRoadCreek(Transform parent, string name, float distanceMeters, float lakeSideOffset, float sourceSideOffset)
    {
        var creek = new GameObject(name);
        creek.transform.SetParent(parent, false);

        var previous = CalculateStreamPoint(distanceMeters - 32f, sourceSideOffset);
        var offsets = new[] { 16f, 7.5f, 0f, -7.5f, lakeSideOffset * 0.5f, lakeSideOffset * 0.78f };
        for (var i = 0; i < offsets.Length; i++)
        {
            var next = CalculateStreamPoint(distanceMeters - 22f + i * 10f, offsets[i]);
            CreateWaterSegment(creek.transform, "Creek Water Segment", previous, next, 1.15f);
            previous = next;
        }

        CreateBridgeAt(creek.transform, distanceMeters);
    }

    private static void CreateForestCreek(Transform parent, string name, float distanceMeters, float side, float lakeOffset, float sourceExtraOffset)
    {
        var creek = new GameObject(name);
        creek.transform.SetParent(parent, false);

        var sideSign = side < 0f ? -1f : 1f;
        var previous = CalculateStreamPoint(distanceMeters - 26f, sideSign * (lakeOffset + sourceExtraOffset));
        for (var i = 0; i < 6; i++)
        {
            var offset = sideSign * Mathf.Lerp(lakeOffset + sourceExtraOffset, lakeOffset * 0.72f, i / 5f);
            var next = CalculateStreamPoint(distanceMeters - 18f + i * 9f, offset);
            CreateWaterSegment(creek.transform, "Creek Water Segment", previous, next, 0.9f);
            previous = next;
        }
    }

    private static void CreateWaterSegment(Transform parent, string name, Vector3 start, Vector3 end, float width)
    {
        var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = name;
        segment.transform.SetParent(parent, false);

        var midpoint = (start + end) * 0.5f;
        midpoint.y += 0.018f;
        var direction = end - start;
        direction.y = 0f;
        segment.transform.position = midpoint;
        segment.transform.rotation = direction.sqrMagnitude > 0.001f ? Quaternion.LookRotation(direction.normalized, Vector3.up) : Quaternion.identity;
        segment.transform.localScale = new Vector3(width, 0.028f, Mathf.Max(1f, direction.magnitude));
        segment.GetComponent<Renderer>().material.color = StreamColor;
        DisableCollider(segment);
    }

    private static void CreateBridgeAt(Transform parent, float distanceMeters)
    {
        var bridge = new GameObject("Small Timber Creek Bridge");
        bridge.transform.SetParent(parent, false);
        bridge.transform.position = CoursePath.CenterPointAtDistance(distanceMeters);
        bridge.transform.rotation = CoursePath.RotationAtDistance(distanceMeters);

        AddBridgePart(bridge.transform, "Bridge Deck", new Vector3(0f, BridgeVisualHeight, 0f), new Vector3(EnvironmentPlacement.RoadHalfWidth * 2.25f, 0.08f, 2.15f));
        AddBridgePart(bridge.transform, "Left Bridge Rail", new Vector3(-EnvironmentPlacement.RoadHalfWidth - 0.35f, BridgeVisualHeight + 0.35f, 0f), new Vector3(0.12f, 0.42f, 2.28f));
        AddBridgePart(bridge.transform, "Right Bridge Rail", new Vector3(EnvironmentPlacement.RoadHalfWidth + 0.35f, BridgeVisualHeight + 0.35f, 0f), new Vector3(0.12f, 0.42f, 2.28f));
    }

    private static void AddBridgePart(Transform parent, string name, Vector3 localPosition, Vector3 scale)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material.color = BridgeWoodColor;
        DisableCollider(part);
    }

    private static void CreateWaterFeatureDetails(Transform parent)
    {
        var details = new GameObject("Water Feature Rocks And Vegetation");
        details.transform.SetParent(parent, false);

        CreateWaterDetailCluster(details.transform, 640f, -1f, LakeNearOffset, 0);
        CreateWaterDetailCluster(details.transform, 1660f, 1f, LakeFarOffset, 1);
        CreateWaterDetailCluster(details.transform, 2480f, -1f, LakeNearOffset, 2);
        CreateWaterDetailCluster(details.transform, 610f, -1f, 18f, 3);
        CreateWaterDetailCluster(details.transform, 1600f, 1f, 50f, 4);
    }

    private static void CreateWaterDetailCluster(Transform parent, float distanceMeters, float side, float lateralOffset, int clusterIndex)
    {
        var sideSign = side < 0f ? -1f : 1f;
        for (var i = 0; i < 4; i++)
        {
            var distance = distanceMeters + Mathf.PingPong((clusterIndex + i) * 17.3f, 34f) - 17f;
            var offset = sideSign * (Mathf.Abs(lateralOffset) + 8f + Mathf.PingPong(i * 6.7f + distanceMeters * 0.01f, 14f));
            var position = EnvironmentPlacement.SafePointAtDistance(distance, offset, 1.1f);
            var scale = 0.5f + Mathf.PingPong(distance * 0.018f + i * 0.2f, 0.45f);

            if (i % 2 == 0 && StarterPackEnvironmentAssets.TryCreateRock(parent, position, scale, distance + i * 5f, out _))
            {
                continue;
            }

            CreateWetlandVegetation(parent, position, scale);
        }
    }

    private static void CreateWetlandVegetation(Transform parent, Vector3 position, float scale)
    {
        if (StarterPackEnvironmentAssets.TryCreateGrass(parent, position, scale * 0.75f, position.x + position.z, out _))
        {
            return;
        }

        var tuft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tuft.name = "Fallback Wetland Grass";
        tuft.transform.SetParent(parent, false);
        tuft.transform.position = position + new Vector3(0f, 0.12f * scale, 0f);
        tuft.transform.localScale = new Vector3(0.22f * scale, 0.24f * scale, 0.22f * scale);
        tuft.GetComponent<Renderer>().material.color = WetlandGrassColor;
        DisableCollider(tuft);
    }

    private static void CreateCloserForestBands(Transform parent)
    {
        var forests = new GameObject("Closer Nordic Tree Bands");
        forests.transform.SetParent(parent, false);

        for (var distance = 18f; distance < CoursePath.CourseLengthMeters; distance += ForestTreeSpacingMeters)
        {
            if (IsForestOpening(distance))
            {
                CreateGladeEdgeTrees(forests.transform, distance);
                continue;
            }

            CreateAtmosphereTree(forests.transform, distance, -NearTreeLineOffset, 0.88f);
            CreateAtmosphereTree(forests.transform, distance + 9f, NearTreeLineOffset, 0.9f);
            CreateAtmosphereTree(forests.transform, distance + 15f, -MidTreeLineOffset, 1.05f);
            CreateAtmosphereTree(forests.transform, distance + 21f, MidTreeLineOffset, 1.03f);
        }
    }

    public static bool ShouldSkipRoadsideGrass(float distanceMeters)
    {
        var distance = CoursePath.NormalizeDistance(distanceMeters);
        if (distance <= SignVisibilityClearanceMeters || distance >= CoursePath.CourseLengthMeters - SignVisibilityClearanceMeters)
        {
            return true;
        }

        for (var marker = KilometerMarkerSignRuntimeUpdater.MarkerSpacingMeters; marker <= CoursePath.CourseLengthMeters; marker += KilometerMarkerSignRuntimeUpdater.MarkerSpacingMeters)
        {
            if (Mathf.Abs(distance - marker) <= SignVisibilityClearanceMeters)
            {
                return true;
            }
        }

        return false;
    }

    public static Vector3 CalculateRoadsideGrassPosition(float distanceMeters, float side, int clusterIndex)
    {
        var sideSign = side < 0f ? -1f : 1f;
        var lateralJitter = Mathf.PingPong(distanceMeters * 0.071f + clusterIndex * 0.37f, RoadsideGrassOuterOffset - RoadsideGrassInnerOffset);
        var forwardJitter = Mathf.PingPong(distanceMeters * 0.19f + clusterIndex * 1.7f, 4.8f) - 2.4f;
        return EnvironmentPlacement.SafePointAtDistance(
            distanceMeters + forwardJitter,
            sideSign * (RoadsideGrassInnerOffset + lateralJitter),
            RoadsideGrassFootprintRadius);
    }

    private static void CreateRoadsideGrassClusters(Transform parent)
    {
        if (parent.Find("Roadside Starter Pack Grass") != null)
        {
            return;
        }

        var grasses = new GameObject("Roadside Starter Pack Grass");
        grasses.transform.SetParent(parent, false);

        for (var distance = 24f; distance < CoursePath.CourseLengthMeters; distance += RoadsideGrassClusterSpacingMeters)
        {
            if (ShouldSkipRoadsideGrass(distance))
            {
                continue;
            }

            CreateGrassCluster(grasses.transform, distance, -1f, 0);
            CreateGrassCluster(grasses.transform, distance + 5.5f, 1f, 1);

            if (Mathf.FloorToInt(distance / RoadsideGrassClusterSpacingMeters) % 3 != 0)
            {
                CreateGrassCluster(grasses.transform, distance + 8.5f, -1f, 2);
            }

            if (Mathf.FloorToInt(distance / RoadsideGrassClusterSpacingMeters) % 4 != 0)
            {
                CreateGrassCluster(grasses.transform, distance + 11.5f, 1f, 3);
            }
        }
    }

    private static void EnsureRoadsideGrassLayer(Transform parent)
    {
        if (parent.Find("Roadside Starter Pack Grass") == null)
        {
            CreateRoadsideGrassClusters(parent);
        }
    }

    private static void CreateGrassCluster(Transform parent, float distanceMeters, float side, int clusterIndex)
    {
        if (ShouldSkipRoadsideGrass(distanceMeters))
        {
            return;
        }

        var position = CalculateRoadsideGrassPosition(distanceMeters, side, clusterIndex);
        var scale = 0.58f + Mathf.PingPong(distanceMeters * 0.043f + clusterIndex * 0.19f, 0.42f);

        if (StarterPackEnvironmentAssets.TryCreateGrass(parent, position, scale, distanceMeters + side * 17f + clusterIndex * 3.1f, out _))
        {
            return;
        }

        CreateFallbackGrass(parent, position, scale);
    }

    private static void CreateFallbackGrass(Transform parent, Vector3 position, float scale)
    {
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "Fallback Roadside Grass";
        grass.transform.SetParent(parent, false);
        grass.transform.position = position + new Vector3(0f, 0.08f * scale, 0f);
        grass.transform.localScale = new Vector3(0.18f * scale, 0.16f * scale, 0.18f * scale);
        grass.GetComponent<Renderer>().material.color = new Color(0.12f, 0.46f, 0.08f, 1f);
        DisableCollider(grass);
    }

    private static void CreateGladeEdgeTrees(Transform parent, float distanceMeters)
    {
        CreateAtmosphereTree(parent, distanceMeters, -MidTreeLineOffset - 5f, 1.04f);
        CreateAtmosphereTree(parent, distanceMeters + 12f, MidTreeLineOffset + 5f, 1.02f);
    }

    private static void CreateAtmosphereTree(Transform parent, float distanceMeters, float lateralOffset, float scale)
    {
        var position = EnvironmentPlacement.SafePointAtDistance(distanceMeters, lateralOffset, EnvironmentPlacement.MaxTreeRadius);
        var variedScale = scale + Mathf.PingPong(distanceMeters * 0.017f + lateralOffset * 0.03f, 0.28f);

        if (StarterPackEnvironmentAssets.TryCreateMixedTree(parent, position, variedScale * 0.85f, distanceMeters + lateralOffset * 13f, out _))
        {
            return;
        }

        var tree = new GameObject("Fallback Nordic Tree");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 0.58f * variedScale, 0f);
        trunk.transform.localScale = new Vector3(0.16f * variedScale, 0.58f * variedScale, 0.16f * variedScale);
        trunk.GetComponent<Renderer>().material.color = FallbackTrunkColor;
        DisableCollider(trunk);

        var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform, false);
        crown.transform.localPosition = new Vector3(0f, 1.55f * variedScale, 0f);
        crown.transform.localScale = new Vector3(1.05f * variedScale, 1.32f * variedScale, 1.05f * variedScale);
        crown.GetComponent<Renderer>().material.color = FallbackCrownColor;
        DisableCollider(crown);
    }

    private static void DisableCollider(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }
    }
}
