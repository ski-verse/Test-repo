using NUnit.Framework;
using UnityEngine;

public class NordicEnvironmentSettingsTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (var settings in Object.FindObjectsByType<NordicEnvironmentSettings>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(settings.gameObject);
        }

        var landscape = GameObject.Find(NordicLandscapeRuntimeUpdater.LandscapeRootName);
        if (landscape != null)
        {
            Object.DestroyImmediate(landscape);
        }
    }

    [Test]
    public void Defaults_MatchCurrentNordicEnvironmentValues()
    {
        var settings = new GameObject("Settings").AddComponent<NordicEnvironmentSettings>();

        Assert.AreEqual(1f, settings.treeDensity, 0.001f);
        Assert.AreEqual(1f, settings.grassDensity, 0.001f);
        Assert.AreEqual(1f, settings.roadsideGrassDensity, 0.001f);
        Assert.AreEqual(1f, settings.rockDensity, 0.001f);
        Assert.AreEqual(3, settings.lakeCount);
        Assert.AreEqual(1f, settings.lakeVisibilitySize, 0.001f);
        Assert.AreEqual(NordicLandscapeRuntimeUpdater.NearTreeLineOffset, settings.SafeNearTreeLineOffset, 0.001f);
        Assert.AreEqual(NordicLandscapeRuntimeUpdater.MidTreeLineOffset, settings.SafeMidTreeLineOffset, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.NearTreeOffset, settings.EffectiveNearTreeOffset, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.MidTreeOffset, settings.EffectiveMidTreeOffset, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.FarTreeOffset, settings.EffectiveFarTreeOffset, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.NearForestOffset, settings.EffectiveNearForestOffset, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.HighForestOffset, settings.EffectiveHighForestOffset, 0.001f);
        Assert.AreEqual(NordicLandscapeRuntimeUpdater.LakeFootprintRadius, settings.EffectiveLakeFootprintRadius, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.MountainHeightScale, settings.mountainHeightScale, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.NearMountainOffset, settings.SafeNearMountainOffset, 0.001f);
        Assert.AreEqual(EnvironmentPlacement.FarMountainOffset, settings.SafeFarMountainOffset, 0.001f);
    }

    [Test]
    public void DensityMultipliers_AdjustGeneratedSpacing()
    {
        var settings = new GameObject("Settings").AddComponent<NordicEnvironmentSettings>();
        settings.treeDensity = 2f;
        settings.grassDensity = 2f;
        settings.roadsideGrassDensity = 2f;
        settings.rockDensity = 2f;

        Assert.AreEqual(NordicLandscapeRuntimeUpdater.ForestTreeSpacingMeters * 0.5f, settings.EffectiveForestTreeSpacingMeters, 0.001f);
        Assert.AreEqual(NordicLandscapeRuntimeUpdater.OpenVegetationClusterSpacingMeters * 0.5f, settings.EffectiveOpenVegetationSpacingMeters, 0.001f);
        Assert.AreEqual(NordicLandscapeRuntimeUpdater.RoadsideGrassClusterSpacingMeters * 0.5f, settings.EffectiveRoadsideGrassSpacingMeters, 0.001f);
        Assert.AreEqual(SkiErgGameBootstrap.DefaultRockClusterSpacingMeters * 0.5f, settings.EffectiveRockSpacingMeters, 0.001f);
    }

    [Test]
    public void EnsureNordicLandscapeAtmosphere_UsesInspectableLakeCount()
    {
        var settings = new GameObject("Settings").AddComponent<NordicEnvironmentSettings>();
        settings.lakeCount = 1;

        var landscape = NordicLandscapeRuntimeUpdater.EnsureNordicLandscapeAtmosphere(settings);
        var lakes = GameObject.Find("Jamtland Lakes");

        Assert.IsNotNull(landscape);
        Assert.IsNotNull(lakes);
        Assert.AreEqual(1, lakes.transform.childCount);
        Assert.IsNotNull(GameObject.Find("Left Roadside Lake"));
        Assert.IsNull(GameObject.Find("Right Forest Lake"));
        Assert.IsNull(GameObject.Find("Left Open Glade Lake"));
    }

    [Test]
    public void MountainRanges_UseInspectableHeightScaleAndDistance()
    {
        var settings = new GameObject("Settings").AddComponent<NordicEnvironmentSettings>();
        settings.mountainHeightScale = 0.35f;
        settings.nearMountainDistance = 330f;
        settings.farMountainDistance = 510f;
        var root = new GameObject("Mountains");

        try
        {
            MountainRangeSceneUpdater.BuildMountainRanges(root.transform, settings);

            var firstChain = root.transform.GetChild(0);
            Assert.AreEqual(82.5f * 0.35f, firstChain.localScale.y, 0.001f);
            Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(firstChain.position, EnvironmentPlacement.NearMountainHalfWidth));
            Assert.GreaterOrEqual(settings.SafeNearMountainOffset, 330f);
            Assert.GreaterOrEqual(settings.SafeFarMountainOffset, 510f);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
