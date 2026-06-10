using UnityEngine;

[DisallowMultipleComponent]
public class SpeedFeelingRuntimeUpdater : MonoBehaviour
{
    public const float CameraHeightMultiplier = 1.15f;
    public const float EnhancedMaxFieldOfView = 104f;
    public const float EnhancedSpeedForMaxFieldOfViewKmh = 60f;
    public const float MotionCueCourseLengthMeters = CoursePath.CourseLengthMeters;
    public const float MotionCueStartDistanceMeters = 14f;
    public const float EdgeFlowCueSpacingMeters = 9.5f;
    public const float EdgeFlowCueLateralOffset = EnvironmentPlacement.RoadHalfWidth + EnvironmentPlacement.OpenTerrainMargin + 0.75f;
    public const float EdgeFlowCueHalfWidth = 0.055f;
    public static readonly Color EdgeFlowGrassCueColorA = new Color(0.16f, 0.46f, 0.16f, 1f);
    public static readonly Color EdgeFlowGrassCueColorB = new Color(0.2f, 0.58f, 0.2f, 1f);

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
        return CalculateMotionCueCount(courseLengthMeters, MotionCueStartDistanceMeters + EdgeFlowCueSpacingMeters * 0.5f, EdgeFlowCueSpacingMeters);
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

        for (var z = MotionCueStartDistanceMeters + EdgeFlowCueSpacingMeters * 0.5f; z < MotionCueCourseLengthMeters; z += EdgeFlowCueSpacingMeters)
        {
            CreateEdgeFlowCue(root.transform, z, -EdgeFlowCueLateralOffset, cueCount);
            CreateEdgeFlowCue(root.transform, z + EdgeFlowCueSpacingMeters * 0.5f, EdgeFlowCueLateralOffset, cueCount + 1);
            cueCount += 2;
        }

        motionCuesCreated = true;
    }

    private static void CreateEdgeFlowCue(Transform parent, float zPosition, float lateralOffset, int index)
    {
        var cue = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cue.name = "Road Edge Flow Cue";
        cue.transform.SetParent(parent, false);

        var position = EnvironmentPlacement.SafePointAtDistance(zPosition, lateralOffset, Mathf.Max(EdgeFlowCueHalfWidth, 0.75f));
        position.y += 0.035f;
        cue.transform.position = position;
        cue.transform.rotation = CoursePath.RotationAtDistance(zPosition);
        cue.transform.localScale = new Vector3(EdgeFlowCueHalfWidth * 2f, 0.035f, 1.45f);

        var color = index % 4 < 2 ? EdgeFlowGrassCueColorA : EdgeFlowGrassCueColorB;
        cue.GetComponent<Renderer>().material.color = color;
    }
}
