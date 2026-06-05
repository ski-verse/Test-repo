using NUnit.Framework;
using UnityEngine;

public class RollerSkierVisualModelTests
{
    [Test]
    public void ProperRollerSkierVisual_UsesEnduranceAthleteProportions()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceShoulderWidth, 0.64f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceWaistWidth, 0.2f);
        Assert.Greater(ProperRollerSkierRuntimeUpdater.EnduranceShoulderWidth, ProperRollerSkierRuntimeUpdater.EnduranceWaistWidth * 3f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceLegVisualLength, 0.86f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceHeadDiameter, 0.19f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleNeckHeight, 0.11f);
    }

    [Test]
    public void ProperRollerSkierVisual_UsesModernClassicRollerSkiGeometry()
    {
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiFrameLength, 1.18f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiFrameWidth, 0.05f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiWheelDiameter, 0.22f);
        Assert.Less(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiRearWheelZ, ProperRollerSkierRuntimeUpdater.ClassicRollerSkiHeelZ);
        Assert.Less(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiHeelZ - ProperRollerSkierRuntimeUpdater.ClassicRollerSkiRearWheelZ, 0.1f);
        Assert.Greater(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiFrontWheelZ - ProperRollerSkierRuntimeUpdater.ClassicRollerSkiToeZ, 0.65f);
    }

    [Test]
    public void ProperRollerSkierVisual_KeepsEquipmentReadableFromGameplayCamera()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleBindingHeight, 0.055f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleBootCuffHeight, 0.16f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleWheelSidewallWidth, 0.09f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftRadius, 0.027f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleTipLateralOffset, 0.28f);
    }
}
