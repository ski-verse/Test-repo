using UnityEngine;

public static class CoursePath
{
    private const float PrimaryCurveAmplitude = 5.5f;
    private const float SecondaryCurveAmplitude = 2.2f;
    private const float PrimaryCurveFrequency = 0.0032f;
    private const float SecondaryCurveFrequency = 0.0011f;
    private const float DirectionSampleDistance = 6f;

    public static float CenterXAtDistance(float zPosition)
    {
        return Mathf.Sin(zPosition * PrimaryCurveFrequency) * PrimaryCurveAmplitude
            + Mathf.Sin(zPosition * SecondaryCurveFrequency) * SecondaryCurveAmplitude;
    }

    public static Vector3 CenterPointAtDistance(float zPosition)
    {
        return new Vector3(CenterXAtDistance(zPosition), 0f, zPosition);
    }

    public static Vector3 DirectionAtDistance(float zPosition)
    {
        var previousX = CenterXAtDistance(zPosition - DirectionSampleDistance);
        var nextX = CenterXAtDistance(zPosition + DirectionSampleDistance);
        return new Vector3(nextX - previousX, 0f, DirectionSampleDistance * 2f).normalized;
    }

    public static Vector3 RightAtDistance(float zPosition)
    {
        var direction = DirectionAtDistance(zPosition);
        return new Vector3(direction.z, 0f, -direction.x).normalized;
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
