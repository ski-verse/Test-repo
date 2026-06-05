using UnityEngine;

public static class CoursePath
{
    public const float CourseLengthMeters = 5000f;
    public const float MajorClimbStartMeters = 2100f;
    public const float MajorClimbLengthMeters = 700f;
    public const float MajorClimbGradePercent = 5.5f;
    public const float MajorClimbEndMeters = MajorClimbStartMeters + MajorClimbLengthMeters;

    private const float MajorClimbDescentStartMeters = 3300f;
    private const float MajorClimbDescentLengthMeters = 900f;
    private const float PrimaryCurveAmplitude = 42f;
    private const float SecondaryCurveAmplitude = 24f;
    private const float AccentCurveAmplitude = 8f;
    private const float PrimaryCurveFrequency = 0.0048f;
    private const float SecondaryCurveFrequency = 0.00175f;
    private const float AccentCurveFrequency = 0.0105f;
    private const float SecondaryCurvePhase = 160f;
    private const float PrimaryHillAmplitude = 9f;
    private const float SecondaryHillAmplitude = 13f;
    private const float PrimaryHillFrequency = 0.0062f;
    private const float SecondaryHillFrequency = 0.0024f;
    private const float PrimaryHillPhase = -80f;
    private const float SecondaryHillPhase = 220f;
    private const float DirectionSampleDistance = 8f;
    private const float GradientSampleDistance = 10f;

    public static float CenterXAtDistance(float zPosition)
    {
        return Mathf.Sin(zPosition * PrimaryCurveFrequency) * PrimaryCurveAmplitude
            + Mathf.Sin((zPosition + SecondaryCurvePhase) * SecondaryCurveFrequency) * SecondaryCurveAmplitude
            + Mathf.Sin(zPosition * AccentCurveFrequency) * AccentCurveAmplitude;
    }

    public static float HeightAtDistance(float zPosition)
    {
        var rawHeight = RollingHeightAtDistance(zPosition) + MajorClimbHeightAtDistance(zPosition);
        return rawHeight - CalculateLoopHeightCorrection(zPosition);
    }

    public static float GradientPercentAtDistance(float zPosition)
    {
        var previousDistance = Mathf.Max(0f, zPosition - GradientSampleDistance);
        var nextDistance = Mathf.Min(CourseLengthMeters, zPosition + GradientSampleDistance);
        var sampleLength = nextDistance - previousDistance;

        if (sampleLength <= 0.001f)
        {
            return 0f;
        }

        return (HeightAtDistance(nextDistance) - HeightAtDistance(previousDistance)) / sampleLength * 100f;
    }

    public static Vector3 CenterPointAtDistance(float zPosition)
    {
        return new Vector3(CenterXAtDistance(zPosition), HeightAtDistance(zPosition), zPosition);
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
        return Mathf.Sin((zPosition + PrimaryHillPhase) * PrimaryHillFrequency) * PrimaryHillAmplitude
            + Mathf.Sin((zPosition + SecondaryHillPhase) * SecondaryHillFrequency) * SecondaryHillAmplitude;
    }

    private static float MajorClimbHeightAtDistance(float zPosition)
    {
        var climbGrade = MajorClimbGradePercent / 100f;
        var climbGain = MajorClimbLengthMeters * climbGrade;

        if (zPosition <= MajorClimbStartMeters)
        {
            return 0f;
        }

        if (zPosition < MajorClimbEndMeters)
        {
            return (zPosition - MajorClimbStartMeters) * climbGrade;
        }

        if (zPosition < MajorClimbDescentStartMeters)
        {
            return climbGain;
        }

        if (zPosition < MajorClimbDescentStartMeters + MajorClimbDescentLengthMeters)
        {
            var descentProgress = (zPosition - MajorClimbDescentStartMeters) / MajorClimbDescentLengthMeters;
            return Mathf.Lerp(climbGain, 0f, descentProgress);
        }

        return 0f;
    }

    private static float CalculateLoopHeightCorrection(float zPosition)
    {
        var clampedDistance = Mathf.Clamp(zPosition, 0f, CourseLengthMeters);
        var rawStartHeight = RollingHeightAtDistance(0f) + MajorClimbHeightAtDistance(0f);
        var rawEndHeight = RollingHeightAtDistance(CourseLengthMeters) + MajorClimbHeightAtDistance(CourseLengthMeters);
        return (rawEndHeight - rawStartHeight) * (clampedDistance / CourseLengthMeters);
    }
}
