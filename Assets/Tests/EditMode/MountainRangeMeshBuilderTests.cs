using NUnit.Framework;
using UnityEngine;

public class MountainRangeMeshBuilderTests
{
    [Test]
    public void CreateRangeMesh_BuildsNaturalContinuousLightweightMountainChain()
    {
        var mesh = MountainRangeMeshBuilder.CreateRangeMesh(8, 12.5f);

        Assert.Greater(mesh.vertexCount, 40);
        Assert.Less(mesh.vertexCount, 56);
        Assert.Less(mesh.triangles.Length / 3, 90);
        Assert.Greater(mesh.bounds.size.x, 0.9f);
        Assert.Greater(mesh.bounds.size.y, 0.45f);
        Assert.Greater(mesh.bounds.size.z, 1.1f);
        Assert.Greater(CountDistinctHighPoints(mesh.vertices), 3);
        Assert.Greater(CountShoulderPoints(mesh.vertices), 12);

        Object.DestroyImmediate(mesh);
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
            if (vertices[index].y <= 0.25f)
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
            if (vertices[index].y > 0.08f && vertices[index].y < 0.42f)
            {
                shoulderCount++;
            }
        }

        return shoulderCount;
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
}
