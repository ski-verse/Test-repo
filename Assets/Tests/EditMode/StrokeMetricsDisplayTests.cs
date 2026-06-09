using NUnit.Framework;
using TMPro;
using UnityEngine;

public class StrokeMetricsDisplayTests
{
    [Test]
    public void CalculateSimulatedStrokeRate_UsesPropulsionInputOnly()
    {
        Assert.AreEqual(0f, StrokeMetricsDisplay.CalculateSimulatedStrokeRateSpm(PlayerMovementInput.None, 12f));

        var easy = StrokeMetricsDisplay.CalculateSimulatedStrokeRateSpm(PlayerMovementInput.Accelerate, 4f);
        var fast = StrokeMetricsDisplay.CalculateSimulatedStrokeRateSpm(PlayerMovementInput.Accelerate, 14f);

        Assert.Greater(easy, 0f);
        Assert.Greater(fast, easy);
    }

    [Test]
    public void AdvanceSimulatedStrokeSession_AccumulatesAndResetsTotalStrokes()
    {
        var display = new GameObject("Stroke Metrics").AddComponent<StrokeMetricsDisplay>();

        display.AdvanceSimulatedStrokeSession(2f, 30f);

        Assert.AreEqual(1, display.TotalStrokes);

        display.ResetStrokeSession();

        Assert.AreEqual(0, display.TotalStrokes);
        Assert.AreEqual(0f, display.CurrentStrokeRateSpm, 0.001f);

        Object.DestroyImmediate(display.gameObject);
    }

    [Test]
    public void Refresh_WritesStrokeRateAndTotalStrokeHudText()
    {
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();
        player.ApplyMovementInputAndGradientResistance(PlayerMovementInput.Accelerate, 0.1f);

        var display = new GameObject("Stroke Metrics").AddComponent<StrokeMetricsDisplay>();
        display.player = player;
        display.strokeRateText = new GameObject("Stroke Rate Text").AddComponent<TextMeshProUGUI>();
        display.totalStrokesText = new GameObject("Total Strokes Text").AddComponent<TextMeshProUGUI>();

        display.Refresh(1f);

        StringAssert.Contains("Stroke Rate:", display.strokeRateText.text);
        StringAssert.Contains("SPM", display.strokeRateText.text);
        StringAssert.Contains("Total Strokes:", display.totalStrokesText.text);

        Object.DestroyImmediate(display.strokeRateText.gameObject);
        Object.DestroyImmediate(display.totalStrokesText.gameObject);
        Object.DestroyImmediate(display.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void Refresh_UsesExternalStrokeMetricsSourceWhenAvailable()
    {
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();
        var source = playerObject.AddComponent<FakeStrokeMetricsSource>();
        source.hasStrokeMetrics = true;
        source.strokeRateSpm = 42f;
        source.totalStrokes = 123;

        var display = new GameObject("Stroke Metrics").AddComponent<StrokeMetricsDisplay>();
        display.player = player;
        display.strokeMetricsSourceBehaviour = source;

        display.Refresh(1f);

        Assert.AreEqual(42f, display.CurrentStrokeRateSpm, 0.001f);
        Assert.AreEqual(123, display.TotalStrokes);

        Object.DestroyImmediate(display.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void WorkoutSessionStart_ResetsStrokeMetricsForNewSession()
    {
        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        var strokeDisplay = new GameObject("Stroke Metrics").AddComponent<StrokeMetricsDisplay>();
        strokeDisplay.AdvanceSimulatedStrokeSession(10f, 30f);

        session.StartSession();

        Assert.AreEqual(0, strokeDisplay.TotalStrokes);

        Object.DestroyImmediate(strokeDisplay.gameObject);
        Object.DestroyImmediate(session.gameObject);
    }

    [Test]
    public void CreateRuntimeStrokeHud_AddsTwoTextsNearExistingLeftHud()
    {
        var canvas = new GameObject("Race HUD").AddComponent<Canvas>();
        var playerObject = new GameObject("Player");
        var player = playerObject.AddComponent<PlayerSpeedController>();

        var display = StrokeMetricsDisplay.CreateRuntimeStrokeHud(canvas.transform, player);

        Assert.AreEqual(player, display.player);
        Assert.IsNotNull(display.strokeRateText);
        Assert.IsNotNull(display.totalStrokesText);
        Assert.AreEqual(StrokeMetricsDisplay.StrokeRateTextPosition, display.strokeRateText.rectTransform.anchoredPosition);
        Assert.AreEqual(StrokeMetricsDisplay.TotalStrokesTextPosition, display.totalStrokesText.rectTransform.anchoredPosition);

        Object.DestroyImmediate(canvas.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    private class FakeStrokeMetricsSource : MonoBehaviour, IStrokeMetricsSource
    {
        public bool hasStrokeMetrics;
        public float strokeRateSpm;
        public int totalStrokes;

        public bool HasStrokeMetrics => hasStrokeMetrics;

        public float StrokeRateSpm => strokeRateSpm;

        public int TotalStrokes => totalStrokes;
    }
}
