using NUnit.Framework;
using UnityEngine;

public sealed class SkiClassicsGameplayModelRuntimeUpdaterTests
{
    [Test]
    public void ApplyGameplayModelSwap_WaitsForProperSkierBootstrapMarker()
    {
        GameObject skier = new GameObject("Low Poly Roller Skier");
        try
        {
            skier.AddComponent<RollerSkierAnimator>();
            Transform visualRoot = new GameObject("Roller Skier Visual").transform;
            visualRoot.SetParent(skier.transform, false);
            GameObject placeholder = new GameObject("Placeholder Capsule");
            placeholder.transform.SetParent(visualRoot, false);

            bool applied = SkiClassicsGameplayModelRuntimeUpdater.ApplyGameplayModelSwap();

            Assert.IsFalse(applied);
            Assert.IsNotNull(visualRoot.Find("Placeholder Capsule"));
            Assert.IsNull(visualRoot.Find(SkiClassicsSkierModelBuilder.GameplayModelAppliedMarkerName));
        }
        finally
        {
            Object.DestroyImmediate(skier);
        }
    }

    [Test]
    public void ApplyGameplayModelSwap_ReplacesRuntimeVisualWithSkiClassicsModel()
    {
        GameObject skier = new GameObject("Low Poly Roller Skier");
        try
        {
            RollerSkierAnimator animator = skier.AddComponent<RollerSkierAnimator>();
            Transform visualRoot = new GameObject("Roller Skier Visual").transform;
            visualRoot.SetParent(skier.transform, false);
            GameObject oldRuntimeModel = new GameObject("Forward Hinged Athletic Hips");
            oldRuntimeModel.transform.SetParent(visualRoot, false);
            GameObject properMarker = new GameObject(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName);
            properMarker.transform.SetParent(visualRoot, false);

            bool applied = SkiClassicsGameplayModelRuntimeUpdater.ApplyGameplayModelSwap();

            Assert.IsTrue(applied);
            Assert.IsNull(FindChildRecursive(visualRoot, "Forward Hinged Athletic Hips"));
            Transform newModel = FindChildRecursive(visualRoot, SkiClassicsSkierModelBuilder.DefaultModelName);
            Assert.IsNotNull(newModel);
            Assert.IsNotNull(visualRoot.Find(SkiClassicsSkierModelBuilder.GameplayModelAppliedMarkerName));
            Assert.IsNotNull(visualRoot.Find(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName));

            Assert.AreEqual(FindChildRecursive(newModel, "Readable Hips"), animator.hips);
            Assert.AreEqual(FindChildRecursive(newModel, "Forward Lean Body Pivot"), animator.torso);
            Assert.AreEqual(FindChildRecursive(newModel, "Neutral Head Looking Forward"), animator.head);
            Assert.AreEqual(FindChildRecursive(newModel, "Left Pole Arm"), animator.leftArm);
            Assert.AreEqual(FindChildRecursive(newModel, "Right Pole Arm"), animator.rightArm);
            Assert.AreEqual(FindChildRecursive(newModel, "Left Gloved Hand"), animator.leftHand);
            Assert.AreEqual(FindChildRecursive(newModel, "Right Gloved Hand"), animator.rightHand);
            Assert.AreEqual(FindChildRecursive(newModel, "Left Classic Roller Ski"), animator.leftSki);
            Assert.AreEqual(FindChildRecursive(newModel, "Right Classic Roller Ski"), animator.rightSki);

            Assert.AreSame(animator.leftHand, animator.leftPole.parent);
            Assert.AreSame(animator.rightHand, animator.rightPole.parent);
            Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Always Visible Gameplay Pole Shaft"));
            Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Always Visible Gameplay Pole Shaft"));
        }
        finally
        {
            Object.DestroyImmediate(skier);
        }
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
