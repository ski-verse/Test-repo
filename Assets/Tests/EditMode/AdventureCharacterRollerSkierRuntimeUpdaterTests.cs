using NUnit.Framework;
using UnityEngine;

public sealed class AdventureCharacterRollerSkierRuntimeUpdaterTests
{
    [Test]
    public void AdventureCharacterConfiguration_UsesImportedAdventureCharacterPrefab()
    {
        Assert.AreEqual("Assets/Adventure_Character/Prefabs/Man_01.prefab", AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterPrefabPath);
        Assert.AreEqual("Adventure Character Roller Skier Applied", AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterAppliedMarkerName);
        Assert.AreEqual("Adventure Character Roller Skier", AdventureCharacterRollerSkierRuntimeUpdater.HumanoidRootName);
        Assert.AreEqual("Adventure Stable Equipment Constraint Rig", AdventureCharacterRollerSkierRuntimeUpdater.BoneAttachedEquipmentRootName);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.DisableProceduralAnimationForAdventure);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.SkipGenericPoleVisibilityForAdventure);
        Assert.IsFalse(AdventureCharacterRollerSkierRuntimeUpdater.AttachAdventurePolesDirectlyToHands);
        Assert.IsFalse(AdventureCharacterRollerSkierRuntimeUpdater.AttachAdventureEquipmentToHumanoid);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.UseAdventureCharacterPrefabInGameplay);
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.CharacterYawDegrees);
    }

    [Test]
    public void ApplyAdventureCharacterSwap_UsesAdventureCharacterOnlyAsStableBody()
    {
        var skier = new GameObject("Low Poly Roller Skier");
        try
        {
            var animator = skier.AddComponent<RollerSkierAnimator>();
            var visualRoot = new GameObject("Roller Skier Visual").transform;
            visualRoot.SetParent(skier.transform, false);
            new GameObject(SkiClassicsSkierModelBuilder.GameplayModelAppliedMarkerName).transform.SetParent(visualRoot, false);
            new GameObject("Old Procedural Body Part").transform.SetParent(visualRoot, false);

            var applied = AdventureCharacterRollerSkierRuntimeUpdater.ApplyAdventureCharacterSwap();

            Assert.IsTrue(applied);
            Assert.IsFalse(animator.enabled);
            Assert.IsNotNull(visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.HumanoidRootName));
            Assert.IsNotNull(visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterAppliedMarkerName));
            Assert.IsNull(visualRoot.Find("Old Procedural Body Part"));
            Assert.IsNull(visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.BoneAttachedEquipmentRootName));
            Assert.IsNull(FindChildRecursive(visualRoot, AdventureCharacterRollerSkierRuntimeUpdater.LeftAdventurePoleName));
            Assert.IsNull(FindChildRecursive(visualRoot, AdventureCharacterRollerSkierRuntimeUpdater.RightAdventurePoleName));
            Assert.IsNull(FindChildRecursive(visualRoot, "Left Adventure Roller Ski"));
            Assert.IsNull(FindChildRecursive(visualRoot, "Right Adventure Roller Ski"));
            Assert.IsNull(animator.leftPole);
            Assert.IsNull(animator.rightPole);
            Assert.IsNull(animator.leftSki);
            Assert.IsNull(animator.rightSki);
        }
        finally
        {
            Object.DestroyImmediate(skier);
        }
    }

    [Test]
    public void AdventureCharacterBasePose_UsesMildStableRollerSkierStance()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseUpperArmDropMuscle, 0.4f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseUpperArmDropMuscle, 0.55f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseForearmBendMuscle, 0.15f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseHipHingeMuscle, 0.15f);
        Assert.GreaterOrEqual(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseKneeBendMuscle, 0f);
        Assert.LessOrEqual(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseKneeBendMuscle, 0.02f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardAngleDegrees, 0f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardZOffset, 0f);
    }

    [Test]
    public void AdventureCharacterStance_UsesTightParallelRollerSkiLegsInsteadOfGoalieStance()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.68f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.78f);
        Assert.GreaterOrEqual(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseLegInwardMuscle, -0.02f);
        Assert.LessOrEqual(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseLegInwardMuscle, 0.02f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.LegChainLateralCompression, 0.15f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.LegChainLateralCompression, 0.3f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.LowerLegChainLateralCompression, 0.15f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.LowerLegChainLateralCompression, 0.35f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.NarrowFootTrackHalfWidth, 0.08f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.NarrowFootTrackHalfWidth, 0.13f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.NarrowUpperLegTrackHalfWidth, AdventureCharacterRollerSkierRuntimeUpdater.NarrowFootTrackHalfWidth);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.NarrowUpperLegTrackHalfWidth, 0.18f);
    }

    [Test]
    public void AdventureCharacterEquipment_KeepsBindingsUnderFeet()
    {
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.FootBindingLateralOffset);
        Assert.LessOrEqual(Mathf.Abs(AdventureCharacterRollerSkierRuntimeUpdater.FootBindingLateralOffset), 0.02f);
    }

    [Test]
    public void AdventureCharacterPoles_AreNotAttachedWhileProceduralBodyMotionIsDisabled()
    {
        Assert.AreEqual("Left Adventure Ski Pole", AdventureCharacterRollerSkierRuntimeUpdater.LeftAdventurePoleName);
        Assert.AreEqual("Right Adventure Ski Pole", AdventureCharacterRollerSkierRuntimeUpdater.RightAdventurePoleName);
        Assert.IsFalse(AdventureCharacterRollerSkierRuntimeUpdater.AttachAdventurePolesDirectlyToHands);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.DisableProceduralAnimationForAdventure);
    }

    [Test]
    public void AdventureEquipmentBoneFollower_KeepsHandAttachedPoleParentedAndFollowingHand()
    {
        var root = new GameObject("Player Visual Root").transform;
        var hand = new GameObject("Hand Bone").transform;
        var pole = new GameObject("Hand Attached Pole").transform;

        try
        {
            hand.SetParent(root, false);
            pole.SetParent(hand, false);
            hand.position = new Vector3(0.35f, 1.1f, 0.2f);
            root.rotation = Quaternion.Euler(0f, 15f, 0f);

            var follower = pole.gameObject.AddComponent<AdventureEquipmentBoneFollower>();
            follower.target = hand;
            follower.orientationRoot = root;
            follower.ApplyNow();

            Assert.AreSame(hand, pole.parent);
            Assert.AreEqual(hand.position, pole.position);
        }
        finally
        {
            Object.DestroyImmediate(root.gameObject);
        }
    }

    [Test]
    public void AdventureEquipmentBoneFollower_CanLockFootEquipmentToNarrowSkiTrack()
    {
        var root = new GameObject("Player Visual Root").transform;
        var foot = new GameObject("Wide Imported Foot Bone").transform;
        var ski = new GameObject("Narrow Roller Ski").transform;

        try
        {
            foot.SetParent(root, false);
            ski.SetParent(root, false);
            foot.localPosition = new Vector3(0.45f, 0.2f, 0.1f);

            var follower = ski.gameObject.AddComponent<AdventureEquipmentBoneFollower>();
            follower.target = foot;
            follower.orientationRoot = root;
            follower.lockRootSpaceX = true;
            follower.lockedRootSpaceX = AdventureCharacterRollerSkierRuntimeUpdater.NarrowFootTrackHalfWidth;
            follower.ApplyNow();

            var rootSpaceSkiPosition = root.InverseTransformPoint(ski.position);
            Assert.AreEqual(AdventureCharacterRollerSkierRuntimeUpdater.NarrowFootTrackHalfWidth, rootSpaceSkiPosition.x, 0.001f);
            Assert.AreEqual(foot.localPosition.y, rootSpaceSkiPosition.y, 0.001f);
            Assert.AreEqual(foot.localPosition.z, rootSpaceSkiPosition.z, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(root.gameObject);
        }
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
