using UnityEngine;

[DisallowMultipleComponent]
public class RollerSkierAnimator : MonoBehaviour
{
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
        var drive = Mathf.Sin(phase * Mathf.PI);
        return Mathf.Lerp(-58f, 34f, drive);
    }

    public static float CalculatePolePitch(float phase)
    {
        var drive = Mathf.Sin(phase * Mathf.PI);
        return Mathf.Lerp(24f, -34f, drive);
    }

    public static float CalculateTorsoPitch(float phase)
    {
        var drive = Mathf.Sin(phase * Mathf.PI);
        return Mathf.Lerp(12f, 22f, drive);
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
            leftArm.localRotation = Quaternion.Euler(armPitch, -12f, -8f);
        }

        if (rightArm != null)
        {
            rightArm.localRotation = Quaternion.Euler(armPitch, 12f, 8f);
        }

        if (leftPole != null)
        {
            leftPole.localRotation = Quaternion.Euler(polePitch, -4f, 10f);
        }

        if (rightPole != null)
        {
            rightPole.localRotation = Quaternion.Euler(polePitch, 4f, -10f);
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
}
