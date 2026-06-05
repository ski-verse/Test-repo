using NUnit.Framework;

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
    public void AdventureCharacterStance_UsesNarrowerLegAndSkiSpacing()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 0.9f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.CharacterWidthScale, 1f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.EquipmentNarrowStanceOffset, 0.04f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.EquipmentNarrowStanceOffset, 0.08f);
    }
}
