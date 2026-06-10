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
    public const float LakeNearOffset = 58f;
    public const float LakeFarOffset = 82f;
    public const float LakeFootprintRadius = 26f;
    private static readonly Color WaterColor = new Color(0.18f, 0.48f, 0.68f, 0.88f);
    private static readonly Color ShoreColor = new Color(0.22f, 0.58f, 0.28f, 1f);
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

            EnsureRoadsideGrassLayer(existing.transform);
            return existing;
        }

        var root = new GameObject(LandscapeRootName);
        root.AddComponent<NordicLandscapeRuntimeUpdater>();
        CreateLakes(root.transform);
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

        CreateLake(lakes.transform, "Left Roadside Lake", 640f, -1f, new Vector2(34f, 20f), 8f);
        CreateLake(lakes.transform, "Right Forest Lake", 1660f, 1f, new Vector2(42f, 24f), -12f);
        CreateLake(lakes.transform, "Left Open Glade Lake", 2480f, -1f, new Vector2(30f, 18f), 18f);
    }

    private static void CreateLake(Transform parent, string name, float distanceMeters, float side, Vector2 size, float forwardOffset)
    {
        var lake = new GameObject(name);
        lake.transform.SetParent(parent, false);
        lake.transform.position = CalculateLakePosition(distanceMeters, side, Mathf.Max(size.x, size.y) * 0.5f);
        lake.transform.rotation = CoursePath.RotationAtDistance(distanceMeters);

        AddLakeDisc(lake.transform, "Grass Shore", new Vector3(0f, -0.02f, forwardOffset), new Vector3(size.x * 1.15f, 0.04f, size.y * 1.15f), ShoreColor);
        AddLakeDisc(lake.transform, "Calm Blue Lake", new Vector3(0f, 0.015f, forwardOffset), new Vector3(size.x, 0.035f, size.y), WaterColor);
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
