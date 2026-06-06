using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class CourseMinimapDisplayTests
{
    [Test]
    public void CalculateMapPosition_ReturnsSamePointOnNextLap()
    {
        var bounds = CourseMinimapDisplay.CalculateCourseBounds(CourseMinimapDisplay.CourseSampleCount);
        var firstLap = CourseMinimapDisplay.CalculateMapPosition(475f, CourseMinimapDisplay.DefaultMapSize, bounds);
        var secondLap = CourseMinimapDisplay.CalculateMapPosition(CoursePath.CourseLengthMeters + 475f, CourseMinimapDisplay.DefaultMapSize, bounds);

        Assert.AreEqual(firstLap.x, secondLap.x, 0.001f);
        Assert.AreEqual(firstLap.y, secondLap.y, 0.001f);
    }

    [Test]
    public void CreateRuntimeMinimap_BuildsPanelCourseDotsAndPlayerDot()
    {
        var canvas = new GameObject("Canvas").AddComponent<Canvas>();
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();
        player.AlignToCourse(1500f);

        var minimap = CourseMinimapDisplay.CreateRuntimeMinimap(canvas.transform, player);

        Assert.AreEqual(player, minimap.player);
        Assert.IsNotNull(minimap.playerDot);
        Assert.IsNotNull(minimap.courseShapeRoot);
        Assert.IsNotNull(minimap.GetComponent<Image>());
        Assert.GreaterOrEqual(minimap.courseShapeRoot.childCount, CourseMinimapDisplay.CourseSampleCount);

        var expectedPosition = CourseMinimapDisplay.CalculateMapPosition(
            player.CurrentLapProgressMeters,
            minimap.mapSize,
            CourseMinimapDisplay.CalculateCourseBounds(CourseMinimapDisplay.CourseSampleCount));
        Assert.AreEqual(expectedPosition.x, minimap.playerDot.anchoredPosition.x, 0.001f);
        Assert.AreEqual(expectedPosition.y, minimap.playerDot.anchoredPosition.y, 0.001f);

        Object.DestroyImmediate(canvas.gameObject);
        Object.DestroyImmediate(playerObject);
    }
}
