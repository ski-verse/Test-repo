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
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.CharacterYawDegrees);
    }

    [Test]
    public void AdventureCharacterBasePose_UsesMildStableRollerSkierStance()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseUpperArmDropMuscle, 0.4f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseUpperArmDropMuscle, 0.55f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseForearmBendMuscle, 0.15f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseHipHingeMuscle, 0.15f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseKneeBendMuscle, 0.1f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardAngleDegrees, 0f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardZOffset, 0f);
    }

    [Test]
    public void AdventureCharacterStance_UsesNarrowDoublePolingSpacing()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.86f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.93f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseLegInwardMuscle, 0.1f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseLegInwardMuscle, 0.22f);
    }

    [Test]
    public void AdventureCharacterEquipment_KeepsBindingsUnderFeet()
    {
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.FootBindingLateralOffset);
        Assert.LessOrEqual(Mathf.Abs(AdventureCharacterRollerSkierRuntimeUpdater.FootBindingLateralOffset), 0.02f);
    }

    [Test]
    public void AdventureCharacterPoles_AreHandAttachedAndProceduralPoleMotionIsDisabled()
    {
        Assert.AreEqual("Left Adventure Ski Pole", AdventureCharacterRollerSkierRuntimeUpdater.LeftAdventurePoleName);
        Assert.AreEqual("Right Adventure Ski Pole", AdventureCharacterRollerSkierRuntimeUpdater.RightAdventurePoleName);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.AttachAdventurePolesDirectlyToHands);
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
}
