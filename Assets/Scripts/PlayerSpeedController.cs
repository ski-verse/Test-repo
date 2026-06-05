using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSpeedController : MonoBehaviour
{
    [Header("Speed")]
    public float acceleration = 3f;
    public float deceleration = 4f;
    public float minSpeed = 0f;
    public float maxSpeed = 18f;

    [Header("Runtime")]
    [SerializeField]
    private float currentSpeed = 4f;
    [SerializeField]
    private float startDistanceZ;

    public float CurrentSpeed
    {
        get => currentSpeed;
        set => currentSpeed = Mathf.Clamp(value, minSpeed, maxSpeed);
    }

    public float SpeedKmh => CurrentSpeed * 3.6f;

    public float DistanceKm => Mathf.Max(0f, transform.position.z - startDistanceZ) / 1000f;

    private void Start()
    {
        CurrentSpeed = currentSpeed;
        startDistanceZ = transform.position.z;
        AlignToCourse(transform.position.z);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            IncreaseSpeed(Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            DecreaseSpeed(Time.deltaTime);
        }

        var nextZ = transform.position.z + CurrentSpeed * Time.deltaTime;
        AlignToCourse(nextZ);
    }

    public void IncreaseSpeed(float deltaTime)
    {
        CurrentSpeed += acceleration * deltaTime;
    }

    public void DecreaseSpeed(float deltaTime)
    {
        CurrentSpeed -= deceleration * deltaTime;
    }

    public Vector3 CalculateNextPosition(Vector3 startPosition, Vector3 forwardDirection, float deltaTime)
    {
        return startPosition + forwardDirection.normalized * CurrentSpeed * deltaTime;
    }

    public void AlignToCourse(float zPosition)
    {
        transform.position = CoursePath.PointAtDistance(zPosition, 0f);
        transform.rotation = CoursePath.RotationAtDistance(zPosition);
    }

    public void SetStartDistanceZ(float zPosition)
    {
        startDistanceZ = zPosition;
    }
}
