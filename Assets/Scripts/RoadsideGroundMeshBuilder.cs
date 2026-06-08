using UnityEngine;

public static class RoadsideGroundMeshBuilder
{
    public const int DefaultSampleCount = 384;
    public const float InnerOffset = EnvironmentPlacement.ShoulderOuterOffset - 0.05f;
    public const float OuterOffset = 72f;
    public const float SurfaceYOffset = 0.028f;

    public static Mesh CreateGroundMesh(float side, int sampleCount = DefaultSampleCount)
    {
        var safeSampleCount = Mathf.Max(12, sampleCount);
        var sideSign = side < 0f ? -1f : 1f;
        var vertices = new Vector3[(safeSampleCount + 1) * 2];
        var triangles = new int[safeSampleCount * 6];

        for (var index = 0; index <= safeSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)safeSampleCount);
            vertices[index * 2] = CalculateGroundPoint(distance, sideSign * InnerOffset);
            vertices[index * 2 + 1] = CalculateGroundPoint(distance, sideSign * OuterOffset);
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
            name = "Continuous Green Roadside Ground Mesh",
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Vector3 CalculateGroundPoint(float distanceMeters, float lateralOffset)
    {
        var point = CoursePath.PointAtDistance(distanceMeters, lateralOffset);
        point.y += SurfaceYOffset;
        return point;
    }
}
