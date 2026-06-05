using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkoutSessionControllerTests
{
    [Test]
    public void FormatElapsedTime_UsesMinutesAndSecondsThenHoursWhenNeeded()
    {
        Assert.AreEqual("00:00", WorkoutSessionController.FormatElapsedTime(0f));
        Assert.AreEqual("02:05", WorkoutSessionController.FormatElapsedTime(125.9f));
        Assert.AreEqual("1:01:01", WorkoutSessionController.FormatElapsedTime(3661f));
    }

    [Test]
    public void CalculateAverageSpeedKmh_UsesDistanceAndElapsedTime()
    {
        Assert.AreEqual(0f, WorkoutSessionController.CalculateAverageSpeedKmh(5f, 0f));
        Assert.AreEqual(18f, WorkoutSessionController.CalculateAverageSpeedKmh(5f, 1000f), 0.001f);
    }

    [Test]
    public void BuildFinishSummary_IncludesWorkoutMetrics()
    {
        var summary = WorkoutSessionController.BuildFinishSummary(1000f, 5f, 18f, 42.5f);

        StringAssert.Contains("Finish", summary);
        StringAssert.Contains("Time: 16:40", summary);
        StringAssert.Contains("Distance: 5.00 km", summary);
        StringAssert.Contains("Average speed: 18.0 km/h", summary);
        StringAssert.Contains("Max speed: 42.5 km/h", summary);
    }

    [Test]
    public void StartSession_ResetsTimeMaxSpeedAndHidesSummary()
    {
        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        session.elapsedTimeText = new GameObject("Elapsed Time").AddComponent<TextMeshProUGUI>();
        session.finishSummaryPanel = new GameObject("Finish Summary Panel");

        session.AdvanceSession(10f, 1f, 35f);
        session.StartSession();

        Assert.IsFalse(session.IsFinished);
        Assert.AreEqual(0f, session.ElapsedTimeSeconds, 0.001f);
        Assert.AreEqual(0f, session.MaxSpeedKmh, 0.001f);
        Assert.AreEqual("Time: 00:00", session.elapsedTimeText.text);
        Assert.IsFalse(session.finishSummaryPanel.activeSelf);

        Object.DestroyImmediate(session.elapsedTimeText.gameObject);
        Object.DestroyImmediate(session.finishSummaryPanel);
        Object.DestroyImmediate(session.gameObject);
    }

    [Test]
    public void AdvanceSession_TracksElapsedTimeAndMaxSpeedBeforeFinish()
    {
        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        session.elapsedTimeText = new GameObject("Elapsed Time").AddComponent<TextMeshProUGUI>();

        session.StartSession();
        session.AdvanceSession(2f, 0.1f, 18f);
        session.AdvanceSession(3f, 0.2f, 24f);
        session.AdvanceSession(1f, 0.3f, 20f);

        Assert.IsFalse(session.IsFinished);
        Assert.AreEqual(6f, session.ElapsedTimeSeconds, 0.001f);
        Assert.AreEqual(24f, session.MaxSpeedKmh, 0.001f);
        Assert.AreEqual("Time: 00:06", session.elapsedTimeText.text);

        Object.DestroyImmediate(session.elapsedTimeText.gameObject);
        Object.DestroyImmediate(session.gameObject);
    }

    [Test]
    public void AdvanceSession_ShowsFinishSummaryAtFiveKilometersAndStopsPlayer()
    {
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();
        player.CurrentSpeed = 9f;

        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        session.player = player;
        session.elapsedTimeText = new GameObject("Elapsed Time").AddComponent<TextMeshProUGUI>();
        session.finishSummaryText = new GameObject("Summary Text").AddComponent<TextMeshProUGUI>();
        session.finishSummaryPanel = new GameObject("Finish Summary Panel");
        session.finishSummaryPanel.SetActive(false);

        session.StartSession();
        session.AdvanceSession(1000f, 5f, 42.5f);

        Assert.IsTrue(session.IsFinished);
        Assert.IsTrue(session.finishSummaryPanel.activeSelf);
        Assert.IsFalse(player.enabled);
        Assert.AreEqual(0f, player.CurrentSpeed, 0.001f);
        StringAssert.Contains("Time: 16:40", session.finishSummaryText.text);
        StringAssert.Contains("Distance: 5.00 km", session.finishSummaryText.text);
        StringAssert.Contains("Average speed: 18.0 km/h", session.finishSummaryText.text);
        StringAssert.Contains("Max speed: 42.5 km/h", session.finishSummaryText.text);

        Object.DestroyImmediate(session.elapsedTimeText.gameObject);
        Object.DestroyImmediate(session.finishSummaryText.gameObject);
        Object.DestroyImmediate(session.finishSummaryPanel);
        Object.DestroyImmediate(session.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void ReturnToStartSession_ResetsPlayerAndStartsNewSessionWithoutReloadingScene()
    {
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();
        player.AlignToCourse(1200f);
        player.SetStartDistanceZ(0f);
        player.CurrentSpeed = 12f;

        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        session.player = player;
        session.elapsedTimeText = new GameObject("Elapsed Time").AddComponent<TextMeshProUGUI>();
        session.finishSummaryText = new GameObject("Summary Text").AddComponent<TextMeshProUGUI>();
        session.finishSummaryPanel = new GameObject("Finish Summary Panel");

        session.StartSession();
        session.AdvanceSession(1000f, 5f, 40f);
        session.ReturnToStartSession();

        Assert.IsFalse(session.IsFinished);
        Assert.IsTrue(player.enabled);
        Assert.AreEqual(0f, session.ElapsedTimeSeconds, 0.001f);
        Assert.AreEqual(0f, session.MaxSpeedKmh, 0.001f);
        Assert.AreEqual(4f, player.CurrentSpeed, 0.001f);
        Assert.AreEqual(0f, player.DistanceKm, 0.001f);
        Assert.AreEqual("Time: 00:00", session.elapsedTimeText.text);
        Assert.IsFalse(session.finishSummaryPanel.activeSelf);

        Object.DestroyImmediate(session.elapsedTimeText.gameObject);
        Object.DestroyImmediate(session.finishSummaryText.gameObject);
        Object.DestroyImmediate(session.finishSummaryPanel);
        Object.DestroyImmediate(session.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void RuntimeUi_CreatesFinishButtonsAndEventSystemAutomatically()
    {
        var sessionObject = new GameObject("Workout Session");
        var session = sessionObject.AddComponent<WorkoutSessionController>();

        session.SendMessage("Start");

        Assert.IsNotNull(session.elapsedTimeText);
        Assert.IsNotNull(session.finishSummaryPanel);
        Assert.IsNotNull(session.finishSummaryText);
        Assert.IsNotNull(session.restartButton);
        Assert.IsNotNull(session.returnToStartButton);
        Assert.IsNotNull(Object.FindFirstObjectByType<EventSystem>());
        Assert.IsFalse(session.finishSummaryPanel.activeSelf);

        Object.DestroyImmediate(sessionObject);
        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            Object.DestroyImmediate(eventSystem.gameObject);
        }
    }
}
