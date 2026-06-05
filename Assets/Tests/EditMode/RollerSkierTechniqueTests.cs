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
}
