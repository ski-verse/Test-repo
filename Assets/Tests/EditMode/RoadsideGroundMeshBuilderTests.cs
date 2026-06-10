using NUnit.Framework;
using UnityEngine;

public class RoadsideGroundMeshBuilderTests
{
    [Test]
    public void CreateGroundMesh_BuildsContinuousGreenSurfaceOutsideRoad()
    {
        var left = RoadsideGroundMeshBuilder.CreateGroundMesh(-1f, 24);
        var right = RoadsideGroundMeshBuilder.CreateGroundMesh(1f, 24);

        Assert.AreEqual((24 + 1) * 4, left.vertexCount);
        Assert.AreEqual(24 * 12, left.triangles.Length);
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
        Assert.Greater(RoadsideGroundMeshBuilder.OuterOffset, EnvironmentPlacement.FarMountainOffset);
    }

    [Test]
    public void CalculateGroundPoint_StaysBelowRoadButAboveOldSideStrips()
    {
        for (var distance = 0f; distance < CoursePath.CourseLengthMeters; distance += 375f)
        {
            var roadCenter = CoursePath.CenterPointAtDistance(distance);
            var point = RoadsideGroundMeshBuilder.CalculateGroundPoint(distance, RoadsideGroundMeshBuilder.InnerOffset);

            Assert.Greater(point.y, roadCenter.y);
            Assert.Less(point.y, roadCenter.y + 0.14f);
        }
    }

    [Test]
    public void CreateGroundMesh_BuildsBackfaceSafeOverlay()
    {
        var mesh = RoadsideGroundMeshBuilder.CreateGroundMesh(-1f, 24);

        AssertEveryTriangleHasReverseFace(mesh);

        Object.DestroyImmediate(mesh);
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
