using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Ski-Verse/Nordic Environment Settings")]
public class NordicEnvironmentSettings : MonoBehaviour
{
    public const string RuntimeSettingsName = "Nordic Environment Settings";

    [Header("Trees")]
    [Min(0.1f)]
    [Tooltip("Multiplier for generated tree count. 1 keeps the current default, 2 creates roughly twice as many trees.")]
    public float treeDensity = 1f;
    [Min(3.25f)]
    [Tooltip("Distance from the road edge to the first forest line in meters.")]
    public float forestDistanceFromRoad = NordicLandscapeRuntimeUpdater.NearTreeLineOffset - EnvironmentPlacement.RoadHalfWidth;
    [Min(5f)]
    [Tooltip("Distance from the road edge to the second forest line in meters.")]
    public float midForestDistanceFromRoad = NordicLandscapeRuntimeUpdater.MidTreeLineOffset - EnvironmentPlacement.RoadHalfWidth;

    [Header("Grass")]
    [Min(0.1f)]
    [Tooltip("Multiplier for open terrain vegetation patches. 1 keeps the current default.")]
    public float grassDensity = 1f;
    [Min(0.1f)]
    [Tooltip("Multiplier for grass clusters close to the road. 1 keeps the current default.")]
    public float roadsideGrassDensity = 1f;

    [Header("Rocks")]
    [Min(0.1f)]
    [Tooltip("Multiplier for generated rock clusters. 1 keeps the current default.")]
    public float rockDensity = 1f;

    [Header("Water")]
    [Range(0, 3)]
    public int lakeCount = 3;
    [Min(0.25f)]
    [Tooltip("Multiplier for lake size and visibility. 1 keeps the current default.")]
    public float lakeVisibilitySize = 1f;
    [Min(8f)]
    public float nearLakeDistanceFromRoad = NordicLandscapeRuntimeUpdater.LakeNearOffset;
    [Min(8f)]
    public float farLakeDistanceFromRoad = NordicLandscapeRuntimeUpdater.LakeFarOffset;

    [Header("Mountains")]
    [Min(0.1f)]
    public float mountainHeightScale = EnvironmentPlacement.MountainHeightScale;
    [Min(80f)]
    public float nearMountainDistance = EnvironmentPlacement.NearMountainOffset;
    [Min(120f)]
    public float farMountainDistance = EnvironmentPlacement.FarMountainOffset;
    [Min(150f)]
    public float mountainSpacing = EnvironmentPlacement.MountainSpacing;

    public float EffectiveForestTreeSpacingMeters => DivideSpacing(NordicLandscapeRuntimeUpdater.ForestTreeSpacingMeters, treeDensity);
    public float EffectiveBootstrapTreeSpacingMeters => DivideSpacing(SkiErgGameBootstrap.DefaultTreeSpacingMeters, treeDensity);
    public float EffectiveDistantForestSpacingMeters => DivideSpacing(SkiErgGameBootstrap.DefaultDistantForestSpacingMeters, treeDensity);
    public float EffectiveRoadsideGrassSpacingMeters => DivideSpacing(NordicLandscapeRuntimeUpdater.RoadsideGrassClusterSpacingMeters, roadsideGrassDensity);
    public float EffectiveOpenVegetationSpacingMeters => DivideSpacing(NordicLandscapeRuntimeUpdater.OpenVegetationClusterSpacingMeters, grassDensity);
    public float EffectiveRockSpacingMeters => DivideSpacing(SkiErgGameBootstrap.DefaultRockClusterSpacingMeters, rockDensity);
    public float EffectiveLakeFootprintRadius => NordicLandscapeRuntimeUpdater.LakeFootprintRadius * Mathf.Max(0.25f, lakeVisibilitySize);
    public float SafeNearTreeLineOffset => EnvironmentPlacement.RoadHalfWidth + Mathf.Max(EnvironmentPlacement.OpenTerrainMargin + 0.35f, forestDistanceFromRoad);
    public float SafeMidTreeLineOffset => EnvironmentPlacement.RoadHalfWidth + Mathf.Max(SafeNearTreeLineOffset - EnvironmentPlacement.RoadHalfWidth + 2f, midForestDistanceFromRoad);
    public float SafeNearLakeOffset => Mathf.Max(EnvironmentPlacement.ShoulderOuterOffset + EffectiveLakeFootprintRadius, nearLakeDistanceFromRoad);
    public float SafeFarLakeOffset => Mathf.Max(SafeNearLakeOffset + 6f, farLakeDistanceFromRoad);
    public float SafeNearMountainOffset => Mathf.Max(
        EnvironmentPlacement.NearMountainOffset,
        EnvironmentPlacement.HighForestOffset + EnvironmentPlacement.NearMountainHalfWidth + 120f,
        nearMountainDistance);
    public float SafeFarMountainOffset => Mathf.Max(
        EnvironmentPlacement.FarMountainOffset,
        SafeNearMountainOffset + EnvironmentPlacement.FarMountainHalfWidth + 120f,
        farMountainDistance);
    public float EffectiveNearTreeOffset => Mathf.Max(
        EnvironmentPlacement.RoadHalfWidth + EnvironmentPlacement.OpenTerrainMargin + EnvironmentPlacement.MaxTreeRadius,
        SafeNearTreeLineOffset + (EnvironmentPlacement.NearTreeOffset - NordicLandscapeRuntimeUpdater.NearTreeLineOffset));
    public float EffectiveMidTreeOffset => EffectiveNearTreeOffset + (EnvironmentPlacement.MidTreeOffset - EnvironmentPlacement.NearTreeOffset);
    public float EffectiveFarTreeOffset => EffectiveNearTreeOffset + (EnvironmentPlacement.FarTreeOffset - EnvironmentPlacement.NearTreeOffset);
    public float EffectiveNearForestOffset => EffectiveNearTreeOffset + (EnvironmentPlacement.NearForestOffset - EnvironmentPlacement.NearTreeOffset);
    public float EffectiveFarForestOffset => EffectiveNearTreeOffset + (EnvironmentPlacement.FarForestOffset - EnvironmentPlacement.NearTreeOffset);
    public float EffectiveMidForestOffset => EffectiveNearTreeOffset + (EnvironmentPlacement.MidForestOffset - EnvironmentPlacement.NearTreeOffset);
    public float EffectiveHighForestOffset => EffectiveNearTreeOffset + (EnvironmentPlacement.HighForestOffset - EnvironmentPlacement.NearTreeOffset);

    public static NordicEnvironmentSettings GetOrCreateRuntimeSettings()
    {
        var existing = FindActiveSettings();
        if (existing != null)
        {
            return existing;
        }

        var settingsObject = new GameObject(RuntimeSettingsName);
        return settingsObject.AddComponent<NordicEnvironmentSettings>();
    }

    public static NordicEnvironmentSettings FindActiveSettings()
    {
        return Object.FindFirstObjectByType<NordicEnvironmentSettings>();
    }

    private static float DivideSpacing(float defaultSpacing, float density)
    {
        return defaultSpacing / Mathf.Max(0.1f, density);
    }
}
