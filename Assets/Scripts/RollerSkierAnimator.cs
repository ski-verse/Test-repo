using UnityEngine;

[DisallowMultipleComponent]
public class RollerSkierAnimator : MonoBehaviour
{
    public const float DefaultPropulsionWattsThreshold = 25f;

    private const float PlantStartPhase = 0.18f;
    private const float PlantPeakPhase = 0.46f;
    private const float ReturnEndPhase = 0.92f;
    private const float IdlePhase = 0f;

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
    public float idleReturnRate = 2.5f;
    public float propulsionWattsThreshold = DefaultPropulsionWattsThreshold;

    private float phase;

    private void LateUpdate()
    {
        var speedKmh = player != null ? player.SpeedKmh : 0f;
        var movementInput = player != null ? player.LastMovementInput : PlayerMovementInput.None;
        var shouldDoublePole = ShouldDoublePole(movementInput, propulsionWattsThreshold);
        phase = CalculateNextPhase(phase, speedKmh, Time.deltaTime, baseCycleRate, speedCycleRate, shouldDoublePole, idleReturnRate);
        ApplyPose(phase);
    }

    public static bool ShouldDoublePole(PlayerMovementInput movementInput, float wattsThreshold)
    {
        return movementInput.SpeedAxis > 0f || movementInput.PropulsionWatts > Mathf.Max(0f, wattsThreshold);
    }

    public static float CalculateNextPhase(float currentPhase, float speedKmh, float deltaTime, float baseRate, float speedRate)
    {
        return CalculateNextPhase(currentPhase, speedKmh, deltaTime, baseRate, speedRate, true, 0f);
    }

    public static float CalculateNextPhase(float currentPhase, float speedKmh, float deltaTime, float baseRate, float speedRate, bool shouldDoublePole, float idleReturnRate)
    {
        if (!shouldDoublePole)
        {
            return Mathf.MoveTowards(Mathf.Repeat(currentPhase, 1f), IdlePhase, Mathf.Max(0f, idleReturnRate) * Mathf.Max(0f, deltaTime));
        }

        var cycleRate = Mathf.Max(0f, baseRate + speedKmh * speedRate);
        return Mathf.Repeat(currentPhase + cycleRate * deltaTime, 1f);
    }

    public static float CalculateArmPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(-52f, 36f, plantAmount) - returnLift * 8f;
    }

    public static float CalculatePolePitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(32f, -42f, plantAmount) + returnLift * 8f;
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(10f, 30f, plantAmount) - returnLift * 2f;
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
            leftArm.localRotation = Quaternion.Euler(armPitch, -1.5f, -1f);
        }

        if (rightArm != null)
        {
            rightArm.localRotation = Quaternion.Euler(armPitch, 1.5f, 1f);
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
            var driveIn = Mathf.InverseLerp(PlantStartPhase, PlantPeakPhase, phase);
            return Smooth01(driveIn);
        }

        var release = Mathf.InverseLerp(PlantPeakPhase, ReturnEndPhase, phase);
        return Smooth01(1f - release);
    }

    private static float CalculateReturnLift(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);

        if (phase < PlantPeakPhase)
        {
            return 0f;
        }

        var returnPhase = Mathf.InverseLerp(PlantPeakPhase, 1f, phase);
        return Mathf.Sin(returnPhase * Mathf.PI);
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
