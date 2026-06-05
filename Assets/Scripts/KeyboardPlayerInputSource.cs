using UnityEngine;

[DisallowMultipleComponent]
public class KeyboardPlayerInputSource : MonoBehaviour, IPlayerInputSource
{
    public KeyCode accelerateKey = KeyCode.W;
    public KeyCode decelerateKey = KeyCode.S;

    public PlayerMovementInput ReadMovementInput()
    {
        var speedAxis = 0f;

        if (Input.GetKey(accelerateKey))
        {
            speedAxis += 1f;
        }

        if (Input.GetKey(decelerateKey))
        {
            speedAxis -= 1f;
        }

        return new PlayerMovementInput(speedAxis);
    }
}
