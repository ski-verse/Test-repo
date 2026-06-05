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
    public void PlayerSpeedController_ReducesEffectiveSpeedOnClimbsOnly()
    {
        Assert.AreEqual(1f, PlayerSpeedController.CalculateGradientSpeedMultiplier(0f), 0.001f);
        Assert.AreEqual(1f, PlayerSpeedController.CalculateGradientSpeedMultiplier(-4f), 0.001f);
        Assert.Less(PlayerSpeedController.CalculateGradientSpeedMultiplier(6.5f), 1f);
        Assert.Greater(PlayerSpeedController.CalculateGradientSpeedMultiplier(6.5f), 0.5f);
    }

    [Test]
    public void PlayerSpeedController_UsesGradientAdjustedMovementSpeed()
    {
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 10f;
        controller.AlignToCourse(CoursePath.MajorClimbStartMeters + CoursePath.MajorClimbLengthMeters * 0.5f);

        Assert.GreaterOrEqual(controller.CurrentGradientPercent, 5f);
        Assert.Less(controller.EffectiveCurrentSpeed, controller.CurrentSpeed);
        Assert.Less(controller.SpeedKmh, 36f);

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
}
