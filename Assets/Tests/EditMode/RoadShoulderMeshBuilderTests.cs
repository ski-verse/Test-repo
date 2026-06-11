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

        Assert.AreEqual((24 + 1) * 4, left.vertexCount);
        Assert.AreEqual(24 * 12, left.triangles.Length);
        Assert.AreEqual(left.vertexCount, right.vertexCount);
        Assert.AreEqual(left.triangles.Length, right.triangles.Length);
    }

    [Test]
    public void CreateShoulderMesh_BuildsBackfaceSafeTerrainStrip()
    {
        var mesh = RoadShoulderMeshBuilder.CreateShoulderMesh(-1f, 24);

        AssertEveryTriangleHasReverseFace(mesh);

        Object.DestroyImmediate(mesh);
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

    [Test]
    public void CalculateShoulderPoint_BlendsIntoRoadsideGroundWithoutWideStrip()
    {
        for (var distance = 0f; distance < CoursePath.CourseLengthMeters; distance += 375f)
        {
            AssertSmoothShoulderBlend(distance, -1f);
            AssertSmoothShoulderBlend(distance, 1f);
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
        Assert.LessOrEqual(outerClearance, RoadsideGroundMeshBuilder.InnerOffset + 0.02f);
        Assert.Greater(outerClearance, innerClearance);
        Assert.Greater(inner.y, outer.y);
        Assert.LessOrEqual(inner.y, roadCenter.y + 0.05f);
    }

    private static void AssertSmoothShoulderBlend(float distance, float side)
    {
        var roadCenter = CoursePath.CenterPointAtDistance(distance);
        var inner = RoadShoulderMeshBuilder.CalculateShoulderPoint(distance, side, true);
        var outer = RoadShoulderMeshBuilder.CalculateShoulderPoint(distance, side, false);
        var roadsideGround = RoadsideGroundMeshBuilder.CalculateGroundPoint(distance, side * RoadsideGroundMeshBuilder.InnerOffset);
        var innerClearance = HorizontalDistance(roadCenter, inner);
        var outerClearance = HorizontalDistance(roadCenter, outer);

        Assert.LessOrEqual(outerClearance - innerClearance, 0.01f);
        Assert.Less(inner.y, roadCenter.y + LoopRoadMeshBuilder.SurfaceYOffset);
        Assert.AreEqual(roadsideGround.y, outer.y, 0.012f);
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        var dx = a.x - b.x;
        var dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
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
            Assert.IsTrue(
                ContainsTriangle(vertices, triangles, a, c, b),
                $"Triangle {a},{b},{c} is missing a reversed backface.");
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
