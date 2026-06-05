using UnityEngine;

[DisallowMultipleComponent]
public class RollerSkierAnimator : MonoBehaviour
{
    private const float PlantStartPhase = 0.18f;
    private const float PlantPeakPhase = 0.46f;
    private const float ReturnEndPhase = 0.92f;

    private const float ArmRecoveryX = 0.24f;
    private const float ArmPlantX = 0.17f;
    private const float ArmRecoveryY = 1.52f;
    private const float ArmPlantY = 0.76f;
    private const float ArmRecoveryZ = -0.12f;
    private const float ArmPlantZ = 0.46f;

    private const float PoleRecoveryX = 0.34f;
    private const float PolePlantX = 0.23f;
    private const float PoleRecoveryY = 0.96f;
    private const float PolePlantY = 0.58f;
    private const float PoleRecoveryZ = 0.08f;
    private const float PolePlantZ = 0.72f;

    public PlayerSpeedController player;
    public Transform torso;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftPole;
    public Transform rightPole;
    public Transform leftSki;
    public Transform rightSki;
    public float baseCycleRate = 0.65f;
    public float speedCycleRate = 0.018f;

    private float phase;

    private void LateUpdate()
    {
        var speedKmh = player != null ? player.SpeedKmh : 0f;
        phase = CalculateNextPhase(phase, speedKmh, Time.deltaTime, baseCycleRate, speedCycleRate);
        ApplyPose(phase);
    }

    public static float CalculateNextPhase(float currentPhase, float speedKmh, float deltaTime, float baseRate, float speedRate)
    {
        var cycleRate = Mathf.Max(0f, baseRate + speedKmh * speedRate);
        return Mathf.Repeat(currentPhase + cycleRate * deltaTime, 1f);
    }

    public static float CalculateArmPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var compression = CalculateCompressionAmount(phase);
        return Mathf.Lerp(-106f, 88f, plantAmount) - returnLift * 30f + compression * 5f;
    }

    public static float CalculatePolePitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var compression = CalculateCompressionAmount(phase);
        return Mathf.Lerp(74f, -104f, plantAmount) + returnLift * 28f - compression * 6f;
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var compression = CalculateCompressionAmount(phase);
        return Mathf.Lerp(15f, 64f, plantAmount) - returnLift * 4f + compression * 4f;
    }

    public static Vector3 CalculateTorsoPivotPosition(float phase)
    {
        var compression = CalculateCompressionAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return new Vector3(0f, Mathf.Lerp(0f, -0.22f, compression) + returnLift * 0.035f, Mathf.Lerp(0f, 0.16f, compression));
    }

    public static Vector3 CalculateArmPivotPosition(float side, float phase)
    {
        var sideSign = SideSign(side);
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var compression = CalculateCompressionAmount(phase);
        var x = Mathf.Lerp(ArmRecoveryX, ArmPlantX, plantAmount) * sideSign;
        var y = Mathf.Lerp(ArmRecoveryY, ArmPlantY, plantAmount) + returnLift * 0.08f - compression * 0.06f;
        var z = Mathf.Lerp(ArmRecoveryZ, ArmPlantZ, plantAmount) - returnLift * 0.08f + compression * 0.05f;
        return new Vector3(x, y, z);
    }

    public static Vector3 CalculatePolePivotPosition(float side, float phase)
    {
        var sideSign = SideSign(side);
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var compression = CalculateCompressionAmount(phase);
        var x = Mathf.Lerp(PoleRecoveryX, PolePlantX, plantAmount) * sideSign;
        var y = Mathf.Lerp(PoleRecoveryY, PolePlantY, plantAmount) + returnLift * 0.06f - compression * 0.04f;
        var z = Mathf.Lerp(PoleRecoveryZ, PolePlantZ, plantAmount) - returnLift * 0.06f + compression * 0.04f;
        return new Vector3(x, y, z);
    }

    public void ApplyPose(float posePhase)
    {
        var armPitch = CalculateArmPitch(posePhase);
        var polePitch = CalculatePolePitch(posePhase);
        var torsoPitch = CalculateTorsoPitch(posePhase);

        if (torso != null)
        {
            torso.localPosition = CalculateTorsoPivotPosition(posePhase);
            torso.localRotation = Quaternion.Euler(torsoPitch, 0f, 0f);
        }

        if (leftArm != null)
        {
            leftArm.localPosition = CalculateArmPivotPosition(-1f, posePhase);
            leftArm.localRotation = Quaternion.Euler(armPitch, -4f, -3f);
        }

        if (rightArm != null)
        {
            rightArm.localPosition = CalculateArmPivotPosition(1f, posePhase);
            rightArm.localRotation = Quaternion.Euler(armPitch, 4f, 3f);
        }

        if (leftPole != null)
        {
            leftPole.localPosition = CalculatePolePivotPosition(-1f, posePhase);
            leftPole.localRotation = Quaternion.Euler(polePitch, 0f, 0f);
        }

        if (rightPole != null)
        {
            rightPole.localPosition = CalculatePolePivotPosition(1f, posePhase);
            rightPole.localRotation = Quaternion.Euler(polePitch, 0f, 0f);
        }

        if (leftSki != null)
        {
            leftSki.localRotation = Quaternion.identity;
        }

        if (rightSki != null)
        {
            rightSki.localRotation = Quaternion.identity;
        }
    }

    private static float CalculatePlantAmount(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);

        if (phase < PlantStartPhase)
        {
            return 0f;
        }

        if (phase < PlantPeakPhase)
        {
            var driveIn = Mathf.InverseLerp(PlantStartPhase, PlantPeakPhase, phase);
            return EaseInOutSine(driveIn);
        }

        var release = Mathf.InverseLerp(PlantPeakPhase, ReturnEndPhase, phase);
        return Mathf.SmoothStep(1f, 0f, release);
    }

    private static float CalculateCompressionAmount(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);
        var driveCompression = Mathf.Sin(Mathf.InverseLerp(PlantStartPhase, 0.58f, phase) * Mathf.PI);
        return Mathf.Clamp01(driveCompression) * CalculatePlantAmount(phase);
    }

    private static float CalculateReturnLift(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);

        if (phase < PlantPeakPhase)
        {
            return 0f;
        }

        var returnPhase = Mathf.InverseLerp(PlantPeakPhase, 1f, phase);
        return Mathf.SmoothStep(0f, 1f, Mathf.Sin(returnPhase * Mathf.PI));
    }

    private static float EaseInOutSine(float value)
    {
        return 0.5f - Mathf.Cos(Mathf.Clamp01(value) * Mathf.PI) * 0.5f;
    }

    private static float SideSign(float side)
    {
        return side < 0f ? -1f : 1f;
    }
}
