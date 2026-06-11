using NUnit.Framework;
using UnityEngine;

public class CourseGroundCoverageMeshBuilderTests
{
    [Test]
    public void CreateCoverageMesh_BuildsLargeSingleSidedSafetyGroundFill()
    {
        var mesh = CourseGroundCoverageMeshBuilder.CreateCoverageMesh(12);

        Assert.AreEqual((12 + 1) * (12 + 1), mesh.vertexCount);
        Assert.AreEqual(12 * 12 * 6, mesh.triangles.Length);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CreateCoverageMesh_ExtendsBeyondRoadsideGroundStrips()
    {
        var mesh = CourseGroundCoverageMeshBuilder.CreateCoverageMesh(12);
        var bounds = mesh.bounds;

        Assert.Greater(bounds.size.x, RoadsideGroundMeshBuilder.OuterOffset * 2f);
        Assert.Greater(bounds.size.z, RoadsideGroundMeshBuilder.OuterOffset * 2f);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CalculateCoveragePoint_StaysBelowRoadSoRoadSurfaceRemainsClear()
    {
        for (var distance = 0f; distance < CoursePath.CourseLengthMeters; distance += 375f)
        {
            var roadCenter = CoursePath.CenterPointAtDistance(distance);
            var coverage = CourseGroundCoverageMeshBuilder.CalculateCoveragePoint(roadCenter.x, roadCenter.z);

            Assert.Less(coverage.y, roadCenter.y - 4f);
        }
    }

    [Test]
    public void DefaultCoverageResolution_IsDenseEnoughToAvoidVisibleTerrainGaps()
    {
        Assert.GreaterOrEqual(CourseGroundCoverageMeshBuilder.DefaultGridResolution, 72);
        Assert.GreaterOrEqual(CourseGroundCoverageMeshBuilder.CourseSampleCount, 384);
        Assert.GreaterOrEqual(CourseGroundCoverageMeshBuilder.SurfaceBelowRoadMeters, 4f);
    }
}
