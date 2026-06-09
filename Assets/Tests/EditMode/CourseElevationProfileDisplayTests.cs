using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CourseElevationProfileDisplayTests
{
    [Test]
    public void CalculateProfilePosition_WrapsDistanceOnLaterLaps()
    {
        var bounds = CourseElevationProfileDisplay.CalculateElevationBounds(CourseElevationProfileDisplay.ProfileSampleCount);
        var firstLap = CourseElevationProfileDisplay.CalculateProfilePosition(640f, CourseElevationProfileDisplay.DefaultProfileSize, bounds);
        var secondLap = CourseElevationProfileDisplay.CalculateProfilePosition(CoursePath.CourseLengthMeters + 640f, CourseElevationProfileDisplay.DefaultProfileSize, bounds);

        Assert.AreEqual(firstLap.x, secondLap.x, 0.001f);
        Assert.AreEqual(firstLap.y, secondLap.y, 0.001f);
    }

    [Test]
    public void CreateRuntimeProfile_BuildsPanelProfileMarkerAndGradientText()
    {
        var canvas = new GameObject("Race HUD").AddComponent<Canvas>();
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();
        player.AlignToCourse(CoursePath.MajorClimbStartMeters + 60f);

        var profile = CourseElevationProfileDisplay.CreateRuntimeProfile(canvas.transform, player);

        Assert.AreEqual(player, profile.player);
        Assert.IsNotNull(profile.playerMarker);
        Assert.IsNotNull(profile.profileShapeRoot);
        Assert.IsNotNull(profile.gradientText);
        Assert.IsNotNull(profile.GetComponent<Image>());
        Assert.GreaterOrEqual(profile.profileShapeRoot.childCount, CourseElevationProfileDisplay.ProfileSampleCount - 1);

        profile.Refresh();

        var expectedPosition = CourseElevationProfileDisplay.CalculateProfilePosition(
            player.CurrentLapProgressMeters,
            profile.profileSize,
            CourseElevationProfileDisplay.CalculateElevationBounds(CourseElevationProfileDisplay.ProfileSampleCount));
        Assert.AreEqual(expectedPosition.x, profile.playerMarker.anchoredPosition.x, 0.001f);
        Assert.AreEqual(expectedPosition.y, profile.playerMarker.anchoredPosition.y, 0.001f);
        StringAssert.Contains("Gradient:", profile.gradientText.text);
        StringAssert.Contains("%", profile.gradientText.text);

        Object.DestroyImmediate(canvas.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void DefaultProfilePosition_SitsBelowMinimap()
    {
        var minimapBottom = CourseMinimapDisplay.DefaultMapPosition.y - CourseMinimapDisplay.DefaultMapSize.y;

        Assert.Less(CourseElevationProfileDisplay.DefaultProfilePosition.y, minimapBottom);
    }

    [Test]
    public void EnsureRuntimeProfile_UsesExistingRaceHudAndAvoidsDuplicates()
    {
        var canvas = new GameObject("Race HUD").AddComponent<Canvas>();
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();

        var first = CourseElevationProfileDisplay.EnsureRuntimeProfile(player);
        var second = CourseElevationProfileDisplay.EnsureRuntimeProfile(player);

        Assert.IsNotNull(first);
        Assert.AreSame(first, second);
        Assert.AreEqual(1, Object.FindObjectsByType<CourseElevationProfileDisplay>(FindObjectsSortMode.None).Length);

        Object.DestroyImmediate(canvas.gameObject);
        Object.DestroyImmediate(playerObject);
    }
}
