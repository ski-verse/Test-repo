using NUnit.Framework;

public sealed class AdventureCharacterRollerSkierRuntimeUpdaterTests
{
    [Test]
    public void AdventureCharacterConfiguration_UsesImportedAdventureCharacterPrefab()
    {
        Assert.AreEqual("Assets/Adventure_Character/Prefabs/Man_01.prefab", AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterPrefabPath);
        Assert.AreEqual("Adventure Character Roller Skier Applied", AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterAppliedMarkerName);
        Assert.AreEqual("Adventure Character Roller Skier", AdventureCharacterRollerSkierRuntimeUpdater.HumanoidRootName);
        Assert.AreEqual("Adventure Bone Attached Roller Ski Equipment", AdventureCharacterRollerSkierRuntimeUpdater.BoneAttachedEquipmentRootName);
        Assert.IsTrue(AdventureCharacterRollerSkierRuntimeUpdater.DisableProceduralAnimationForAdventure);
        Assert.AreEqual(0f, AdventureCharacterRollerSkierRuntimeUpdater.CharacterYawDegrees);
    }

    [Test]
    public void AdventureCharacterBasePose_UsesAthleticRollerSkierStance()
    {
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseUpperArmDropMuscle, 0.6f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseForearmBendMuscle, 0.25f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseHipHingeMuscle, 0.1f);
        Assert.Greater(AdventureCharacterRollerSkierRuntimeUpdater.BasePoseKneeBendMuscle, 0.1f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardAngleDegrees, 0f);
        Assert.Less(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardZOffset, 0f);
    }
}
