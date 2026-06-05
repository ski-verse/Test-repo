using UnityEngine;

public static class CoursePath
{
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

    public static float CenterXAtDistance(float zPosition)
    {
        return Mathf.Sin(zPosition * PrimaryCurveFrequency) * PrimaryCurveAmplitude
            + Mathf.Sin((zPosition + SecondaryCurvePhase) * SecondaryCurveFrequency) * SecondaryCurveAmplitude
            + Mathf.Sin(zPosition * AccentCurveFrequency) * AccentCurveAmplitude;
    }

    public static float HeightAtDistance(float zPosition)
    {
        return Mathf.Sin((zPosition + PrimaryHillPhase) * PrimaryHillFrequency) * PrimaryHillAmplitude
            + Mathf.Sin((zPosition + SecondaryHillPhase) * SecondaryHillFrequency) * SecondaryHillAmplitude;
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
}
