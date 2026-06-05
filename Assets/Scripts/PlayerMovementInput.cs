using UnityEngine;

public readonly struct PlayerMovementInput
{
    public static readonly PlayerMovementInput None = new PlayerMovementInput(0f);
    public static readonly PlayerMovementInput Accelerate = new PlayerMovementInput(1f);
    public static readonly PlayerMovementInput Decelerate = new PlayerMovementInput(-1f);

    public PlayerMovementInput(float speedAxis)
    {
        SpeedAxis = Mathf.Clamp(speedAxis, -1f, 1f);
    }

    public float SpeedAxis { get; }
}
