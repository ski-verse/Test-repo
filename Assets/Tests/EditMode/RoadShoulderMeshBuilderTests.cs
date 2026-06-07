using NUnit.Framework;
using UnityEngine;

public class RoadShoulderMeshBuilderTests
{
    [Test]
    public void EnvironmentPlacement_DefinesShoulderInsideClearCorridorButOutsideRoad()
    {
        var innerOffset = EnvironmentPlacement.RoadHalfWidth + EnvironmentPlacement.ShoulderInnerClearance;

        Assert.IsTrue(EnvironmentPlacement.IsRoadShoulderInsideClearCorridor(innerOffset));
        Assert.IsTrue(EnvironmentPlacement.IsRoadShoulderInsideClearCorridor(EnvironmentPlacement.ShoulderOuterOffset));
        Assert.IsFalse(EnvironmentPlacement.IsRoadShoulderInsideClearCorridor(EnvironmentPlacement.RoadHalfWidth - 0.01f));
        Assert.IsTrue(EnvironmentPlacement.IsRoadShoulderHeightAligned(EnvironmentPlacement.ShoulderInnerYOffset, EnvironmentPlacement.ShoulderOuterYOffset));
    }

    [Test]
    public void CreateShoulderMesh_BuildsContinuousStripOnBothSides()
    {
        var left = RoadShoulderMeshBuilder.CreateShoulderMesh(-1f, 24);
        var right = RoadShoulderMeshBuilder.CreateShoulderMesh(1f, 24);

        Assert.AreEqual((24 + 1) * 2, left.vertexCount);
        Assert.AreEqual(24 * 6, left.triangles.Length);
        Assert.AreEqual(left.vertexCount, right.vertexCount);
        Assert.AreEqual(left.triangles.Length, right.triangles.Length);
    }

    [Test]
    public void CalculateShoulderPoint_KeepsTerrainBelowRoadTopAndSlopesOutward()
    {
        for (var distance = 0f; distance < CoursePath.CourseLengthMeters; distance += 375f)
        {
            AssertShoulderSide(distance, -1f);
            AssertShoulderSide(distance, 1f);
        }
    }

    private static void AssertShoulderSide(float distance, float side)
    {
        var roadCenter = CoursePath.CenterPointAtDistance(distance);
        var inner = RoadShoulderMeshBuilder.CalculateShoulderPoint(distance, side, true);
        var outer = RoadShoulderMeshBuilder.CalculateShoulderPoint(distance, side, false);
        var innerClearance = HorizontalDistance(roadCenter, inner);
        var outerClearance = HorizontalDistance(roadCenter, outer);

        Assert.Greater(innerClearance, EnvironmentPlacement.RoadHalfWidth);
        Assert.LessOrEqual(innerClearance, EnvironmentPlacement.RoadHalfWidth + 0.5f);
        Assert.AreEqual(EnvironmentPlacement.ShoulderOuterOffset, outerClearance, 0.05f);
        Assert.Greater(outerClearance, innerClearance);
        Assert.Greater(inner.y, outer.y);
        Assert.LessOrEqual(inner.y, roadCenter.y + 0.05f);
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        var dx = a.x - b.x;
        var dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
