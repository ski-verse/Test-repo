using NUnit.Framework;
using UnityEngine;

public class MountainRangeMeshBuilderTests
{
    [Test]
    public void CreateRangeMesh_BuildsIrregularLightweightMountainChain()
    {
        var mesh = MountainRangeMeshBuilder.CreateRangeMesh(8, 12.5f);

        Assert.Greater(mesh.vertexCount, 20);
        Assert.Less(mesh.vertexCount, 32);
        Assert.Less(mesh.triangles.Length / 3, 60);
        Assert.Greater(mesh.bounds.size.x, 0.9f);
        Assert.Greater(mesh.bounds.size.y, 0.45f);
        Assert.Greater(mesh.bounds.size.z, 1f);
        Assert.Greater(CountDistinctHighPoints(mesh.vertices), 3);

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
