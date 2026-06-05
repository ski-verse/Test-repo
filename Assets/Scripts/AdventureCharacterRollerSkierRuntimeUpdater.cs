using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class AdventureCharacterRollerSkierRuntimeUpdater : MonoBehaviour
{
    public const string AdventureCharacterPrefabPath = "Assets/Adventure_Character/Prefabs/Man_01.prefab";
    public const string AdventureCharacterAppliedMarkerName = "Adventure Character Roller Skier Applied";
    public const string HumanoidRootName = "Adventure Character Roller Skier";
    public const string AnimationProxyRootName = "Adventure Roller Skier Animation Proxy Rig";
    public const float CharacterYawDegrees = 0f;
    public const float BasePoseUpperArmDropMuscle = 0.72f;
    public const float BasePoseForearmBendMuscle = 0.34f;
    public const float BasePoseHipHingeMuscle = 0.2f;
    public const float BasePoseKneeBendMuscle = 0.16f;
    public const float BasePosePoleBackwardAngleDegrees = -16f;
    public const float BasePosePoleBackwardZOffset = -0.2f;

    private const string VisualRootName = "Roller Skier Visual";
    private const float EquipmentSkiLength = 1.05f;
    private const float EquipmentSkiWidth = 0.045f;
    private const float EquipmentWheelRadius = 0.105f;
    private const float PoleRadius = 0.045f;
    private const float PoleLength = 1.5f;

    private bool applied;
    private int attempts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<AdventureCharacterRollerSkierRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Adventure Character Roller Skier Runtime Updater");
        updater.AddComponent<AdventureCharacterRollerSkierRuntimeUpdater>();
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
        applied = ApplyAdventureCharacterSwap();
        attempts++;
        if (!applied && (attempts == 1 || attempts % 60 == 0))
        {
            Debug.Log("[Ski-Verse] AdventureCharacterRollerSkierRuntimeUpdater waiting for Adventure Character prefab and skier visual.");
        }
    }

    public static bool ApplyAdventureCharacterSwap()
    {
        var animator = Object.FindFirstObjectByType<RollerSkierAnimator>();
        if (animator == null)
        {
            return false;
        }

        var visualRoot = animator.transform.Find(VisualRootName);
        if (visualRoot == null)
        {
            return false;
        }

        if (visualRoot.Find(AdventureCharacterAppliedMarkerName) != null)
        {
            return true;
        }

        if (visualRoot.Find(SkiClassicsSkierModelBuilder.GameplayModelAppliedMarkerName) == null)
        {
            return false;
        }

#if UNITY_EDITOR
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AdventureCharacterPrefabPath);
        if (prefab == null)
        {
            return false;
        }

        ClearChildren(visualRoot);

        var character = Object.Instantiate(prefab);
        character.name = HumanoidRootName;
        character.transform.SetParent(visualRoot, false);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.Euler(0f, CharacterYawDegrees, 0f);
        character.transform.localScale = Vector3.one;
        ApplyHumanoidBasePose(character);

        CreateAnimationProxyRig(visualRoot, animator);
        AddRollerSkiEquipment(visualRoot, animator);
        AddHelmetOverlay(visualRoot);

        new GameObject(AdventureCharacterAppliedMarkerName).transform.SetParent(visualRoot, false);

        animator.ResetBasePose();
        animator.ApplyPose(0f);
        PoleVisibilityRuntimeUpdater.ApplyPoleVisibilityPass();

        Debug.Log("[Ski-Verse] Adventure Character roller skier base pose applied: arms lowered, hands on poles, slight knee bend, hip hinge, and backward pole angle.");
        return true;
#else
        return false;
#endif
    }

    private static void ApplyHumanoidBasePose(GameObject character)
    {
        if (character == null)
        {
            return;
        }

        if (TryApplyHumanPose(character))
        {
            return;
        }

        ApplyFallbackBonePose(character.transform);
    }

    private static bool TryApplyHumanPose(GameObject character)
    {
        var humanoidAnimator = character.GetComponentInChildren<Animator>();
        if (humanoidAnimator == null || humanoidAnimator.avatar == null || !humanoidAnimator.avatar.isHuman)
        {
            return false;
        }

        try
        {
            var poseHandler = new HumanPoseHandler(humanoidAnimator.avatar, humanoidAnimator.transform);
            var pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);
            var muscles = pose.muscles;

            SetMuscle(muscles, "Spine Front-Back", -BasePoseHipHingeMuscle);
            SetMuscle(muscles, "Chest Front-Back", -BasePoseHipHingeMuscle * 0.7f);
            SetMuscle(muscles, "UpperChest Front-Back", -BasePoseHipHingeMuscle * 0.45f);
            SetMuscle(muscles, "Neck Nod Down-Up", 0.04f);
            SetMuscle(muscles, "Head Nod Down-Up", 0.03f);

            SetMuscle(muscles, "Left Arm Down-Up", -BasePoseUpperArmDropMuscle);
            SetMuscle(muscles, "Right Arm Down-Up", -BasePoseUpperArmDropMuscle);
            SetMuscle(muscles, "Left Arm Front-Back", 0.14f);
            SetMuscle(muscles, "Right Arm Front-Back", 0.14f);
            SetMuscle(muscles, "Left Forearm Stretch", -BasePoseForearmBendMuscle);
            SetMuscle(muscles, "Right Forearm Stretch", -BasePoseForearmBendMuscle);
            SetMuscle(muscles, "Left Hand Down-Up", -0.08f);
            SetMuscle(muscles, "Right Hand Down-Up", -0.08f);

            SetMuscle(muscles, "Left Upper Leg Front-Back", -0.08f);
            SetMuscle(muscles, "Right Upper Leg Front-Back", -0.08f);
            SetMuscle(muscles, "Left Lower Leg Stretch", -BasePoseKneeBendMuscle);
            SetMuscle(muscles, "Right Lower Leg Stretch", -BasePoseKneeBendMuscle);
            SetMuscle(muscles, "Left Foot Up-Down", -0.05f);
            SetMuscle(muscles, "Right Foot Up-Down", -0.05f);

            pose.muscles = muscles;
            poseHandler.SetHumanPose(ref pose);
            humanoidAnimator.enabled = false;
            Debug.Log("[Ski-Verse] Adventure Character humanoid base pose applied through Unity Humanoid muscles.");
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("[Ski-Verse] Humanoid base pose failed; using named-bone fallback. " + exception.Message);
            return false;
        }
    }

    private static void SetMuscle(float[] muscles, string muscleName, float value)
    {
        if (muscles == null)
        {
            return;
        }

        var names = HumanTrait.MuscleName;
        for (var i = 0; i < names.Length && i < muscles.Length; i++)
        {
            if (names[i] == muscleName)
            {
                muscles[i] = Mathf.Clamp(value, -1f, 1f);
                return;
            }
        }
    }

    private static void ApplyFallbackBonePose(Transform root)
    {
        ApplyLocalRotationDelta(root, "pelvis", new Vector3(-6f, 0f, 0f));
        ApplyLocalRotationDelta(root, "spine_01", new Vector3(-5f, 0f, 0f));
        ApplyLocalRotationDelta(root, "spine_02", new Vector3(-4f, 0f, 0f));
        ApplyLocalRotationDelta(root, "spine_03", new Vector3(-3f, 0f, 0f));
        ApplyLocalRotationDelta(root, "head", new Vector3(3f, 0f, 0f));

        ApplyLocalRotationDelta(root, "upperarm_l", new Vector3(0f, 0f, -64f));
        ApplyLocalRotationDelta(root, "upperarm_r", new Vector3(0f, 0f, 64f));
        ApplyLocalRotationDelta(root, "lowerarm_l", new Vector3(0f, 0f, -18f));
        ApplyLocalRotationDelta(root, "lowerarm_r", new Vector3(0f, 0f, 18f));
        ApplyLocalRotationDelta(root, "hand_l", new Vector3(-6f, 0f, -6f));
        ApplyLocalRotationDelta(root, "hand_r", new Vector3(-6f, 0f, 6f));

        ApplyLocalRotationDelta(root, "thigh_l", new Vector3(5f, 0f, 0f));
        ApplyLocalRotationDelta(root, "thigh_r", new Vector3(5f, 0f, 0f));
        ApplyLocalRotationDelta(root, "calf_l", new Vector3(-9f, 0f, 0f));
        ApplyLocalRotationDelta(root, "calf_r", new Vector3(-9f, 0f, 0f));
        ApplyLocalRotationDelta(root, "foot_l", new Vector3(3f, 0f, 0f));
        ApplyLocalRotationDelta(root, "foot_r", new Vector3(3f, 0f, 0f));
        Debug.Log("[Ski-Verse] Adventure Character fallback named-bone base pose applied.");
    }

    private static void ApplyLocalRotationDelta(Transform root, string boneName, Vector3 localEulerDelta)
    {
        var bone = FindDeepChild(root, boneName);
        if (bone == null)
        {
            return;
        }

        bone.localRotation *= Quaternion.Euler(localEulerDelta);
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == name)
        {
            return parent;
        }

        for (var i = 0; i < parent.childCount; i++)
        {
            var result = FindDeepChild(parent.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void CreateAnimationProxyRig(Transform visualRoot, RollerSkierAnimator animator)
    {
        var proxyRoot = CreateChild(visualRoot, AnimationProxyRootName, Vector3.zero);

        animator.hips = CreateChild(proxyRoot, "Adventure Proxy Hips", new Vector3(0f, 0.91f, 0.16f));
        animator.torso = CreateChild(proxyRoot, "Adventure Proxy Torso", new Vector3(0f, 1.08f, 0.14f));
        animator.head = CreateChild(proxyRoot, "Adventure Proxy Head", new Vector3(0f, 1.78f, -0.08f));

        animator.leftArm = CreateChild(proxyRoot, "Adventure Proxy Left Arm", new Vector3(-0.31f, 1.36f, 0.02f));
        animator.rightArm = CreateChild(proxyRoot, "Adventure Proxy Right Arm", new Vector3(0.31f, 1.36f, 0.02f));
        animator.leftHand = CreateChild(animator.leftArm, "Adventure Proxy Left Hand", new Vector3(-0.035f, -0.66f, 0.13f));
        animator.rightHand = CreateChild(animator.rightArm, "Adventure Proxy Right Hand", new Vector3(0.035f, -0.66f, 0.13f));

        animator.leftThigh = CreateChild(proxyRoot, "Adventure Proxy Left Thigh", new Vector3(-0.145f, 0.67f, 0.16f));
        animator.rightThigh = CreateChild(proxyRoot, "Adventure Proxy Right Thigh", new Vector3(0.145f, 0.67f, 0.16f));
        animator.leftShin = CreateChild(proxyRoot, "Adventure Proxy Left Calf", new Vector3(-0.165f, 0.3f, 0.075f));
        animator.rightShin = CreateChild(proxyRoot, "Adventure Proxy Right Calf", new Vector3(0.165f, 0.3f, 0.075f));
        animator.leftFoot = CreateChild(proxyRoot, "Adventure Proxy Left Foot", new Vector3(-0.22f, 0.16f, 0.08f));
        animator.rightFoot = CreateChild(proxyRoot, "Adventure Proxy Right Foot", new Vector3(0.22f, 0.16f, 0.08f));
    }

    private static void AddRollerSkiEquipment(Transform visualRoot, RollerSkierAnimator animator)
    {
        var aluminium = new Color(0.72f, 0.78f, 0.78f);
        var black = new Color(0.005f, 0.006f, 0.008f);
        var neon = new Color(0.72f, 0.9f, 0.08f);
        var white = new Color(0.92f, 0.94f, 0.9f);

        animator.leftSki = CreateRollerSki(visualRoot, "Left Adventure Roller Ski", -0.22f, aluminium, black, neon);
        animator.rightSki = CreateRollerSki(visualRoot, "Right Adventure Roller Ski", 0.22f, aluminium, black, neon);

        AddBootDetails(animator.leftFoot, "Left", neon, black);
        AddBootDetails(animator.rightFoot, "Right", neon, black);

        animator.leftPole = CreatePole(animator.leftHand, "Left Adventure Ski Pole", -1f, black, white);
        animator.rightPole = CreatePole(animator.rightHand, "Right Adventure Ski Pole", 1f, black, white);
    }

    private static Transform CreateRollerSki(Transform parent, string name, float x, Color frameColor, Color wheelColor, Color accentColor)
    {
        var ski = CreateChild(parent, name, new Vector3(x, 0.03f, 0.14f));
        AddPart(ski, "Slim Roller Ski Frame", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0.18f), new Vector3(EquipmentSkiWidth, 0.025f, EquipmentSkiLength), frameColor, Vector3.zero);
        AddPart(ski, "Front Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0.67f), new Vector3(EquipmentWheelRadius * 2f, 0.085f, EquipmentWheelRadius * 2f), wheelColor, new Vector3(0f, 0f, 90f));
        AddPart(ski, "Rear Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, -0.37f), new Vector3(EquipmentWheelRadius * 2f, 0.085f, EquipmentWheelRadius * 2f), wheelColor, new Vector3(0f, 0f, 90f));
        AddPart(ski, "Classic Binding Block", PrimitiveType.Cube, new Vector3(0f, 0.13f, 0.03f), new Vector3(0.1f, 0.055f, 0.12f), accentColor, Vector3.zero);
        return ski;
    }

    private static Transform CreatePole(Transform hand, string name, float side, Color poleColor, Color highlightColor)
    {
        var pole = CreateChild(hand, name, Vector3.zero);
        AddPart(pole, "Humanoid Pole Grip", PrimitiveType.Capsule, new Vector3(0.035f * side, -0.03f, 0.02f), new Vector3(0.055f, 0.14f, 0.055f), poleColor, new Vector3(12f, 0f, 0f));
        AddPart(pole, "Humanoid Pole Strap", PrimitiveType.Capsule, new Vector3(0.1f * side, -0.14f, -0.02f), new Vector3(0.03f, 0.24f, 0.03f), poleColor, new Vector3(26f, 0f, 14f * side));
        AddPart(pole, "Humanoid Pole Shaft", PrimitiveType.Cylinder, new Vector3(0.44f * side, -0.8f, BasePosePoleBackwardZOffset), new Vector3(PoleRadius, PoleLength, PoleRadius), poleColor, new Vector3(BasePosePoleBackwardAngleDegrees, 0f, -2.5f * side));
        AddPart(pole, "Humanoid Pole Basket", PrimitiveType.Cylinder, new Vector3(0.6f * side, -1.49f, -0.56f), new Vector3(0.12f, 0.018f, 0.12f), poleColor, new Vector3(90f, 0f, 0f));
        AddPart(pole, "Humanoid Pole Force Highlight", PrimitiveType.Cylinder, new Vector3(0.51f * side, -1.05f, -0.36f), new Vector3(0.035f, 0.44f, 0.035f), highlightColor, new Vector3(BasePosePoleBackwardAngleDegrees, 0f, -2.5f * side));
        return pole;
    }

    private static void AddBootDetails(Transform foot, string sideName, Color accentColor, Color bootColor)
    {
        if (foot == null)
        {
            return;
        }

        AddPart(foot, sideName + " Roller Ski Boot Shell", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.03f), new Vector3(0.13f, 0.1f, 0.28f), bootColor, Vector3.zero);
        AddPart(foot, sideName + " Neon Boot Cuff", PrimitiveType.Cube, new Vector3(0f, 0.085f, -0.03f), new Vector3(0.14f, 0.05f, 0.08f), accentColor, Vector3.zero);
        AddPart(foot, sideName + " Heel Binding Accent", PrimitiveType.Cube, new Vector3(0f, 0.015f, -0.13f), new Vector3(0.12f, 0.04f, 0.06f), accentColor, Vector3.zero);
    }

    private static void AddHelmetOverlay(Transform visualRoot)
    {
        var helmet = CreateChild(visualRoot, "Adventure Roller Ski Helmet Overlay", new Vector3(0f, 1.82f, -0.08f));
        var black = new Color(0.005f, 0.006f, 0.008f);
        var white = new Color(0.92f, 0.94f, 0.9f);
        AddPart(helmet, "Roller Ski Helmet Shell", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.19f, 0.105f, 0.19f), black, Vector3.zero);
        AddPart(helmet, "White Helmet Rear Stripe", PrimitiveType.Cube, new Vector3(0f, 0.015f, 0.08f), new Vector3(0.095f, 0.095f, 0.035f), white, new Vector3(8f, 0f, 0f));
    }

    private static void ClearChildren(Transform root)
    {
        for (var i = root.childCount - 1; i >= 0; i--)
        {
            var child = root.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                Object.DestroyImmediate(child);
            }
        }
    }

    private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        var child = new GameObject(name).transform;
        if (parent != null)
        {
            child.SetParent(parent, false);
        }

        child.localPosition = localPosition;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return child;
    }

    private static Transform AddPart(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color, Vector3 localRotation)
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
