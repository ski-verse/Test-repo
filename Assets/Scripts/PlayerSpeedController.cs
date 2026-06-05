using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSpeedController : MonoBehaviour
{
    public const float GradientSpeedReductionPerPercent = 0.055f;
    public const float MinimumClimbSpeedMultiplier = 0.5f;

    [Header("Speed")]
    public float acceleration = 3f;
    public float deceleration = 4f;
    public float minSpeed = 0f;
    public float maxSpeed = 18f;

    [Header("Input")]
    [SerializeField]
    private MonoBehaviour inputSourceBehaviour;

    [Header("Runtime")]
    [SerializeField]
    private float currentSpeed = 4f;
    [SerializeField]
    private float startDistanceZ;

    private IPlayerInputSource inputSource;

    public IPlayerInputSource InputSource
    {
        get => inputSource;
        set
        {
            inputSource = value;
            inputSourceBehaviour = value as MonoBehaviour;
        }
    }

    public float CurrentSpeed
    {
        get => currentSpeed;
        set => currentSpeed = Mathf.Clamp(value, minSpeed, maxSpeed);
    }

    public float CurrentGradientPercent => CoursePath.GradientPercentAtDistance(transform.position.z);

    public float EffectiveCurrentSpeed => CurrentSpeed * CalculateGradientSpeedMultiplier(CurrentGradientPercent);

    public float SpeedKmh => EffectiveCurrentSpeed * 3.6f;

    public float DistanceKm => Mathf.Max(0f, transform.position.z - startDistanceZ) / 1000f;

    private void Awake()
    {
        EnsureInputSource();
    }

    private void Start()
    {
        EnsureInputSource();
        CurrentSpeed = currentSpeed;
        startDistanceZ = transform.position.z;
        AlignToCourse(transform.position.z);
    }

    private void Update()
    {
        ApplyInputSource(Time.deltaTime);
        MoveAlongCourse(Time.deltaTime);
    }

    public static float CalculateGradientSpeedMultiplier(float gradientPercent)
    {
        if (gradientPercent <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp(1f - gradientPercent * GradientSpeedReductionPerPercent, MinimumClimbSpeedMultiplier, 1f);
    }

    public void EnsureInputSource()
    {
        if (inputSource != null)
        {
            return;
        }

        if (inputSourceBehaviour is IPlayerInputSource configuredSource)
        {
            inputSource = configuredSource;
            return;
        }

        foreach (var behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is IPlayerInputSource discoveredSource)
            {
                inputSourceBehaviour = behaviour;
                inputSource = discoveredSource;
                return;
            }
        }

        var keyboardInput = gameObject.AddComponent<KeyboardPlayerInputSource>();
        inputSourceBehaviour = keyboardInput;
        inputSource = keyboardInput;
    }

    public void ApplyInputSource(float deltaTime)
    {
        EnsureInputSource();
        ApplyMovementInput(inputSource != null ? inputSource.ReadMovementInput() : PlayerMovementInput.None, deltaTime);
    }

    public void ApplyMovementInput(PlayerMovementInput movementInput, float deltaTime)
    {
        if (movementInput.SpeedAxis > 0f)
        {
            IncreaseSpeed(deltaTime * movementInput.SpeedAxis);
        }
        else if (movementInput.SpeedAxis < 0f)
        {
            DecreaseSpeed(deltaTime * -movementInput.SpeedAxis);
        }
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

    private void MoveAlongCourse(float deltaTime)
    {
        var nextZ = transform.position.z + EffectiveCurrentSpeed * deltaTime;
        AlignToCourse(nextZ);
    }
}
