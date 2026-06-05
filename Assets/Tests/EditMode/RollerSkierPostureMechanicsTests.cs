using NUnit.Framework;
using UnityEngine;

public class RollerSkierPostureMechanicsTests
{
    [Test]
    public void CalculatePostureMechanics_DrivePhaseMovesBodyWeightOverPoles()
    {
        var drivePhase = 0.42f;
        var recoveryPhase = 0.82f;

        var torsoDrive = RollerSkierAnimator.CalculateTorsoForwardDrive(drivePhase);
        var hipDrive = RollerSkierAnimator.CalculateHipHingeForwardDrive(drivePhase);
        var handDrive = RollerSkierAnimator.CalculateHandForwardDrive(drivePhase);

        Assert.Greater(torsoDrive, 0.21f);
        Assert.Greater(hipDrive, 0.14f);
        Assert.Greater(torsoDrive, handDrive * 3f);
        Assert.Greater(hipDrive, handDrive * 2f);
        Assert.Greater(torsoDrive, RollerSkierAnimator.CalculateTorsoForwardDrive(recoveryPhase));
    }

    [Test]
    public void CalculatePostureMechanics_UsesVisibleCompressionWithoutArmDominance()
    {
        var drivePhase = 0.42f;
        var recoveryPhase = 0.82f;

        Assert.Greater(RollerSkierAnimator.CalculateBodyCompression(drivePhase), 0.17f);
        Assert.Greater(RollerSkierAnimator.CalculateTorsoPitch(drivePhase), 39f);
        Assert.Less(Mathf.Abs(RollerSkierAnimator.CalculateArmPitch(drivePhase) - RollerSkierAnimator.CalculateArmPitch(0.1f)), 44f);
        Assert.Greater(RollerSkierAnimator.CalculateBodyCompression(drivePhase), RollerSkierAnimator.CalculateBodyCompression(recoveryPhase));
    }

    [Test]
    public void ApplyPose_ShowsBodyGeneratedPowerWithStableHead()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.torso = new GameObject("Torso Pivot").transform;
        animator.hips = new GameObject("Forward Hinged Athletic Hips").transform;
        animator.head = new GameObject("Head Stabilizer").transform;
        animator.leftArm = new GameObject("Left Connected Arm").transform;
        animator.leftHand = new GameObject("Left Hand").transform;

        animator.torso.SetParent(root.transform, false);
        animator.hips.SetParent(root.transform, false);
        animator.head.SetParent(animator.torso, false);
        animator.leftArm.SetParent(root.transform, false);
        animator.leftHand.SetParent(animator.leftArm, false);

        animator.torso.localPosition = new Vector3(0f, 1.105f, 0.085f);
        animator.hips.localPosition = new Vector3(0f, 0.955f, 0.105f);
        animator.head.localPosition = new Vector3(0f, 0.79f, -0.19f);
        animator.leftArm.localPosition = new Vector3(-0.305f, 1.49f, -0.08f);
        animator.leftHand.localPosition = new Vector3(-0.04f, -0.895f, 0.43f);

        animator.ApplyPose(0.42f);

        Assert.Greater(animator.torso.localPosition.z - 0.085f, 0.21f);
        Assert.Greater(animator.hips.localPosition.z - 0.105f, 0.14f);
        Assert.Less(animator.torso.localPosition.y - 1.105f, -0.055f);
        Assert.Greater(animator.torso.localPosition.z - 0.085f, Mathf.Abs(animator.leftArm.localPosition.z - -0.08f) * 2.4f);
        Assert.Less(Mathf.Abs(animator.head.localPosition.y - 0.79f), 0.025f);

        Object.DestroyImmediate(root);
    }
}
