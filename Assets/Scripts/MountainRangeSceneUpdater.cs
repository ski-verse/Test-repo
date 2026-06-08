using UnityEngine;

public class MountainRangeSceneUpdater : MonoBehaviour
{
    private const float RoadLengthMeters = CoursePath.CourseLengthMeters;

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

        existingMountains.SetActive(false);
        Object.Destroy(existingMountains);

        var ranges = new GameObject("Nordic Mountain Ranges");
        BuildMountainRanges(ranges.transform);
        return true;
    }

    public static void BuildMountainRanges(Transform parent)
    {
        var nearColor = new Color(0.31f, 0.37f, 0.43f);
        var farColor = new Color(0.46f, 0.51f, 0.56f);

        for (var z = EnvironmentPlacement.MountainFirstDistance; z <= RoadLengthMeters; z += EnvironmentPlacement.MountainSpacing)
        {
            CreateMountainChain(
                parent,
                "Left Near Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z, -EnvironmentPlacement.NearMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2f, 360f)),
                new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2f, 82.5f * EnvironmentPlacement.MountainHeightScale, 360f),
                nearColor,
                11f + z * 0.017f,
                8);

            CreateMountainChain(
                parent,
                "Right Near Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z + 120f, EnvironmentPlacement.NearMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2f, 390f)),
                new Vector3(EnvironmentPlacement.NearMountainHalfWidth * 2f, 91f * EnvironmentPlacement.MountainHeightScale, 390f),
                nearColor,
                29f + z * 0.019f,
                9);

            CreateMountainChain(
                parent,
                "Left Far Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z + 240f, -EnvironmentPlacement.FarMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2f, 460f)),
                new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2f, 115f * EnvironmentPlacement.MountainHeightScale, 460f),
                farColor,
                47f + z * 0.013f,
                9);

            CreateMountainChain(
                parent,
                "Right Far Mountain Chain",
                EnvironmentPlacement.SafePointAtDistance(z + 360f, EnvironmentPlacement.FarMountainOffset, CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2f, 490f)),
                new Vector3(EnvironmentPlacement.FarMountainHalfWidth * 2f, 122.5f * EnvironmentPlacement.MountainHeightScale, 490f),
                farColor,
                73f + z * 0.015f,
                8);
        }
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
}
