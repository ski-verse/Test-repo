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
            Assert.IsNotNull(GameObject.Find("Closer Nordic Tree Bands"));
            Assert.GreaterOrEqual(GameObject.Find("Jamtland Lakes").transform.childCount, 3);
            Assert.Greater(GameObject.Find("Closer Nordic Tree Bands").transform.childCount, 100);
        }
        finally
        {
            Object.DestroyImmediate(first);
        }
    }
}
