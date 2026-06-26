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
        Assert.AreEqual("Searching...", SkiVerseStartScreenController.Pm5SearchingText);
        Assert.AreEqual("PM5 Found", SkiVerseStartScreenController.Pm5FoundText);
        Assert.AreEqual("Connecting...", SkiVerseStartScreenController.Pm5ConnectingText);
        Assert.AreEqual("Connected", SkiVerseStartScreenController.Pm5ConnectedText);
        Assert.AreEqual("Connection Failed", SkiVerseStartScreenController.Pm5ConnectionFailedText);
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
        Assert.IsNotNull(controller.pm5DeviceListText);
        Assert.IsNotNull(controller.pm5DeviceButtonContainer);
        Assert.IsNotNull(controller.pm5Connector);
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
    public void ConnectPm5_StartsBleScanAndShowsSearching()
    {
        var controller = new GameObject("Start Screen").AddComponent<SkiVerseStartScreenController>();
        var fakeClient = new FakePm5BleClient();

        controller.SendMessage("Start");
        controller.pm5Connector.Client = fakeClient;
        controller.ConnectPm5();

        Assert.IsTrue(fakeClient.StartScanCalled);
        Assert.IsFalse(controller.IsPm5Connected);
        Assert.AreEqual(SkiVerseStartScreenController.Pm5SearchingText, controller.pm5StatusText.text);
    }

    [Test]
    public void SelectPm5AndConnect_ConnectsSelectedDevice()
    {
        var controller = new GameObject("Start Screen").AddComponent<SkiVerseStartScreenController>();
        var fakeClient = new FakePm5BleClient();
        fakeClient.AddDevice(new Pm5BleDeviceInfo("pm5-1", "PM5 12345", 12345));

        controller.SendMessage("Start");
        controller.pm5Connector.Client = fakeClient;
        controller.SendMessage("Update");
        controller.SelectPm5Device(0);
        controller.ConnectPm5();

        Assert.IsTrue(fakeClient.ConnectCalled);
        Assert.AreEqual("pm5-1", fakeClient.ConnectedDevice.DeviceId);
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

    private sealed class FakePm5BleClient : IPm5BleClient
    {
        private readonly System.Collections.Generic.List<Pm5BleDeviceInfo> devices = new System.Collections.Generic.List<Pm5BleDeviceInfo>();

        public event System.Action StateChanged;

        public bool StartScanCalled { get; private set; }

        public bool ConnectCalled { get; private set; }

        public Pm5BleDeviceInfo ConnectedDevice { get; private set; }

        public Pm5BleConnectionStatus Status { get; private set; } = Pm5BleConnectionStatus.NotConnected;

        public System.Collections.Generic.IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices => devices;

        public void AddDevice(Pm5BleDeviceInfo device)
        {
            devices.Add(device);
            Status = Pm5BleConnectionStatus.Pm5Found;
            StateChanged?.Invoke();
        }

        public void StartScan()
        {
            StartScanCalled = true;
            Status = Pm5BleConnectionStatus.Searching;
            StateChanged?.Invoke();
        }

        public void StopScan()
        {
        }

        public void Connect(Pm5BleDeviceInfo device)
        {
            ConnectCalled = true;
            ConnectedDevice = device;
            Status = Pm5BleConnectionStatus.Connected;
            StateChanged?.Invoke();
        }
    }
}
