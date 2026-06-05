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
        Assert.IsNotNull(torsoPivot.Find("Narrow Waist"));
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
    public void ApplyProperRollerSkierModel_UsesCompactRealisticRollerSkiProportions()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var leftSki = visualRoot.transform.Find("Left Parallel Roller Ski");
        var deck = leftSki.Find("Roller Ski Deck");
        var innerRail = leftSki.Find("Inner Side Rail");
        var outerRail = leftSki.Find("Outer Side Rail");
        var frontWheel = leftSki.Find("Front Roller Wheel");
        var rearWheel = leftSki.Find("Rear Roller Wheel");
        var binding = leftSki.Find("Binding Plate");
        var wheelbase = frontWheel.localPosition.z - rearWheel.localPosition.z;

        Assert.Greater(deck.localScale.z, 1.3f);
        Assert.Less(deck.localScale.z, 1.6f);
        Assert.IsNotNull(innerRail);
        Assert.IsNotNull(outerRail);
        Assert.Greater(wheelbase, 1.2f);
        Assert.Less(wheelbase, 1.4f);
        Assert.Greater(binding.localPosition.y, deck.localPosition.y);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ApplyProperRollerSkierModel_UsesSlenderPolesWithGripStrapBasketAndTip()
    {
        var skier = CreateSkierRoot(out var visualRoot, out _);

        ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        var leftPole = visualRoot.transform.Find("Left Ski Pole");
        var shaft = leftPole.Find("Pole Shaft");
        var grip = leftPole.Find("Ergonomic Pole Grip");
        var strap = leftPole.Find("Wrist Strap");
        var basket = leftPole.Find("Compact Pole Basket");
        var tip = leftPole.Find("Pole Tip");

        Assert.IsNotNull(grip);
        Assert.IsNotNull(strap);
        Assert.IsNotNull(basket);
        Assert.IsNotNull(tip);
        Assert.Less(shaft.localScale.x, 0.02f);
        Assert.Greater(shaft.localScale.y, 1f);
        Assert.Less(basket.localScale.x, 0.08f);

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
