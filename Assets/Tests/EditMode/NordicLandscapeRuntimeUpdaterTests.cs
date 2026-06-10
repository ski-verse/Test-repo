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
        var lake = NordicLandscapeRuntimeUpdater.CalculateLakePosition(650f, -1f, 26f);

        Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(lake, 26f));
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
            Assert.IsNotNull(GameObject.Find("Roadside Starter Pack Grass"));
            Assert.IsNotNull(GameObject.Find("Closer Nordic Tree Bands"));
            Assert.GreaterOrEqual(GameObject.Find("Jamtland Lakes").transform.childCount, 3);
            Assert.Greater(GameObject.Find("Roadside Starter Pack Grass").transform.childCount, 150);
            Assert.Greater(GameObject.Find("Closer Nordic Tree Bands").transform.childCount, 100);
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

            Assert.AreSame(existing, first);
            Assert.AreSame(existing, second);
            Assert.IsNotNull(grass);
            Assert.AreEqual(1, CountChildrenNamed(existing.transform, "Roadside Starter Pack Grass"));
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
