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
    public void CalculateCoastDeceleration_GrowsWithSpeed()
    {
        var slowDecay = PlayerSpeedController.CalculateCoastDeceleration(2f);
        var fastDecay = PlayerSpeedController.CalculateCoastDeceleration(10f);

        Assert.Greater(slowDecay, 0f);
        Assert.Greater(fastDecay, slowDecay);
    }

    [Test]
    public void ApplyMovementInputAndGradientResistance_DecaysSpeedOnFlatWhenNoPropulsionIsApplied()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(900f);
        controller.CurrentSpeed = 10f;

        controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.None, 1f);

        Assert.LessOrEqual(controller.CurrentGradientPercent, 0f);
        Assert.Less(controller.CurrentSpeed, 10f);
        Assert.Greater(controller.CurrentSpeed, 0f);

        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void ApplyMovementInputAndGradientResistance_EventuallyStopsWithoutNewPropulsion()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(900f);
        controller.CurrentSpeed = 4f;

        for (var i = 0; i < 80; i++)
        {
            controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.None, 0.5f);
        }

        Assert.AreEqual(0f, controller.CurrentSpeed, 0.001f);

        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void ApplyMovementInputAndGradientResistance_HoldingPropulsionStillIncreasesSpeed()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(900f);
        controller.acceleration = 3f;
        controller.CurrentSpeed = 4f;

        controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.Accelerate, 1f);

        Assert.LessOrEqual(controller.CurrentGradientPercent, 0f);
        Assert.Greater(controller.CurrentSpeed, 4f);

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
    public void SpeedKmh_ConvertsMetersPerSecondToKilometersPerHourOnNonClimbTerrain()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(900f);
        controller.CurrentSpeed = 10f;

        Assert.LessOrEqual(controller.CurrentGradientPercent, 0f);
        Assert.AreEqual(36f, controller.SpeedKmh, 0.001f);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void DistanceKm_UsesForwardProgressFromStartPosition()
    {
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(2500f);
        controller.SetStartDistanceZ(500f);

        Assert.AreEqual(2f, controller.DistanceKm, 0.001f);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void PlayerSpeedController_TracksLapAndWrapsCourseProgress()
    {
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();

        controller.AlignToCourse(CoursePath.CourseLengthMeters + 125f);

        Assert.AreEqual(2, controller.CurrentLapNumber);
        Assert.AreEqual(125f, controller.CurrentLapProgressMeters, 0.001f);
        Assert.AreEqual(CoursePath.CenterPointAtDistance(125f), player.transform.position);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void Refresh_FormatsSpeedAndDistanceTextWithTextMeshPro()
    {
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        controller.AlignToCourse(1234f);
        controller.CurrentSpeed = 8f;
        controller.SetStartDistanceZ(0f);

        var hud = new GameObject("HUD").AddComponent<SpeedDistanceDisplay>();
        hud.player = controller;
        hud.speedText = new GameObject("Speed Text").AddComponent<TextMeshProUGUI>();
        hud.distanceText = new GameObject("Distance Text").AddComponent<TextMeshProUGUI>();
        hud.lapText = new GameObject("Lap Text").AddComponent<TextMeshProUGUI>();

        hud.Refresh();

        StringAssert.StartsWith("Speed: ", hud.speedText.text);
        StringAssert.EndsWith(" km/h", hud.speedText.text);
        Assert.AreEqual("Distance: 1.23 km", hud.distanceText.text);
        Assert.AreEqual("Lap: 1", hud.lapText.text);

        Object.DestroyImmediate(hud.speedText.gameObject);
        Object.DestroyImmediate(hud.distanceText.gameObject);
        Object.DestroyImmediate(hud.lapText.gameObject);
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
    public void CameraDefaults_RaiseViewWithoutMovingFurtherBack()
    {
        Assert.AreEqual(new Vector3(0f, 3.25f, -4.8f), FollowCamera.FocusedPlayerOffset);
        Assert.AreEqual(1.55f, FollowCamera.FocusedLookTargetHeight, 0.001f);
    }

    [Test]
    public void CalculateLookAheadDistance_ShowsMoreUpcomingCurvesAndHills()
    {
        var followCamera = new GameObject("Follow Camera").AddComponent<FollowCamera>();
        followCamera.baseLookAheadDistance = 22f;
        followCamera.maxLookAheadDistance = 62f;
        followCamera.speedForMaxLookAheadKmh = 72f;

        Assert.AreEqual(22f, followCamera.CalculateLookAheadDistance(0f), 0.001f);
        Assert.AreEqual(42f, followCamera.CalculateLookAheadDistance(36f), 0.001f);
        Assert.AreEqual(62f, followCamera.CalculateLookAheadDistance(90f), 0.001f);

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
        var climb = CoursePath.HeightAtDistance(CoursePath.MajorClimbEndMeters) - CoursePath.HeightAtDistance(CoursePath.MajorClimbStartMeters);
        var descent = CoursePath.HeightAtDistance(2850f) - CoursePath.HeightAtDistance(CoursePath.MajorClimbEndMeters);
        var direction = CoursePath.DirectionAtDistance(160f);

        Assert.Greater(Mathf.Abs(firstTurn.x - start.x), 45f);
        Assert.Greater(climb, 30f);
        Assert.Less(descent, -20f);
        Assert.AreEqual(1f, direction.magnitude, 0.001f);
        Assert.Greater(Mathf.Abs(direction.y), 0.01f);
    }

    [Test]
    public void CoursePath_WrapsAtThreeKilometers()
    {
        Assert.AreEqual(3000f, CoursePath.CourseLengthMeters, 0.001f);
        Assert.AreEqual(125f, CoursePath.NormalizeDistance(CoursePath.CourseLengthMeters + 125f), 0.001f);
        Assert.AreEqual(CoursePath.CenterPointAtDistance(0f), CoursePath.CenterPointAtDistance(CoursePath.CourseLengthMeters));
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
    public void RollerSkierAnimator_DoublePolingUsesMatchedArmPitchAndParallelSkiToeRise()
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
        Assert.AreEqual(animator.leftSki.localEulerAngles.x, animator.rightSki.localEulerAngles.x, 0.001f);
        Assert.AreEqual(animator.leftSki.localEulerAngles.y, animator.rightSki.localEulerAngles.y, 0.001f);
        Assert.AreEqual(animator.leftSki.localEulerAngles.z, animator.rightSki.localEulerAngles.z, 0.001f);

        Object.DestroyImmediate(root);
    }
}
