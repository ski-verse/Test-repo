using UnityEngine;

[DisallowMultipleComponent]
public class RollerSkierAnimator : MonoBehaviour
{
    private const float PlantStartPhase = 0.18f;
    private const float PlantPeakPhase = 0.46f;
    private const float ReturnEndPhase = 0.92f;

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
        return Mathf.Lerp(-62f, 44f, plantAmount) - returnLift * 10f;
    }

    public static float CalculatePolePitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(34f, -46f, plantAmount) + returnLift * 8f;
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(12f, 31f, plantAmount) - returnLift * 2f;
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
            leftArm.localRotation = Quaternion.Euler(armPitch, -10f, -7f);
        }

        if (rightArm != null)
        {
            rightArm.localRotation = Quaternion.Euler(armPitch, 10f, 7f);
        }

        if (leftPole != null)
        {
            leftPole.localRotation = Quaternion.Euler(polePitch, 0f, 0f);
        }

        if (rightPole != null)
        {
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
}
