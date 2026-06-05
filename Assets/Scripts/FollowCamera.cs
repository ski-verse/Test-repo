using UnityEngine;

[DisallowMultipleComponent]
public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public PlayerSpeedController player;
    public Vector3 offset = new Vector3(0f, 3.2f, -7f);
    public float followSpeed = 9f;
    public float lookAheadDistance = 5f;
    public float baseFieldOfView = 62f;
    public float maxFieldOfView = 78f;
    public float speedForMaxFieldOfViewKmh = 72f;
    public float fieldOfViewSmoothSpeed = 4f;

    private Camera followCamera;

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

        var desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.15f + target.forward * lookAheadDistance);

        if (followCamera != null && player != null)
        {
            var targetFieldOfView = CalculateTargetFieldOfView(player.SpeedKmh);
            followCamera.fieldOfView = Mathf.Lerp(followCamera.fieldOfView, targetFieldOfView, fieldOfViewSmoothSpeed * Time.deltaTime);
        }
    }

    public float CalculateTargetFieldOfView(float speedKmh)
    {
        var speedRatio = Mathf.Clamp01(speedKmh / speedForMaxFieldOfViewKmh);
        return Mathf.Lerp(baseFieldOfView, maxFieldOfView, speedRatio);
    }
}
