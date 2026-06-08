using NUnit.Framework;
using UnityEngine;

public class LoopRoadMeshBuilderTests
{
    [Test]
    public void CreateRoadMesh_BuildsSingleContinuousLoopSurface()
    {
        var mesh = LoopRoadMeshBuilder.CreateRoadMesh(8f, 96);

        Assert.AreEqual((96 + 1) * 2, mesh.vertexCount);
        Assert.AreEqual(96 * 6, mesh.triangles.Length);
        Assert.AreEqual(ExpectedRoadPoint(0f, -4f), mesh.vertices[0]);
        Assert.AreEqual(ExpectedRoadPoint(0f, 4f), mesh.vertices[1]);
        Assert.AreEqual(ExpectedRoadPoint(CoursePath.CourseLengthMeters, -4f), mesh.vertices[mesh.vertexCount - 2]);
        Assert.AreEqual(ExpectedRoadPoint(CoursePath.CourseLengthMeters, 4f), mesh.vertices[mesh.vertexCount - 1]);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CreateRoadMesh_KeepsRoadSurfaceSlightlyAboveCenterlineForMarkings()
    {
        var mesh = LoopRoadMeshBuilder.CreateRoadMesh(8f, 24);

        for (var index = 0; index < mesh.vertices.Length; index += 2)
        {
            var distance = CoursePath.CourseLengthMeters * (index / 2f / 24f);
            var roadCenter = CoursePath.CenterPointAtDistance(distance);
            Assert.Greater(mesh.vertices[index].y, roadCenter.y);
            Assert.AreEqual(mesh.vertices[index].y, mesh.vertices[index + 1].y, 0.001f);
        }

        Object.DestroyImmediate(mesh);
    }

    private static Vector3 ExpectedRoadPoint(float distance, float lateralOffset)
    {
        var point = CoursePath.PointAtDistance(distance, lateralOffset);
        point.y += LoopRoadMeshBuilder.SurfaceYOffset;
        return point;
    }
}
