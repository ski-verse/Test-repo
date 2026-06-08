using NUnit.Framework;
using UnityEngine;

public class RoadsideGroundMeshBuilderTests
{
    [Test]
    public void CreateGroundMesh_BuildsContinuousGreenSurfaceOutsideRoad()
    {
        var left = RoadsideGroundMeshBuilder.CreateGroundMesh(-1f, 24);
        var right = RoadsideGroundMeshBuilder.CreateGroundMesh(1f, 24);

        Assert.AreEqual((24 + 1) * 2, left.vertexCount);
        Assert.AreEqual(24 * 6, left.triangles.Length);
        Assert.AreEqual(left.vertexCount, right.vertexCount);
        Assert.AreEqual(left.triangles.Length, right.triangles.Length);

        Object.DestroyImmediate(left);
        Object.DestroyImmediate(right);
    }

    [Test]
    public void GroundOffsets_CoverRoadsideAreaBetweenShoulderAndTrees()
    {
        Assert.Greater(RoadsideGroundMeshBuilder.InnerOffset, EnvironmentPlacement.RoadHalfWidth);
        Assert.LessOrEqual(RoadsideGroundMeshBuilder.InnerOffset, EnvironmentPlacement.ShoulderOuterOffset);
        Assert.Greater(RoadsideGroundMeshBuilder.OuterOffset, EnvironmentPlacement.HighForestOffset + 90f);
    }

    [Test]
    public void CalculateGroundPoint_StaysBelowRoadButAboveOldSideStrips()
    {
        for (var distance = 0f; distance < CoursePath.CourseLengthMeters; distance += 375f)
        {
            var roadCenter = CoursePath.CenterPointAtDistance(distance);
            var point = RoadsideGroundMeshBuilder.CalculateGroundPoint(distance, RoadsideGroundMeshBuilder.InnerOffset);

            Assert.Greater(point.y, roadCenter.y);
            Assert.Less(point.y, roadCenter.y + 0.12f);
        }
    }
}
