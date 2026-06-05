using NUnit.Framework;
using UnityEngine;

public class RollerSkierDoublePolingAnimationTests
{
    [Test]
    public void DoublePolingCycle_HasVeryObviousPlantAndRecoveryPhases()
    {
        var recoveryArmPitch = RollerSkierAnimator.CalculateArmPitch(0.05f);
        var plantArmPitch = RollerSkierAnimator.CalculateArmPitch(0.42f);
        var returnArmPitch = RollerSkierAnimator.CalculateArmPitch(0.85f);

        Assert.Less(recoveryArmPitch, -100f);
        Assert.Greater(plantArmPitch, 80f);
        Assert.Less(returnArmPitch, -105f);
        Assert.Greater(plantArmPitch - returnArmPitch, 185f);
    }

    [Test]
    public void DoublePolingCycle_LeansBodySignificantlyFurtherForwardDuringPolePlant()
    {
        var recoveryTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.05f);
        var plantTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.42f);
        var returnTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(0.85f);

        Assert.Less(recoveryTorsoPitch, 17f);
        Assert.Greater(plantTorsoPitch, 58f);
        Assert.Less(returnTorsoPitch, 18f);
    }

    [Test]
    public void DoublePolingCycle_MakesPolePlantAndRecoveryMuchMoreVisible()
    {
        var recoveryPolePitch = RollerSkierAnimator.CalculatePolePitch(0.05f);
        var plantPolePitch = RollerSkierAnimator.CalculatePolePitch(0.42f);
        var returnPolePitch = RollerSkierAnimator.CalculatePolePitch(0.85f);

        Assert.Greater(recoveryPolePitch, 70f);
        Assert.Less(plantPolePitch, -95f);
        Assert.Greater(returnPolePitch, 70f);
        Assert.Greater(recoveryPolePitch - plantPolePitch, 165f);
    }

    [Test]
    public void PowerPhase_DoublesVisibleHandTravelTowardKnees()
    {
        var recoveryLeftHand = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.05f);
        var plantLeftHand = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.42f);
        var plantRightHand = RollerSkierAnimator.CalculateArmPivotPosition(1f, 0.42f);

        Assert.Less(Mathf.Abs(plantLeftHand.x), 0.23f);
        Assert.AreEqual(-plantLeftHand.x, plantRightHand.x, 0.001f);
        Assert.Less(plantLeftHand.y, recoveryLeftHand.y - 0.72f);
        Assert.Greater(plantLeftHand.z, recoveryLeftHand.z + 0.52f);
    }

    [Test]
    public void PowerPhase_PlantsBothPolesClearlyInFrontAndSymmetrically()
    {
        var recoveryLeftPole = RollerSkierAnimator.CalculatePolePivotPosition(-1f, 0.05f);
        var plantLeftPole = RollerSkierAnimator.CalculatePolePivotPosition(-1f, 0.42f);
        var plantRightPole = RollerSkierAnimator.CalculatePolePivotPosition(1f, 0.42f);

        Assert.Less(Mathf.Abs(plantLeftPole.x), 0.3f);
        Assert.AreEqual(-plantLeftPole.x, plantRightPole.x, 0.001f);
        Assert.AreEqual(plantLeftPole.y, plantRightPole.y, 0.001f);
        Assert.Greater(plantLeftPole.z, recoveryLeftPole.z + 0.58f);
    }

    [Test]
    public void DrivePhase_CompressesUpperBodyAndTransfersWeightIntoPoles()
    {
        var recoveryTorso = RollerSkierAnimator.CalculateTorsoPivotPosition(0.05f);
        var driveTorso = RollerSkierAnimator.CalculateTorsoPivotPosition(0.42f);
        var recoveryArm = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.05f);
        var driveArm = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.42f);
        var recoveryPole = RollerSkierAnimator.CalculatePolePivotPosition(-1f, 0.05f);
        var drivePole = RollerSkierAnimator.CalculatePolePivotPosition(-1f, 0.42f);

        Assert.Less(driveTorso.y, recoveryTorso.y - 0.16f);
        Assert.Greater(driveTorso.z, recoveryTorso.z + 0.1f);
        Assert.AreEqual(driveArm.z - recoveryArm.z, drivePole.z - recoveryPole.z, 0.08f);
    }

    [Test]
    public void PlantCurve_EasesSmoothlyAroundTheDriveInsteadOfSnapping()
    {
        var beforeDrive = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.34f);
        var drive = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.42f);
        var afterDrive = RollerSkierAnimator.CalculateArmPivotPosition(-1f, 0.5f);

        Assert.Greater(drive.z - beforeDrive.z, 0.05f);
        Assert.Less(drive.z - beforeDrive.z, 0.25f);
        Assert.Less(afterDrive.z - drive.z, 0.18f);
        Assert.Greater(afterDrive.y, drive.y - 0.08f);
    }

    [Test]
    public void ApplyPose_KeepsArmsTogetherPolesParallelAndTorsoCompressed()
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
        Assert.Greater(animator.torso.localEulerAngles.x, 58f);
        Assert.Less(animator.torso.localPosition.y, -0.16f);

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
