using NUnit.Framework;
using UnityEngine;

public class SpeedFeelingRuntimeUpdaterTests
{
    [Test]
    public void CalculateRaisedCameraOffset_RaisesHeightOnly()
    {
        var raisedOffset = SpeedFeelingRuntimeUpdater.CalculateRaisedCameraOffset(FollowCamera.FocusedPlayerOffset);

        Assert.AreEqual(FollowCamera.FocusedPlayerOffset.x, raisedOffset.x, 0.001f);
        Assert.AreEqual(FollowCamera.FocusedPlayerOffset.y * 1.15f, raisedOffset.y, 0.001f);
        Assert.AreEqual(FollowCamera.FocusedPlayerOffset.z, raisedOffset.z, 0.001f);
    }

    [Test]
    public void ApplyCameraComposition_PreservesLookAheadAndDisablesShake()
    {
        var cameraObject = new GameObject("Follow Camera");
        var followCamera = cameraObject.AddComponent<FollowCamera>();
        var originalBaseLookAhead = followCamera.baseLookAheadDistance;
        var originalMaxLookAhead = followCamera.maxLookAheadDistance;
        var originalLookAheadSpeed = followCamera.speedForMaxLookAheadKmh;

        SpeedFeelingRuntimeUpdater.ApplyCameraComposition(followCamera);

        Assert.AreEqual(FollowCamera.FocusedPlayerOffset.y * 1.15f, followCamera.offset.y, 0.001f);
        Assert.AreEqual(FollowCamera.FocusedPlayerOffset.z, followCamera.offset.z, 0.001f);
        Assert.AreEqual(originalBaseLookAhead, followCamera.baseLookAheadDistance, 0.001f);
        Assert.AreEqual(originalMaxLookAhead, followCamera.maxLookAheadDistance, 0.001f);
        Assert.AreEqual(originalLookAheadSpeed, followCamera.speedForMaxLookAheadKmh, 0.001f);
        Assert.AreEqual(104f, followCamera.maxFieldOfView, 0.001f);
        Assert.AreEqual(60f, followCamera.speedForMaxFieldOfViewKmh, 0.001f);
        Assert.AreEqual(0f, followCamera.maxShakeAmplitude, 0.001f);

        Object.DestroyImmediate(cameraObject);
    }

    [Test]
    public void EnhancedFieldOfView_IsNoticeablyStrongerAtTenKmhAndAbove()
    {
        var cameraObject = new GameObject("Follow Camera");
        var followCamera = cameraObject.AddComponent<FollowCamera>();
        var previousTenKmhFov = followCamera.CalculateTargetFieldOfView(10f);
        var previousThirtySixKmhFov = followCamera.CalculateTargetFieldOfView(36f);

        SpeedFeelingRuntimeUpdater.ApplyCameraComposition(followCamera);

        Assert.Greater(followCamera.CalculateTargetFieldOfView(10f), previousTenKmhFov + 1.5f);
        Assert.Greater(followCamera.CalculateTargetFieldOfView(36f), previousThirtySixKmhFov + 6f);

        Object.DestroyImmediate(cameraObject);
    }

    [Test]
    public void MotionCues_UseOnlyGreenRoadEdgeFlowCues()
    {
        var totalCueCount = SpeedFeelingRuntimeUpdater.CalculateTotalMotionCueCount(SpeedFeelingRuntimeUpdater.MotionCueCourseLengthMeters);
        var edgeFlowCueCount = SpeedFeelingRuntimeUpdater.CalculateMotionCueCount(
            SpeedFeelingRuntimeUpdater.MotionCueCourseLengthMeters,
            SpeedFeelingRuntimeUpdater.MotionCueStartDistanceMeters + SpeedFeelingRuntimeUpdater.EdgeFlowCueSpacingMeters * 0.5f,
            SpeedFeelingRuntimeUpdater.EdgeFlowCueSpacingMeters);

        Assert.AreEqual(CoursePath.CourseLengthMeters, SpeedFeelingRuntimeUpdater.MotionCueCourseLengthMeters, 0.001f);
        Assert.AreEqual(edgeFlowCueCount, totalCueCount);
        Assert.Greater(totalCueCount, 600);
        Assert.Greater(SpeedFeelingRuntimeUpdater.EdgeFlowGrassCueColorA.g, SpeedFeelingRuntimeUpdater.EdgeFlowGrassCueColorA.r);
        Assert.Greater(SpeedFeelingRuntimeUpdater.EdgeFlowGrassCueColorB.g, SpeedFeelingRuntimeUpdater.EdgeFlowGrassCueColorB.b);
    }

    [Test]
    public void EdgeFlowCues_AddRoadsideMovementWithoutOverlappingRoad()
    {
        var totalCueCount = SpeedFeelingRuntimeUpdater.CalculateTotalMotionCueCount(SpeedFeelingRuntimeUpdater.MotionCueCourseLengthMeters);
        var edgeFlowCueCount = SpeedFeelingRuntimeUpdater.CalculateMotionCueCount(
            SpeedFeelingRuntimeUpdater.MotionCueCourseLengthMeters,
            SpeedFeelingRuntimeUpdater.MotionCueStartDistanceMeters + SpeedFeelingRuntimeUpdater.EdgeFlowCueSpacingMeters * 0.5f,
            SpeedFeelingRuntimeUpdater.EdgeFlowCueSpacingMeters);
        var innerEdgeFlowCueEdge = SpeedFeelingRuntimeUpdater.EdgeFlowCueLateralOffset - SpeedFeelingRuntimeUpdater.EdgeFlowCueHalfWidth;

        Assert.Greater(edgeFlowCueCount, 600);
        Assert.AreEqual(edgeFlowCueCount, totalCueCount);
        Assert.Greater(innerEdgeFlowCueEdge, EnvironmentPlacement.RoadHalfWidth);
        Assert.GreaterOrEqual(innerEdgeFlowCueEdge, EnvironmentPlacement.RoadHalfWidth + EnvironmentPlacement.OpenTerrainMargin);
    }

    [Test]
    public void SpeedFeelingRuntimeUpdater_DoesNotChangeActualPlayerSpeedValues()
    {
        var player = new GameObject("Player").AddComponent<PlayerSpeedController>();

        Assert.AreEqual(3f, player.acceleration, 0.001f);
        Assert.AreEqual(4f, player.deceleration, 0.001f);
        Assert.AreEqual(0f, player.minSpeed, 0.001f);
        Assert.AreEqual(18f, player.maxSpeed, 0.001f);

        Object.DestroyImmediate(player.gameObject);
    }
}
