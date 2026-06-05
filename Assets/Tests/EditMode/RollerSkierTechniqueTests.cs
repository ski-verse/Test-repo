using NUnit.Framework;
using UnityEngine;

public class RollerSkierTechniqueTests
{
    [Test]
    public void CalculateBodyCompression_PeaksDuringDrivePhase()
    {
        var setupCompression = RollerSkierAnimator.CalculateBodyCompression(0.1f);
        var driveCompression = RollerSkierAnimator.CalculateBodyCompression(0.42f);
        var recoveryCompression = RollerSkierAnimator.CalculateBodyCompression(0.8f);

        Assert.Greater(driveCompression, setupCompression);
        Assert.Greater(driveCompression, recoveryCompression);
    }

    [Test]
    public void CalculateToeRise_PeaksNearPolePlantAndReturnsInRecovery()
    {
        var setupToeRise = RollerSkierAnimator.CalculateToeRise(0.1f);
        var plantToeRise = RollerSkierAnimator.CalculateToeRise(0.42f);
        var recoveryToeRise = RollerSkierAnimator.CalculateToeRise(0.82f);

        Assert.Greater(plantToeRise, setupToeRise);
        Assert.Greater(plantToeRise, recoveryToeRise);
    }

    [Test]
    public void CalculateSkiClassicsBiomechanics_UsesCoreDriveMoreThanArmSwing()
    {
        var drivePhase = 0.42f;
        var recoveryPhase = 0.82f;

        Assert.Greater(RollerSkierAnimator.CalculateBodyWeightTransfer(drivePhase), RollerSkierAnimator.CalculateBodyWeightTransfer(recoveryPhase));
        Assert.Greater(RollerSkierAnimator.CalculateTorsoForwardDrive(drivePhase), RollerSkierAnimator.CalculateHandForwardDrive(drivePhase) * 1.6f);
        Assert.Greater(RollerSkierAnimator.CalculateHipHingeForwardDrive(drivePhase), RollerSkierAnimator.CalculateHandForwardDrive(drivePhase));
        Assert.Less(RollerSkierAnimator.CalculateHandOutwardDrift(drivePhase), 0.02f);
    }

    [Test]
    public void ApplyPose_UsesStableHeadDuringStrongTorsoLean()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.torso = new GameObject("Torso Pivot").transform;
        animator.head = new GameObject("Head Stabilizer").transform;

        animator.torso.SetParent(root.transform, false);
        animator.head.SetParent(animator.torso, false);
        animator.torso.localPosition = new Vector3(0f, 1.105f, 0.085f);
        animator.head.localPosition = new Vector3(0f, 0.79f, -0.19f);

        animator.ApplyPose(0.42f);

        var torsoPitch = NormalizeAngle(animator.torso.localEulerAngles.x);
        var headCounterPitch = NormalizeAngle(animator.head.localEulerAngles.x);
        Assert.Greater(torsoPitch, 30f);
        Assert.Less(headCounterPitch, -10f);
        Assert.Less(Mathf.Abs(animator.head.localPosition.y - 0.79f), 0.03f);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void CalculatePolePlant_StaysInFrontWithEfficientHandPath()
    {
        var drivePhase = 0.42f;
        var recoveryPhase = 0.82f;

        Assert.Greater(RollerSkierAnimator.CalculatePolePlantForwardOffset(drivePhase), 0.07f);
        Assert.Greater(RollerSkierAnimator.CalculatePolePressure(drivePhase), RollerSkierAnimator.CalculatePolePressure(recoveryPhase));
        Assert.Less(RollerSkierAnimator.CalculateHandRecoveryLift(drivePhase), RollerSkierAnimator.CalculateHandRecoveryLift(recoveryPhase));
    }

    [Test]
    public void ProperRollerSkierVisual_UsesVisibleGameplayCameraPoles()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftRadius, 0.027f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleGripRadius, 0.06f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleLateralOffset, 0.12f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleTipLateralOffset, 0.28f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftLength, 1.38f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftLength, 1.45f);
        Assert.Less(CalculateLuminance(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftColor), 0.08f);
        Assert.Greater(CalculateLuminance(ProperRollerSkierRuntimeUpdater.PoleHighlightColor) - CalculateLuminance(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftColor), 0.75f);
    }

    [Test]
    public void ApplyPose_KeepsPolesAttachedToHands()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftHand = new GameObject("Left Hand").transform;
        animator.rightHand = new GameObject("Right Hand").transform;
        animator.leftPole = new GameObject("Left Pole").transform;
        animator.rightPole = new GameObject("Right Pole").transform;

        animator.leftHand.SetParent(root.transform, false);
        animator.rightHand.SetParent(root.transform, false);
        animator.leftPole.SetParent(root.transform, false);
        animator.rightPole.SetParent(root.transform, false);
        animator.leftHand.localPosition = new Vector3(-0.18f, 0.7f, 0.2f);
        animator.rightHand.localPosition = new Vector3(0.18f, 0.7f, 0.2f);

        animator.ApplyPose(0.42f);

        Assert.AreEqual(animator.leftHand.position, animator.leftPole.position);
        Assert.AreEqual(animator.rightHand.position, animator.rightPole.position);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void ApplyPose_AddsCompressionWithoutBreakingStableSkiAlignment()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.torso = new GameObject("Torso Pivot").transform;
        animator.hips = new GameObject("Hips Pivot").transform;
        animator.leftSki = new GameObject("Left Roller Ski").transform;
        animator.rightSki = new GameObject("Right Roller Ski").transform;

        animator.torso.SetParent(root.transform, false);
        animator.hips.SetParent(root.transform, false);
        animator.leftSki.SetParent(root.transform, false);
        animator.rightSki.SetParent(root.transform, false);

        var neutralTorsoPosition = animator.torso.localPosition;
        animator.ApplyPose(0.42f);

        Assert.Less(animator.torso.localPosition.y, neutralTorsoPosition.y);
        Assert.Greater(animator.torso.localPosition.z, neutralTorsoPosition.z);
        Assert.AreEqual(animator.leftSki.localEulerAngles.x, animator.rightSki.localEulerAngles.x, 0.001f);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void ApplyPose_PreservesTorsoHipClearanceDuringDrive()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.torso = new GameObject("Torso Pivot").transform;
        animator.hips = new GameObject("Forward Hinged Athletic Hips").transform;

        animator.torso.SetParent(root.transform, false);
        animator.hips.SetParent(root.transform, false);
        animator.torso.localPosition = new Vector3(0f, 1.105f, 0.085f);
        animator.hips.localPosition = new Vector3(0f, 0.955f, 0.105f);

        animator.ApplyPose(0.42f);

        Assert.Greater(animator.torso.localPosition.y, animator.hips.localPosition.y);
        Assert.Greater(animator.torso.localPosition.y - animator.hips.localPosition.y, 0.12f);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void ApplyPose_FeetDriveStableRollerSkis()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftFoot = new GameObject("Left Boot").transform;
        animator.rightFoot = new GameObject("Right Boot").transform;
        animator.leftSki = new GameObject("Left Parallel Roller Ski").transform;
        animator.rightSki = new GameObject("Right Parallel Roller Ski").transform;

        animator.leftFoot.SetParent(root.transform, false);
        animator.rightFoot.SetParent(root.transform, false);
        animator.leftSki.SetParent(root.transform, false);
        animator.rightSki.SetParent(root.transform, false);
        animator.leftFoot.localPosition = new Vector3(-0.24f, 0.15f, 0.1f);
        animator.rightFoot.localPosition = new Vector3(0.24f, 0.15f, 0.1f);
        animator.leftSki.localPosition = new Vector3(-0.24f, 0f, 0.13f);
        animator.rightSki.localPosition = new Vector3(0.24f, 0f, 0.13f);

        animator.ApplyPose(0.42f);

        var leftFootPitch = Mathf.Abs(NormalizeAngle(animator.leftFoot.localEulerAngles.x));
        var leftSkiPitch = Mathf.Abs(NormalizeAngle(animator.leftSki.localEulerAngles.x));
        Assert.Greater(leftFootPitch, leftSkiPitch * 2f);
        Assert.AreEqual(animator.leftSki.localEulerAngles.x, animator.rightSki.localEulerAngles.x, 0.001f);
        Assert.Less(Mathf.Abs(animator.leftSki.localPosition.y), 0.01f);
        Assert.Less(Mathf.Abs(animator.rightSki.localPosition.y), 0.01f);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void ResetBasePose_RecapturesLateAssignedHandPosition()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();

        animator.ApplyPose(0.15f);

        animator.leftHand = new GameObject("Left Hand").transform;
        animator.leftPole = new GameObject("Left Pole").transform;
        animator.leftHand.SetParent(root.transform, false);
        animator.leftPole.SetParent(root.transform, false);
        animator.leftHand.localPosition = new Vector3(-0.2f, 0.72f, 0.28f);

        animator.ResetBasePose();
        animator.ApplyPose(0.1f);

        Assert.AreEqual(new Vector3(-0.2f, 0.72f, 0.28f), animator.leftHand.localPosition);
        Assert.AreEqual(animator.leftHand.position, animator.leftPole.position);

        Object.DestroyImmediate(root);
    }

    [Test]
    public void SkierTechniqueRuntimeUpdater_AttachesExistingPolesToExistingHands()
    {
        var root = new GameObject("Low Poly Roller Skier");
        var animator = root.AddComponent<RollerSkierAnimator>();
        var leftArm = new GameObject("Left Double-Poling Arm").transform;
        var rightArm = new GameObject("Right Double-Poling Arm").transform;
        var leftHand = new GameObject("Hand").transform;
        var rightHand = new GameObject("Hand").transform;
        var leftPole = new GameObject("Left Carbon Pole").transform;
        var rightPole = new GameObject("Right Carbon Pole").transform;

        leftArm.SetParent(root.transform, false);
        rightArm.SetParent(root.transform, false);
        leftHand.SetParent(leftArm, false);
        rightHand.SetParent(rightArm, false);
        leftPole.SetParent(root.transform, false);
        rightPole.SetParent(root.transform, false);

        var configured = SkierTechniqueRuntimeUpdater.ConfigureAnimator(animator);

        Assert.IsTrue(configured);
        Assert.AreEqual(leftHand, animator.leftHand);
        Assert.AreEqual(rightHand, animator.rightHand);
        Assert.AreEqual(leftHand, leftPole.parent);
        Assert.AreEqual(rightHand, rightPole.parent);
        Assert.AreEqual(Vector3.zero, leftPole.localPosition);
        Assert.AreEqual(Vector3.zero, rightPole.localPosition);

        Object.DestroyImmediate(root);
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private static float CalculateLuminance(Color color)
    {
        return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
    }
}
