using NUnit.Framework;
using UnityEngine;

public class RollerSkierDoublePolingAnimationTests
{
    [Test]
    public void DoublePolingCycle_HasPlantAndReturnPhases()
    {
        var recoveryArmPitch = RollerSkierAnimator.CalculateArmPitch(0.05f);
        var plantArmPitch = RollerSkierAnimator.CalculateArmPitch(0.42f);
        var returnArmPitch = RollerSkierAnimator.CalculateArmPitch(0.85f);

        Assert.Less(recoveryArmPitch, -45f);
        Assert.Greater(plantArmPitch, 30f);
        Assert.Less(returnArmPitch, -35f);
        Assert.Greater(plantArmPitch - returnArmPitch, 65f);
    }

    [Test]
    public void DoublePolingCycle_LeansBodyForwardDuringPolePlant()
    {
        var recoveryTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.05f);
        var plantTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.42f);
        var returnTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.85f);

        Assert.Less(recoveryTorsoPitch, 15f);
        Assert.Greater(plantTorsoPitch, 27f);
        Assert.Less(returnTorsoPitch, 18f);
    }

    [Test]
    public void ApplyPose_KeepsArmsTogetherAndPolesParallel()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftArm = new GameObject("Left Arm Pivot").transform;
        animator.rightArm = new GameObject("Right Arm Pivot").transform;
        animator.leftPole = new GameObject("Left Pole Pivot").transform;
        animator.rightPole = new GameObject("Right Pole Pivot").transform;
        animator.torso = new GameObject("Torso Pivot").transform;

        animator.leftArm.SetParent(root.transform);
        animator.rightArm.SetParent(root.transform);
        animator.leftPole.SetParent(root.transform);
        animator.rightPole.SetParent(root.transform);
        animator.torso.SetParent(root.transform);

        animator.ApplyPose(0.42f);

        Assert.AreEqual(animator.leftArm.localEulerAngles.x, animator.rightArm.localEulerAngles.x, 0.001f);
        Assert.AreEqual(animator.leftPole.localEulerAngles.x, animator.rightPole.localEulerAngles.x, 0.001f);
        Assert.AreEqual(-animator.leftPole.localEulerAngles.y, animator.rightPole.localEulerAngles.y, 0.001f);
        Assert.Greater(animator.torso.localEulerAngles.x, 27f);

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
}
