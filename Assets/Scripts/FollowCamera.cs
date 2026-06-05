using UnityEngine;

[DisallowMultipleComponent]
public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public PlayerSpeedController player;
    public Vector3 offset = new Vector3(0f, 3f, -6.4f);
    public float positionSmoothTime = 0.16f;
    public float rotationSmoothSpeed = 10f;
    public float lookAheadDistance = 7f;
    public float baseFieldOfView = 64f;
    public float maxFieldOfView = 82f;
    public float speedForMaxFieldOfViewKmh = 72f;
    public float fieldOfViewSmoothTime = 0.18f;
    public float maxShakeAmplitude = 0.18f;
    public float speedForMaxShakeKmh = 72f;
    public float shakeFrequency = 18f;

    private Camera followCamera;
    private Vector3 positionVelocity;
    private float fieldOfViewVelocity;

    private void Awake()
    {
        followCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desiredPosition = target.position + target.TransformDirection(offset) + CalculateShakeOffset();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

        var lookTarget = target.position + Vector3.up * 1.1f + target.forward * lookAheadDistance;
        var desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);

        if (followCamera != null && player != null)
        {
            var targetFieldOfView = CalculateTargetFieldOfView(player.SpeedKmh);
            followCamera.fieldOfView = Mathf.SmoothDamp(followCamera.fieldOfView, targetFieldOfView, ref fieldOfViewVelocity, fieldOfViewSmoothTime);
        }
    }

    public float CalculateTargetFieldOfView(float speedKmh)
    {
        var speedRatio = Mathf.Clamp01(speedKmh / speedForMaxFieldOfViewKmh);
        return Mathf.Lerp(baseFieldOfView, maxFieldOfView, speedRatio);
    }

    public float CalculateShakeAmplitude(float speedKmh)
    {
        var speedRatio = Mathf.Clamp01(speedKmh / speedForMaxShakeKmh);
        return Mathf.Lerp(0f, maxShakeAmplitude, speedRatio);
    }

    private Vector3 CalculateShakeOffset()
    {
        if (player == null)
        {
            return Vector3.zero;
        }

        var amplitude = CalculateShakeAmplitude(player.SpeedKmh);
        var time = Time.time * shakeFrequency;
        var horizontal = (Mathf.PerlinNoise(time, 0.35f) - 0.5f) * amplitude;
        var vertical = (Mathf.PerlinNoise(0.65f, time) - 0.5f) * amplitude * 0.55f;
        return target.right * horizontal + Vector3.up * vertical;
    }
}
