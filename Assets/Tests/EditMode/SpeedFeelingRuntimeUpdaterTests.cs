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
        Assert.AreEqual(100f, followCamera.maxFieldOfView, 0.001f);
        Assert.AreEqual(68f, followCamera.speedForMaxFieldOfViewKmh, 0.001f);
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

        Assert.Greater(followCamera.CalculateTargetFieldOfView(10f), previousTenKmhFov);
        Assert.Greater(followCamera.CalculateTargetFieldOfView(36f), previousThirtySixKmhFov);

        Object.DestroyImmediate(cameraObject);
    }

    [Test]
    public void MotionCues_AreFrequentAndStayOutsideRoadEdge()
    {
        var cueCount = SpeedFeelingRuntimeUpdater.CalculateMotionCueCount(
            SpeedFeelingRuntimeUpdater.MotionCueCourseLengthMeters,
            SpeedFeelingRuntimeUpdater.MotionCueStartDistanceMeters,
            SpeedFeelingRuntimeUpdater.MotionCueSpacingMeters);
        var innerCueEdge = SpeedFeelingRuntimeUpdater.MotionCueLateralOffset - SpeedFeelingRuntimeUpdater.MotionCueHalfWidth;

        Assert.Greater(cueCount, 700);
        Assert.Greater(innerCueEdge, EnvironmentPlacement.RoadHalfWidth + 1f);
    }
}
