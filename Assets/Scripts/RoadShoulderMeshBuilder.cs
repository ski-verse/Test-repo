using UnityEngine;

public static class RoadShoulderMeshBuilder
{
    public const int DefaultSampleCount = 192;

    public static Mesh CreateShoulderMesh(float side, int sampleCount = DefaultSampleCount)
    {
        var safeSampleCount = Mathf.Max(12, sampleCount);
        var sideSign = side < 0f ? -1f : 1f;
        var vertices = new Vector3[(safeSampleCount + 1) * 2];
        var triangles = new int[safeSampleCount * 6];

        for (var i = 0; i <= safeSampleCount; i++)
        {
            var distance = CoursePath.CourseLengthMeters * (i / (float)safeSampleCount);
            vertices[i * 2] = CalculateShoulderPoint(distance, sideSign, true);
            vertices[i * 2 + 1] = CalculateShoulderPoint(distance, sideSign, false);
        }

        for (var i = 0; i < safeSampleCount; i++)
        {
            var vertex = i * 2;
            var triangle = i * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 2;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + 2;
            triangles[triangle + 5] = vertex + 3;
        }

        var mesh = new Mesh { name = "Road Shoulder Mesh", vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Vector3 CalculateShoulderPoint(float distanceMeters, float side, bool innerEdge)
    {
        var sideSign = side < 0f ? -1f : 1f;
        var lateralOffset = innerEdge
            ? sideSign * (EnvironmentPlacement.RoadHalfWidth + EnvironmentPlacement.ShoulderInnerClearance)
            : sideSign * EnvironmentPlacement.ShoulderOuterOffset;
        var point = CoursePath.PointAtDistance(distanceMeters, lateralOffset);
        point.y += innerEdge ? EnvironmentPlacement.ShoulderInnerYOffset : EnvironmentPlacement.ShoulderOuterYOffset;
        return point;
    }
}
