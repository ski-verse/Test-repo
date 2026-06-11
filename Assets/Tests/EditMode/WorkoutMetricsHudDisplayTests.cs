using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkoutMetricsHudDisplayTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (var display in Object.FindObjectsByType<WorkoutMetricsHudDisplay>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(display.gameObject);
        }

        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(canvas.gameObject);
        }

        foreach (var player in Object.FindObjectsByType<PlayerSpeedController>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(player.gameObject);
        }

        foreach (var session in Object.FindObjectsByType<WorkoutSessionController>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(session.gameObject);
        }
    }

    [Test]
    public void CreateRuntimeHud_BuildsCompactLowerLeftWorkoutPanelWithAllMetrics()
    {
        var canvas = new GameObject("Race HUD").AddComponent<Canvas>();
        var player = new GameObject("Player").AddComponent<PlayerSpeedController>();

        var display = WorkoutMetricsHudDisplay.CreateRuntimeHud(canvas.transform, player);

        Assert.IsNotNull(display);
        Assert.IsNotNull(display.panelRoot);
        Assert.IsNotNull(display.speedValueText);
        Assert.IsNotNull(display.wattsValueText);
        Assert.IsNotNull(display.heartRateValueText);
        Assert.IsNotNull(display.strokeRateValueText);
        Assert.IsNotNull(display.timeValueText);
        Assert.IsNotNull(display.distanceValueText);
        Assert.IsNotNull(display.gradientValueText);
        Assert.IsNotNull(display.lapValueText);
        Assert.IsNotNull(display.totalStrokesValueText);

        var panelRect = display.panelRoot.GetComponent<RectTransform>();
        Assert.AreEqual(new Vector2(0f, 0f), panelRect.anchorMin);
        Assert.AreEqual(new Vector2(0f, 0f), panelRect.anchorMax);
        Assert.LessOrEqual(panelRect.sizeDelta.x, 330f);
        Assert.LessOrEqual(panelRect.sizeDelta.y, 370f);
    }

    [Test]
    public void Refresh_FormatsWorkoutMetricsForTrainingHud()
    {
        var canvas = new GameObject("Race HUD").AddComponent<Canvas>();
        var player = new GameObject("Player").AddComponent<PlayerSpeedController>();
        player.CurrentSpeed = 8f;
        player.AlignToCourse(375f);
        player.ApplyMovementInput(new PlayerMovementInput(1f, 0f), 0f);

        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        session.player = player;
        session.AdvanceSession(125f, player.DistanceKm, player.SpeedKmh);

        var display = WorkoutMetricsHudDisplay.CreateRuntimeHud(canvas.transform, player);
        display.session = session;
        display.Refresh();

        StringAssert.StartsWith("28", display.speedValueText.text);
        Assert.AreEqual("WATTS", display.wattsLabelText.text);
        Assert.AreEqual("BPM", display.heartRateLabelText.text);
        Assert.AreEqual("SPM", display.strokeRateLabelText.text);
        Assert.AreEqual("02:05", display.timeValueText.text);
        StringAssert.EndsWith("km", display.distanceValueText.text);
        StringAssert.EndsWith("%", display.gradientValueText.text);
        Assert.AreEqual("1", display.lapValueText.text);
    }

    [Test]
    public void SimulatedPm5Values_ArePreparedForFutureMetricsSource()
    {
        var activeInput = new PlayerMovementInput(1f, 0f);
        var coastingInput = PlayerMovementInput.None;

        Assert.Greater(WorkoutMetricsHudDisplay.CalculateDisplayWatts(activeInput, 20f), 0);
        Assert.AreEqual(0, WorkoutMetricsHudDisplay.CalculateDisplayWatts(coastingInput, 20f));
        Assert.Greater(WorkoutMetricsHudDisplay.CalculateDisplayHeartRateBpm(20f), 100);
    }
}
