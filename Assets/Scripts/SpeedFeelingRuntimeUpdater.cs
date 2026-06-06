using UnityEngine;

[DisallowMultipleComponent]
public class SpeedFeelingRuntimeUpdater : MonoBehaviour
{
    public const float CameraHeightMultiplier = 1.15f;
    public const float EnhancedMaxFieldOfView = 104f;
    public const float EnhancedSpeedForMaxFieldOfViewKmh = 60f;
    public const float MotionCueCourseLengthMeters = CoursePath.CourseLengthMeters;
    public const float MotionCueStartDistanceMeters = 14f;
    public const float MotionCueSpacingMeters = 8f;
    public const float MotionCueLateralOffset = 5.45f;
    public const float MotionCueHalfWidth = 0.07f;
    public const float EdgeFlowCueSpacingMeters = 9.5f;
    public const float EdgeFlowCueLateralOffset = 4.55f;
    public const float EdgeFlowCueHalfWidth = 0.055f;

    private bool cameraConfigured;
    private bool motionCuesCreated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallSpeedFeelingUpdater()
    {
        if (Object.FindFirstObjectByType<SpeedFeelingRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Speed Feeling Runtime Updater");
        updater.AddComponent<SpeedFeelingRuntimeUpdater>();
    }

    private void Start()
    {
        ConfigureCameraIfAvailable();
        CreateMotionCuesIfNeeded();
    }

    private void Update()
    {
        if (!cameraConfigured)
        {
            ConfigureCameraIfAvailable();
        }

        if (!motionCuesCreated)
        {
            CreateMotionCuesIfNeeded();
        }
    }

    public static Vector3 CalculateRaisedCameraOffset(Vector3 currentOffset)
    {
        return new Vector3(currentOffset.x, currentOffset.y * CameraHeightMultiplier, currentOffset.z);
    }

    public static void ApplyCameraComposition(FollowCamera followCamera)
    {
        if (followCamera == null)
        {
            return;
        }

        followCamera.offset = CalculateRaisedCameraOffset(FollowCamera.FocusedPlayerOffset);
        followCamera.maxFieldOfView = EnhancedMaxFieldOfView;
        followCamera.speedForMaxFieldOfViewKmh = EnhancedSpeedForMaxFieldOfViewKmh;
        followCamera.maxShakeAmplitude = 0f;
    }

    public static int CalculateMotionCueCount(float courseLengthMeters, float startDistanceMeters, float spacingMeters)
    {
        if (courseLengthMeters <= startDistanceMeters || spacingMeters <= 0f)
        {
            return 0;
        }

        return Mathf.CeilToInt((courseLengthMeters - startDistanceMeters) / spacingMeters) * 2;
    }

    public static int CalculateTotalMotionCueCount(float courseLengthMeters)
    {
        return CalculateMotionCueCount(courseLengthMeters, MotionCueStartDistanceMeters, MotionCueSpacingMeters) +
               CalculateMotionCueCount(courseLengthMeters, MotionCueStartDistanceMeters + EdgeFlowCueSpacingMeters * 0.5f, EdgeFlowCueSpacingMeters);
    }

    private void ConfigureCameraIfAvailable()
    {
        var followCamera = Object.FindFirstObjectByType<FollowCamera>();
        if (followCamera == null)
        {
            return;
        }

        ApplyCameraComposition(followCamera);
        cameraConfigured = true;
    }

    private void CreateMotionCuesIfNeeded()
    {
        if (GameObject.Find("Roadside Motion Cues") != null)
        {
            motionCuesCreated = true;
            return;
        }

        var root = new GameObject("Roadside Motion Cues");
        var cueCount = 0;

        for (var z = MotionCueStartDistanceMeters; z < MotionCueCourseLengthMeters; z += MotionCueSpacingMeters)
        {
            CreateMotionCue(root.transform, z, -MotionCueLateralOffset, cueCount);
            CreateMotionCue(root.transform, z + MotionCueSpacingMeters * 0.5f, MotionCueLateralOffset, cueCount + 1);
            cueCount += 2;
        }

        for (var z = MotionCueStartDistanceMeters + EdgeFlowCueSpacingMeters * 0.5f; z < MotionCueCourseLengthMeters; z += EdgeFlowCueSpacingMeters)
        {
            CreateEdgeFlowCue(root.transform, z, -EdgeFlowCueLateralOffset, cueCount);
            CreateEdgeFlowCue(root.transform, z + EdgeFlowCueSpacingMeters * 0.5f, EdgeFlowCueLateralOffset, cueCount + 1);
            cueCount += 2;
        }

        motionCuesCreated = true;
    }

    private static void CreateMotionCue(Transform parent, float zPosition, float lateralOffset, int index)
    {
        var cue = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cue.name = "Roadside Motion Cue";
        cue.transform.SetParent(parent, false);

        var position = CoursePath.PointAtDistance(zPosition, lateralOffset);
        position.y += 0.25f;
        cue.transform.position = position;
        cue.transform.rotation = CoursePath.RotationAtDistance(zPosition);
        cue.transform.localScale = new Vector3(MotionCueHalfWidth * 2f, 0.5f, 0.95f);

        var color = index % 4 < 2 ? Color.white : new Color(0.12f, 0.45f, 0.95f);
        cue.GetComponent<Renderer>().material.color = color;
    }

    private static void CreateEdgeFlowCue(Transform parent, float zPosition, float lateralOffset, int index)
    {
        var cue = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cue.name = "Road Edge Flow Cue";
        cue.transform.SetParent(parent, false);

        var position = CoursePath.PointAtDistance(zPosition, lateralOffset);
        position.y += 0.035f;
        cue.transform.position = position;
        cue.transform.rotation = CoursePath.RotationAtDistance(zPosition);
        cue.transform.localScale = new Vector3(EdgeFlowCueHalfWidth * 2f, 0.035f, 1.45f);

        var color = index % 4 < 2 ? new Color(0.92f, 0.96f, 1f) : new Color(0.16f, 0.58f, 0.95f);
        cue.GetComponent<Renderer>().material.color = color;
    }
}
