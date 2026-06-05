using NUnit.Framework;
using UnityEngine;

public class RollerSkierDoublePolingAnimationTests
{
    [Test]
    public void DoublePolingCycle_HasObviousPlantAndStrongRecoveryPhases()
    {
        var recoveryArmPitch = RollerSkierAnimator.CalculateArmPitch(0.05f);
        var plantArmPitch = RollerSkierAnimator.CalculateArmPitch(0.42f);
        var returnArmPitch = RollerSkierAnimator.CalculateArmPitch(0.85f);

        Assert.Less(recoveryArmPitch, -70f);
        Assert.Greater(plantArmPitch, 55f);
        Assert.Less(returnArmPitch, -75f);
        Assert.Greater(plantArmPitch - returnArmPitch, 130f);
    }

    [Test]
    public void DoublePolingCycle_LeansBodyFurtherForwardDuringPolePlant()
    {
        var recoveryTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.05f);
        var plantTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.42f);
        var returnTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.85f);

        Assert.Less(recoveryTorsoPitch, 17f);
        Assert.Greater(plantTorsoPitch, 40f);
        Assert.Less(returnTorsoPitch, 18f);
    }

    [Test]
    public void DoublePolingCycle_MakesPolePlantAndRecoveryVisible()
    {
        var recoveryPolePitch = RollerSkierAnimator.CalculatePolePitch(0.05f);
        var plantPolePitch = RollerSkierAnimator.CalculatePolePitch(0.42f);
        var returnPolePitch = RollerSkierAnimator.CalculatePolePitch(0.85f);

        Assert.Greater(recoveryPolePitch, 45f);
        Assert.Less(plantPolePitch, -65f);
        Assert.Greater(returnPolePitch, 45f);
        Assert.Greater(recoveryPolePitch - plantPolePitch, 110f);
    }

    [Test]
    public void PowerPhase_KeepsArmsCloseAndMovesHandsTowardKnees()
    {
        var recoveryLeftHand = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.05f);
        var plantLeftHand = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.42f);
        var plantRightHand = RollerSkierAnimator.CalculateArmPivotPosition(1f, 0.42f);

        Assert.Less(Mathf.Abs(plantLeftHand.x), 0.23f);
        Assert.AreEqual(-plantLeftHand.x, plantRightHand.x, 0.001f);
        Assert.Less(plantLeftHand.y, recoveryLeftHand.y - 0.3f);
        Assert.Greater(plantLeftHand.z, recoveryLeftHand.z + 0.2f);
    }

    [Test]
    public void PowerPhase_PlantsBothPolesInFrontAndSymmetrically()
    {
        var recoveryLeftPole = RollerSkierAnimator.CalculatePolePivotPosition(-1f, 0.05f);
        var plantLeftPole = RollerSkierAnimator.CalculatePolePivotPosition(-1f, 0.42f);
        var plantRightPole = RollerSkierAnimator.CalculatePolePivotPosition(1f, 0.42f);

        Assert.Less(Mathf.Abs(plantLeftPole.x), 0.3f);
        Assert.AreEqual(-plantLeftPole.x, plantRightPole.x, 0.001f);
        Assert.AreEqual(plantLeftPole.y, plantRightPole.y, 0.001f);
        Assert.Greater(plantLeftPole.z, recoveryLeftPole.z + 0.25f);
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
        Assert.AreEqual(0f, animator.leftPole.localEulerAngles.y, 0.001f);
        Assert.AreEqual(0f, animator.rightPole.localEulerAngles.y, 0.001f);
        Assert.AreEqual(-animator.leftArm.localPosition.x, animator.rightArm.localPosition.x, 0.001f);
        Assert.AreEqual(-animator.leftPole.localPosition.x, animator.rightPole.localPosition.x, 0.001f);
        Assert.Greater(animator.torso.localEulerAngles.x, 40f);

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
