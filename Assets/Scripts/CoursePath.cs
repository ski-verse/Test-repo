using UnityEngine;

public static class CoursePath
{
    public const float CourseLengthMeters = 3000f;
    public const float MajorClimbStartMeters = 1200f;
    public const float MajorClimbLengthMeters = 700f;
    public const float MajorClimbGradePercent = 5.5f;
    public const float MajorClimbEndMeters = MajorClimbStartMeters + MajorClimbLengthMeters;

    private const float MajorClimbDescentStartMeters = 2050f;
    private const float MajorClimbDescentLengthMeters = 800f;
    private const float LoopHalfWidth = 330f;
    private const float LoopHalfLength = 500f;
    private const float PrimaryCurveAmplitude = 58f;
    private const float SecondaryCurveAmplitude = 24f;
    private const float PrimaryHillAmplitude = 9f;
    private const float SecondaryHillAmplitude = 13f;
    private const float DirectionSampleDistance = 8f;
    private const float GradientSampleDistance = 10f;

    public static float CenterXAtDistance(float zPosition)
    {
        var angle = AngleAtDistance(zPosition);
        return Mathf.Sin(angle) * LoopHalfWidth
            + Mathf.Sin(angle * 3f + 0.5f) * PrimaryCurveAmplitude
            + Mathf.Sin(angle * 5f - 0.25f) * SecondaryCurveAmplitude;
    }

    public static float CenterZAtDistance(float zPosition)
    {
        var angle = AngleAtDistance(zPosition);
        return (1f - Mathf.Cos(angle)) * LoopHalfLength
            + Mathf.Sin(angle * 2f + 1.1f) * PrimaryCurveAmplitude;
    }

    public static float HeightAtDistance(float zPosition)
    {
        return RollingHeightAtDistance(zPosition) + MajorClimbHeightAtDistance(zPosition);
    }

    public static float GradientPercentAtDistance(float zPosition)
    {
        var plannedClimbGradient = PlannedClimbGradientPercentAtDistance(zPosition);
        if (Mathf.Abs(plannedClimbGradient) > 0.001f)
        {
            return plannedClimbGradient;
        }

        var previousDistance = zPosition - GradientSampleDistance;
        var nextDistance = zPosition + GradientSampleDistance;
        var sampleLength = nextDistance - previousDistance;

        if (sampleLength <= 0.001f)
        {
            return 0f;
        }

        var sampledGradient = (HeightAtDistance(nextDistance) - HeightAtDistance(previousDistance)) / sampleLength * 100f;
        return Mathf.Clamp(sampledGradient, -4f, 4f);
    }

    public static Vector3 CenterPointAtDistance(float zPosition)
    {
        return new Vector3(CenterXAtDistance(zPosition), HeightAtDistance(zPosition), CenterZAtDistance(zPosition));
    }

    public static Vector3 DirectionAtDistance(float zPosition)
    {
        var previousPoint = CenterPointAtDistance(zPosition - DirectionSampleDistance);
        var nextPoint = CenterPointAtDistance(zPosition + DirectionSampleDistance);
        return (nextPoint - previousPoint).normalized;
    }

    public static Vector3 RightAtDistance(float zPosition)
    {
        var direction = DirectionAtDistance(zPosition);
        return Vector3.Cross(Vector3.up, direction).normalized;
    }

    public static Vector3 PointAtDistance(float zPosition, float lateralOffset)
    {
        return CenterPointAtDistance(zPosition) + RightAtDistance(zPosition) * lateralOffset;
    }

    public static Quaternion RotationAtDistance(float zPosition)
    {
        return Quaternion.LookRotation(DirectionAtDistance(zPosition), Vector3.up);
    }

    private static float RollingHeightAtDistance(float zPosition)
    {
        var angle = AngleAtDistance(zPosition);
        return Mathf.Sin(angle * 2f - 0.6f) * PrimaryHillAmplitude
            + Mathf.Sin(angle * 4f + 0.8f) * SecondaryHillAmplitude;
    }

    private static float MajorClimbHeightAtDistance(float zPosition)
    {
        var distance = NormalizeDistance(zPosition);
        var climbGrade = MajorClimbGradePercent / 100f;
        var climbGain = MajorClimbLengthMeters * climbGrade;

        if (distance <= MajorClimbStartMeters)
        {
            return 0f;
        }

        if (distance < MajorClimbEndMeters)
        {
            return (distance - MajorClimbStartMeters) * climbGrade;
        }

        if (distance < MajorClimbDescentStartMeters)
        {
            return climbGain;
        }

        if (distance < MajorClimbDescentStartMeters + MajorClimbDescentLengthMeters)
        {
            var descentProgress = (distance - MajorClimbDescentStartMeters) / MajorClimbDescentLengthMeters;
            return Mathf.Lerp(climbGain, 0f, descentProgress);
        }

        return 0f;
    }

    private static float PlannedClimbGradientPercentAtDistance(float zPosition)
    {
        var distance = NormalizeDistance(zPosition);

        if (distance > MajorClimbStartMeters && distance < MajorClimbEndMeters)
        {
            return MajorClimbGradePercent;
        }

        if (distance > MajorClimbDescentStartMeters && distance < MajorClimbDescentStartMeters + MajorClimbDescentLengthMeters)
        {
            var climbGain = MajorClimbLengthMeters * (MajorClimbGradePercent / 100f);
            return -(climbGain / MajorClimbDescentLengthMeters) * 100f;
        }

        return 0f;
    }

    public static float NormalizeDistance(float distanceMeters)
    {
        if (CourseLengthMeters <= 0f)
        {
            return 0f;
        }

        var normalized = distanceMeters % CourseLengthMeters;
        if (normalized < 0f)
        {
            normalized += CourseLengthMeters;
        }

        return normalized;
    }

    public static float Progress01AtDistance(float distanceMeters)
    {
        return NormalizeDistance(distanceMeters) / CourseLengthMeters;
    }

    private static float AngleAtDistance(float zPosition)
    {
        return Progress01AtDistance(zPosition) * Mathf.PI * 2f;
    }
}
