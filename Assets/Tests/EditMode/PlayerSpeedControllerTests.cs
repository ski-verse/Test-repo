using NUnit.Framework;
using TMPro;
using UnityEngine;

public class PlayerSpeedControllerTests
{
    [Test]
    public void IncreaseSpeed_AddsAccelerationOverDeltaTime()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.acceleration = 4f;
        controller.CurrentSpeed = 1f;

        controller.IncreaseSpeed(0.5f);

        Assert.AreEqual(3f, controller.CurrentSpeed, 0.001f);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void DecreaseSpeed_SubtractsDecelerationOverDeltaTime()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.deceleration = 2f;
        controller.CurrentSpeed = 5f;

        controller.DecreaseSpeed(1.5f);

        Assert.AreEqual(2f, controller.CurrentSpeed, 0.001f);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void Speed_IsClampedBetweenMinimumAndMaximum()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.minSpeed = 0f;
        controller.maxSpeed = 10f;
        controller.acceleration = 100f;
        controller.deceleration = 100f;

        controller.IncreaseSpeed(1f);
        Assert.AreEqual(10f, controller.CurrentSpeed, 0.001f);

        controller.DecreaseSpeed(1f);
        Assert.AreEqual(0f, controller.CurrentSpeed, 0.001f);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void CalculateNextPosition_MovesForwardBySpeedAndDeltaTime()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 6f;

        var next = controller.CalculateNextPosition(Vector3.zero, Vector3.forward, 0.5f);

        Assert.AreEqual(new Vector3(0f, 0f, 3f), next);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void SpeedKmh_ConvertsMetersPerSecondToKilometersPerHour()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 10f;

        Assert.AreEqual(36f, controller.SpeedKmh, 0.001f);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void DistanceKm_UsesForwardProgressFromStartPosition()
    {
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0f, 2500f);
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.SetStartDistanceZ(500f);

        Assert.AreEqual(2f, controller.DistanceKm, 0.001f);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void Refresh_FormatsSpeedAndDistanceTextWithTextMeshPro()
    {
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0f, 1234f);
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 8f;
        controller.SetStartDistanceZ(0f);

        var hud = new GameObject("HUD").AddComponent<SpeedDistanceDisplay>();
        hud.player = controller;
        hud.speedText = new GameObject("Speed Text").AddComponent<TextMeshProUGUI>();
        hud.distanceText = new GameObject("Distance Text").AddComponent<TextMeshProUGUI>();

        hud.Refresh();

        Assert.AreEqual("Speed: 28.8 km/h", hud.speedText.text);
        Assert.AreEqual("Distance: 1.23 km", hud.distanceText.text);

        Object.DestroyImmediate(hud.speedText.gameObject);
        Object.DestroyImmediate(hud.distanceText.gameObject);
        Object.DestroyImmediate(hud.gameObject);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void CalculateTargetFieldOfView_WidensNoticeablyAsSpeedIncreases()
    {
        var followCamera = new GameObject("Follow Camera").AddComponent<FollowCamera>();
        followCamera.baseFieldOfView = 60f;
        followCamera.maxFieldOfView = 96f;
        followCamera.speedForMaxFieldOfViewKmh = 72f;

        Assert.AreEqual(60f, followCamera.CalculateTargetFieldOfView(0f), 0.001f);
        Assert.AreEqual(78f, followCamera.CalculateTargetFieldOfView(36f), 0.001f);
        Assert.AreEqual(96f, followCamera.CalculateTargetFieldOfView(90f), 0.001f);

        Object.DestroyImmediate(followCamera.gameObject);
    }

    [Test]
    public void CalculateLookAheadDistance_LooksFurtherAheadAtHighSpeed()
    {
        var followCamera = new GameObject("Follow Camera").AddComponent<FollowCamera>();
        followCamera.baseLookAheadDistance = 8f;
        followCamera.maxLookAheadDistance = 24f;
        followCamera.speedForMaxLookAheadKmh = 72f;

        Assert.AreEqual(8f, followCamera.CalculateLookAheadDistance(0f), 0.001f);
        Assert.AreEqual(16f, followCamera.CalculateLookAheadDistance(36f), 0.001f);
        Assert.AreEqual(24f, followCamera.CalculateLookAheadDistance(90f), 0.001f);

        Object.DestroyImmediate(followCamera.gameObject);
    }

    [Test]
    public void CalculateShakeAmplitude_GrowsWithSpeedAndClamps()
    {
        var followCamera = new GameObject("Follow Camera").AddComponent<FollowCamera>();
        followCamera.maxShakeAmplitude = 0.18f;
        followCamera.speedForMaxShakeKmh = 72f;

        Assert.AreEqual(0f, followCamera.CalculateShakeAmplitude(0f), 0.001f);
        Assert.AreEqual(0.09f, followCamera.CalculateShakeAmplitude(36f), 0.001f);
        Assert.AreEqual(0.18f, followCamera.CalculateShakeAmplitude(100f), 0.001f);

        Object.DestroyImmediate(followCamera.gameObject);
    }

    [Test]
    public void CoursePath_HasClearlyVisibleCurvesAndNormalizedDirection()
    {
        var start = CoursePath.CenterXAtDistance(0f);
        var later = CoursePath.CenterXAtDistance(400f);
        var direction = CoursePath.DirectionAtDistance(400f);

        Assert.Greater(Mathf.Abs(later - start), 12f);
        Assert.AreEqual(1f, direction.magnitude, 0.001f);
    }
}
