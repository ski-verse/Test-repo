using NUnit.Framework;
using UnityEngine;

public class RollerSkierDoublePolingAnimationTests
{
    [Test]
    public void DoublePolingCycle_DefinesSixReferencePhasesInOrder()
    {
        Assert.AreEqual(0f, RollerSkierAnimator.PhasePolePlant, 0.001f);
        Assert.Greater(RollerSkierAnimator.PhaseLoad, RollerSkierAnimator.PhasePolePlant);
        Assert.Greater(RollerSkierAnimator.PhasePower, RollerSkierAnimator.PhaseLoad);
        Assert.Greater(RollerSkierAnimator.PhaseRelease, RollerSkierAnimator.PhasePower);
        Assert.Greater(RollerSkierAnimator.PhaseRecovery, RollerSkierAnimator.PhaseRelease);
        Assert.Greater(RollerSkierAnimator.PhasePreparation, RollerSkierAnimator.PhaseRecovery);
        Assert.Less(RollerSkierAnimator.PhasePreparation, 1f);
    }

    [Test]
    public void DoublePolingCycle_MapsTorsoToReferenceBodyPosture()
    {
        var plantTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(RollerSkierAnimator.PhasePolePlant);
        var loadTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(RollerSkierAnimator.PhaseLoad);
        var powerTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(RollerSkierAnimator.PhasePower);
        var releaseTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(RollerSkierAnimator.PhaseRelease);
        var recoveryTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(RollerSkierAnimator.PhaseRecovery);
        var preparationTorsoPitch = RollerSkierAnimator.CalculateTorsoPitch(RollerSkierAnimator.PhasePreparation);

        Assert.Greater(loadTorsoPitch, plantTorsoPitch + 10f);
        Assert.Greater(powerTorsoPitch, loadTorsoPitch + 10f);
        Assert.Greater(powerTorsoPitch, 40f);
        Assert.Greater(releaseTorsoPitch, recoveryTorsoPitch + 15f);
        Assert.Less(recoveryTorsoPitch, 20f);
        Assert.Less(preparationTorsoPitch, 15f);
    }

    [Test]
    public void DoublePolingCycle_MapsHandsAndPolesToReferencePositions()
    {
        var plantHand = RollerSkierAnimator.CalculateHandForwardDrive(RollerSkierAnimator.PhasePolePlant);
        var loadHand = RollerSkierAnimator.CalculateHandForwardDrive(RollerSkierAnimator.PhaseLoad);
        var powerHand = RollerSkierAnimator.CalculateHandForwardDrive(RollerSkierAnimator.PhasePower);
        var releaseHand = RollerSkierAnimator.CalculateHandForwardDrive(RollerSkierAnimator.PhaseRelease);
        var recoveryHand = RollerSkierAnimator.CalculateHandForwardDrive(RollerSkierAnimator.PhaseRecovery);
        var preparationHand = RollerSkierAnimator.CalculateHandForwardDrive(RollerSkierAnimator.PhasePreparation);

        Assert.Greater(plantHand, 0.12f);
        Assert.Greater(loadHand, powerHand);
        Assert.Less(releaseHand, 0f);
        Assert.Greater(recoveryHand, releaseHand);
        Assert.Greater(preparationHand, plantHand);
        Assert.Greater(RollerSkierAnimator.CalculatePolePlantForwardOffset(RollerSkierAnimator.PhasePolePlant), 0.1f);
        Assert.Greater(RollerSkierAnimator.CalculatePolePlantForwardOffset(RollerSkierAnimator.PhasePreparation), 0.1f);
        Assert.Less(RollerSkierAnimator.CalculatePolePitch(RollerSkierAnimator.PhasePower), -35f);
        Assert.Greater(RollerSkierAnimator.CalculatePolePitch(RollerSkierAnimator.PhaseRecovery), 20f);
    }

    [Test]
    public void DoublePolingCycle_UsesCoreDrivenPowerWithoutExaggeratedArmSwing()
    {
        var plantArmPitch = RollerSkierAnimator.CalculateArmPitch(RollerSkierAnimator.PhasePolePlant);
        var powerArmPitch = RollerSkierAnimator.CalculateArmPitch(RollerSkierAnimator.PhasePower);
        var releaseArmPitch = RollerSkierAnimator.CalculateArmPitch(RollerSkierAnimator.PhaseRelease);
        var recoveryArmPitch = RollerSkierAnimator.CalculateArmPitch(RollerSkierAnimator.PhaseRecovery);

        Assert.Less(plantArmPitch, -35f);
        Assert.Greater(powerArmPitch, 5f);
        Assert.Greater(releaseArmPitch, powerArmPitch);
        Assert.Less(recoveryArmPitch, -25f);
        Assert.Less(releaseArmPitch - plantArmPitch, 70f);
    }

    [Test]
    public void ApplyPose_KeepsRigPivotsInTheirHierarchy()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftArm = CreateChild(root.transform, "Left Arm Pivot", new Vector3(-0.31f, 1.46f, -0.04f));
        animator.rightArm = CreateChild(root.transform, "Right Arm Pivot", new Vector3(0.31f, 1.46f, -0.04f));
        animator.leftHand = CreateChild(animator.leftArm, "Left Hand", new Vector3(-0.04f, -0.82f, 0.18f));
        animator.rightHand = CreateChild(animator.rightArm, "Right Hand", new Vector3(0.04f, -0.82f, 0.18f));
        animator.leftPole = CreateChild(animator.leftHand, "Left Pole Pivot", Vector3.zero);
        animator.rightPole = CreateChild(animator.rightHand, "Right Pole Pivot", Vector3.zero);
        animator.torso = CreateChild(root.transform, "Torso Pivot", new Vector3(0f, 1.08f, 0.04f));

        animator.ApplyPose(RollerSkierAnimator.PhasePower);
        animator.ApplyPose(RollerSkierAnimator.PhaseRecovery);

        Assert.AreEqual(root.transform, animator.leftArm.parent);
        Assert.AreEqual(root.transform, animator.rightArm.parent);
        Assert.AreEqual(animator.leftArm, animator.leftHand.parent);
        Assert.AreEqual(animator.rightArm, animator.rightHand.parent);
        Assert.AreEqual(animator.leftHand.position, animator.leftPole.position);
        Assert.AreEqual(animator.rightHand.position, animator.rightPole.position);
        Assert.Less(Mathf.Abs(animator.torso.localPosition.z - 0.04f), 0.001f);

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

        animator.ApplyPose(RollerSkierAnimator.PhasePower);

        Assert.AreEqual(animator.leftArm.localEulerAngles.x, animator.rightArm.localEulerAngles.x, 0.001f);
        Assert.AreEqual(animator.leftPole.localEulerAngles.x, animator.rightPole.localEulerAngles.x, 0.001f);
        Assert.AreEqual(0f, animator.leftPole.localEulerAngles.y, 0.001f);
        Assert.AreEqual(0f, animator.rightPole.localEulerAngles.y, 0.001f);
        Assert.AreEqual(animator.leftSki.localEulerAngles.x, animator.rightSki.localEulerAngles.x, 0.001f);
        Assert.Greater(Mathf.DeltaAngle(0f, animator.torso.localEulerAngles.x), 40f);
        Assert.Less(Mathf.Abs(Mathf.DeltaAngle(0f, animator.leftArm.localEulerAngles.x)), 25f);
        Assert.Less(Mathf.DeltaAngle(0f, animator.leftPole.localEulerAngles.x), -35f);

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

        animator.ApplyPose(RollerSkierAnimator.PhasePower);

        Assert.AreEqual(-0.6f, Mathf.DeltaAngle(0f, animator.leftArm.localEulerAngles.y), 0.001f);
        Assert.AreEqual(0.6f, Mathf.DeltaAngle(0f, animator.rightArm.localEulerAngles.y), 0.001f);
        Assert.AreEqual(-0.35f, Mathf.DeltaAngle(0f, animator.leftArm.localEulerAngles.z), 0.001f);
        Assert.AreEqual(0.35f, Mathf.DeltaAngle(0f, animator.rightArm.localEulerAngles.z), 0.001f);
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
}
