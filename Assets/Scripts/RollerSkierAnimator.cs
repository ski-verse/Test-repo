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
    public Transform hips;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftPole;
    public Transform rightPole;
    public Transform leftThigh;
    public Transform rightThigh;
    public Transform leftShin;
    public Transform rightShin;
    public Transform leftSki;
    public Transform rightSki;
    public float baseCycleRate = 0.65f;
    public float speedCycleRate = 0.018f;
    public float idleReturnRate = 2.5f;
    public float propulsionWattsThreshold = DefaultPropulsionWattsThreshold;

    private float phase;
    private bool capturedBasePose;
    private Vector3 torsoBasePosition;
    private Vector3 hipsBasePosition;
    private Vector3 leftArmBasePosition;
    private Vector3 rightArmBasePosition;
    private Vector3 leftHandBasePosition;
    private Vector3 rightHandBasePosition;

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

    public static float CalculateBodyCompression(float phase)
    {
        return CalculatePlantAmount(phase) * 0.16f;
    }

    public static float CalculateToeRise(float phase)
    {
        return CalculatePlantAmount(phase) * (1f - CalculateReturnLift(phase) * 0.35f);
    }

    public static float CalculateArmPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(-48f, 26f, plantAmount) - returnLift * 5f;
    }

    public static float CalculatePolePitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(24f, -34f, plantAmount) + returnLift * 5f;
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var plantAmount = CalculatePlantAmount(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(8f, 28f, plantAmount) - returnLift * 4f;
    }

    public void ApplyPose(float posePhase)
    {
        CaptureBasePoseIfNeeded();

        var plantAmount = CalculatePlantAmount(posePhase);
        var returnLift = CalculateReturnLift(posePhase);
        var compression = CalculateBodyCompression(posePhase);
        var toeRise = CalculateToeRise(posePhase);
        var armPitch = CalculateArmPitch(posePhase);
        var polePitch = CalculatePolePitch(posePhase);
        var torsoPitch = CalculateTorsoPitch(posePhase);
        var recoveryExtension = returnLift * 0.045f;
        var handDrive = plantAmount * 0.18f;
        var handRecoveryLift = returnLift * 0.08f;

        if (hips != null)
        {
            hips.localPosition = hipsBasePosition + new Vector3(0f, -compression * 0.45f + recoveryExtension * 0.35f, plantAmount * 0.035f);
            hips.localRotation = Quaternion.Euler(-5f + plantAmount * 3f - returnLift * 1.5f, 0f, 0f);
        }

        if (torso != null)
        {
            torso.localPosition = torsoBasePosition + new Vector3(0f, -compression + recoveryExtension, plantAmount * 0.08f - returnLift * 0.025f);
            torso.localRotation = Quaternion.Euler(torsoPitch, 0f, 0f);
        }

        if (leftArm != null)
        {
            leftArm.localPosition = leftArmBasePosition + new Vector3(0.025f * plantAmount, -compression * 0.55f + handRecoveryLift, handDrive);
            leftArm.localRotation = Quaternion.Euler(armPitch, -1f, -0.5f);
        }

        if (rightArm != null)
        {
            rightArm.localPosition = rightArmBasePosition + new Vector3(-0.025f * plantAmount, -compression * 0.55f + handRecoveryLift, handDrive);
            rightArm.localRotation = Quaternion.Euler(armPitch, 1f, 0.5f);
        }

        if (leftHand != null)
        {
            leftHand.localPosition = leftHandBasePosition + new Vector3(0.015f * plantAmount, -compression * 0.35f, handDrive * 0.35f);
        }

        if (rightHand != null)
        {
            rightHand.localPosition = rightHandBasePosition + new Vector3(-0.015f * plantAmount, -compression * 0.35f, handDrive * 0.35f);
        }

        AttachPoleToHand(leftPole, leftHand, polePitch);
        AttachPoleToHand(rightPole, rightHand, polePitch);

        if (leftThigh != null)
        {
            leftThigh.localRotation = Quaternion.Euler(-20f - plantAmount * 5f + returnLift * 2f, 0f, 4f);
        }

        if (rightThigh != null)
        {
            rightThigh.localRotation = Quaternion.Euler(-20f - plantAmount * 5f + returnLift * 2f, 0f, -4f);
        }

        if (leftShin != null)
        {
            leftShin.localRotation = Quaternion.Euler(8f + plantAmount * 4f - returnLift * 1.5f, 0f, -3f);
        }

        if (rightShin != null)
        {
            rightShin.localRotation = Quaternion.Euler(8f + plantAmount * 4f - returnLift * 1.5f, 0f, 3f);
        }

        var skiPitch = -toeRise * 3.5f;
        if (leftSki != null)
        {
            leftSki.localRotation = Quaternion.Euler(skiPitch, 0f, 0f);
        }

        if (rightSki != null)
        {
            rightSki.localRotation = Quaternion.Euler(skiPitch, 0f, 0f);
        }
    }

    public void ResetBasePose()
    {
        capturedBasePose = false;
    }

    private void CaptureBasePoseIfNeeded()
    {
        if (capturedBasePose)
        {
            return;
        }

        torsoBasePosition = torso != null ? torso.localPosition : Vector3.zero;
        hipsBasePosition = hips != null ? hips.localPosition : Vector3.zero;
        leftArmBasePosition = leftArm != null ? leftArm.localPosition : Vector3.zero;
        rightArmBasePosition = rightArm != null ? rightArm.localPosition : Vector3.zero;
        leftHandBasePosition = leftHand != null ? leftHand.localPosition : Vector3.zero;
        rightHandBasePosition = rightHand != null ? rightHand.localPosition : Vector3.zero;
        capturedBasePose = true;
    }

    private static void AttachPoleToHand(Transform pole, Transform hand, float polePitch)
    {
        if (pole == null)
        {
            return;
        }

        if (hand != null)
        {
            pole.position = hand.position;
            pole.rotation = hand.rotation * Quaternion.Euler(polePitch, 0f, 0f);
            return;
        }

        pole.localRotation = Quaternion.Euler(polePitch, 0f, 0f);
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
