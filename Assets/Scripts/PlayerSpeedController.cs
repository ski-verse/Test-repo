using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSpeedController : MonoBehaviour
{
    public const float UphillAccelerationReductionPerPercent = 0.07f;
    public const float MinimumUphillAccelerationMultiplier = 0.45f;
    public const float UphillMaxSpeedReductionPerPercent = 0.075f;
    public const float MinimumUphillMaxSpeedMultiplier = 0.4f;
    public const float UphillCoastDecelerationPerPercent = 0.08f;
    public const float MinimumUphillMovementSpeed = 1.4f;

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

    public float EffectiveCurrentSpeed => CurrentSpeed;

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

    public static float CalculateUphillAccelerationMultiplier(float gradientPercent)
    {
        if (gradientPercent <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp(1f - gradientPercent * UphillAccelerationReductionPerPercent, MinimumUphillAccelerationMultiplier, 1f);
    }

    public static float CalculateUphillMaxSpeed(float baseMaxSpeed, float gradientPercent)
    {
        if (gradientPercent <= 0f)
        {
            return baseMaxSpeed;
        }

        var multiplier = Mathf.Clamp(1f - gradientPercent * UphillMaxSpeedReductionPerPercent, MinimumUphillMaxSpeedMultiplier, 1f);
        return Mathf.Max(MinimumUphillMovementSpeed, baseMaxSpeed * multiplier);
    }

    public static float CalculateUphillCoastDeceleration(float gradientPercent)
    {
        if (gradientPercent <= 0f)
        {
            return 0f;
        }

        return gradientPercent * UphillCoastDecelerationPerPercent;
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
        ApplyMovementInputAndGradientResistance(inputSource != null ? inputSource.ReadMovementInput() : PlayerMovementInput.None, deltaTime);
    }

    public void ApplyMovementInputAndGradientResistance(PlayerMovementInput movementInput, float deltaTime)
    {
        var safeDeltaTime = Mathf.Max(0f, deltaTime);
        var gradientPercent = CurrentGradientPercent;

        if (movementInput.SpeedAxis > 0f)
        {
            IncreaseSpeed(safeDeltaTime * movementInput.SpeedAxis * CalculateUphillAccelerationMultiplier(gradientPercent));
            ApplyMinimumUphillMovementSpeed(gradientPercent);
        }
        else if (movementInput.SpeedAxis < 0f)
        {
            DecreaseSpeed(safeDeltaTime * -movementInput.SpeedAxis);
        }
        else
        {
            ApplyUphillCoastDecay(gradientPercent, safeDeltaTime);
        }

        ApplyUphillMaxSpeedLimit(gradientPercent);
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

    private void ApplyMinimumUphillMovementSpeed(float gradientPercent)
    {
        if (gradientPercent <= 0f)
        {
            return;
        }

        CurrentSpeed = Mathf.Max(CurrentSpeed, Mathf.Min(MinimumUphillMovementSpeed, CalculateUphillMaxSpeed(maxSpeed, gradientPercent)));
    }

    private void ApplyUphillCoastDecay(float gradientPercent, float deltaTime)
    {
        CurrentSpeed -= CalculateUphillCoastDeceleration(gradientPercent) * deltaTime;
    }

    private void ApplyUphillMaxSpeedLimit(float gradientPercent)
    {
        CurrentSpeed = Mathf.Min(CurrentSpeed, CalculateUphillMaxSpeed(maxSpeed, gradientPercent));
    }

    private void MoveAlongCourse(float deltaTime)
    {
        var nextZ = transform.position.z + EffectiveCurrentSpeed * deltaTime;
        AlignToCourse(nextZ);
    }
}
