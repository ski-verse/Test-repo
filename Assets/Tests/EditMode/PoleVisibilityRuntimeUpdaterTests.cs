using NUnit.Framework;
using UnityEngine;

public class PoleVisibilityRuntimeUpdaterTests
{
    [Test]
    public void PoleVisibilityPass_UsesHighContrastGameplaySilhouette()
    {
        Assert.GreaterOrEqual(PoleVisibilityRuntimeUpdater.AlwaysVisiblePoleRadius, 0.07f);
        Assert.GreaterOrEqual(PoleVisibilityRuntimeUpdater.AlwaysVisiblePoleOutsideOffset, 0.52f);
        Assert.GreaterOrEqual(PoleVisibilityRuntimeUpdater.VisiblePoleStrapWidth, 0.08f);
        Assert.GreaterOrEqual(PoleVisibilityRuntimeUpdater.VisiblePolePlantDiscRadius, 0.14f);
        Assert.GreaterOrEqual(PoleVisibilityRuntimeUpdater.ForceCueRadius, 0.05f);
        Assert.GreaterOrEqual(PoleVisibilityRuntimeUpdater.HandGripLockRadius, 0.105f);
    }

    [Test]
    public void ApplyPoleVisibilityPass_AddsReadableGripStrapPlantAndForceCues()
    {
        var skier = new GameObject("Low Poly Roller Skier");
        var animator = skier.AddComponent<RollerSkierAnimator>();
        var visualRoot = new GameObject("Roller Skier Visual").transform;
        visualRoot.SetParent(skier.transform, false);
        visualRoot.localScale = Vector3.one;

        Assert.IsTrue(ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel());
        Assert.IsTrue(SkierHumanSilhouetteRuntimeUpdater.ApplyHumanSilhouettePass());
        Assert.IsTrue(PoleVisibilityRuntimeUpdater.ApplyPoleVisibilityPass());

        Assert.IsNotNull(visualRoot.Find(PoleVisibilityRuntimeUpdater.PoleVisibilityAppliedMarkerName));
        Assert.IsNotNull(FindChildRecursive(animator.leftHand, "Gameplay Hand Grip Lock"));
        Assert.IsNotNull(FindChildRecursive(animator.rightHand, "Gameplay Hand Grip Lock"));
        Assert.IsNotNull(FindChildRecursive(animator.leftHand, "Visible Pole Strap Anchor"));
        Assert.IsNotNull(FindChildRecursive(animator.rightHand, "Visible Pole Strap Anchor"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Always Visible Gameplay Pole Shaft"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Always Visible Gameplay Pole Shaft"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Cross Country Pole Strap Loop"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Cross Country Pole Strap Loop"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Readable Pole Plant Disc"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Readable Pole Plant Disc"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Pole Force Cue"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Pole Force Cue"));
        Assert.AreEqual(animator.leftHand, animator.leftPole.parent);
        Assert.AreEqual(animator.rightHand, animator.rightPole.parent);

        Object.DestroyImmediate(skier);
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var match = FindChildRecursive(root.GetChild(i), childName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
