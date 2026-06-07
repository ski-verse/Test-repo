using UnityEngine;

public static class EnvironmentPlacement
{
    public const float RoadHalfWidth = 4f;
    public const float OpenTerrainMargin = 10f;
    public const int LoopClearanceSampleCount = 384;
    public const float ShoulderInnerClearance = 0.03f;
    public const float ShoulderOuterOffset = RoadHalfWidth + OpenTerrainMargin;
    public const float ShoulderInnerYOffset = 0.04f;
    public const float ShoulderOuterYOffset = -0.18f;

    public const float NearTreeOffset = 23.5f;
    public const float MidTreeOffset = 32f;
    public const float FarTreeOffset = 44f;
    public const float MaxTreeRadius = 1.9f;

    public const float NearForestOffset = 56f;
    public const float FarForestOffset = 78f;
    public const float MaxForestTreeRadius = 2.4f;

    public const float TurnSignOffset = 20.5f;

    public const float NearHillOffset = 58f;
    public const float FarHillOffset = 72f;
    public const float NearHillHalfWidth = 17f;
    public const float FarHillHalfWidth = 20f;

    public const float MountainFirstDistance = 520f;
    public const float MountainSpacing = 620f;
    public const float NearMountainOffset = 180f;
    public const float FarMountainOffset = 275f;
    public const float NearMountainHalfWidth = 95f;
    public const float FarMountainHalfWidth = 125f;

    public static bool HasOpenRoadMargin(float centerOffset, float halfWidth)
    {
        return centerOffset - halfWidth >= RoadHalfWidth + OpenTerrainMargin;
    }

    public static bool IsRoadShoulderInsideClearCorridor(float lateralOffset)
    {
        var absoluteOffset = Mathf.Abs(lateralOffset);
        return absoluteOffset > RoadHalfWidth && absoluteOffset <= ShoulderOuterOffset;
    }

    public static bool IsRoadShoulderHeightAligned(float innerYOffset, float outerYOffset)
    {
        return innerYOffset >= -0.04f && innerYOffset <= 0.05f && outerYOffset <= innerYOffset;
    }

    public static Vector3 SafePointAtDistance(float distanceMeters, float lateralOffset, float footprintRadius)
    {
        var side = lateralOffset < 0f ? -1f : 1f;
        var safeOffset = Mathf.Max(Mathf.Abs(lateralOffset), RoadHalfWidth + OpenTerrainMargin + Mathf.Max(0f, footprintRadius));
        var point = CoursePath.PointAtDistance(distanceMeters, side * safeOffset);

        for (var attempt = 0; attempt < 80; attempt++)
        {
            if (HasLoopRoadClearance(point, footprintRadius))
            {
                return point;
            }

            safeOffset += 8f;
            point = CoursePath.PointAtDistance(distanceMeters, side * safeOffset);
        }

        return point;
    }

    public static bool HasLoopRoadClearance(Vector3 worldPosition, float footprintRadius)
    {
        return MinDistanceToLoopRoadCenter(worldPosition) - Mathf.Max(0f, footprintRadius) >= RoadHalfWidth + OpenTerrainMargin;
    }

    public static float MinDistanceToLoopRoadCenter(Vector3 worldPosition)
    {
        var minimumDistance = float.MaxValue;

        for (var i = 0; i < LoopClearanceSampleCount; i++)
        {
            var distance = CoursePath.CourseLengthMeters * (i / (float)LoopClearanceSampleCount);
            var center = CoursePath.CenterPointAtDistance(distance);
            var dx = worldPosition.x - center.x;
            var dz = worldPosition.z - center.z;
            minimumDistance = Mathf.Min(minimumDistance, Mathf.Sqrt(dx * dx + dz * dz));
        }

        return minimumDistance;
    }
}
