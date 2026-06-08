using NUnit.Framework;
using UnityEngine;

public class MountainRangeMeshBuilderTests
{
    [Test]
    public void CreateRangeMesh_BuildsRoundedSupportingMountainChain()
    {
        var mesh = MountainRangeMeshBuilder.CreateRangeMesh(8, 12.5f);

        Assert.Greater(mesh.vertexCount, 80);
        Assert.Less(mesh.vertexCount, 112);
        Assert.Less(mesh.triangles.Length / 3, 180);
        Assert.Greater(mesh.bounds.size.x, 0.9f);
        Assert.Greater(mesh.bounds.size.y, 0.35f);
        Assert.Less(mesh.bounds.size.y, 0.75f);
        Assert.Greater(mesh.bounds.size.z, 1.1f);
        Assert.Greater(CountDistinctHighPoints(mesh.vertices), 3);
        Assert.Greater(CountShoulderPoints(mesh.vertices), 12);
        Assert.Less(CalculateLargestAdjacentHeightJump(mesh.vertices), 0.24f);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CreateRangeMesh_BuildsBackfaceSafeSolidGeometry()
    {
        var mesh = MountainRangeMeshBuilder.CreateRangeMesh(8, 12.5f);

        AssertEveryTriangleHasReverseFace(mesh);

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void CalculatePeakHeight_StaysRoundedAndModerate()
    {
        var previousHeight = MountainRangeMeshBuilder.CalculatePeakHeight(0, 12.5f);
        var highestJump = 0f;

        for (var index = 1; index <= 8; index++)
        {
            var height = MountainRangeMeshBuilder.CalculatePeakHeight(index, 12.5f);
            highestJump = Mathf.Max(highestJump, Mathf.Abs(height - previousHeight));
            Assert.Less(height, 0.76f);
            previousHeight = height;
        }

        Assert.Less(highestJump, 0.24f);
    }

    [Test]
    public void MountainFootprints_StillRespectOpenRoadClearance()
    {
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.NearMountainOffset, EnvironmentPlacement.NearMountainHalfWidth));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.FarMountainOffset, EnvironmentPlacement.FarMountainHalfWidth));
    }

    private static int CountDistinctHighPoints(Vector3[] vertices)
    {
        var distinctHeights = new float[16];
        var distinctCount = 0;

        for (var index = 0; index < vertices.Length; index++)
        {
            if (vertices[index].y <= 0.2f)
            {
                continue;
            }

            var roundedHeight = Mathf.Round(vertices[index].y * 10f) * 0.1f;

            if (Contains(distinctHeights, distinctCount, roundedHeight))
            {
                continue;
            }

            distinctHeights[distinctCount] = roundedHeight;
            distinctCount++;
        }

        return distinctCount;
    }

    private static int CountShoulderPoints(Vector3[] vertices)
    {
        var shoulderCount = 0;

        for (var index = 0; index < vertices.Length; index++)
        {
            if (vertices[index].y > 0.06f && vertices[index].y < 0.3f)
            {
                shoulderCount++;
            }
        }

        return shoulderCount;
    }

    private static float CalculateLargestAdjacentHeightJump(Vector3[] vertices)
    {
        var largestJump = 0f;
        var previousHeight = vertices[2].y;

        for (var index = 7; index < vertices.Length; index += 5)
        {
            var currentHeight = vertices[index].y;
            largestJump = Mathf.Max(largestJump, Mathf.Abs(currentHeight - previousHeight));
            previousHeight = currentHeight;
        }

        return largestJump;
    }

    private static bool Contains(float[] values, int count, float value)
    {
        for (var index = 0; index < count; index++)
        {
            if (Mathf.Approximately(values[index], value))
            {
                return true;
            }
        }

        return false;
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
