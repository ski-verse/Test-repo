using UnityEngine;

public class MountainRangeSceneUpdater : MonoBehaviour
{
    private const float RoadLengthMeters = CoursePath.CourseLengthMeters;
    public const float NearMountainChainLength = 240f;
    public const float FarMountainChainLength = 320f;
    public const float MountainRoadVisualClearance = 120f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<MountainRangeSceneUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Mountain Range Runtime Updater");
        updater.AddComponent<MountainRangeSceneUpdater>();
    }

    private void Start()
    {
        TryReplacePrototypeMountains();
        Destroy(gameObject);
    }

    public static bool TryReplacePrototypeMountains()
    {
        var existingMountains = GameObject.Find("Nordic Mountain Ranges");

        if (existingMountains == null)
        {
            return false;
        }

        if (ContainsGeneratedMountainChains(existingMountains.transform))
        {
            RebuildMountainRanges(existingMountains.transform, NordicEnvironmentSettings.GetOrCreateRuntimeSettings());
            return true;
        }

        existingMountains.SetActive(false);
        Object.Destroy(existingMountains);

        var ranges = new GameObject("Nordic Mountain Ranges");
        BuildMountainRanges(ranges.transform, NordicEnvironmentSettings.GetOrCreateRuntimeSettings());
        return true;
    }

    public static bool ContainsGeneratedMountainChains(Transform parent)
    {
        if (parent == null)
        {
            return false;
        }

        for (var index = 0; index < parent.childCount; index++)
        {
            if (parent.GetChild(index).name.Contains("Mountain Chain"))
            {
                return true;
            }
        }

        return false;
    }

    public static void BuildMountainRanges(Transform parent)
    {
        BuildMountainRanges(parent, NordicEnvironmentSettings.FindActiveSettings());
    }

    public static void BuildMountainRanges(Transform parent, NordicEnvironmentSettings settings)
    {
        var nearColor = new Color(0.31f, 0.37f, 0.43f);
        var farColor = new Color(0.46f, 0.51f, 0.56f);
        var mountainSpacing = settings != null ? settings.mountainSpacing : EnvironmentPlacement.MountainSpacing;
        var nearMountainOffset = settings != null ? settings.SafeNearMountainOffset : EnvironmentPlacement.NearMountainOffset;
        var farMountainOffset = settings != null ? settings.SafeFarMountainOffset : EnvironmentPlacement.FarMountainOffset;
        var mountainHeightScale = settings != null ? Mathf.Max(0.1f, settings.mountainHeightScale) : EnvironmentPlacement.MountainHeightScale;

        for (var z = EnvironmentPlacement.MountainFirstDistance; z <= RoadLengthMeters; z += mountainSpacing)
        {
            CreateMountainChain(
                parent,
                "Left Near Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z, -nearMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2f, NearMountainChainLength) + MountainRoadVisualClearance),
                new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2f, 82.5f * mountainHeightScale, NearMountainChainLength),
                nearColor,
                11f + z * 0.017f,
                8);

            CreateMountainChain(
                parent,
                "Right Near Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z + 120f, nearMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2f, NearMountainChainLength) + MountainRoadVisualClearance),
                new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2f, 91f * mountainHeightScale, NearMountainChainLength),
                nearColor,
                29f + z * 0.019f,
                9);

            CreateMountainChain(
                parent,
                "Left Far Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z + 240f, -farMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2f, FarMountainChainLength) + MountainRoadVisualClearance),
                new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2f, 115f * mountainHeightScale, FarMountainChainLength),
                farColor,
                47f + z * 0.013f,
                9);

            CreateMountainChain(
                parent,
                "Right Far Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z + 360f, farMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2f, FarMountainChainLength) + MountainRoadVisualClearance),
                new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2f, 122.5f * mountainHeightScale, FarMountainChainLength),
                farColor,
                73f + z * 0.015f,
                8);
        }
    }

    public static void RebuildMountainRanges(Transform parent, NordicEnvironmentSettings settings)
    {
        for (var index = parent.childCount - 1; index >= 0; index--)
        {
            DestroyObject(parent.GetChild(index).gameObject);
        }

        BuildMountainRanges(parent, settings);
    }

    private static void CreateMountainChain(Transform parent, string name, Vector3 position, Vector3 scale, Color color, float seed, int peakCount)
    {
        var chain = new GameObject(name);
        chain.transform.SetParent(parent, false);
        position.y -= 10f;
        chain.transform.position = position;
        chain.transform.localScale = scale;

        var meshFilter = chain.AddComponent<MeshFilter>();
        meshFilter.mesh = MountainRangeMeshBuilder.CreateRangeMesh(peakCount, seed);
        chain.AddComponent<MeshRenderer>().material.color = color;
    }

    private static float CalculateFootprintRadius(float width, float length)
    {
        return Mathf.Sqrt(width * width + length * length) * 0.5f;
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
}
