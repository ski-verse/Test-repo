using UnityEngine;

[DisallowMultipleComponent]
public class EnvironmentGroundRenderingCleanup : MonoBehaviour
{
    public const string RuntimeGroundRootName = "Runtime Continuous Green Roadside Ground";
    private const float FlatGroundMaxHeight = 0.45f;
    private const float LargeSurfaceMinWidth = 1.8f;
    private const float LargeSurfaceMinLength = 2.8f;
    private const float LargeBrownGroundMinSpan = 8f;
    public static readonly Color RoadShoulderGrassColor = new Color(0.18f, 0.48f, 0.17f, 1f);
    public static readonly Color OpenTerrainGrassColor = new Color(0.2f, 0.56f, 0.2f, 1f);
    public static readonly Color RoadsideStripGrassColor = new Color(0.17f, 0.52f, 0.18f, 1f);

    private bool cleanupApplied;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeCleanup()
    {
        if (Object.FindFirstObjectByType<EnvironmentGroundRenderingCleanup>() != null)
        {
            return;
        }

        var cleanup = new GameObject("Environment Ground Rendering Cleanup");
        cleanup.AddComponent<EnvironmentGroundRenderingCleanup>();
    }

    private void Start()
    {
        ApplyCleanupIfNeeded();
    }

    public static bool IsGeneratedGrassSurface(GameObject gameObject)
    {
        return HasSelfOrAncestorName(gameObject, "Roadside Embankment Shoulders")
            || HasSelfOrAncestorName(gameObject, "Road Shoulder")
            || HasSelfOrAncestorName(gameObject, "Continuous Green Roadside Ground")
            || HasSelfOrAncestorName(gameObject, "Continuous Grass Terrain")
            || HasSelfOrAncestorName(gameObject, "Open Grass Shoulders")
            || HasSelfOrAncestorName(gameObject, "Open Grass Segment")
            || HasSelfOrAncestorName(gameObject, "Road Edge Flow Cue");
    }

    public static bool IsExtraneousGroundOrTerrain(GameObject gameObject)
    {
        if (gameObject == null || IsGeneratedGrassSurface(gameObject) || HasSelfOrAncestorName(gameObject, "Sweeping 3 km Loop Road"))
        {
            return false;
        }

        return HasSelfOrAncestorName(gameObject, "Transparent Ground")
            || HasSelfOrAncestorName(gameObject, "Terrain")
            || HasSelfOrAncestorName(gameObject, "Field")
            || HasSelfOrAncestorName(gameObject, "Forest")
            || HasSelfOrAncestorName(gameObject, "Canyon")
            || HasSelfOrAncestorName(gameObject, "Lake")
            || HasSelfOrAncestorName(gameObject, "Water")
            || HasSelfOrAncestorName(gameObject, "Pavement")
            || HasSelfOrAncestorName(gameObject, "Brown Roadside")
            || HasSelfOrAncestorName(gameObject, "Ground Strip")
            || HasSelfOrAncestorName(gameObject, "Roadside Ground Strip");
    }

    public static bool IsFlatRoadsideGroundCandidate(Renderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        var gameObject = renderer.gameObject;

        if (IsProtectedRoadObject(gameObject) || IsGeneratedGrassSurface(gameObject))
        {
            return false;
        }

        var bounds = renderer.bounds;
        var wideEnough = Mathf.Max(bounds.size.x, bounds.size.z) >= LargeSurfaceMinLength
            && Mathf.Min(bounds.size.x, bounds.size.z) >= LargeSurfaceMinWidth;
        var flatEnough = bounds.size.y <= FlatGroundMaxHeight;

        if (!wideEnough || !flatEnough)
        {
            return false;
        }

        return IsBrownGreyOrTransparent(renderer);
    }

    public static void ApplyOpaqueGrassMaterial(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

        var material = renderer.material;
        color.a = 1f;
        material.color = color;
        SetMaterialColorIfAvailable(material, "_Color", color);
        SetMaterialColorIfAvailable(material, "_BaseColor", color);
        SetMaterialFloatIfAvailable(material, "_Mode", 0f);
        SetMaterialFloatIfAvailable(material, "_Surface", 0f);
        SetMaterialFloatIfAvailable(material, "_AlphaClip", 0f);
        SetMaterialFloatIfAvailable(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        SetMaterialFloatIfAvailable(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
        SetMaterialFloatIfAvailable(material, "_ZWrite", 1f);
        SetMaterialFloatIfAvailable(material, "_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
    }

    public static void CleanupSceneGround()
    {
        using (StartupPerformanceProfiler.Measure("EnvironmentGroundRenderingCleanup.CleanupSceneGround"))
        {
            var stats = CleanupSceneGroundWithStats();
            StartupPerformanceProfiler.Log($"Environment cleanup scanned {stats.RenderersScanned} renderers, disabled {stats.DisabledObjects}, recolored {stats.RecoloredObjects}");
        }
    }

    public static EnvironmentCleanupStats CleanupSceneGroundWithStats()
    {
        EnsureContinuousGreenRoadsideGroundExists();

        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var stats = new EnvironmentCleanupStats(renderers.Length);

        for (var index = 0; index < renderers.Length; index++)
        {
            var renderer = renderers[index];
            var target = renderer.gameObject;

            if (IsGeneratedGrassSurface(target))
            {
                var color = HasSelfOrAncestorName(target, "Open Grass") ? OpenTerrainGrassColor : RoadShoulderGrassColor;
                color = HasSelfOrAncestorName(target, "Road Edge Flow Cue") ? RoadsideStripGrassColor : color;
                ApplyOpaqueGrassMaterial(renderer, color);
                stats.RecoloredObjects++;
                continue;
            }

            if (IsExtraneousGroundOrTerrain(target))
            {
                target.SetActive(false);
                stats.DisabledObjects++;
                continue;
            }

            if (IsFlatRoadsideGroundCandidate(renderer))
            {
                ApplyOpaqueGrassMaterial(renderer, RoadsideStripGrassColor);
                stats.RecoloredObjects++;
                continue;
            }

            if (IsLargeBrownGroundCandidate(renderer))
            {
                ApplyOpaqueGrassMaterial(renderer, OpenTerrainGrassColor);
                stats.RecoloredObjects++;
            }
        }

        return stats;
    }

    public static GameObject EnsureContinuousGreenRoadsideGroundExists()
    {
        var existing = GameObject.Find(RuntimeGroundRootName) ?? GameObject.Find("Continuous Green Roadside Ground");

        if (existing != null)
        {
            RefreshContinuousGreenRoadsideGround(existing.transform);
            return existing;
        }

        var root = new GameObject(RuntimeGroundRootName);
        RefreshContinuousGreenRoadsideGround(root.transform);
        return root;
    }

    private static void RefreshContinuousGreenRoadsideGround(Transform parent)
    {
        RefreshRuntimeGroundSide(parent, "Left Runtime Grass Terrain", -1f, "Left");
        RefreshRuntimeGroundSide(parent, "Right Runtime Grass Terrain", 1f, "Right");
    }

    private static void RefreshRuntimeGroundSide(Transform parent, string name, float side, string sideName)
    {
        var ground = FindChildContaining(parent, sideName);

        if (ground == null)
        {
            ground = new GameObject(name).transform;
            ground.SetParent(parent, false);
        }

        ground.name = name;
        var meshFilter = ground.GetComponent<MeshFilter>() ?? ground.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = RoadsideGroundMeshBuilder.CreateGroundMesh(side);
        var renderer = ground.GetComponent<MeshRenderer>() ?? ground.gameObject.AddComponent<MeshRenderer>();
        ApplyOpaqueGrassMaterial(renderer, OpenTerrainGrassColor);
    }

    private static Transform FindChildContaining(Transform parent, string namePart)
    {
        for (var index = 0; index < parent.childCount; index++)
        {
            var child = parent.GetChild(index);

            if (child.name.Contains(namePart))
            {
                return child;
            }
        }

        return null;
    }

    private void ApplyCleanupIfNeeded()
    {
        if (cleanupApplied)
        {
            Destroy(gameObject);
            return;
        }

        CleanupSceneGround();
        cleanupApplied = true;
        Destroy(gameObject);
    }

    private static bool IsProtectedRoadObject(GameObject gameObject)
    {
        return HasSelfOrAncestorName(gameObject, "Sweeping 3 km Loop Road")
            || HasSelfOrAncestorName(gameObject, "Road Markings")
            || HasSelfOrAncestorName(gameObject, "Road Surface Variation")
            || HasSelfOrAncestorName(gameObject, "Asphalt Tone Patch")
            || HasSelfOrAncestorName(gameObject, "Roadside Gravel Shoulders")
            || HasSelfOrAncestorName(gameObject, "Gravel Shoulder")
            || HasSelfOrAncestorName(gameObject, "Start Finish")
            || HasSelfOrAncestorName(gameObject, "Speed Post")
            || HasSelfOrAncestorName(gameObject, "Turn Warning")
            || HasSelfOrAncestorName(gameObject, "Turn Sign");
    }

    private static bool IsBrownGreyOrTransparent(Renderer renderer)
    {
        var material = renderer.material;

        if (material == null)
        {
            return false;
        }

        var color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
        var mostlyBrown = color.r > color.g * 1.08f && color.g >= color.b * 0.85f;
        var greyRoadLike = Mathf.Abs(color.r - color.g) < 0.09f && Mathf.Abs(color.g - color.b) < 0.09f && color.r > 0.18f && color.r < 0.72f;
        var transparent = color.a < 0.95f || material.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return mostlyBrown || greyRoadLike || transparent;
    }

    public static bool IsLargeBrownGroundCandidate(Renderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        var gameObject = renderer.gameObject;

        if (IsProtectedRoadObject(gameObject) || IsGeneratedGrassSurface(gameObject))
        {
            return false;
        }

        var bounds = renderer.bounds;
        var largeGroundSpan = Mathf.Max(bounds.size.x, bounds.size.z) >= LargeBrownGroundMinSpan
            && Mathf.Min(bounds.size.x, bounds.size.z) >= LargeSurfaceMinWidth;

        if (!largeGroundSpan)
        {
            return false;
        }

        return IsBrown(renderer);
    }

    private static bool IsBrown(Renderer renderer)
    {
        var material = renderer.material;

        if (material == null)
        {
            return false;
        }

        var color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
        return color.r > color.g * 1.05f && color.g >= color.b * 0.75f && color.r > 0.18f;
    }

    private static bool HasSelfOrAncestorName(GameObject gameObject, string text)
    {
        if (gameObject == null)
        {
            return false;
        }

        var current = gameObject.transform;

        while (current != null)
        {
            if (current.name.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static void SetMaterialColorIfAvailable(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

    private static void SetMaterialFloatIfAvailable(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}

public struct EnvironmentCleanupStats
{
    public EnvironmentCleanupStats(int renderersScanned)
    {
        RenderersScanned = renderersScanned;
        DisabledObjects = 0;
        RecoloredObjects = 0;
    }

    public int RenderersScanned { get; }

    public int DisabledObjects { get; set; }

    public int RecoloredObjects { get; set; }
}
