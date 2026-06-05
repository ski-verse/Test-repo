using NUnit.Framework;
using UnityEngine;

public class RollerSkierDoublePolingAnimationTests
{
    [Test]
    public void DoublePolingCycle_UsesSafeReadableArmMovement()
    {
        var recoveryArmPitch = RollerSkierAnimator.CalculateArmPitch(0.05f);
        var plantArmPitch = RollerSkierAnimator.CalculateArmPitch(0.42f);
        var returnArmPitch = RollerSkierAnimator.CalculateArmPitch(0.85f);

        Assert.Less(recoveryArmPitch, -45f);
        Assert.Greater(plantArmPitch, 25f);
        Assert.Less(returnArmPitch, -45f);
        Assert.Less(plantArmPitch, 45f);
        Assert.Greater(plantArmPitch - returnArmPitch, 75f);
    }

    [Test]
    public void DoublePolingCycle_KeepsTorsoUprightWithModerateForwardLean()
    {
        var recoveryTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.05f);
        var plantTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.42f);
        var returnTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.85f);

        Assert.GreaterOrEqual(recoveryTorsoPitch, 8f);
        Assert.Less(recoveryTorsoPitch, 13f);
        Assert.Greater(plantTorsoPitch, 25f);
        Assert.Less(plantTorsoPitch, 35f);
        Assert.Less(returnTorsoPitch, 12f);
    }

    [Test]
    public void DoublePolingCycle_KeepsPolesReadableWithoutExtremeSwing()
    {
        var recoveryPolePitch = RollerSkierAnimator.CalculatePolePitch(0.05f);
        var plantPolePitch = RollerSkierAnimator.CalculatePolePitch(0.42f);
        var returnPolePitch = RollerSkierAnimator.CalculatePolePitch(0.85f);

        Assert.Greater(recoveryPolePitch, 25f);
        Assert.Less(plantPolePitch, -30f);
        Assert.Greater(returnPolePitch, 30f);
        Assert.Less(returnPolePitch, 45f);
        Assert.Greater(recoveryPolePitch - plantPolePitch, 60f);
    }

    [Test]
    public void ApplyPose_DoesNotMoveRigPivotsOutOfHierarchy()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftArm = CreateChild(root.transform, "Left Arm Pivot", new Vector3(-0.31f, 1.46f, -0.04f));
        animator.rightArm = CreateChild(root.transform, "Right Arm Pivot", new Vector3(0.31f, 1.46f, -0.04f));
        animator.leftPole = CreateChild(root.transform, "Left Pole Pivot", new Vector3(-0.48f, 0.9f, 0.16f));
        animator.rightPole = CreateChild(root.transform, "Right Pole Pivot", new Vector3(0.48f, 0.9f, 0.16f));
        animator.torso = CreateChild(root.transform, "Torso Pivot", new Vector3(0f, 1.08f, 0.04f));

        var leftArmPosition = animator.leftArm.localPosition;
        var rightArmPosition = animator.rightArm.localPosition;
        var leftPolePosition = animator.leftPole.localPosition;
        var rightPolePosition = animator.rightPole.localPosition;
        var torsoPosition = animator.torso.localPosition;

        animator.ApplyPose(0.42f);
        animator.ApplyPose(0.85f);

        AssertVector3Approximately(leftArmPosition, animator.leftArm.localPosition);
        AssertVector3Approximately(rightArmPosition, animator.rightArm.localPosition);
        AssertVector3Approximately(leftPolePosition, animator.leftPole.localPosition);
        AssertVector3Approximately(rightPolePosition, animator.rightPole.localPosition);
        AssertVector3Approximately(torsoPosition, animator.torso.localPosition);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void ApplyPose_KeepsArmsTogetherPolesParallelAndSkisStable()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftArm = CreateChild(root.transform, "Left Arm Pivot", new Vector3(-0.31f, 1.46f, -0.04f));
        animator.rightArm = CreateChild(root.transform, "Right Arm Pivot", new Vector3(0.31f, 1.46f, -0.04f));
        animator.leftPole = CreateChild(root.transform, "Left Pole Pivot", new Vector3(-0.48f, 0.9f, 0.16f));
        animator.rightPole = CreateChild(root.transform, "Right Pole Pivot", new Vector3(0.48f, 0.9f, 0.16f));
        animator.torso = CreateChild(root.transform, "Torso Pivot", new Vector3(0f, 1.08f, 0.04f));
        animator.leftSki = CreateChild(root.transform, "Left Ski", new Vector3(-0.24f, 0f, 0.22f));
        animator.rightSki = CreateChild(root.transform, "Right Ski", new Vector3(0.24f, 0f, 0.22f));

        animator.ApplyPose(0.42f);

        Assert.AreEqual(animator.leftArm.localEulerAngles.x, animator.rightArm.localEulerAngles.x, 0.001f);
        Assert.AreEqual(animator.leftPole.localEulerAngles.x, animator.rightPole.localEulerAngles.x, 0.001f);
        Assert.AreEqual(0f, animator.leftPole.localEulerAngles.y, 0.001f);
        Assert.AreEqual(0f, animator.rightPole.localEulerAngles.y, 0.001f);
        Assert.AreEqual(Quaternion.identity, animator.leftSki.localRotation);
        Assert.AreEqual(Quaternion.identity, animator.rightSki.localRotation);
        Assert.Greater(Mathf.DeltaAngle(0f, animator.torso.localEulerAngles.x), 25f);
        Assert.Greater(Mathf.DeltaAngle(0f, animator.leftArm.localEulerAngles.x), 25f);
        Assert.Less(Mathf.DeltaAngle(0f, animator.leftPole.localEulerAngles.x), -30f);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void ApplyPose_KeepsArmsCloseToBodyWithMinimalOutwardSwing()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftArm = CreateChild(root.transform, "Left Arm Pivot", new Vector3(-0.31f, 1.46f, -0.04f));
        animator.rightArm = CreateChild(root.transform, "Right Arm Pivot", new Vector3(0.31f, 1.46f, -0.04f));
        animator.leftPole = CreateChild(root.transform, "Left Pole Pivot", new Vector3(-0.48f, 0.9f, 0.16f));
        animator.rightPole = CreateChild(root.transform, "Right Pole Pivot", new Vector3(0.48f, 0.9f, 0.16f));

        animator.ApplyPose(0.42f);

        Assert.AreEqual(-1.5f, Mathf.DeltaAngle(0f, animator.leftArm.localEulerAngles.y), 0.001f);
        Assert.AreEqual(1.5f, Mathf.DeltaAngle(0f, animator.rightArm.localEulerAngles.y), 0.001f);
        Assert.AreEqual(-1f, Mathf.DeltaAngle(0f, animator.leftArm.localEulerAngles.z), 0.001f);
        Assert.AreEqual(1f, Mathf.DeltaAngle(0f, animator.rightArm.localEulerAngles.z), 0.001f);
        Assert.AreEqual(animator.leftPole.localRotation, animator.rightPole.localRotation);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void CalculateNextPhase_ScalesCycleSpeedWithMovementSpeed()
    {
        var slowPhase = RollerSkierAnimator.CalculateNextPhase(0f, 0f, 0.5f, 0.65f, 0.018f);
        var fastPhase = RollerSkierAnimator.CalculateNextPhase(0f, 72f, 0.5f, 0.65f, 0.018f);

        Assert.AreEqual(0.325f, slowPhase, 0.001f);
        Assert.AreEqual(0.973f, fastPhase, 0.001f);
        Assert.Greater(fastPhase, slowPhase);
    }

    private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent, false);
        child.localPosition = localPosition;
        return child;
    }

    private static void AssertVector3Approximately(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.x, actual.x, 0.001f);
        Assert.AreEqual(expected.y, actual.y, 0.001f);
        Assert.AreEqual(expected.z, actual.z, 0.001f);
    }
}
