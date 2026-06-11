using NUnit.Framework;
using UnityEngine;

public sealed class AdventureCharacterRollerSkierRuntimeUpdaterTests
{
    [Test]
    public void AdventureCharacterConfiguration_UsesImportedAdventureCharacterPrefab()
    {
        Assert.AreEqual("Assets/Adventure_Character/Prefabs/Man_01.prefab", AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterPrefabPath);
        Assert.AreEqual("Assets/skier doublepoling_03.fbx", AdventureCharacterRollerSkierRuntimeUpdater.ImportedDoublePolingFbxPath);
        Assert.AreEqual("Assets/skier_doublepoling_03.fbx", AdventureCharacterRollerSkierRuntimeUpdater.ImportedDoublePolingFbxFallbackPath);
        Assert.AreEqual("Adventure Character Roller Skier Applied", AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterAppliedMarkerName);
        Assert.AreEqual("Adventure Character Roller Skier", AdventureCharacterRollerSkierRuntimeUpdater.HumanoidRootName);
        Assert.AreEqual("Imported Double Poling Test Skier", AdventureCharacterRollerSkierRuntimeUpdater.ImportedDoublePolingTestRootName);
        Assert.AreEqual("Adventure Stable Equipment Constraint Rig", AdventureCharacterRollerSkierRuntimeUpdater.BoneAttachedEquipmentRootName);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.DisableProceduralAnimationForAdventure);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.SkipGenericPoleVisibilityForAdventure);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.AttachAdventurePolesDirectlyToHands);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.AttachAdventureEquipmentToHumanoid);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.UseAdventureCharacterPrefabInGameplay);
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.CharacterYawDegrees);
        Assert.GreaterOrEqual(AdventureCharacterRollerSkierRuntimeUpdater.NeutralUpperArmDownDegrees, 75f);
        Assert.LessOrEqual(AdventureCharacterRollerSkierRuntimeUpdater.NeutralUpperArmDownDegrees, 90f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.NeutralArmDownMuscle, -0.8f);
        Assert.LessOrEqual(Mathf.Abs(AdventureCharacterRollerSkierRuntimeUpdater.NeutralForearmStretchMuscle), 0.12f);
    }

    [Test]
    public void ImportedAnimationTestFlag_DefaultsOffForNormalGameplay()
    {
        var settingsObject = new GameObject("Skier Visual Settings");

        try
        {
            var settings = settingsObject.AddComponent<SkierVisualSettings>();

            Assert.IsFalse(settings.useImportedDoublePolingAnimationTest);
            Assert.IsNull(settings.importedDoublePolingController);
        }
        finally
        {
            Object.DestroyImmediate(settingsObject);
        }
    }

    [Test]
    public void RuntimeUpdater_ReadsImportedAnimationTestFromSkierVisualSettings()
    {
        var settingsObject = new GameObject("Skier Visual Settings");

        try
        {
            var settings = settingsObject.AddComponent<SkierVisualSettings>();
            settings.useImportedDoublePolingAnimationTest = true;

            Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.ShouldUseImportedAnimationTest());
        }
        finally
        {
            Object.DestroyImmediate(settingsObject);
        }
    }

    [Test]
    public void RuntimeUpdater_ReadsImportedDoublePolingControllerFromSkierVisualSettings()
    {
        var settingsObject = new GameObject("Skier Visual Settings");
        var controller = new AnimatorOverrideController();

        try
        {
            var settings = settingsObject.AddComponent<SkierVisualSettings>();
            settings.importedDoublePolingController = controller;

            Assert.AreSame(controller, AdventureCharacterRollerSkierRuntimeUpdater.GetImportedDoublePolingController());
        }
        finally
        {
            Object.DestroyImmediate(settingsObject);
            Object.DestroyImmediate(controller);
        }
    }

    [Test]
    public void SkierVisualSettings_AttachesToExistingNordicEnvironmentSettingsObject()
    {
        var environmentSettingsObject = new GameObject(NordicEnvironmentSettings.RuntimeSettingsName);

        try
        {
            environmentSettingsObject.AddComponent<NordicEnvironmentSettings>();

            var settings = SkierVisualSettings.GetOrCreateRuntimeSettings();

            Assert.AreSame(environmentSettingsObject, settings.gameObject);
        }
        finally
        {
            Object.DestroyImmediate(environmentSettingsObject);
        }
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
            var character = visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.HumanoidRootName);
            Assert.IsNotNull(character);
            Assert.AreEqual(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, character.localScale.x, 0.001f);
            Assert.AreEqual(1f, character.localScale.y, 0.001f);
            Assert.AreEqual(1f, character.localScale.z, 0.001f);
            Assert.IsNotNull(visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterAppliedMarkerName));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "upperarm_l"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "upperarm_r"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "thigh_l"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "thigh_r"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "calf_l"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "calf_r"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "foot_l"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "foot_r"));
            Assert.IsNull(visualRoot.Find("Old Procedural Body Part"));
            Assert.IsNotNull(visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.BoneAttachedEquipmentRootName));
            Assert.IsNotNull(FindChildRecursive(visualRoot, AdventureCharacterRollerSkierRuntimeUpdater.LeftAdventurePoleName));
            Assert.IsNotNull(FindChildRecursive(visualRoot, AdventureCharacterRollerSkierRuntimeUpdater.RightAdventurePoleName));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "Left Adventure Roller Ski"));
            Assert.IsNotNull(FindChildRecursive(visualRoot, "Right Adventure Roller Ski"));
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
    public void ImportedAnimationTest_KeepsAdventureCharacterAnimatorEnabled()
    {
        var playerRoot = new GameObject("Low Poly Roller Skier");
        var character = new GameObject("Imported Double Poling Test Skier");
        var armature = new GameObject("Armature");
        var controller = new AnimatorOverrideController();

        try
        {
            character.transform.SetParent(playerRoot.transform, false);
            var player = playerRoot.AddComponent<PlayerSpeedController>();
            armature.transform.SetParent(character.transform, false);
            var importedAnimator = armature.AddComponent<Animator>();
            importedAnimator.enabled = true;

            AdventureCharacterRollerSkierRuntimeUpdater.ConfigureImportedDoublePolingTestVisual(character, controller, player);

            Assert.IsTrue(importedAnimator.enabled);
            Assert.AreSame(controller, importedAnimator.runtimeAnimatorController);
            var driver = character.GetComponent<ImportedDoublePolingAnimationInputDriver>();
            Assert.IsNotNull(driver);
            Assert.AreSame(importedAnimator, driver.animator);
            Assert.AreSame(player, driver.player);
        }
        finally
        {
            Object.DestroyImmediate(playerRoot);
            Object.DestroyImmediate(controller);
        }
    }

    [Test]
    public void DefaultAdventureMode_DisablesImportedAdventureCharacterAnimator()
    {
        var character = new GameObject("Man_01");
        var armature = new GameObject("Armature");

        try
        {
            armature.transform.SetParent(character.transform, false);
            var importedAnimator = armature.AddComponent<Animator>();
            importedAnimator.enabled = true;

            AdventureCharacterRollerSkierRuntimeUpdater.ConfigureImportedCharacterAnimationMode(character, false);

            Assert.IsFalse(importedAnimator.enabled);
        }
        finally
        {
            Object.DestroyImmediate(character);
        }
    }

    [Test]
    public void ImportedAnimationTest_DoesNotClearProceduralAnimatorBodyReferences()
    {
        var skier = new GameObject("Low Poly Roller Skier");
        var torso = new GameObject("Torso Reference").transform;

        try
        {
            torso.SetParent(skier.transform, false);
            var animator = skier.AddComponent<RollerSkierAnimator>();
            animator.torso = torso;
            animator.enabled = true;

            AdventureCharacterRollerSkierRuntimeUpdater.ConfigureProceduralAnimatorForAdventureMode(animator, true);

            Assert.IsFalse(animator.enabled);
            Assert.AreSame(torso, animator.torso);
        }
        finally
        {
            Object.DestroyImmediate(skier);
        }
    }

    [Test]
    public void AdventureCharacterStance_UsesNaturalLegsAndEquipmentFollowsFeet()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.68f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.78f);
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.NeutralLegInwardMuscle);
    }

    [Test]
    public void AdventureCharacterEquipment_KeepsBindingsUnderFeet()
    {
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.FootBindingLateralOffset);
        Assert.LessOrEqual(Mathf.Abs(AdventureCharacterRollerSkierRuntimeUpdater.FootBindingLateralOffset), 0.02f);
    }

    [Test]
    public void AdventureCharacterPoles_AreHandAttachedWhileProceduralBodyMotionIsDisabled()
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

    [Test]
    public void AdventureEquipmentBoneFollower_KeepsRollerSkisDirectlyUnderFeetWithoutSideLocking()
    {
        var root = new GameObject("Player Visual Root").transform;
        var foot = new GameObject("Foot Bone").transform;
        var ski = new GameObject("Foot Attached Roller Ski").transform;

        try
        {
            foot.SetParent(root, false);
            ski.SetParent(root, false);
            foot.localPosition = new Vector3(0.16f, 0.35f, 0.1f);

            var follower = ski.gameObject.AddComponent<AdventureEquipmentBoneFollower>();
            follower.target = foot;
            follower.orientationRoot = root;
            follower.rootSpaceOffset = new Vector3(0f, -0.09f, 0.12f);
            follower.ApplyNow();

            var rootSpaceSkiPosition = root.InverseTransformPoint(ski.position);
            Assert.AreEqual(foot.localPosition.x, rootSpaceSkiPosition.x, 0.001f);
            Assert.AreEqual(foot.localPosition.y - 0.09f, rootSpaceSkiPosition.y, 0.001f);
            Assert.AreEqual(foot.localPosition.z + 0.12f, rootSpaceSkiPosition.z, 0.001f);
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
