using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkiVerseStartScreenControllerTests
{
    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;

        foreach (var controller in Object.FindObjectsByType<SkiVerseStartScreenController>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(controller.gameObject);
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
    public void CourseLabels_MatchAlphaMenuRequirements()
    {
        Assert.AreEqual("Ski-Verse", SkiVerseStartScreenController.TitleText);
        Assert.AreEqual("3 km Circuit", SkiVerseStartScreenController.ThreeKmCourseLabel);
        Assert.AreEqual("40 km Long Course", SkiVerseStartScreenController.FortyKmCourseLabel);
        Assert.AreEqual("PM5: Not connected", SkiVerseStartScreenController.Pm5NotConnectedText);
        Assert.AreEqual("PM5: Connected", SkiVerseStartScreenController.Pm5ConnectedText);
        Assert.AreEqual("Jämtland Ski Tour", SkiVerseStartScreenController.JamtlandTourLabel);
    }
    [Test]
    public void Start_CreatesProfessionalAlphaMenu()
    {
        var controller = new GameObject("Start Screen").AddComponent<SkiVerseStartScreenController>();

        controller.SendMessage("Start");

        Assert.IsNotNull(controller.startScreenPanel);
        Assert.IsTrue(controller.startScreenPanel.activeSelf);
        Assert.IsNotNull(controller.startSessionButton);
        Assert.IsNotNull(controller.threeKmToggle);
        Assert.IsNotNull(controller.fortyKmToggle);
        Assert.IsNotNull(controller.jamtlandToggle);
        Assert.IsNotNull(controller.connectPm5Button);
        Assert.IsNotNull(controller.pm5StatusText);
        Assert.IsTrue(controller.threeKmToggle.isOn);
        Assert.AreEqual(SkiVerseStartScreenController.CourseSelection.ThreeKmCircuit, controller.SelectedCourse);
        Assert.AreEqual(SkiVerseStartScreenController.Pm5NotConnectedText, controller.pm5StatusText.text);

        var title = GameObject.Find("Ski-Verse Title").GetComponent<TextMeshProUGUI>();
        Assert.AreEqual("Ski-Verse", title.text);
        Assert.GreaterOrEqual(title.fontSize, 72f);

        var panelRect = controller.startScreenPanel.GetComponent<RectTransform>();
        Assert.GreaterOrEqual(panelRect.sizeDelta.x, 980f);
        Assert.GreaterOrEqual(panelRect.sizeDelta.y, 720f);
    }

    [Test]
    public void ConnectPm5_SimulatesConnectionAndUpdatesStatus()
    {
        var controller = new GameObject("Start Screen").AddComponent<SkiVerseStartScreenController>();

        controller.SendMessage("Start");
        controller.ConnectPm5();

        Assert.IsTrue(controller.IsPm5Connected);
        Assert.AreEqual(SkiVerseStartScreenController.Pm5ConnectedText, controller.pm5StatusText.text);
    }

    [Test]
    public void StartSelectedSession_HidesMenuAndStartsGameplayAtThreeKmCircuit()
    {
        var player = new GameObject("Player").AddComponent<PlayerSpeedController>();
        player.CurrentSpeed = 0f;
        player.enabled = false;

        var session = new GameObject("Workout Session").AddComponent<WorkoutSessionController>();
        session.player = player;

        var controller = new GameObject("Start Screen").AddComponent<SkiVerseStartScreenController>();
        controller.startScreenPanel = new GameObject("Start Panel");
        controller.startScreenPanel.SetActive(true);
        controller.SetCourseSelection(SkiVerseStartScreenController.CourseSelection.ThreeKmCircuit);

        Time.timeScale = 0f;
        controller.StartSelectedSession();

        Assert.IsTrue(controller.HasStartedSession);
        Assert.AreEqual(1f, Time.timeScale, 0.001f);
        Assert.IsFalse(controller.startScreenPanel.activeSelf);
        Assert.IsTrue(player.enabled);
        Assert.AreEqual(4f, player.CurrentSpeed, 0.001f);
        Assert.AreEqual(1, session.CurrentLapNumber);
        Assert.IsFalse(session.IsFinished);
    }
}
