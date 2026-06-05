using UnityEngine;

public static class MountainRangeMeshBuilder
{
    private const int MinimumPeakCount = 5;
    private const int MaximumPeakCount = 9;
    private const int VerticesPerProfile = 5;

    public static Mesh CreateRangeMesh(int peakCount, float seed)
    {
        var segmentCount = Mathf.Clamp(peakCount, MinimumPeakCount, MaximumPeakCount);
        var pointCount = segmentCount + 1;
        var vertices = new Vector3[pointCount * VerticesPerProfile];

        for (var index = 0; index < pointCount; index++)
        {
            var x = CalculateProfileX(index, segmentCount, seed);
            var height = CalculatePeakHeight(index, seed);
            var shoulderHeight = height * Mathf.Lerp(0.28f, 0.34f, SmoothNoise01(seed * 0.29f + index * 3.17f));
            var ridgeZ = Mathf.Lerp(-0.12f, 0.12f, SmoothNoise01(seed * 0.41f + index * 2.23f));
            var frontShoulderZ = Mathf.Lerp(-0.43f, -0.31f, SmoothNoise01(seed * 0.53f + index * 1.81f));
            var backShoulderZ = Mathf.Lerp(0.31f, 0.43f, SmoothNoise01(seed * 0.67f + index * 2.71f));

            if (index == 0 || index == segmentCount)
            {
                height *= 0.78f;
                shoulderHeight *= 0.82f;
            }

            var vertexIndex = index * VerticesPerProfile;
            vertices[vertexIndex] = new Vector3(x, 0f, -0.62f);
            vertices[vertexIndex + 1] = new Vector3(x, shoulderHeight, frontShoulderZ);
            vertices[vertexIndex + 2] = new Vector3(x, height, ridgeZ);
            vertices[vertexIndex + 3] = new Vector3(x, shoulderHeight * 0.94f, backShoulderZ);
            vertices[vertexIndex + 4] = new Vector3(x, 0f, 0.62f);
        }

        var triangles = new int[segmentCount * 24 + 18];
        var triangleIndex = 0;

        for (var segment = 0; segment < segmentCount; segment++)
        {
            var current = segment * VerticesPerProfile;
            var next = (segment + 1) * VerticesPerProfile;

            AddQuad(triangles, ref triangleIndex, current, current + 1, next + 1, next);
            AddQuad(triangles, ref triangleIndex, current + 1, current + 2, next + 2, next + 1);
            AddQuad(triangles, ref triangleIndex, current + 2, current + 3, next + 3, next + 2);
            AddQuad(triangles, ref triangleIndex, current + 3, current + 4, next + 4, next + 3);
        }

        AddProfileCap(triangles, ref triangleIndex, 0, false);
        AddProfileCap(triangles, ref triangleIndex, segmentCount * VerticesPerProfile, true);

        var mesh = new Mesh
        {
            name = "Rounded Low Poly Mountain Range Mesh",
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static float CalculatePeakHeight(int index, float seed)
    {
        var broadWave = Mathf.Sin(seed * 0.13f + index * 0.72f) * 0.5f + 0.5f;
        var secondaryWave = Mathf.Sin(seed * 0.31f + index * 1.08f) * 0.5f + 0.5f;
        return Mathf.Lerp(0.44f, 0.72f, broadWave * 0.7f + secondaryWave * 0.3f);
    }

    private static float CalculateProfileX(int index, int segmentCount, float seed)
    {
        var x = Mathf.Lerp(-0.5f, 0.5f, index / (float)segmentCount);

        if (index == 0 || index == segmentCount)
        {
            return x;
        }

        return x + Mathf.Lerp(-0.012f, 0.012f, SmoothNoise01(seed * 0.73f + index * 5.19f));
    }

    private static void AddQuad(int[] triangles, ref int triangleIndex, int a, int b, int c, int d)
    {
        AddTriangle(triangles, ref triangleIndex, a, b, c);
        AddTriangle(triangles, ref triangleIndex, a, c, d);
    }

    private static void AddProfileCap(int[] triangles, ref int triangleIndex, int start, bool reverse)
    {
        if (reverse)
        {
            AddTriangle(triangles, ref triangleIndex, start, start + 2, start + 1);
            AddTriangle(triangles, ref triangleIndex, start, start + 3, start + 2);
            AddTriangle(triangles, ref triangleIndex, start, start + 4, start + 3);
            return;
        }

        AddTriangle(triangles, ref triangleIndex, start, start + 1, start + 2);
        AddTriangle(triangles, ref triangleIndex, start, start + 2, start + 3);
        AddTriangle(triangles, ref triangleIndex, start, start + 3, start + 4);
    }

    private static void AddTriangle(int[] triangles, ref int triangleIndex, int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    private static float SmoothNoise01(float value)
    {
        var noise = Mathf.Repeat(Mathf.Sin(value * 12.9898f) * 43758.5453f, 1f);
        return Mathf.SmoothStep(0f, 1f, noise);
    }
}
