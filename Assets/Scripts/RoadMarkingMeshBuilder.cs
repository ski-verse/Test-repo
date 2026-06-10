using System.Collections.Generic;
using UnityEngine;

public static class RoadMarkingMeshBuilder
{
    public const int DefaultSolidLineSampleCount = 384;
    public const float SurfaceYOffset = LoopRoadMeshBuilder.SurfaceYOffset + 0.022f;

    public static Mesh CreateSolidLineMesh(float lateralOffset, float width, int sampleCount = DefaultSolidLineSampleCount)
    {
        var safeSampleCount = Mathf.Max(12, sampleCount);
        var vertices = new Vector3[(safeSampleCount + 1) * 2];
        var triangles = new int[safeSampleCount * 6];

        for (var index = 0; index <= safeSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)safeSampleCount);
            AddLineVertices(vertices, index * 2, distance, lateralOffset, width);
        }

        AddStripTriangles(triangles, 0, 0, safeSampleCount);

        return CreateDoubleSidedMesh("Curved Road Marking Mesh", vertices, triangles);
    }

    public static Mesh CreateDashedLineMesh(float lateralOffset, float width, float dashLength, float gapLength, float startOffset, float sampleSpacingMeters = 1.5f)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var stepLength = Mathf.Max(0.1f, dashLength + Mathf.Max(0f, gapLength));

        for (var dashStart = startOffset; dashStart < CoursePath.CourseLengthMeters; dashStart += stepLength)
        {
            var dashEnd = Mathf.Min(dashStart + dashLength, CoursePath.CourseLengthMeters);
            if (dashEnd <= dashStart)
            {
                continue;
            }

            var segmentSamples = Mathf.Max(2, Mathf.CeilToInt((dashEnd - dashStart) / Mathf.Max(0.25f, sampleSpacingMeters)));
            var vertexStart = vertices.Count;

            for (var index = 0; index <= segmentSamples; index++)
            {
                var distance = Mathf.Lerp(dashStart, dashEnd, index / (float)segmentSamples);
                AddLineVertices(vertices, distance, lateralOffset, width);
            }

            AddStripTriangles(triangles, vertexStart, segmentSamples);
        }

        return CreateDoubleSidedMesh("Curved Dashed Road Marking Mesh", vertices.ToArray(), triangles.ToArray());
    }

    public static Vector3 CalculateMarkingPoint(float distanceMeters, float lateralOffset)
    {
        var point = CoursePath.PointAtDistance(distanceMeters, lateralOffset);
        point.y += SurfaceYOffset;
        return point;
    }

    private static void AddLineVertices(Vector3[] vertices, int vertexIndex, float distanceMeters, float lateralOffset, float width)
    {
        var halfWidth = width * 0.5f;
        vertices[vertexIndex] = CalculateMarkingPoint(distanceMeters, lateralOffset - halfWidth);
        vertices[vertexIndex + 1] = CalculateMarkingPoint(distanceMeters, lateralOffset + halfWidth);
    }

    private static void AddLineVertices(List<Vector3> vertices, float distanceMeters, float lateralOffset, float width)
    {
        var halfWidth = width * 0.5f;
        vertices.Add(CalculateMarkingPoint(distanceMeters, lateralOffset - halfWidth));
        vertices.Add(CalculateMarkingPoint(distanceMeters, lateralOffset + halfWidth));
    }

    private static void AddStripTriangles(int[] triangles, int triangleStart, int vertexStart, int segmentCount)
    {
        for (var index = 0; index < segmentCount; index++)
        {
            var vertex = vertexStart + index * 2;
            var triangle = triangleStart + index * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 2;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + 2;
            triangles[triangle + 5] = vertex + 3;
        }
    }

    private static void AddStripTriangles(List<int> triangles, int vertexStart, int segmentCount)
    {
        for (var index = 0; index < segmentCount; index++)
        {
            var vertex = vertexStart + index * 2;
            triangles.Add(vertex);
            triangles.Add(vertex + 2);
            triangles.Add(vertex + 1);
            triangles.Add(vertex + 1);
            triangles.Add(vertex + 2);
            triangles.Add(vertex + 3);
        }
    }

    private static Mesh CreateDoubleSidedMesh(string name, Vector3[] frontVertices, int[] frontTriangles)
    {
        var mesh = new Mesh
        {
            name = name,
            vertices = BuildDoubleSidedVertices(frontVertices),
            triangles = BuildDoubleSidedTriangles(frontTriangles, frontVertices.Length)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
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
