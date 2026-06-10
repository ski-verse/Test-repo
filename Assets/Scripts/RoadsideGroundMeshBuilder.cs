using UnityEngine;

public static class RoadsideGroundMeshBuilder
{
    public const int DefaultSampleCount = 384;
    public const float InnerOffset = EnvironmentPlacement.RoadHalfWidth + 0.18f;
    public const float OuterOffset = 420f;
    public const float InnerSurfaceYOffset = -0.035f;
    public const float OuterSurfaceYOffset = -0.11f;

    public static Mesh CreateGroundMesh(float side, int sampleCount = DefaultSampleCount)
    {
        var safeSampleCount = Mathf.Max(12, sampleCount);
        var sideSign = side < 0f ? -1f : 1f;
        var vertices = new Vector3[(safeSampleCount + 1) * 2];
        var frontTriangles = new int[safeSampleCount * 6];

        for (var index = 0; index <= safeSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)safeSampleCount);
            vertices[index * 2] = CalculateGroundPoint(distance, sideSign * InnerOffset, true);
            vertices[index * 2 + 1] = CalculateGroundPoint(distance, sideSign * OuterOffset, false);
        }

        for (var index = 0; index < safeSampleCount; index++)
        {
            var vertex = index * 2;
            var triangle = index * 6;
            frontTriangles[triangle] = vertex;
            frontTriangles[triangle + 1] = vertex + 2;
            frontTriangles[triangle + 2] = vertex + 1;
            frontTriangles[triangle + 3] = vertex + 1;
            frontTriangles[triangle + 4] = vertex + 2;
            frontTriangles[triangle + 5] = vertex + 3;
        }

        var mesh = new Mesh
        {
            name = "Continuous Green Roadside Ground Mesh",
            vertices = BuildDoubleSidedVertices(vertices),
            triangles = BuildDoubleSidedTriangles(frontTriangles, vertices.Length)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Vector3 CalculateGroundPoint(float distanceMeters, float lateralOffset)
    {
        return CalculateGroundPoint(distanceMeters, lateralOffset, Mathf.Abs(lateralOffset) <= InnerOffset + 0.01f);
    }

    public static Vector3 CalculateGroundPoint(float distanceMeters, float lateralOffset, bool innerEdge)
    {
        var point = CoursePath.PointAtDistance(distanceMeters, lateralOffset);
        point.y += innerEdge ? InnerSurfaceYOffset : OuterSurfaceYOffset;
        return point;
    }

    private static Vector3[] BuildDoubleSidedVertices(Vector3[] frontVertices)
    {
        var vertices = new Vector3[frontVertices.Length * 2];
        frontVertices.CopyTo(vertices, 0);
        frontVertices.CopyTo(vertices, frontVertices.Length);
        return vertices;
    }

    private static int[] BuildDoubleSidedTriangles(int[] frontTriangles, int backfaceVertexOffset)
    {
        var triangles = new int[frontTriangles.Length * 2];
        frontTriangles.CopyTo(triangles, 0);

        for (var index = 0; index < frontTriangles.Length; index += 3)
        {
            var reverseIndex = frontTriangles.Length + index;
            triangles[reverseIndex] = frontTriangles[index] + backfaceVertexOffset;
            triangles[reverseIndex + 1] = frontTriangles[index + 2] + backfaceVertexOffset;
            triangles[reverseIndex + 2] = frontTriangles[index + 1] + backfaceVertexOffset;
        }

        return triangles;
    }
}
