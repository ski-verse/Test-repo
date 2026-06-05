using NUnit.Framework;
using TMPro;
using UnityEngine;

public class CourseClimbTests
{
    [Test]
    public void CoursePath_DefinesMajorClimbWithinRequestedRange()
    {
        Assert.GreaterOrEqual(CoursePath.MajorClimbLengthMeters, 500f);
        Assert.LessOrEqual(CoursePath.MajorClimbLengthMeters, 1000f);
        Assert.GreaterOrEqual(CoursePath.MajorClimbGradePercent, 5f);
        Assert.LessOrEqual(CoursePath.MajorClimbGradePercent, 8f);
    }

    [Test]
    public void CoursePath_MajorClimbIsClearlyVisibleAndLoopable()
    {
        var startHeight = CoursePath.HeightAtDistance(CoursePath.MajorClimbStartMeters);
        var summitHeight = CoursePath.HeightAtDistance(CoursePath.MajorClimbEndMeters);
        var loopStartHeight = CoursePath.HeightAtDistance(0f);
        var loopEndHeight = CoursePath.HeightAtDistance(CoursePath.CourseLengthMeters);

        Assert.Greater(summitHeight - startHeight, 30f);
        Assert.AreEqual(loopStartHeight, loopEndHeight, 0.001f);
    }

    [Test]
    public void CoursePath_CurrentGradientReportsMajorClimb()
    {
        var climbMidpoint = CoursePath.MajorClimbStartMeters + CoursePath.MajorClimbLengthMeters * 0.5f;
        var gradientPercent = CoursePath.GradientPercentAtDistance(climbMidpoint);

        Assert.GreaterOrEqual(gradientPercent, 5f);
        Assert.LessOrEqual(gradientPercent, 8f);
    }

    [Test]
    public void PlayerSpeedController_CalculatesGradientResistanceOnlyForClimbs()
    {
        Assert.AreEqual(0f, PlayerSpeedController.CalculateGradientResistanceDeceleration(0f), 0.001f);
        Assert.AreEqual(0f, PlayerSpeedController.CalculateGradientResistanceDeceleration(-4f), 0.001f);
        Assert.Greater(PlayerSpeedController.CalculateGradientResistanceDeceleration(6.5f), 0f);
    }

    [Test]
    public void PlayerSpeedController_AcceleratesDuringClimbButGradientAddsResistance()
    {
        var climbDistance = CoursePath.MajorClimbStartMeters + CoursePath.MajorClimbLengthMeters * 0.5f;
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.acceleration = 3f;
        controller.CurrentSpeed = 6f;
        controller.AlignToCourse(climbDistance);

        controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.Accelerate, 1f);

        Assert.GreaterOrEqual(controller.CurrentGradientPercent, 5f);
        Assert.Greater(controller.CurrentSpeed, 6f);
        Assert.Less(controller.CurrentSpeed, 9f);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void PlayerSpeedController_DeceleratesDuringClimbWhenBrakeInputIsActive()
    {
        var climbDistance = CoursePath.MajorClimbStartMeters + CoursePath.MajorClimbLengthMeters * 0.5f;
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.deceleration = 4f;
        controller.CurrentSpeed = 8f;
        controller.AlignToCourse(climbDistance);

        controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.Decelerate, 0.5f);

        Assert.Less(controller.CurrentSpeed, 8f);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void PlayerSpeedController_GradientResistanceReducesUnpoweredClimbSpeed()
    {
        var climbDistance = CoursePath.MajorClimbStartMeters + CoursePath.MajorClimbLengthMeters * 0.5f;
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 10f;
        controller.AlignToCourse(climbDistance);

        controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.None, 1f);

        Assert.GreaterOrEqual(controller.CurrentGradientPercent, 5f);
        Assert.Less(controller.CurrentSpeed, 10f);
        Assert.AreEqual(controller.CurrentSpeed * 3.6f, controller.SpeedKmh, 0.001f);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void SpeedDistanceDisplay_FormatsGradientTextWithTextMeshPro()
    {
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(CoursePath.MajorClimbStartMeters + CoursePath.MajorClimbLengthMeters * 0.5f);

        var hud = new GameObject("HUD").AddComponent<SpeedDistanceDisplay>();
        hud.player = controller;
        hud.gradientText = new GameObject("Gradient Text").AddComponent<TextMeshProUGUI>();

        hud.Refresh();

        StringAssert.StartsWith("Gradient: ", hud.gradientText.text);
        StringAssert.EndsWith("%", hud.gradientText.text);

        Object.DestroyImmediate(hud.gradientText.gameObject);
        Object.DestroyImmediate(hud.gameObject);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void GradientHudRuntimeUpdater_CreatesGradientTextAutomatically()
    {
        var hud = new GameObject("Race HUD").AddComponent<SpeedDistanceDisplay>();

        var created = GradientHudRuntimeUpdater.EnsureGradientText(hud);

        Assert.IsTrue(created);
        Assert.IsNotNull(hud.gradientText);
        Assert.AreEqual("Gradient Text", hud.gradientText.gameObject.name);

        Object.DestroyImmediate(hud.gradientText.gameObject);
        Object.DestroyImmediate(hud.gameObject);
    }
}
