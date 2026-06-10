using NUnit.Framework;
using UnityEngine;

public class CourseGroundCoverageMeshBuilderTests
{
    [Test]
    public void CreateCoverageMesh_BuildsLargeDoubleSidedGroundFill()
    {
        var mesh = CourseGroundCoverageMeshBuilder.CreateCoverageMesh(12);

        Assert.AreEqual((12 + 1) * (12 + 1) * 2, mesh.vertexCount);
        Assert.AreEqual(12 * 12 * 12, mesh.triangles.Length);
        AssertEveryTriangleHasReverseFace(mesh);

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

            Assert.Less(coverage.y, roadCenter.y);
            Assert.Greater(coverage.y, roadCenter.y - 0.12f);
        }
    }

    [Test]
    public void DefaultCoverageResolution_IsDenseEnoughToAvoidVisibleTerrainGaps()
    {
        Assert.GreaterOrEqual(CourseGroundCoverageMeshBuilder.DefaultGridResolution, 72);
        Assert.GreaterOrEqual(CourseGroundCoverageMeshBuilder.CourseSampleCount, 384);
        Assert.LessOrEqual(CourseGroundCoverageMeshBuilder.SurfaceBelowRoadMeters, 0.08f);
    }

    private static void AssertEveryTriangleHasReverseFace(Mesh mesh)
    {
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        for (var index = 0; index < triangles.Length; index += 3)
        {
            var a = vertices[triangles[index]];
            var b = vertices[triangles[index + 1]];
            var c = vertices[triangles[index + 2]];
            Assert.IsTrue(ContainsTriangle(vertices, triangles, a, c, b));
        }
    }

    private static bool ContainsTriangle(Vector3[] vertices, int[] triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        for (var index = 0; index < triangles.Length; index += 3)
        {
            if (Approximately(vertices[triangles[index]], a)
                && Approximately(vertices[triangles[index + 1]], b)
                && Approximately(vertices[triangles[index + 2]], c))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Approximately(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x)
            && Mathf.Approximately(a.y, b.y)
            && Mathf.Approximately(a.z, b.z);
    }
}
