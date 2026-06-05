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
        return CalculateBodyWeightTransfer(phase) * 0.16f;
    }

    public static float CalculateToeRise(float phase)
    {
        return CalculatePolePressure(phase) * (1f - CalculateReturnLift(phase) * 0.35f);
    }

    public static float CalculateBodyWeightTransfer(float phase)
    {
        return CalculatePlantAmount(phase) * (1f - CalculateReturnLift(phase) * 0.18f);
    }

    public static float CalculatePolePressure(float phase)
    {
        return CalculatePlantAmount(phase) * (1f - CalculateReturnLift(phase) * 0.35f);
    }

    public static float CalculateTorsoForwardDrive(float phase)
    {
        return CalculateBodyWeightTransfer(phase) * 0.18f - CalculateReturnLift(phase) * 0.03f;
    }

    public static float CalculateHipHingeForwardDrive(float phase)
    {
        return CalculateBodyWeightTransfer(phase) * 0.11f - CalculateReturnLift(phase) * 0.018f;
    }

    public static float CalculateHandForwardDrive(float phase)
    {
        return CalculatePolePressure(phase) * 0.085f;
    }

    public static float CalculateHandOutwardDrift(float phase)
    {
        return CalculatePolePressure(phase) * 0.012f;
    }

    public static float CalculateHandRecoveryLift(float phase)
    {
        return CalculateReturnLift(phase) * 0.08f;
    }

    public static float CalculatePolePlantForwardOffset(float phase)
    {
        return CalculatePolePressure(phase) * 0.09f;
    }

    public static float CalculateHeadCounterPitch(float phase)
    {
        return -CalculateTorsoPitch(phase) * 0.45f;
    }

    public static float CalculateArmPitch(float phase)
    {
        var polePressure = CalculatePolePressure(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(-36f, 20f, polePressure) - returnLift * 4f;
    }

    public static float CalculatePolePitch(float phase)
    {
        var polePressure = CalculatePolePressure(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(22f, -42f, polePressure) + returnLift * 5f;
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var bodyWeightTransfer = CalculateBodyWeightTransfer(phase);
        var returnLift = CalculateReturnLift(phase);
        return Mathf.Lerp(9f, 36f, bodyWeightTransfer) - returnLift * 5f;
    }

    public void ApplyPose(float posePhase)
    {
        CaptureBasePoseIfNeeded();

        var bodyWeightTransfer = CalculateBodyWeightTransfer(posePhase);
        var polePressure = CalculatePolePressure(posePhase);
        var returnLift = CalculateReturnLift(posePhase);
        var compression = CalculateBodyCompression(posePhase);
        var toeRise = CalculateToeRise(posePhase);
        var armPitch = CalculateArmPitch(posePhase);
        var polePitch = CalculatePolePitch(posePhase);
        var torsoPitch = CalculateTorsoPitch(posePhase);
        var recoveryExtension = returnLift * 0.05f;
        var handDrive = CalculateHandForwardDrive(posePhase);
        var handOutwardDrift = CalculateHandOutwardDrift(posePhase);
        var handRecoveryLift = CalculateHandRecoveryLift(posePhase);
        var polePlantForwardOffset = CalculatePolePlantForwardOffset(posePhase);
        var torsoForwardDrive = CalculateTorsoForwardDrive(posePhase);
        var hipForwardDrive = CalculateHipHingeForwardDrive(posePhase);

        if (hips != null)
        {
            hips.localPosition = hipsBasePosition + new Vector3(0f, -compression * 0.18f + recoveryExtension * 0.2f, hipForwardDrive);
            hips.localRotation = Quaternion.Euler(-6f + bodyWeightTransfer * 4f - returnLift * 1.5f, 0f, 0f);
        }

        if (torso != null)
        {
            torso.localPosition = torsoBasePosition + new Vector3(0f, -compression * 0.3f + recoveryExtension, torsoForwardDrive);
            torso.localRotation = Quaternion.Euler(torsoPitch, 0f, 0f);
        }

        if (head != null)
        {
            head.localPosition = headBasePosition + new Vector3(0f, compression * 0.12f, -bodyWeightTransfer * 0.015f);
            head.localRotation = Quaternion.Euler(CalculateHeadCounterPitch(posePhase), 0f, 0f);
        }

        if (leftArm != null)
        {
            leftArm.localPosition = leftArmBasePosition + new Vector3(handOutwardDrift, -compression * 0.42f + handRecoveryLift, handDrive + polePlantForwardOffset * 0.35f);
            leftArm.localRotation = Quaternion.Euler(armPitch, -0.5f, -0.3f);
        }

        if (rightArm != null)
        {
            rightArm.localPosition = rightArmBasePosition + new Vector3(-handOutwardDrift, -compression * 0.42f + handRecoveryLift, handDrive + polePlantForwardOffset * 0.35f);
            rightArm.localRotation = Quaternion.Euler(armPitch, 0.5f, 0.3f);
        }

        if (leftHand != null)
        {
            leftHand.localPosition = leftHandBasePosition + new Vector3(handOutwardDrift * 0.5f, -compression * 0.25f + handRecoveryLift * 0.35f, handDrive + polePlantForwardOffset);
        }

        if (rightHand != null)
        {
            rightHand.localPosition = rightHandBasePosition + new Vector3(-handOutwardDrift * 0.5f, -compression * 0.25f + handRecoveryLift * 0.35f, handDrive + polePlantForwardOffset);
        }

        AttachPoleToHand(leftPole, leftHand, polePitch);
        AttachPoleToHand(rightPole, rightHand, polePitch);

        if (leftThigh != null)
        {
            leftThigh.localRotation = Quaternion.Euler(-20f - polePressure * 4f + returnLift * 2f, 0f, 3f);
        }

        if (rightThigh != null)
        {
            rightThigh.localRotation = Quaternion.Euler(-20f - polePressure * 4f + returnLift * 2f, 0f, -3f);
        }

        if (leftShin != null)
        {
            leftShin.localRotation = Quaternion.Euler(8f + polePressure * 3f - returnLift * 1.2f, 0f, -2.5f);
        }

        if (rightShin != null)
        {
            rightShin.localRotation = Quaternion.Euler(8f + polePressure * 3f - returnLift * 1.2f, 0f, 2.5f);
        }

        var footPitch = -toeRise * 2.4f;
        var footRise = toeRise * 0.012f;
        var footDrive = polePressure * 0.014f;
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

        var skiPitch = -toeRise * 0.45f;
        var skiRise = toeRise * 0.003f;
        var skiDrive = polePressure * 0.007f;
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
