using UnityEngine;

[DisallowMultipleComponent]
public class ImportedDoublePolingAnimationInputDriver : MonoBehaviour
{
    public const string IsPolingParameterName = "IsPoling";
    public const string DefaultIdleStateName = "Armature|ArmatureAction";
    public const float DefaultPropulsionWattsThreshold = 0f;

    public Animator animator;
    public PlayerSpeedController player;
    public string isPolingParameter = IsPolingParameterName;
    public string idleStateName = DefaultIdleStateName;
    public float propulsionWattsThreshold = DefaultPropulsionWattsThreshold;
    public bool resetToIdlePoseWhenNotPoling = true;

    private bool hasAppliedState;
    private bool lastIsPoling;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Update()
    {
        EnsureReferences();
        var movementInput = player != null ? player.LastMovementInput : PlayerMovementInput.None;
        ApplyPolingState(HasActivePropulsionInput(movementInput, propulsionWattsThreshold));
    }

    public static bool HasActivePropulsionInput(PlayerMovementInput movementInput, float wattsThreshold)
    {
        return movementInput.SpeedAxis > 0f || movementInput.PropulsionWatts > Mathf.Max(0f, wattsThreshold);
    }

    public void EnsureReferences()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (player == null)
        {
            player = GetComponentInParent<PlayerSpeedController>();
        }
    }

    public void ApplyPolingState(bool isPoling)
    {
        if (animator == null)
        {
            return;
        }

        SetBoolParameterIfPresent(animator, isPolingParameter, isPoling);

        if (isPoling)
        {
            animator.speed = 1f;
        }
        else
        {
            if (resetToIdlePoseWhenNotPoling && (!hasAppliedState || lastIsPoling))
            {
                PlayIdlePose(animator, idleStateName);
            }

            animator.speed = 0f;
        }

        lastIsPoling = isPoling;
        hasAppliedState = true;
    }

    private static void SetBoolParameterIfPresent(Animator targetAnimator, string parameterName, bool value)
    {
        if (targetAnimator == null || string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        var parameters = targetAnimator.parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Bool && parameters[i].name == parameterName)
            {
                targetAnimator.SetBool(parameterName, value);
                return;
            }
        }
    }

    private static void PlayIdlePose(Animator targetAnimator, string stateName)
    {
        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null || string.IsNullOrEmpty(stateName) || !targetAnimator.isInitialized)
        {
            return;
        }

        targetAnimator.Play(stateName, 0, 0f);
        targetAnimator.Update(0f);
    }
}
