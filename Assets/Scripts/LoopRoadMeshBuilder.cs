using UnityEngine;

public static class LoopRoadMeshBuilder
{
    public const int DefaultSampleCount = 384;
    public const float SurfaceYOffset = 0.035f;

    public static Mesh CreateRoadMesh(float roadWidth, int sampleCount = DefaultSampleCount)
    {
        var safeSampleCount = Mathf.Max(12, sampleCount);
        var halfWidth = roadWidth * 0.5f;
        var vertices = new Vector3[(safeSampleCount + 1) * 2];
        var triangles = new int[safeSampleCount * 6];

        for (var index = 0; index <= safeSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)safeSampleCount);
            vertices[index * 2] = CalculateRoadPoint(distance, -halfWidth);
            vertices[index * 2 + 1] = CalculateRoadPoint(distance, halfWidth);
        }

        for (var index = 0; index < safeSampleCount; index++)
        {
            var vertex = index * 2;
            var triangle = index * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 2;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + 2;
            triangles[triangle + 5] = vertex + 3;
        }

        var mesh = new Mesh
        {
            name = "Seamless 3 km Loop Road Mesh",
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Vector3 CalculateRoadPoint(float distance, float lateralOffset)
    {
        var point = CoursePath.PointAtDistance(distance, lateralOffset);
        point.y += SurfaceYOffset;
        return point;
    }
}
