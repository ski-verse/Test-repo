using NUnit.Framework;
using UnityEngine;

public class StarterPackEnvironmentAssetsTests
{
    [Test]
    public void StarterPackEnvironmentAssets_DefinesNordicTreeAndRockPrefabSets()
    {
        Assert.GreaterOrEqual(StarterPackEnvironmentAssets.PinePrefabPaths.Length, 6);
        Assert.GreaterOrEqual(StarterPackEnvironmentAssets.TreePrefabPaths.Length, 8);
        Assert.GreaterOrEqual(StarterPackEnvironmentAssets.RockPrefabPaths.Length, 12);
        StringAssert.Contains("Low Poly Environment Starter Kit/Prefabs/Standard/Trees", StarterPackEnvironmentAssets.PinePrefabPaths[0]);
        StringAssert.Contains("Low Poly Environment Starter Kit/Prefabs/Standard/Rocks", StarterPackEnvironmentAssets.RockPrefabPaths[0]);
    }

    [Test]
    public void SelectPath_UsesSeedToChooseDeterministicPrefabVariant()
    {
        var first = StarterPackEnvironmentAssets.SelectPath(StarterPackEnvironmentAssets.PinePrefabPaths, 12.5f);
        var second = StarterPackEnvironmentAssets.SelectPath(StarterPackEnvironmentAssets.PinePrefabPaths, 12.5f);
        var different = StarterPackEnvironmentAssets.SelectPath(StarterPackEnvironmentAssets.PinePrefabPaths, 13.5f);

        Assert.AreEqual(first, second);
        Assert.AreNotEqual(first, different);
    }

    [Test]
    public void StarterPackRockPlacement_StaysOutsideRoadClearance()
    {
        for (var distance = 70f; distance < CoursePath.CourseLengthMeters; distance += 460f)
        {
            AssertSafePoint(distance, -EnvironmentPlacement.MidTreeOffset - 6f, 2.8f);
            AssertSafePoint(distance + 47f, EnvironmentPlacement.FarTreeOffset + 5f, 3.2f);
        }
    }

    private static void AssertSafePoint(float distance, float lateralOffset, float footprintRadius)
    {
        var point = EnvironmentPlacement.SafePointAtDistance(distance, lateralOffset, footprintRadius);
        Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(point, footprintRadius));
        Assert.GreaterOrEqual(
            EnvironmentPlacement.MinDistanceToLoopRoadCenter(point) - footprintRadius,
            EnvironmentPlacement.RoadHalfWidth + EnvironmentPlacement.OpenTerrainMargin);
    }
}
