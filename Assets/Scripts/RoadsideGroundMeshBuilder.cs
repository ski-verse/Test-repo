using UnityEngine;

public static class RoadsideGroundMeshBuilder
{
    public const int DefaultSampleCount = 384;
    public const float InnerOffset = EnvironmentPlacement.RoadHalfWidth + 0.005f;
    public const float OuterOffset = 420f;
    public const float InnerSurfaceYOffset = -0.005f;
    public const float OuterSurfaceYOffset = -0.11f;
    public static readonly float[] CoverageOffsets =
    {
        InnerOffset,
        EnvironmentPlacement.RoadHalfWidth + 0.7f,
        EnvironmentPlacement.ShoulderOuterOffset + 0.25f,
        12f,
        24f,
        48f,
        96f,
        180f,
        OuterOffset
    };

    public static Mesh CreateGroundMesh(float side, int sampleCount = DefaultSampleCount)
    {
        var safeSampleCount = Mathf.Max(12, sampleCount);
        var sideSign = side < 0f ? -1f : 1f;
        var vertices = new Vector3[(safeSampleCount + 1) * CoverageOffsets.Length];
        var frontTriangles = new int[safeSampleCount * (CoverageOffsets.Length - 1) * 6];

        for (var index = 0; index <= safeSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)safeSampleCount);

            for (var band = 0; band < CoverageOffsets.Length; band++)
            {
                vertices[index * CoverageOffsets.Length + band] = CalculateGroundPoint(distance, sideSign * CoverageOffsets[band]);
            }
        }

        for (var index = 0; index < safeSampleCount; index++)
        {
            for (var band = 0; band < CoverageOffsets.Length - 1; band++)
            {
                var vertex = index * CoverageOffsets.Length + band;
                var nextRowVertex = vertex + CoverageOffsets.Length;
                var triangle = (index * (CoverageOffsets.Length - 1) + band) * 6;
                frontTriangles[triangle] = vertex;
                frontTriangles[triangle + 1] = nextRowVertex;
                frontTriangles[triangle + 2] = vertex + 1;
                frontTriangles[triangle + 3] = vertex + 1;
                frontTriangles[triangle + 4] = nextRowVertex;
                frontTriangles[triangle + 5] = nextRowVertex + 1;
            }
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
        var point = CoursePath.PointAtDistance(distanceMeters, lateralOffset);
        var distanceFromRoad = Mathf.Abs(lateralOffset);
        var blend = Mathf.InverseLerp(InnerOffset, OuterOffset, distanceFromRoad);
        point.y += Mathf.Lerp(InnerSurfaceYOffset, OuterSurfaceYOffset, blend);
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
