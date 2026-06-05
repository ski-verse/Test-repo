using NUnit.Framework;
using UnityEngine;

public class ProperRollerSkierRuntimeUpdaterTests
{
    [Test]
    public void ApplyProperRollerSkierModel_ReplacesPlaceholderChildren()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);
        new GameObject("Old Placeholder Capsule").transform.SetParent(visualRoot.transform, false);

        var applied = ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        Assert.IsTrue(applied);
        Assert.IsNull(visualRoot.transform.Find("Old Placeholder Capsule"));
        Assert.IsNotNull(visualRoot.transform.Find("Torso Pivot"));
        Assert.IsNotNull(visualRoot.transform.Find("Left Parallel Roller Ski"));
        Assert.IsNotNull(visualRoot.transform.Find("Right Parallel Roller Ski"));

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_CreatesRecognizableRollerSkierParts()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var torsoPivot = visualRoot.transform.Find("Torso Pivot");
        var leftSki = visualRoot.transform.Find("Left Parallel Roller Ski");
        var rightSki = visualRoot.transform.Find("Right Parallel Roller Ski");
        var leftPole = visualRoot.transform.Find("Left Ski Pole");
        var rightPole = visualRoot.transform.Find("Right Ski Pole");

        Assert.IsNotNull(torsoPivot.Find("Athletic Forward Leaning Torso"));
        Assert.IsNotNull(torsoPivot.Find("V Shape Upper Torso"));
        Assert.IsNotNull(torsoPivot.Find("Narrow Waist"));
        Assert.IsNotNull(torsoPivot.Find("Broad Relaxed Shoulder Line"));
        Assert.IsNotNull(torsoPivot.Find("Left Soft Shoulder Cap"));
        Assert.IsNotNull(torsoPivot.Find("Right Soft Shoulder Cap"));
        Assert.IsNotNull(torsoPivot.Find("Low Poly Helmet"));
        Assert.IsNotNull(torsoPivot.Find("Helmet Visor"));
        Assert.IsNotNull(leftSki.Find("Front Roller Wheel"));
        Assert.IsNotNull(leftSki.Find("Rear Roller Wheel"));
        Assert.IsNotNull(rightSki.Find("Front Roller Wheel"));
        Assert.IsNotNull(rightSki.Find("Rear Roller Wheel"));
        Assert.IsNotNull(leftPole.Find("Pole Shaft"));
        Assert.IsNotNull(leftPole.Find("Ergonomic Pole Grip"));
        Assert.IsNotNull(rightPole.Find("Pole Shaft"));
        Assert.IsNotNull(rightPole.Find("Ergonomic Pole Grip"));

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_AssignsAnimatorPivotsForCurrentAnimationSystem()
    {
        var skier = CreateSkierRoot(out _, out var animator);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        Assert.IsNotNull(animator.torso);
        Assert.IsNotNull(animator.leftArm);
        Assert.IsNotNull(animator.rightArm);
        Assert.IsNotNull(animator.leftPole);
        Assert.IsNotNull(animator.rightPole);
        Assert.IsNotNull(animator.leftSki);
        Assert.IsNotNull(animator.rightSki);
        Assert.AreEqual("Torso Pivot", animator.torso.name);
        Assert.AreEqual("Left Connected Double-Poling Arm", animator.leftArm.name);
        Assert.AreEqual("Right Connected Double-Poling Arm", animator.rightArm.name);
        Assert.AreEqual("Left Ski Pole", animator.leftPole.name);
        Assert.AreEqual("Right Ski Pole", animator.rightPole.name);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesHumanProportionsWithHeadAboveTorsoAndSkisBelowLegs()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var hips = visualRoot.transform.Find("Narrow Athletic Hips");
        var torsoPivot = visualRoot.transform.Find("Torso Pivot");
        var head = torsoPivot.Find("Head");
        var leftSki = visualRoot.transform.Find("Left Parallel Roller Ski");
        var rightSki = visualRoot.transform.Find("Right Parallel Roller Ski");

        Assert.Greater(torsoPivot.localPosition.y, hips.localPosition.y);
        Assert.Greater(torsoPivot.TransformPoint(head.localPosition).y, torsoPivot.position.y);
        Assert.Less(leftSki.localPosition.y, hips.localPosition.y);
        Assert.Less(rightSki.localPosition.y, hips.localPosition.y);
        Assert.AreEqual(-leftSki.localPosition.x, rightSki.localPosition.x, 0.001f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesEnduranceAthleteBodyProportions()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var torsoPivot = visualRoot.transform.Find("Torso Pivot");
        var leftThigh = visualRoot.transform.Find("Left Long Athletic Thigh");
        var leftLowerLeg = visualRoot.transform.Find("Left Long Lower Leg");
        var head = torsoPivot.Find("Head");
        var waist = torsoPivot.Find("Narrow Waist");
        var shoulders = torsoPivot.Find("Broad Relaxed Shoulder Line");
        var chest = torsoPivot.Find("V Shape Upper Torso");

        Assert.Greater(leftThigh.localScale.y + leftLowerLeg.localScale.y, 0.84f);
        Assert.Less(head.localScale.x, 0.21f);
        Assert.Less(waist.localScale.x, 0.22f);
        Assert.Greater(shoulders.localScale.x, 0.54f);
        Assert.Less(shoulders.localScale.x, 0.6f);
        Assert.Greater(chest.localScale.x, waist.localScale.x + 0.18f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesVasaloppStyleVShapeTorsoWithoutBlockyShoulders()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var torsoPivot = visualRoot.transform.Find("Torso Pivot");
        var chest = torsoPivot.Find("V Shape Upper Torso");
        var waist = torsoPivot.Find("Narrow Waist");
        var shoulders = torsoPivot.Find("Broad Relaxed Shoulder Line");
        var leftShoulderCap = torsoPivot.Find("Left Soft Shoulder Cap");
        var rightShoulderCap = torsoPivot.Find("Right Soft Shoulder Cap");

        Assert.Greater(shoulders.localScale.x, chest.localScale.x);
        Assert.Greater(chest.localScale.x, waist.localScale.x);
        Assert.AreEqual(-leftShoulderCap.localPosition.x, rightShoulderCap.localPosition.x, 0.001f);
        Assert.Less(leftShoulderCap.localScale.x, 0.12f);
        Assert.Less(shoulders.localScale.y, 0.09f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesLongReadableRollerSkiProportions()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var leftSki = visualRoot.transform.Find("Left Parallel Roller Ski");
        var deck = leftSki.Find("Slim Long Roller Ski Deck");
        var innerRail = leftSki.Find("Inner Side Rail");
        var outerRail = leftSki.Find("Outer Side Rail");
        var frontWheel = leftSki.Find("Front Roller Wheel");
        var rearWheel = leftSki.Find("Rear Roller Wheel");
        var binding = leftSki.Find("Binding Plate");
        var wheelbase = frontWheel.localPosition.z - rearWheel.localPosition.z;

        Assert.Greater(deck.localScale.y, 0.95f);
        Assert.Less(deck.localScale.x, 0.065f);
        Assert.Greater(innerRail.localScale.z, 2.1f);
        Assert.Greater(outerRail.localScale.z, 2.1f);
        Assert.Greater(wheelbase, 2.1f);
        Assert.Less(wheelbase, 2.2f);
        Assert.Greater(binding.localPosition.y, deck.localPosition.y);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesLongSlenderPolesWithParallelPlantParts()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var leftPole = visualRoot.transform.Find("Left Ski Pole");
        var rightPole = visualRoot.transform.Find("Right Ski Pole");
        var shaft = leftPole.Find("Pole Shaft");
        var grip = leftPole.Find("Ergonomic Pole Grip");
        var strap = leftPole.Find("Wrist Strap");
        var basket = leftPole.Find("Compact Pole Basket");
        var tip = leftPole.Find("Pole Tip");

        Assert.IsNotNull(grip);
        Assert.IsNotNull(strap);
        Assert.IsNotNull(basket);
        Assert.IsNotNull(tip);
        Assert.Less(shaft.localScale.x, 0.013f);
        Assert.Greater(shaft.localScale.y, 1.35f);
        Assert.Less(basket.localScale.x, 0.055f);
        Assert.Less(tip.localPosition.y, -1.45f);
        Assert.AreEqual(-leftPole.localPosition.x, rightPole.localPosition.x, 0.001f);
        Assert.AreEqual(leftPole.localPosition.y, rightPole.localPosition.y, 0.001f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesRelaxedSlightlyLongerArmsForSkiClassicsDoublePoling()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var leftArm = visualRoot.transform.Find("Left Connected Double-Poling Arm");
        var rightArm = visualRoot.transform.Find("Right Connected Double-Poling Arm");
        var upperArm = leftArm.Find("Relaxed Upper Arm");
        var forearm = leftArm.Find("Long Forearm");
        var hand = leftArm.Find("Hand On Pole Grip");

        Assert.Greater(upperArm.localScale.y, 0.33f);
        Assert.Greater(forearm.localScale.y, 0.41f);
        Assert.Less(hand.localPosition.y, -0.86f);
        Assert.Greater(Mathf.Abs(leftArm.localPosition.x), 0.28f);
        Assert.Less(Mathf.Abs(leftArm.localPosition.x), 0.31f);
        Assert.AreEqual(-leftArm.localPosition.x, rightArm.localPosition.x, 0.001f);
        Assert.Less(Mathf.Abs(hand.localPosition.x), 0.06f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_KeepsAthleticPostureWithSmallForwardLeanAndNeutralHead()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var hips = visualRoot.transform.Find("Narrow Athletic Hips");
        var torsoPivot = visualRoot.transform.Find("Torso Pivot");
        var chestPanel = torsoPivot.Find("V Shape Upper Torso");
        var head = torsoPivot.Find("Head");
        var waist = torsoPivot.Find("Narrow Waist");

        Assert.Less(Mathf.DeltaAngle(0f, hips.localEulerAngles.x), -8f);
        Assert.Greater(Mathf.DeltaAngle(0f, chestPanel.localEulerAngles.x), 5f);
        Assert.AreEqual(0f, Mathf.DeltaAngle(0f, head.localEulerAngles.x), 0.001f);
        Assert.Less(waist.localScale.x, chestPanel.localScale.x);
        Assert.AreEqual(10f, RollerSkierAnimator.CalculateTorsoPitch(0.15f), 0.001f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_ReturnsFalseWhenSceneHasNoSkierVisualRoot()
    {
        Assert.IsFalse(ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel());
    }

    private static GameObject CreateSkierRoot(out GameObject visualRoot, out RollerSkierAnimator animator)
    {
        var skier = new GameObject("Low Poly Roller Skier");
        animator = skier.AddComponent<RollerSkierAnimator>();
        visualRoot = new GameObject("Roller Skier Visual");
        visualRoot.transform.SetParent(skier.transform, false);
        return skier;
    }
}
