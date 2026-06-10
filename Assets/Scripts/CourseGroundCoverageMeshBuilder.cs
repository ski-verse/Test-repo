using UnityEngine;

public static class CourseGroundCoverageMeshBuilder
{
    public const int DefaultGridResolution = 72;
    public const int SafetyBaseGridResolution = 28;
    public const int CourseSampleCount = 384;
    public const float BoundsPaddingMeters = 460f;
    public const float SafetyBaseBoundsPaddingMeters = 900f;
    public const float SurfaceBelowRoadMeters = 0.06f;
    public const float SafetyBaseBelowRoadMeters = 0.22f;
    private const float FarTerrainDropPerMeter = 0.0009f;

    public static Mesh CreateCoverageMesh(int gridResolution = DefaultGridResolution)
    {
        var safeResolution = Mathf.Max(4, gridResolution);
        CalculateCourseBounds(out var minX, out var maxX, out var minZ, out var maxZ);

        minX -= BoundsPaddingMeters;
        maxX += BoundsPaddingMeters;
        minZ -= BoundsPaddingMeters;
        maxZ += BoundsPaddingMeters;

        var vertices = new Vector3[(safeResolution + 1) * (safeResolution + 1)];
        var frontTriangles = new int[safeResolution * safeResolution * 6];
        var courseSamples = BuildCourseSamples();

        for (var zIndex = 0; zIndex <= safeResolution; zIndex++)
        {
            var z = Mathf.Lerp(minZ, maxZ, zIndex / (float)safeResolution);

            for (var xIndex = 0; xIndex <= safeResolution; xIndex++)
            {
                var x = Mathf.Lerp(minX, maxX, xIndex / (float)safeResolution);
                vertices[zIndex * (safeResolution + 1) + xIndex] = CalculateCoveragePoint(x, z, courseSamples);
            }
        }

        for (var zIndex = 0; zIndex < safeResolution; zIndex++)
        {
            for (var xIndex = 0; xIndex < safeResolution; xIndex++)
            {
                var vertex = zIndex * (safeResolution + 1) + xIndex;
                var triangle = (zIndex * safeResolution + xIndex) * 6;

                frontTriangles[triangle] = vertex;
                frontTriangles[triangle + 1] = vertex + safeResolution + 1;
                frontTriangles[triangle + 2] = vertex + 1;
                frontTriangles[triangle + 3] = vertex + 1;
                frontTriangles[triangle + 4] = vertex + safeResolution + 1;
                frontTriangles[triangle + 5] = vertex + safeResolution + 2;
            }
        }

        var mesh = new Mesh
        {
            name = "Full Course Ground Coverage Mesh",
            vertices = BuildDoubleSidedVertices(vertices),
            triangles = BuildDoubleSidedTriangles(frontTriangles, vertices.Length)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh CreateSafetyBaseGroundMesh(int gridResolution = SafetyBaseGridResolution)
    {
        var safeResolution = Mathf.Max(4, gridResolution);
        CalculateCourseBounds(out var minX, out var maxX, out var minZ, out var maxZ);

        minX -= SafetyBaseBoundsPaddingMeters;
        maxX += SafetyBaseBoundsPaddingMeters;
        minZ -= SafetyBaseBoundsPaddingMeters;
        maxZ += SafetyBaseBoundsPaddingMeters;

        var vertices = new Vector3[(safeResolution + 1) * (safeResolution + 1)];
        var frontTriangles = new int[safeResolution * safeResolution * 6];
        var courseSamples = BuildCourseSamples();

        for (var zIndex = 0; zIndex <= safeResolution; zIndex++)
        {
            var z = Mathf.Lerp(minZ, maxZ, zIndex / (float)safeResolution);

            for (var xIndex = 0; xIndex <= safeResolution; xIndex++)
            {
                var x = Mathf.Lerp(minX, maxX, xIndex / (float)safeResolution);
                vertices[zIndex * (safeResolution + 1) + xIndex] = CalculateSafetyBaseGroundPoint(x, z, courseSamples);
            }
        }

        for (var zIndex = 0; zIndex < safeResolution; zIndex++)
        {
            for (var xIndex = 0; xIndex < safeResolution; xIndex++)
            {
                var vertex = zIndex * (safeResolution + 1) + xIndex;
                var triangle = (zIndex * safeResolution + xIndex) * 6;

                frontTriangles[triangle] = vertex;
                frontTriangles[triangle + 1] = vertex + safeResolution + 1;
                frontTriangles[triangle + 2] = vertex + 1;
                frontTriangles[triangle + 3] = vertex + 1;
                frontTriangles[triangle + 4] = vertex + safeResolution + 1;
                frontTriangles[triangle + 5] = vertex + safeResolution + 2;
            }
        }

        var mesh = new Mesh
        {
            name = "Safety Base Green Ground Mesh",
            vertices = BuildDoubleSidedVertices(vertices),
            triangles = BuildDoubleSidedTriangles(frontTriangles, vertices.Length)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Vector3 CalculateCoveragePoint(float x, float z)
    {
        return CalculateCoveragePoint(x, z, BuildCourseSamples());
    }

    public static Vector3 CalculateSafetyBaseGroundPoint(float x, float z)
    {
        return CalculateSafetyBaseGroundPoint(x, z, BuildCourseSamples());
    }

    private static Vector3 CalculateCoveragePoint(float x, float z, CourseSample[] courseSamples)
    {
        var nearest = FindNearestCourseSample(x, z, courseSamples);
        var y = nearest.Height - SurfaceBelowRoadMeters - Mathf.Min(nearest.HorizontalDistance, 140f) * FarTerrainDropPerMeter;
        return new Vector3(x, y, z);
    }

    private static Vector3 CalculateSafetyBaseGroundPoint(float x, float z, CourseSample[] courseSamples)
    {
        var nearest = FindNearestCourseSample(x, z, courseSamples);
        var y = nearest.Height - SafetyBaseBelowRoadMeters;
        return new Vector3(x, y, z);
    }

    private static CourseSample FindNearestCourseSample(float x, float z, CourseSample[] courseSamples)
    {
        var nearest = courseSamples[0];
        var nearestDistanceSquared = float.MaxValue;

        for (var index = 0; index < courseSamples.Length; index++)
        {
            var sample = courseSamples[index];
            var dx = x - sample.Position.x;
            var dz = z - sample.Position.z;
            var distanceSquared = dx * dx + dz * dz;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearest = sample;
                nearestDistanceSquared = distanceSquared;
            }
        }

        nearest.HorizontalDistance = Mathf.Sqrt(nearestDistanceSquared);
        return nearest;
    }

    private static CourseSample[] BuildCourseSamples()
    {
        var samples = new CourseSample[CourseSampleCount + 1];

        for (var index = 0; index <= CourseSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)CourseSampleCount);
            var position = CoursePath.CenterPointAtDistance(distance);
            samples[index] = new CourseSample(position, position.y);
        }

        return samples;
    }

    private static void CalculateCourseBounds(out float minX, out float maxX, out float minZ, out float maxZ)
    {
        minX = float.MaxValue;
        maxX = float.MinValue;
        minZ = float.MaxValue;
        maxZ = float.MinValue;

        for (var index = 0; index <= CourseSampleCount; index++)
        {
            var distance = CoursePath.CourseLengthMeters * (index / (float)CourseSampleCount);
            var center = CoursePath.CenterPointAtDistance(distance);
            minX = Mathf.Min(minX, center.x);
            maxX = Mathf.Max(maxX, center.x);
            minZ = Mathf.Min(minZ, center.z);
            maxZ = Mathf.Max(maxZ, center.z);
        }
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

    private struct CourseSample
    {
        public CourseSample(Vector3 position, float height)
        {
            Position = position;
            Height = height;
            HorizontalDistance = 0f;
        }

        public Vector3 Position { get; }

        public float Height { get; }

        public float HorizontalDistance { get; set; }
    }
}
