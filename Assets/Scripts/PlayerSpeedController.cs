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
    public const float RollingResistanceDeceleration = 0.18f;
    public const float AirResistanceDecelerationPerSpeedSquared = 0.012f;
    public const float MinimumPropulsionWatts = 1f;

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
    private float totalDistanceMeters;
    [SerializeField]
    private float startDistanceMeters;

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

    public PlayerMovementInput LastMovementInput { get; private set; } = PlayerMovementInput.None;

    public float CurrentSpeed
    {
        get => currentSpeed;
        set => currentSpeed = Mathf.Clamp(value, minSpeed, maxSpeed);
    }

    public float TotalDistanceMeters => totalDistanceMeters;

    public float CurrentLapProgressMeters => CoursePath.NormalizeDistance(totalDistanceMeters);

    public float CurrentLapProgress01 => CoursePath.Progress01AtDistance(totalDistanceMeters);

    public int CurrentLapNumber => CalculateLapNumber(totalDistanceMeters);

    public float CurrentGradientPercent => CoursePath.GradientPercentAtDistance(totalDistanceMeters);

    public float EffectiveCurrentSpeed => CurrentSpeed;

    public float SpeedKmh => EffectiveCurrentSpeed * 3.6f;

    public float DistanceKm => Mathf.Max(0f, totalDistanceMeters - startDistanceMeters) / 1000f;

    private void Awake()
    {
        EnsureInputSource();
    }

    private void Start()
    {
        EnsureInputSource();
        CurrentSpeed = currentSpeed;
        totalDistanceMeters = Mathf.Max(0f, totalDistanceMeters);
        startDistanceMeters = Mathf.Max(0f, startDistanceMeters);
        AlignToCourse(totalDistanceMeters);
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

    public static float CalculateCoastDeceleration(float speedMetersPerSecond)
    {
        var speed = Mathf.Max(0f, speedMetersPerSecond);
        if (speed <= 0f)
        {
            return 0f;
        }

        return RollingResistanceDeceleration + speed * speed * AirResistanceDecelerationPerSpeedSquared;
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
        LastMovementInput = movementInput;
        var safeDeltaTime = Mathf.Max(0f, deltaTime);
        var gradientPercent = CurrentGradientPercent;

        var hasPropulsionInput = HasPropulsionInput(movementInput);

        if (movementInput.SpeedAxis > 0f)
        {
            IncreaseSpeed(safeDeltaTime * movementInput.SpeedAxis * CalculateUphillAccelerationMultiplier(gradientPercent));
        }
        else if (movementInput.SpeedAxis < 0f)
        {
            DecreaseSpeed(safeDeltaTime * -movementInput.SpeedAxis);
        }

        if (!hasPropulsionInput)
        {
            ApplyCoastDecay(safeDeltaTime);
            ApplyUphillCoastDecay(gradientPercent, safeDeltaTime);
        }

        ApplyUphillMaxSpeedLimit(gradientPercent);

        if (movementInput.SpeedAxis > 0f)
        {
            ApplyMinimumUphillMovementSpeed(gradientPercent);
        }
    }

    public void ApplyMovementInput(PlayerMovementInput movementInput, float deltaTime)
    {
        LastMovementInput = movementInput;

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
        totalDistanceMeters = Mathf.Max(0f, zPosition);
        transform.position = CoursePath.PointAtDistance(zPosition, 0f);
        transform.rotation = CoursePath.RotationAtDistance(zPosition);
    }

    public void SetStartDistanceZ(float zPosition)
    {
        startDistanceMeters = Mathf.Max(0f, zPosition);
    }

    public static int CalculateLapNumber(float distanceMeters)
    {
        if (CoursePath.CourseLengthMeters <= 0f)
        {
            return 1;
        }

        return Mathf.FloorToInt(Mathf.Max(0f, distanceMeters) / CoursePath.CourseLengthMeters) + 1;
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

    private void ApplyCoastDecay(float deltaTime)
    {
        CurrentSpeed -= CalculateCoastDeceleration(CurrentSpeed) * deltaTime;
    }

    private static bool HasPropulsionInput(PlayerMovementInput movementInput)
    {
        return movementInput.SpeedAxis > 0f || movementInput.PropulsionWatts >= MinimumPropulsionWatts;
    }

    private void ApplyUphillMaxSpeedLimit(float gradientPercent)
    {
        CurrentSpeed = Mathf.Min(CurrentSpeed, CalculateUphillMaxSpeed(maxSpeed, gradientPercent));
    }

    private void MoveAlongCourse(float deltaTime)
    {
        AlignToCourse(totalDistanceMeters + EffectiveCurrentSpeed * deltaTime);
    }
}
