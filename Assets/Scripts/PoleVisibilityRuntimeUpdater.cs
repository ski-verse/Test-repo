using UnityEngine;

[DisallowMultipleComponent]
public class PoleVisibilityRuntimeUpdater : MonoBehaviour
{
    public const string PoleVisibilityAppliedMarkerName = "Pole Visibility Pass Applied";
    public const float AlwaysVisiblePoleRadius = 0.072f;
    public const float AlwaysVisiblePoleOutsideOffset = 0.56f;
    public const float VisiblePoleStrapWidth = 0.086f;
    public const float VisiblePolePlantDiscRadius = 0.15f;
    public const float ForceCueRadius = 0.052f;
    public const float HandGripLockRadius = 0.108f;

    private const string VisualRootName = "Roller Skier Visual";

    private bool applied;
    private int attempts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<PoleVisibilityRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Pole Visibility Runtime Updater");
        updater.AddComponent<PoleVisibilityRuntimeUpdater>();
    }

    private void Start()
    {
        TryApplyOrWait();
    }

    private void Update()
    {
        if (applied)
        {
            Destroy(gameObject);
            return;
        }

        TryApplyOrWait();
    }

    private void TryApplyOrWait()
    {
        applied = ApplyPoleVisibilityPass();
        attempts++;
        if (!applied && (attempts == 1 || attempts % 60 == 0))
        {
            Debug.Log("[Ski-Verse] PoleVisibilityRuntimeUpdater waiting for skier pole rig.");
        }
    }

    public static bool ApplyPoleVisibilityPass()
    {
        var animator = Object.FindFirstObjectByType<RollerSkierAnimator>();
        if (animator == null)
        {
            return false;
        }

        var visualRoot = animator.transform.Find(VisualRootName);
        if (visualRoot == null || !HasSupportedSkierModelMarker(visualRoot))
        {
            return false;
        }

        if (visualRoot.Find(PoleVisibilityAppliedMarkerName) != null)
        {
            return true;
        }

        if (animator.leftHand == null || animator.rightHand == null || animator.leftPole == null || animator.rightPole == null)
        {
            SkierTechniqueRuntimeUpdater.ConfigureAnimator(animator);
        }

        if (animator.leftHand == null || animator.rightHand == null || animator.leftPole == null || animator.rightPole == null)
        {
            return false;
        }

        var poleBlack = new Color(0f, 0.001f, 0.002f);
        var strapBlack = new Color(0.002f, 0.003f, 0.004f);
        var forceLight = new Color(0.96f, 0.98f, 0.86f);

        ApplySide(animator.leftHand, animator.leftPole, -1f, poleBlack, strapBlack, forceLight);
        ApplySide(animator.rightHand, animator.rightPole, 1f, poleBlack, strapBlack, forceLight);

        new GameObject(PoleVisibilityAppliedMarkerName).transform.SetParent(visualRoot, false);
        animator.ResetBasePose();
        Debug.Log("[Ski-Verse] Pole visibility pass applied: high-contrast shafts, straps, grip locks, backward plant stance, and force cues.");
        return true;
    }

    private static bool HasSupportedSkierModelMarker(Transform visualRoot)
    {
        return visualRoot.Find(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName) != null
            || visualRoot.Find(AdventureCharacterRollerSkierRuntimeUpdater.AdventureCharacterAppliedMarkerName) != null;
    }

    private static void ApplySide(Transform hand, Transform pole, float side, Color poleColor, Color strapColor, Color forceColor)
    {
        if (pole.parent != hand)
        {
            pole.SetParent(hand, false);
            pole.localPosition = Vector3.zero;
            pole.localRotation = Quaternion.identity;
        }

        AddBodyPart(hand, "Gameplay Hand Grip Lock", PrimitiveType.Capsule, new Vector3(0.026f * side, -0.012f, 0.032f), new Vector3(HandGripLockRadius, 0.082f, HandGripLockRadius), strapColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(hand, "Visible Pole Strap Anchor", PrimitiveType.Cube, new Vector3(0.075f * side, -0.045f, -0.01f), new Vector3(VisiblePoleStrapWidth, 0.035f, 0.115f), forceColor, new Vector3(8f, 0f, 10f * side));

        AddBodyPart(pole, "Always Visible Gameplay Pole Shaft", PrimitiveType.Cylinder, new Vector3(AlwaysVisiblePoleOutsideOffset * side, -0.78f, -0.2f), new Vector3(AlwaysVisiblePoleRadius, ProperRollerSkierRuntimeUpdater.VisiblePoleShaftLength, AlwaysVisiblePoleRadius), poleColor, new Vector3(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardAngleDegrees, 0f, -2.5f * side));
        AddBodyPart(pole, "Cross Country Pole Strap Loop", PrimitiveType.Capsule, new Vector3(0.17f * side, -0.18f, -0.02f), new Vector3(0.035f, 0.26f, 0.035f), strapColor, new Vector3(28f, 0f, 18f * side));
        AddBodyPart(pole, "Readable Pole Plant Disc", PrimitiveType.Cylinder, new Vector3((AlwaysVisiblePoleOutsideOffset + 0.13f) * side, -1.47f, -0.56f), new Vector3(VisiblePolePlantDiscRadius, 0.018f, VisiblePolePlantDiscRadius), poleColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(pole, "Pole Force Cue", PrimitiveType.Cylinder, new Vector3((AlwaysVisiblePoleOutsideOffset + 0.04f) * side, -1.03f, -0.36f), new Vector3(ForceCueRadius, 0.5f, ForceCueRadius), forceColor, new Vector3(AdventureCharacterRollerSkierRuntimeUpdater.BasePosePoleBackwardAngleDegrees, 0f, -2.5f * side));
    }

    private static Transform AddBodyPart(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color, Vector3 localRotation)
    {
        var part = GameObject.CreatePrimitive(primitiveType);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localRotation);
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material.color = color;
        return part.transform;
    }
}
