using UnityEngine;

[DisallowMultipleComponent]
public class RollerSkierAnimator : MonoBehaviour
{
    public const float DefaultPropulsionWattsThreshold = 25f;
    public const float PhasePolePlant = 0f;
    public const float PhaseLoad = 0.18f;
    public const float PhasePower = 0.38f;
    public const float PhaseRelease = 0.56f;
    public const float PhaseRecovery = 0.76f;
    public const float PhasePreparation = 0.92f;

    private const float IdlePhase = PhasePolePlant;
    private const float BodyCompressionDepth = 0.215f;
    private const float StableFootSkiRise = 0.007f;
    private const float StableFootSkiDrive = 0.007f;

    public PlayerSpeedController player;
    public Transform torso;
    public Transform hips;
    public Transform head;
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
    public Transform leftFoot;
    public Transform rightFoot;
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
    private Vector3 headBasePosition;
    private Vector3 leftArmBasePosition;
    private Vector3 rightArmBasePosition;
    private Vector3 leftHandBasePosition;
    private Vector3 rightHandBasePosition;
    private Vector3 leftFootBasePosition;
    private Vector3 rightFootBasePosition;
    private Vector3 leftSkiBasePosition;
    private Vector3 rightSkiBasePosition;

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
        return movementInput.IsActivelyPoling;
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
        return CalculateBodyWeightTransfer(phase) * BodyCompressionDepth;
    }

    public static float CalculateToeRise(float phase)
    {
        return SampleSixStageCurve(phase, 0.08f, 0.24f, 0.62f, 0.32f, 0f, 0.08f);
    }

    public static float CalculateBodyWeightTransfer(float phase)
    {
        return SampleSixStageCurve(phase, 0.18f, 0.58f, 1f, 0.72f, 0.12f, 0.14f);
    }

    public static float CalculatePolePressure(float phase)
    {
        return SampleSixStageCurve(phase, 0.44f, 0.72f, 1f, 0.38f, 0f, 0.28f);
    }

    public static float CalculateTorsoForwardDrive(float phase)
    {
        return SampleSixStageCurve(phase, 0.04f, 0.14f, 0.24f, 0.2f, 0f, 0.035f);
    }

    public static float CalculateHipHingeForwardDrive(float phase)
    {
        return SampleSixStageCurve(phase, 0.035f, 0.1f, 0.165f, 0.14f, 0.005f, 0.03f);
    }

    public static float CalculateHandForwardDrive(float phase)
    {
        return SampleSixStageCurve(phase, 0.14f, 0.09f, -0.015f, -0.075f, 0.055f, 0.15f);
    }

    public static float CalculateHandOutwardDrift(float phase)
    {
        return CalculatePolePressure(phase) * 0.005f;
    }

    public static float CalculateHandRecoveryLift(float phase)
    {
        return SampleSixStageCurve(phase, 0.02f, 0f, 0f, 0.012f, 0.075f, 0.04f);
    }

    public static float CalculatePolePlantForwardOffset(float phase)
    {
        return SampleSixStageCurve(phase, 0.12f, 0.08f, 0.03f, -0.025f, 0.04f, 0.12f);
    }

    public static float CalculateHeadCounterPitch(float phase)
    {
        return -CalculateTorsoPitch(phase) * 0.45f;
    }

    public static float CalculateArmPitch(float phase)
    {
        return SampleSixStageCurve(phase, -42f, -18f, 12f, 20f, -34f, -44f);
    }

    public static float CalculatePolePitch(float phase)
    {
        return SampleSixStageCurve(phase, 8f, -12f, -42f, -34f, 24f, 10f);
    }

    public static float CalculateTorsoPitch(float phase)
    {
        return SampleSixStageCurve(phase, 12f, 27f, 43f, 38f, 16f, 12f);
    }

    public void ApplyPose(float posePhase)
    {
        CaptureBasePoseIfNeeded();

        var bodyWeightTransfer = CalculateBodyWeightTransfer(posePhase);
        var polePressure = CalculatePolePressure(posePhase);
        var compression = CalculateBodyCompression(posePhase);
        var toeRise = CalculateToeRise(posePhase);
        var armPitch = CalculateArmPitch(posePhase);
        var polePitch = CalculatePolePitch(posePhase);
        var torsoPitch = CalculateTorsoPitch(posePhase);
        var handPathForward = CalculateHandForwardDrive(posePhase);
        var handOutwardDrift = CalculateHandOutwardDrift(posePhase);
        var handRecoveryLift = CalculateHandRecoveryLift(posePhase);
        var polePlantForwardOffset = CalculatePolePlantForwardOffset(posePhase);
        var torsoForwardDrive = CalculateTorsoForwardDrive(posePhase);
        var hipForwardDrive = CalculateHipHingeForwardDrive(posePhase);

        if (hips != null)
        {
            hips.localPosition = hipsBasePosition + new Vector3(0f, -compression * 0.2f + handRecoveryLift * 0.2f, hipForwardDrive);
            hips.localRotation = Quaternion.Euler(-7f + bodyWeightTransfer * 5.5f - handRecoveryLift * 8f, 0f, 0f);
        }

        if (torso != null)
        {
            torso.localPosition = torsoBasePosition + new Vector3(0f, -compression * 0.34f + handRecoveryLift * 0.55f, torsoForwardDrive);
            torso.localRotation = Quaternion.Euler(torsoPitch, 0f, 0f);
        }

        if (head != null)
        {
            head.localPosition = headBasePosition + new Vector3(0f, compression * 0.07f, -bodyWeightTransfer * 0.01f);
            head.localRotation = Quaternion.Euler(CalculateHeadCounterPitch(posePhase), 0f, 0f);
        }

        if (leftArm != null)
        {
            leftArm.localPosition = leftArmBasePosition + new Vector3(handOutwardDrift, -compression * 0.24f + handRecoveryLift, handPathForward * 0.42f + torsoForwardDrive * 0.1f);
            leftArm.localRotation = Quaternion.Euler(armPitch, -0.6f, -0.35f);
        }

        if (rightArm != null)
        {
            rightArm.localPosition = rightArmBasePosition + new Vector3(-handOutwardDrift, -compression * 0.24f + handRecoveryLift, handPathForward * 0.42f + torsoForwardDrive * 0.1f);
            rightArm.localRotation = Quaternion.Euler(armPitch, 0.6f, 0.35f);
        }

        if (leftHand != null)
        {
            leftHand.localPosition = leftHandBasePosition + new Vector3(handOutwardDrift * 0.5f, -compression * 0.42f + handRecoveryLift * 0.6f, handPathForward + polePlantForwardOffset * 0.35f);
        }

        if (rightHand != null)
        {
            rightHand.localPosition = rightHandBasePosition + new Vector3(-handOutwardDrift * 0.5f, -compression * 0.42f + handRecoveryLift * 0.6f, handPathForward + polePlantForwardOffset * 0.35f);
        }

        AttachPoleToHand(leftPole, leftHand, polePitch);
        AttachPoleToHand(rightPole, rightHand, polePitch);

        if (leftThigh != null)
        {
            leftThigh.localRotation = Quaternion.Euler(-20f - polePressure * 4f + handRecoveryLift * 20f, 0f, 3f);
        }

        if (rightThigh != null)
        {
            rightThigh.localRotation = Quaternion.Euler(-20f - polePressure * 4f + handRecoveryLift * 20f, 0f, -3f);
        }

        if (leftShin != null)
        {
            leftShin.localRotation = Quaternion.Euler(8f + polePressure * 3f - handRecoveryLift * 12f, 0f, -2.5f);
        }

        if (rightShin != null)
        {
            rightShin.localRotation = Quaternion.Euler(8f + polePressure * 3f - handRecoveryLift * 12f, 0f, 2.5f);
        }

        var footPitch = -toeRise * 1.4f;
        var footRise = toeRise * StableFootSkiRise;
        var footDrive = polePressure * StableFootSkiDrive;
        if (leftFoot != null)
        {
            leftFoot.localPosition = leftFootBasePosition + new Vector3(0f, footRise, footDrive);
            leftFoot.localRotation = Quaternion.Euler(footPitch, 0f, 0f);
        }

        if (rightFoot != null)
        {
            rightFoot.localPosition = rightFootBasePosition + new Vector3(0f, footRise, footDrive);
            rightFoot.localRotation = Quaternion.Euler(footPitch, 0f, 0f);
        }

        var skiPitch = -toeRise * 0.35f;
        var skiRise = footRise;
        var skiDrive = footDrive;
        if (leftSki != null)
        {
            leftSki.localPosition = leftSkiBasePosition + new Vector3(0f, skiRise, skiDrive);
            leftSki.localRotation = Quaternion.Euler(skiPitch, 0f, 0f);
        }

        if (rightSki != null)
        {
            rightSki.localPosition = rightSkiBasePosition + new Vector3(0f, skiRise, skiDrive);
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
        headBasePosition = head != null ? head.localPosition : Vector3.zero;
        leftArmBasePosition = leftArm != null ? leftArm.localPosition : Vector3.zero;
        rightArmBasePosition = rightArm != null ? rightArm.localPosition : Vector3.zero;
        leftHandBasePosition = leftHand != null ? leftHand.localPosition : Vector3.zero;
        rightHandBasePosition = rightHand != null ? rightHand.localPosition : Vector3.zero;
        leftFootBasePosition = leftFoot != null ? leftFoot.localPosition : Vector3.zero;
        rightFootBasePosition = rightFoot != null ? rightFoot.localPosition : Vector3.zero;
        leftSkiBasePosition = leftSki != null ? leftSki.localPosition : Vector3.zero;
        rightSkiBasePosition = rightSki != null ? rightSki.localPosition : Vector3.zero;
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

    private static float SampleSixStageCurve(float phase, float polePlant, float load, float power, float release, float recovery, float preparation)
    {
        phase = Mathf.Repeat(phase, 1f);

        if (phase < PhaseLoad)
        {
            return SmoothLerp(polePlant, load, Mathf.InverseLerp(PhasePolePlant, PhaseLoad, phase));
        }

        if (phase < PhasePower)
        {
            return SmoothLerp(load, power, Mathf.InverseLerp(PhaseLoad, PhasePower, phase));
        }

        if (phase < PhaseRelease)
        {
            return SmoothLerp(power, release, Mathf.InverseLerp(PhasePower, PhaseRelease, phase));
        }

        if (phase < PhaseRecovery)
        {
            return SmoothLerp(release, recovery, Mathf.InverseLerp(PhaseRelease, PhaseRecovery, phase));
        }

        if (phase < PhasePreparation)
        {
            return SmoothLerp(recovery, preparation, Mathf.InverseLerp(PhaseRecovery, PhasePreparation, phase));
        }

        return SmoothLerp(preparation, polePlant, Mathf.InverseLerp(PhasePreparation, 1f, phase));
    }

    private static float SmoothLerp(float from, float to, float value)
    {
        return Mathf.Lerp(from, to, Smooth01(value));
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
