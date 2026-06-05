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
    public void CameraDefaults_MoveCloserLowerAndKeepRoadLookAhead()
    {
        Assert.AreEqual(new Vector3(0f, 2.75f, -4.8f), FollowCamera.FocusedPlayerOffset);
        Assert.AreEqual(1.25f, FollowCamera.FocusedLookTargetHeight, 0.001f);
    }

    [Test]
    public void CalculateLookAheadDistance_LooksFurtherAheadAtHighSpeed()
    {
        var followCamera = new GameObject("Follow Camera").AddComponent<FollowCamera>();
        followCamera.baseLookAheadDistance = 14f;
        followCamera.maxLookAheadDistance = 42f;
        followCamera.speedForMaxLookAheadKmh = 72f;

        Assert.AreEqual(14f, followCamera.CalculateLookAheadDistance(0f), 0.001f);
        Assert.AreEqual(28f, followCamera.CalculateLookAheadDistance(36f), 0.001f);
        Assert.AreEqual(42f, followCamera.CalculateLookAheadDistance(90f), 0.001f);

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
    public void CoursePath_HasLongSweepingTurnsAndVisibleElevationChanges()
    {
        var start = CoursePath.CenterPointAtDistance(0f);
        var firstTurn = CoursePath.CenterPointAtDistance(220f);
        var climb = CoursePath.HeightAtDistance(320f) - CoursePath.HeightAtDistance(0f);
        var descent = CoursePath.HeightAtDistance(900f) - CoursePath.HeightAtDistance(320f);
        var direction = CoursePath.DirectionAtDistance(160f);

        Assert.Greater(Mathf.Abs(firstTurn.x - start.x), 45f);
        Assert.Greater(climb, 16f);
        Assert.Less(descent, -20f);
        Assert.AreEqual(1f, direction.magnitude, 0.001f);
        Assert.Greater(Mathf.Abs(direction.y), 0.01f);
    }

    [Test]
    public void EnvironmentPlacement_KeepsSceneryOutsideOpenRoadMargin()
    {
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.NearTreeOffset, EnvironmentPlacement.MaxTreeRadius));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.MidTreeOffset, EnvironmentPlacement.MaxTreeRadius));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.FarTreeOffset, EnvironmentPlacement.MaxTreeRadius));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.NearForestOffset, EnvironmentPlacement.MaxForestTreeRadius));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.FarForestOffset, EnvironmentPlacement.MaxForestTreeRadius));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.NearHillOffset, EnvironmentPlacement.NearHillHalfWidth));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.FarHillOffset, EnvironmentPlacement.FarHillHalfWidth));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.NearMountainOffset, EnvironmentPlacement.NearMountainHalfWidth));
        Assert.IsTrue(EnvironmentPlacement.HasOpenRoadMargin(EnvironmentPlacement.FarMountainOffset, EnvironmentPlacement.FarMountainHalfWidth));
    }

    [Test]
    public void EnvironmentPlacement_FramesRoadWithVisibleMountainsAndForests()
    {
        var nearMountainInnerEdge = EnvironmentPlacement.NearMountainOffset - EnvironmentPlacement.NearMountainHalfWidth;
        var farMountainInnerEdge = EnvironmentPlacement.FarMountainOffset - EnvironmentPlacement.FarMountainHalfWidth;

        Assert.LessOrEqual(EnvironmentPlacement.MountainFirstDistance, 650f);
        Assert.LessOrEqual(nearMountainInnerEdge, 110f);
        Assert.LessOrEqual(farMountainInnerEdge, 150f);
        Assert.Greater(EnvironmentPlacement.NearMountainOffset, EnvironmentPlacement.FarForestOffset);
        Assert.Greater(nearMountainInnerEdge, EnvironmentPlacement.FarForestOffset + EnvironmentPlacement.MaxForestTreeRadius);
    }

    [Test]
    public void RollerSkierAnimator_CycleAdvancesFasterAtHigherSpeedAndWraps()
    {
        var slowPhase = RollerSkierAnimator.CalculateNextPhase(0.9f, 0f, 0.5f, 0.65f, 0.018f);
        var fastPhase = RollerSkierAnimator.CalculateNextPhase(0.9f, 72f, 0.5f, 0.65f, 0.018f);

        Assert.AreEqual(0.225f, slowPhase, 0.001f);
        Assert.AreEqual(0.873f, fastPhase, 0.001f);
        Assert.Greater(fastPhase, slowPhase);
    }

    [Test]
    public void RollerSkierAnimator_DoublePolingUsesMatchedArmPitchAndParallelSkis()
    {
        var root = new GameObject("Roller Skier Rig");
        var animator = root.AddComponent<RollerSkierAnimator>();
        animator.leftArm = new GameObject("Left Arm Pivot").transform;
        animator.rightArm = new GameObject("Right Arm Pivot").transform;
        animator.leftPole = new GameObject("Left Pole Pivot").transform;
        animator.rightPole = new GameObject("Right Pole Pivot").transform;
        animator.leftSki = new GameObject("Left Roller Ski").transform;
        animator.rightSki = new GameObject("Right Roller Ski").transform;

        animator.leftArm.SetParent(root.transform);
        animator.rightArm.SetParent(root.transform);
        animator.leftPole.SetParent(root.transform);
        animator.rightPole.SetParent(root.transform);
        animator.leftSki.SetParent(root.transform);
        animator.rightSki.SetParent(root.transform);

        animator.ApplyPose(0.5f);

        Assert.AreEqual(animator.leftArm.localEulerAngles.x, animator.rightArm.localEulerAngles.x, 0.001f);
        Assert.AreEqual(animator.leftPole.localEulerAngles.x, animator.rightPole.localEulerAngles.x, 0.001f);
        Assert.AreEqual(Quaternion.identity.eulerAngles, animator.leftSki.localEulerAngles);
        Assert.AreEqual(Quaternion.identity.eulerAngles, animator.rightSki.localEulerAngles);

        Object.DestroyImmediate(root);
    }
}
