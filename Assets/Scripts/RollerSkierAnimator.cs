using UnityEngine;

[DisallowMultipleComponent]
public class RollerSkierAnimator : MonoBehaviour
{
    private const float PlantStartPhase = 0.18f;
    private const float PlantPeakPhase = 0.46f;
    private const float ReturnEndPhase = 0.92f;

    private const float ArmRecoveryX = 0.24f;
    private const float ArmPlantX = 0.18f;
    private const float ArmRecoveryY = 1.46f;
    private const float ArmPlantY = 1.08f;
    private const float ArmRecoveryZ = -0.04f;
    private const float ArmPlantZ = 0.23f;

    private const float PoleRecoveryX = 0.34f;
    private const float PolePlantX = 0.24f;
    private const float PoleRecoveryY = 0.9f;
    private const float PolePlantY = 0.76f;
    private const float PoleRecoveryZ = 0.16f;
    private const float PolePlantZ = 0.48f;

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
        return Mathf.Lerp(-78f, 64f, plantAmount) - returnLift * 18f;
    }

    public static float CalculatePolePitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(50f, -72f, plantAmount) + returnLift * 18f;
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(15f, 44f, plantAmount) - returnLift * 3f;
    }

    public static Vector3 CalculateArmPivotPosition(float side, float phase)
    {
        var sideSign = SideSign(side);
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var x = Mathf.Lerp(ArmRecoveryX, ArmPlantX, plantAmount) * sideSign;
        var y = Mathf.Lerp(ArmRecoveryY, ArmPlantY, plantAmount) + returnLift * 0.04f;
        var z = Mathf.Lerp(ArmRecoveryZ, ArmPlantZ, plantAmount) - returnLift * 0.04f;
        return new Vector3(x, y, z);
    }

    public static Vector3 CalculatePolePivotPosition(float side, float phase)
    {
        var sideSign = SideSign(side);
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        var x = Mathf.Lerp(PoleRecoveryX, PolePlantX, plantAmount) * sideSign;
        var y = Mathf.Lerp(PoleRecoveryY, PolePlantY, plantAmount) + returnLift * 0.03f;
        var z = Mathf.Lerp(PoleRecoveryZ, PolePlantZ, plantAmount) - returnLift * 0.03f;
        return new Vector3(x, y, z);
    }

    public void ApplyPose(float posePhase)
    {
        var armPitch = CalculateArmPitch(posePhase);
        var polePitch = CalculatePolePitch(posePhase);
        var torsoPitch = CalculateTorsoPitch(posePhase);

        if (torso != null)
        {
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
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(PlantStartPhase, PlantPeakPhase, phase));
        }

        return Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(PlantPeakPhase, ReturnEndPhase, phase));
    }

    private static float CalculateReturnLift(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);

        if (phase < PlantPeakPhase)
        {
            return 0f;
        }

        return Mathf.Sin(Mathf.InverseLerp(PlantPeakPhase, 1f, phase) * Mathf.PI);
    }

    private static float SideSign(float side)
    {
        return side < 0f ? -1f : 1f;
    }
}
