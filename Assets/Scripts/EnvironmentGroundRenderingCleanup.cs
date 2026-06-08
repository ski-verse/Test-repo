using UnityEngine;

[DisallowMultipleComponent]
public class EnvironmentGroundRenderingCleanup : MonoBehaviour
{
    public static readonly Color RoadShoulderGrassColor = new Color(0.18f, 0.48f, 0.17f, 1f);
    public static readonly Color OpenTerrainGrassColor = new Color(0.2f, 0.56f, 0.2f, 1f);

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

    private void Update()
    {
        ApplyCleanupIfNeeded();
    }

    public static bool IsGeneratedGrassSurface(GameObject gameObject)
    {
        return HasSelfOrAncestorName(gameObject, "Roadside Embankment Shoulders")
            || HasSelfOrAncestorName(gameObject, "Road Shoulder")
            || HasSelfOrAncestorName(gameObject, "Open Grass Shoulders")
            || HasSelfOrAncestorName(gameObject, "Open Grass Segment");
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
            || HasSelfOrAncestorName(gameObject, "Canyon")
            || HasSelfOrAncestorName(gameObject, "Lake")
            || HasSelfOrAncestorName(gameObject, "Water")
            || HasSelfOrAncestorName(gameObject, "Pavement")
            || HasSelfOrAncestorName(gameObject, "Brown Roadside")
            || HasSelfOrAncestorName(gameObject, "Ground Strip")
            || HasSelfOrAncestorName(gameObject, "Roadside Ground Strip");
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
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        for (var index = 0; index < renderers.Length; index++)
        {
            var renderer = renderers[index];
            var target = renderer.gameObject;

            if (IsGeneratedGrassSurface(target))
            {
                var color = HasSelfOrAncestorName(target, "Open Grass") ? OpenTerrainGrassColor : RoadShoulderGrassColor;
                ApplyOpaqueGrassMaterial(renderer, color);
                continue;
            }

            if (IsExtraneousGroundOrTerrain(target))
            {
                target.SetActive(false);
            }
        }
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
