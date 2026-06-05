using UnityEngine;

public static class MountainRangeMeshBuilder
{
    private const int MinimumPeakCount = 5;
    private const int MaximumPeakCount = 9;

    public static Mesh CreateRangeMesh(int peakCount, float seed)
    {
        var segmentCount = Mathf.Clamp(peakCount, MinimumPeakCount, MaximumPeakCount);
        var pointCount = segmentCount + 1;
        var vertices = new Vector3[pointCount * 3];

        for (var index = 0; index < pointCount; index++)
        {
            var x = Mathf.Lerp(-0.5f, 0.5f, index / (float)segmentCount);
            var height = CalculatePeakHeight(index, seed);
            var ridgeZ = Mathf.Lerp(-0.18f, 0.18f, Noise01(seed * 0.41f + index * 2.23f));

            if (index == 0 || index == segmentCount)
            {
                height *= 0.62f;
            }

            var vertexIndex = index * 3;
            vertices[vertexIndex] = new Vector3(x, 0f, -0.55f);
            vertices[vertexIndex + 1] = new Vector3(x, height, ridgeZ);
            vertices[vertexIndex + 2] = new Vector3(x, 0f, 0.55f);
        }

        var triangles = new int[segmentCount * 18 + 12];
        var triangleIndex = 0;

        for (var segment = 0; segment < segmentCount; segment++)
        {
            var current = segment * 3;
            var next = (segment + 1) * 3;

            AddTriangle(triangles, ref triangleIndex, current, current + 1, next + 1);
            AddTriangle(triangles, ref triangleIndex, current, next + 1, next);
            AddTriangle(triangles, ref triangleIndex, current + 1, current + 2, next + 2);
            AddTriangle(triangles, ref triangleIndex, current + 1, next + 2, next + 1);
            AddTriangle(triangles, ref triangleIndex, current + 2, current, next);
            AddTriangle(triangles, ref triangleIndex, current + 2, next, next + 2);
        }

        AddTriangle(triangles, ref triangleIndex, 0, 1, 2);
        var last = segmentCount * 3;
        AddTriangle(triangles, ref triangleIndex, last, last + 2, last + 1);
        AddTriangle(triangles, ref triangleIndex, 0, 2, 1);
        AddTriangle(triangles, ref triangleIndex, last, last + 1, last + 2);

        var mesh = new Mesh
        {
            name = "Low Poly Mountain Range Mesh",
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static float CalculatePeakHeight(int index, float seed)
    {
        var primary = Noise01(seed + index * 1.73f);
        var secondary = Noise01(seed * 0.37f + index * 4.91f);
        return Mathf.Lerp(0.48f, 1f, primary) * Mathf.Lerp(0.82f, 1.08f, secondary);
    }

    private static void AddTriangle(int[] triangles, ref int triangleIndex, int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    private static float Noise01(float value)
    {
        return Mathf.Repeat(Mathf.Sin(value * 12.9898f) * 43758.5453f, 1f);
    }
}
