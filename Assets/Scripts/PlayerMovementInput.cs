using UnityEngine;

public readonly struct PlayerMovementInput
{
    public static readonly PlayerMovementInput None = new PlayerMovementInput(0f);
    public static readonly PlayerMovementInput Accelerate = new PlayerMovementInput(1f);
    public static readonly PlayerMovementInput Decelerate = new PlayerMovementInput(-1f);

    public PlayerMovementInput(float speedAxis, float propulsionWatts = 0f)
    {
        SpeedAxis = Mathf.Clamp(speedAxis, -1f, 1f);
        PropulsionWatts = Mathf.Max(0f, propulsionWatts);
    }

    public float SpeedAxis { get; }

    public float PropulsionWatts { get; }
}
