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

    public float CurrentSpeed
    {
        get => currentSpeed;
        set => currentSpeed = Mathf.Clamp(value, minSpeed, maxSpeed);
    }

    private void Start()
    {
        CurrentSpeed = currentSpeed;
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

        transform.position = CalculateNextPosition(transform.position, transform.forward, Time.deltaTime);
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
}
