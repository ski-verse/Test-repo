using NUnit.Framework;
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
}
