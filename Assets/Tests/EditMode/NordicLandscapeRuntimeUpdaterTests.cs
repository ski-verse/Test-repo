using NUnit.Framework;
using UnityEngine;

public class NordicLandscapeRuntimeUpdaterTests
{
    [Test]
    public void NearForestOffset_PlacesFirstTreeLineThreeToFiveMetersFromRoadEdge()
    {
        var distanceFromRoadEdge = NordicLandscapeRuntimeUpdater.NearTreeLineOffset - EnvironmentPlacement.RoadHalfWidth;

        Assert.GreaterOrEqual(distanceFromRoadEdge, 3f);
        Assert.LessOrEqual(distanceFromRoadEdge, 5f);
    }

    [Test]
    public void CalculateLakePosition_KeepsLakesOutsideRoadCorridor()
    {
        var lake = NordicLandscapeRuntimeUpdater.CalculateLakePosition(650f, -1f, NordicLandscapeRuntimeUpdater.LakeFootprintRadius);

        Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(lake, NordicLandscapeRuntimeUpdater.LakeFootprintRadius));
    }

    [Test]
    public void LakeSettings_MakeWaterFeaturesMoreVisibleFromRoad()
    {
        Assert.Less(NordicLandscapeRuntimeUpdater.LakeNearOffset, 58f);
        Assert.Greater(NordicLandscapeRuntimeUpdater.LakeFootprintRadius, 30f);
    }

    [Test]
    public void StreamPoints_StayOutsideRoadWhenNotCrossingAtBridge()
    {
        var stream = NordicLandscapeRuntimeUpdater.CalculateStreamPoint(620f, -24f);

        Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(stream, NordicLandscapeRuntimeUpdater.StreamFootprintRadius));
    }

    [Test]
    public void RoadsideGrass_SitsBetweenShoulderAndTreeLine()
    {
        Assert.Greater(NordicLandscapeRuntimeUpdater.RoadsideGrassInnerOffset, EnvironmentPlacement.ShoulderOuterOffset);
        Assert.Less(NordicLandscapeRuntimeUpdater.RoadsideGrassOuterOffset, NordicLandscapeRuntimeUpdater.NearTreeLineOffset);

        var grass = NordicLandscapeRuntimeUpdater.CalculateRoadsideGrassPosition(240f, -1f, 0);

        Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(grass, NordicLandscapeRuntimeUpdater.RoadsideGrassFootprintRadius));
    }

    [Test]
    public void ShouldSkipRoadsideGrass_PreservesMarkerAndPortalVisibility()
    {
        Assert.IsTrue(NordicLandscapeRuntimeUpdater.ShouldSkipRoadsideGrass(8f));
        Assert.IsTrue(NordicLandscapeRuntimeUpdater.ShouldSkipRoadsideGrass(1000f));
        Assert.IsTrue(NordicLandscapeRuntimeUpdater.ShouldSkipRoadsideGrass(2000f));
        Assert.IsFalse(NordicLandscapeRuntimeUpdater.ShouldSkipRoadsideGrass(240f));
    }

    [Test]
    public void IsForestOpening_CreatesGladesAlongTheCourse()
    {
        Assert.IsTrue(NordicLandscapeRuntimeUpdater.IsForestOpening(760f));
        Assert.IsTrue(NordicLandscapeRuntimeUpdater.IsForestOpening(1460f));
        Assert.IsFalse(NordicLandscapeRuntimeUpdater.IsForestOpening(300f));
    }

    [Test]
    public void EnsureNordicLandscapeAtmosphere_CreatesLakesAndAdditionalTreesOnce()
    {
        var first = NordicLandscapeRuntimeUpdater.EnsureNordicLandscapeAtmosphere();
        var second = NordicLandscapeRuntimeUpdater.EnsureNordicLandscapeAtmosphere();

        try
        {
            Assert.AreSame(first, second);
            Assert.IsNotNull(GameObject.Find("Jamtland Lakes"));
            Assert.IsNotNull(GameObject.Find("Nordic Streams And Creeks"));
            Assert.IsNotNull(GameObject.Find("Water Feature Rocks And Vegetation"));
            Assert.IsNotNull(GameObject.Find("Roadside Starter Pack Grass"));
            Assert.IsNotNull(GameObject.Find("Open Terrain Vegetation Patches"));
            Assert.IsNotNull(GameObject.Find("Closer Nordic Tree Bands"));
            Assert.GreaterOrEqual(GameObject.Find("Jamtland Lakes").transform.childCount, 3);
            Assert.GreaterOrEqual(GameObject.Find("Left Roadside Lake").transform.childCount, 5);
            Assert.IsNotNull(GameObject.Find("Small Timber Creek Bridge"));
            Assert.Greater(GameObject.Find("Nordic Streams And Creeks").transform.childCount, 2);
            Assert.Greater(GameObject.Find("Water Feature Rocks And Vegetation").transform.childCount, 12);
            Assert.Greater(GameObject.Find("Roadside Starter Pack Grass").transform.childCount, 150);
            Assert.Greater(GameObject.Find("Open Terrain Vegetation Patches").transform.childCount, 200);
            Assert.Greater(GameObject.Find("Closer Nordic Tree Bands").transform.childCount, 180);
        }
        finally
        {
            Object.DestroyImmediate(first);
        }
    }

    [Test]
    public void EnsureNordicLandscapeAtmosphere_AddsGrassToExistingLandscapeOnce()
    {
        var existing = new GameObject(NordicLandscapeRuntimeUpdater.LandscapeRootName);

        try
        {
            var first = NordicLandscapeRuntimeUpdater.EnsureNordicLandscapeAtmosphere();
            var second = NordicLandscapeRuntimeUpdater.EnsureNordicLandscapeAtmosphere();
            var grass = existing.transform.Find("Roadside Starter Pack Grass");
            var openVegetation = existing.transform.Find("Open Terrain Vegetation Patches");
            var streams = existing.transform.Find("Nordic Streams And Creeks");

            Assert.AreSame(existing, first);
            Assert.AreSame(existing, second);
            Assert.IsNotNull(grass);
            Assert.IsNotNull(openVegetation);
            Assert.IsNotNull(streams);
            Assert.AreEqual(1, CountChildrenNamed(existing.transform, "Roadside Starter Pack Grass"));
            Assert.AreEqual(1, CountChildrenNamed(existing.transform, "Open Terrain Vegetation Patches"));
            Assert.AreEqual(1, CountChildrenNamed(existing.transform, "Nordic Streams And Creeks"));
        }
        finally
        {
            Object.DestroyImmediate(existing);
        }
    }

    private static int CountChildrenNamed(Transform parent, string childName)
    {
        var count = 0;
        for (var index = 0; index < parent.childCount; index++)
        {
            if (parent.GetChild(index).name == childName)
            {
                count++;
            }
        }

        return count;
    }
}
