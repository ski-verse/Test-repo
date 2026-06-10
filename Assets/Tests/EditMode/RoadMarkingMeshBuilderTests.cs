using NUnit.Framework;
using UnityEngine;

public class RoadMarkingMeshBuilderTests
{
    [Test]
    public void CreateSolidLineMesh_BuildsContinuousCurvedLineStrip()
    {
        var mesh = RoadMarkingMeshBuilder.CreateSolidLineMesh(3.65f, 0.12f, 48);

        Assert.AreEqual((48 + 1) * 4, mesh.vertexCount);
        Assert.AreEqual(48 * 12, mesh.triangles.Length);
        Assert.AreEqual(ExpectedMarkingPoint(0f, 3.59f), mesh.vertices[0]);
        Assert.AreEqual(ExpectedMarkingPoint(0f, 3.71f), mesh.vertices[1]);
        Assert.AreEqual(ExpectedMarkingPoint(CoursePath.CourseLengthMeters, 3.59f), mesh.vertices[(48 + 1) * 2 - 2]);
        Assert.AreEqual(ExpectedMarkingPoint(CoursePath.CourseLengthMeters, 3.71f), mesh.vertices[(48 + 1) * 2 - 1]);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CreateDashedLineMesh_BuildsSeparatedDashesThatFollowCoursePath()
    {
        var mesh = RoadMarkingMeshBuilder.CreateDashedLineMesh(0f, 0.16f, 9f, 23f, 14f, 3f);

        Assert.Greater(mesh.vertexCount, 0);
        Assert.Greater(mesh.triangles.Length, 0);
        Assert.AreEqual(ExpectedMarkingPoint(14f, -0.08f), mesh.vertices[0]);
        Assert.AreEqual(ExpectedMarkingPoint(14f, 0.08f), mesh.vertices[1]);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CalculateMarkingPoint_UsesSameCoursePathAsRoadMesh()
    {
        for (var distance = 0f; distance <= CoursePath.CourseLengthMeters; distance += 375f)
        {
            var markingPoint = RoadMarkingMeshBuilder.CalculateMarkingPoint(distance, -3.65f);
            var roadPoint = CoursePath.PointAtDistance(distance, -3.65f);

            Assert.AreEqual(roadPoint.x, markingPoint.x, 0.001f);
            Assert.AreEqual(roadPoint.z, markingPoint.z, 0.001f);
            Assert.AreEqual(LoopRoadMeshBuilder.SurfaceYOffset + 0.022f, markingPoint.y - roadPoint.y, 0.001f);
        }
    }

    [Test]
    public void RoadMarkingConstants_PreserveWhiteCountryRoadMarkings()
    {
        Assert.AreEqual(Color.white, SkiErgGameBootstrap.RoadMarkingColor);
        Assert.AreEqual(9f, SkiErgGameBootstrap.CountryRoadCenterDashLengthMeters);
        Assert.AreEqual(23f, SkiErgGameBootstrap.CountryRoadCenterDashGapMeters);
        Assert.AreEqual(0.12f, SkiErgGameBootstrap.CountryRoadEdgeLineWidthMeters);
    }

    private static Vector3 ExpectedMarkingPoint(float distance, float lateralOffset)
    {
        var point = CoursePath.PointAtDistance(distance, lateralOffset);
        point.y += RoadMarkingMeshBuilder.SurfaceYOffset;
        return point;
    }
}
