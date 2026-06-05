using UnityEngine;

[DisallowMultipleComponent]
public class SpeedFeelingRuntimeUpdater : MonoBehaviour
{
    public const float CameraHeightMultiplier = 1.15f;
    public const float EnhancedMaxFieldOfView = 100f;
    public const float EnhancedSpeedForMaxFieldOfViewKmh = 68f;
    public const float MotionCueCourseLengthMeters = 5000f;
    public const float MotionCueStartDistanceMeters = 18f;
    public const float MotionCueSpacingMeters = 12.5f;
    public const float MotionCueLateralOffset = 5.65f;
    public const float MotionCueHalfWidth = 0.08f;

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

        return (Mathf.FloorToInt((courseLengthMeters - startDistanceMeters) / spacingMeters) + 1) * 2;
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

        motionCuesCreated = true;
    }

    private static void CreateMotionCue(Transform parent, float zPosition, float lateralOffset, int index)
    {
        var cue = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cue.name = "Roadside Motion Cue";
        cue.transform.SetParent(parent, false);

        var position = CoursePath.PointAtDistance(zPosition, lateralOffset);
        position.y += 0.24f;
        cue.transform.position = position;
        cue.transform.rotation = CoursePath.RotationAtDistance(zPosition);
        cue.transform.localScale = new Vector3(MotionCueHalfWidth * 2f, 0.48f, 0.9f);

        var color = index % 4 < 2 ? Color.white : new Color(0.12f, 0.45f, 0.95f);
        cue.GetComponent<Renderer>().material.color = color;
    }
}
