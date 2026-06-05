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
    public const string BoneAttachedEquipmentRootName = "Adventure Stable Equipment Constraint Rig";
    public const string LeftAdventurePoleName = "Left Adventure Ski Pole";
    public const string RightAdventurePoleName = "Right Adventure Ski Pole";
    public const bool DisableProceduralAnimationForAdventure = true;
    public const bool SkipGenericPoleVisibilityForAdventure = true;
    public const bool AttachAdventurePolesDirectlyToHands = false;
    public const bool AttachAdventureEquipmentToHumanoid = false;
    public const bool UseAdventureCharacterPrefabInGameplay = true;
    public const float CharacterYawDegrees = 0f;
    public const float CharacterWidthScale = 0.72f;
    public const float LegChainLateralCompression = 0.22f;
    public const float LowerLegChainLateralCompression = 0.24f;
    public const float NarrowUpperLegTrackHalfWidth = 0.14f;
    public const float NarrowFootTrackHalfWidth = 0.105f;
    public const float FootBindingLateralOffset = 0f;
    public const float BasePoseUpperArmDropMuscle = 0.46f;
    public const float BasePoseForearmBendMuscle = 0.2f;
    public const float BasePoseHipHingeMuscle = 0.12f;
    public const float BasePoseKneeBendMuscle = 0.01f;
    public const float BasePoseLegInwardMuscle = 0f;
    public const float BasePosePoleBackwardAngleDegrees = -14f;
    public const float BasePosePoleBackwardZOffset = -0.16f;

    private const string VisualRootName = "Roller Skier Visual";
    private const float EquipmentSkiLength = 1.0f;
    private const float EquipmentSkiWidth = 0.042f;
    private const float EquipmentWheelRadius = 0.095f;
    private const float PoleRadius = 0.026f;
    private const float PoleLength = 0.72f;

    private bool applied;
    private int attempts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (!UseAdventureCharacterPrefabInGameplay)
        {
            return;
        }

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
        if (!UseAdventureCharacterPrefabInGameplay)
        {
            return false;
        }

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
        DisableImportedCharacterAnimation(character);
        new GameObject(AdventureCharacterAppliedMarkerName).transform.SetParent(visualRoot, false);

        if (DisableProceduralAnimationForAdventure)
        {
            ClearAnimatorBodyReferences(animator);
            animator.ResetBasePose();
            animator.enabled = false;
        }

        if (!SkipGenericPoleVisibilityForAdventure)
        {
            PoleVisibilityRuntimeUpdater.ApplyPoleVisibilityPass();
        }

        Debug.Log("[Ski-Verse] Adventure Character active skier body applied as a stable neutral model with no poles, no roller skis, and no procedural animation.");
        return true;
#else
        return false;
#endif
    }

    private static void DisableImportedCharacterAnimation(GameObject character)
    {
        if (character == null)
        {
            return;
        }

        var animators = character.GetComponentsInChildren<Animator>(true);
        for (var i = 0; i < animators.Length; i++)
        {
            animators[i].enabled = false;
        }
    }

    private static void ClearAnimatorBodyReferences(RollerSkierAnimator animator)
    {
        if (animator == null)
        {
            return;
        }

        animator.hips = null;
        animator.torso = null;
        animator.head = null;
        animator.leftArm = null;
        animator.rightArm = null;
        animator.leftHand = null;
        animator.rightHand = null;
        animator.leftPole = null;
        animator.rightPole = null;
        animator.leftThigh = null;
        animator.rightThigh = null;
        animator.leftShin = null;
        animator.rightShin = null;
        animator.leftFoot = null;
        animator.rightFoot = null;
        animator.leftSki = null;
        animator.rightSki = null;
    }

    private static void ApplyHumanoidBasePose(GameObject character)
    {
        if (character == null)
        {
            return;
        }

        if (!TryApplyHumanPose(character))
        {
            ApplyFallbackBonePose(character.transform);
        }

        ApplyParallelLegChainSpacing(character.transform);
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
            SetMuscle(muscles, "Neck Nod Down-Up", 0.02f);
            SetMuscle(muscles, "Head Nod Down-Up", 0.02f);

            SetMuscle(muscles, "Left Arm Down-Up", -BasePoseUpperArmDropMuscle);
            SetMuscle(muscles, "Right Arm Down-Up", -BasePoseUpperArmDropMuscle);
            SetMuscle(muscles, "Left Arm Front-Back", 0.08f);
            SetMuscle(muscles, "Right Arm Front-Back", 0.08f);
            SetMuscle(muscles, "Left Forearm Stretch", -BasePoseForearmBendMuscle);
            SetMuscle(muscles, "Right Forearm Stretch", -BasePoseForearmBendMuscle);
            SetMuscle(muscles, "Left Hand Down-Up", -0.04f);
            SetMuscle(muscles, "Right Hand Down-Up", -0.04f);

            SetMuscle(muscles, "Left Upper Leg Front-Back", 0f);
            SetMuscle(muscles, "Right Upper Leg Front-Back", 0f);
            SetMuscle(muscles, "Left Upper Leg In-Out", BasePoseLegInwardMuscle);
            SetMuscle(muscles, "Right Upper Leg In-Out", -BasePoseLegInwardMuscle);
            SetMuscle(muscles, "Left Lower Leg Stretch", -BasePoseKneeBendMuscle);
            SetMuscle(muscles, "Right Lower Leg Stretch", -BasePoseKneeBendMuscle);
            SetMuscle(muscles, "Left Foot Up-Down", -0.01f);
            SetMuscle(muscles, "Right Foot Up-Down", -0.01f);

            pose.muscles = muscles;
            poseHandler.SetHumanPose(ref pose);
            humanoidAnimator.enabled = false;
            Debug.Log("[Ski-Verse] Adventure Character humanoid base pose applied with tight full-leg roller ski stance.");
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
        ApplyLocalRotationDelta(root, "pelvis", new Vector3(-3f, 0f, 0f));
        ApplyLocalRotationDelta(root, "spine_01", new Vector3(-3f, 0f, 0f));
        ApplyLocalRotationDelta(root, "spine_02", new Vector3(-2f, 0f, 0f));
        ApplyLocalRotationDelta(root, "spine_03", new Vector3(-2f, 0f, 0f));
        ApplyLocalRotationDelta(root, "head", new Vector3(2f, 0f, 0f));

        ApplyLocalRotationDelta(root, "upperarm_l", new Vector3(0f, 0f, -44f));
        ApplyLocalRotationDelta(root, "upperarm_r", new Vector3(0f, 0f, 44f));
        ApplyLocalRotationDelta(root, "lowerarm_l", new Vector3(0f, 0f, -12f));
        ApplyLocalRotationDelta(root, "lowerarm_r", new Vector3(0f, 0f, 12f));

        ApplyLocalRotationDelta(root, "thigh_l", Vector3.zero);
        ApplyLocalRotationDelta(root, "thigh_r", Vector3.zero);
        ApplyLocalRotationDelta(root, "calf_l", new Vector3(-0.5f, 0f, 0f));
        ApplyLocalRotationDelta(root, "calf_r", new Vector3(-0.5f, 0f, 0f));
        Debug.Log("[Ski-Verse] Adventure Character fallback base pose applied with tight full-leg roller ski stance.");
    }

    private static void ApplyParallelLegChainSpacing(Transform root)
    {
        CompressBoneLateralPosition(root, "thigh_l", LegChainLateralCompression);
        CompressBoneLateralPosition(root, "thigh_r", LegChainLateralCompression);
        CompressBoneLateralPosition(root, "calf_l", LowerLegChainLateralCompression);
        CompressBoneLateralPosition(root, "calf_r", LowerLegChainLateralCompression);
        CompressBoneLateralPosition(root, "foot_l", LowerLegChainLateralCompression);
        CompressBoneLateralPosition(root, "foot_r", LowerLegChainLateralCompression);
    }

    private static void CompressBoneLateralPosition(Transform root, string boneName, float compression)
    {
        var bone = FindDeepChild(root, boneName);
        if (bone == null)
        {
            return;
        }

        var localPosition = bone.localPosition;
        bone.localPosition = new Vector3(localPosition.x * compression, localPosition.y, localPosition.z);
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

    private static bool AttachEquipmentToHumanoidBones(Transform visualRoot, GameObject character, RollerSkierAnimator animator)
    {
        var humanAnimator = character.GetComponentInChildren<Animator>(true);
        var leftFoot = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.LeftFoot, "foot_l");
        var rightFoot = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.RightFoot, "foot_r");
        var leftHand = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.LeftHand, "hand_l");
        var rightHand = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.RightHand, "hand_r");

        if (leftFoot == null || rightFoot == null || leftHand == null || rightHand == null)
        {
            Debug.LogWarning("[Ski-Verse] Adventure Character rig missing foot or hand bones; keeping generated skier fallback instead of creating floating equipment.");
            return false;
        }

        var equipmentRoot = CreateChild(visualRoot, BoneAttachedEquipmentRootName, Vector3.zero);
        var aluminium = new Color(0.72f, 0.78f, 0.78f);
        var black = new Color(0.005f, 0.006f, 0.008f);
        var neon = new Color(0.72f, 0.9f, 0.08f);
        var white = new Color(0.92f, 0.94f, 0.9f);

        animator.hips = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.Hips, "pelvis");
        animator.torso = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.Chest, "spine_03");
        animator.head = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.Head, "head");
        animator.leftArm = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.LeftUpperArm, "upperarm_l");
        animator.rightArm = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.RightUpperArm, "upperarm_r");
        animator.leftHand = leftHand;
        animator.rightHand = rightHand;
        var leftThigh = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.LeftUpperLeg, "thigh_l");
        var rightThigh = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.RightUpperLeg, "thigh_r");
        var leftShin = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.LeftLowerLeg, "calf_l");
        var rightShin = FindHumanoidBone(character.transform, humanAnimator, HumanBodyBones.RightLowerLeg, "calf_r");
        animator.leftThigh = leftThigh;
        animator.rightThigh = rightThigh;
        animator.leftShin = leftShin;
        animator.rightShin = rightShin;
        animator.leftFoot = leftFoot;
        animator.rightFoot = rightFoot;

        AddNarrowStanceConstraint(character, visualRoot, leftThigh, rightThigh, leftShin, rightShin, leftFoot, rightFoot);
        animator.leftSki = CreateConstrainedRollerSki(equipmentRoot, visualRoot, leftFoot, "Left Adventure Roller Ski", -1f, aluminium, black, neon);
        animator.rightSki = CreateConstrainedRollerSki(equipmentRoot, visualRoot, rightFoot, "Right Adventure Roller Ski", 1f, aluminium, black, neon);
        AddConstrainedBootDetails(equipmentRoot, visualRoot, leftFoot, "Left", -1f, neon, black);
        AddConstrainedBootDetails(equipmentRoot, visualRoot, rightFoot, "Right", 1f, neon, black);
        animator.leftPole = CreateHandAttachedPole(visualRoot, leftHand, LeftAdventurePoleName, -1f, black, white);
        animator.rightPole = CreateHandAttachedPole(visualRoot, rightHand, RightAdventurePoleName, 1f, black, white);
        return true;
    }

    private static Transform FindHumanoidBone(Transform root, Animator humanAnimator, HumanBodyBones humanBone, string fallbackName)
    {
        if (humanAnimator != null && humanAnimator.avatar != null && humanAnimator.avatar.isHuman)
        {
            var bone = humanAnimator.GetBoneTransform(humanBone);
            if (bone != null)
            {
                return bone;
            }
        }

        return FindDeepChild(root, fallbackName);
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

    private static Transform CreateConstrainedRollerSki(Transform parent, Transform orientationRoot, Transform foot, string name, float side, Color frameColor, Color wheelColor, Color accentColor)
    {
        var bindingOffset = side * FootBindingLateralOffset;
        var ski = CreateConstrainedAttachment(parent, orientationRoot, foot, name, new Vector3(bindingOffset, -0.09f, 0.12f), Vector3.zero, side * NarrowFootTrackHalfWidth);
        AddPart(ski, "Slim Roller Ski Frame", PrimitiveType.Cube, new Vector3(0f, -0.015f, 0.18f), new Vector3(EquipmentSkiWidth, 0.025f, EquipmentSkiLength), frameColor, Vector3.zero);
        AddPart(ski, "Front Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, -0.06f, 0.64f), new Vector3(EquipmentWheelRadius * 2f, 0.075f, EquipmentWheelRadius * 2f), wheelColor, new Vector3(0f, 0f, 90f));
        AddPart(ski, "Rear Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, -0.06f, -0.34f), new Vector3(EquipmentWheelRadius * 2f, 0.075f, EquipmentWheelRadius * 2f), wheelColor, new Vector3(0f, 0f, 90f));
        AddPart(ski, "Classic Binding Block", PrimitiveType.Cube, new Vector3(0f, 0.025f, 0.03f), new Vector3(0.1f, 0.055f, 0.12f), accentColor, Vector3.zero);
        return ski;
    }

    private static void AddConstrainedBootDetails(Transform parent, Transform orientationRoot, Transform foot, string sideName, float side, Color accentColor, Color bootColor)
    {
        var bindingOffset = side * FootBindingLateralOffset;
        var boot = CreateConstrainedAttachment(parent, orientationRoot, foot, sideName + " Adventure Boot Anchor", new Vector3(bindingOffset, -0.025f, 0.04f), Vector3.zero, side * NarrowFootTrackHalfWidth);
        AddPart(boot, sideName + " Roller Ski Boot Shell", PrimitiveType.Cube, Vector3.zero, new Vector3(0.13f, 0.1f, 0.28f), bootColor, Vector3.zero);
        AddPart(boot, sideName + " Neon Boot Cuff", PrimitiveType.Cube, new Vector3(0f, 0.065f, -0.07f), new Vector3(0.14f, 0.05f, 0.08f), accentColor, Vector3.zero);
        AddPart(boot, sideName + " Heel Binding Accent", PrimitiveType.Cube, new Vector3(0f, -0.005f, -0.15f), new Vector3(0.12f, 0.04f, 0.06f), accentColor, Vector3.zero);
    }

    private static Transform CreateHandAttachedPole(Transform orientationRoot, Transform hand, string name, float side, Color poleColor, Color highlightColor)
    {
        var pole = CreateChild(hand, name, Vector3.zero);
        var follower = pole.gameObject.AddComponent<AdventureEquipmentBoneFollower>();
        follower.target = hand;
        follower.orientationRoot = orientationRoot;
        follower.rootSpaceOffset = Vector3.zero;
        follower.rootSpaceEuler = Vector3.zero;
        follower.ApplyNow();

        AddPart(pole, "Humanoid Hand Grip Collar", PrimitiveType.Sphere, new Vector3(0f, -0.015f, 0f), new Vector3(0.075f, 0.075f, 0.075f), poleColor, Vector3.zero);
        AddPart(pole, "Humanoid Pole Grip", PrimitiveType.Capsule, new Vector3(0.018f * side, -0.04f, 0.01f), new Vector3(0.06f, 0.13f, 0.06f), poleColor, new Vector3(12f, 0f, 0f));
        AddPart(pole, "Humanoid Pole Strap", PrimitiveType.Capsule, new Vector3(0.055f * side, -0.105f, -0.02f), new Vector3(0.028f, 0.17f, 0.028f), poleColor, new Vector3(26f, 0f, 12f * side));
        AddPart(pole, "Humanoid Pole Shaft", PrimitiveType.Cylinder, new Vector3(0.13f * side, -0.62f, BasePosePoleBackwardZOffset), new Vector3(PoleRadius, PoleLength, PoleRadius), poleColor, new Vector3(BasePosePoleBackwardAngleDegrees, 0f, -2.5f * side));
        AddPart(pole, "Humanoid Pole Basket", PrimitiveType.Cylinder, new Vector3(0.18f * side, -1.2f, -0.44f), new Vector3(0.09f, 0.014f, 0.09f), poleColor, new Vector3(90f, 0f, 0f));
        AddPart(pole, "Humanoid Pole Force Highlight", PrimitiveType.Cylinder, new Vector3(0.15f * side, -0.86f, -0.3f), new Vector3(0.026f, 0.28f, 0.026f), highlightColor, new Vector3(BasePosePoleBackwardAngleDegrees, 0f, -2.5f * side));
        return pole;
    }

    private static Transform CreateConstrainedAttachment(Transform parent, Transform orientationRoot, Transform target, string name, Vector3 rootSpaceOffset, Vector3 rootSpaceEuler)
    {
        return CreateConstrainedAttachment(parent, orientationRoot, target, name, rootSpaceOffset, rootSpaceEuler, float.NaN);
    }

    private static Transform CreateConstrainedAttachment(Transform parent, Transform orientationRoot, Transform target, string name, Vector3 rootSpaceOffset, Vector3 rootSpaceEuler, float lockedRootSpaceX)
    {
        var attachment = CreateChild(parent, name, Vector3.zero);
        var follower = attachment.gameObject.AddComponent<AdventureEquipmentBoneFollower>();
        follower.target = target;
        follower.orientationRoot = orientationRoot;
        follower.rootSpaceOffset = rootSpaceOffset;
        follower.rootSpaceEuler = rootSpaceEuler;
        follower.lockRootSpaceX = !float.IsNaN(lockedRootSpaceX);
        follower.lockedRootSpaceX = lockedRootSpaceX;
        follower.ApplyNow();
        return attachment;
    }

    private static void AddNarrowStanceConstraint(GameObject character, Transform orientationRoot, Transform leftThigh, Transform rightThigh, Transform leftShin, Transform rightShin, Transform leftFoot, Transform rightFoot)
    {
        var constraint = character.AddComponent<AdventureNarrowStanceConstraint>();
        constraint.orientationRoot = orientationRoot;
        constraint.leftThigh = leftThigh;
        constraint.rightThigh = rightThigh;
        constraint.leftShin = leftShin;
        constraint.rightShin = rightShin;
        constraint.leftFoot = leftFoot;
        constraint.rightFoot = rightFoot;
        constraint.upperTrackHalfWidth = NarrowUpperLegTrackHalfWidth;
        constraint.lowerTrackHalfWidth = NarrowFootTrackHalfWidth;
        constraint.ApplyNow();
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

public sealed class AdventureEquipmentBoneFollower : MonoBehaviour
{
    public Transform target;
    public Transform orientationRoot;
    public Vector3 rootSpaceOffset;
    public Vector3 rootSpaceEuler;
    public bool lockRootSpaceX;
    public float lockedRootSpaceX;

    private void LateUpdate()
    {
        ApplyNow();
    }

    public void ApplyNow()
    {
        if (target == null)
        {
            return;
        }

        var rootRotation = orientationRoot != null ? orientationRoot.rotation : Quaternion.identity;
        var nextPosition = target.position + rootRotation * rootSpaceOffset;
        if (lockRootSpaceX && orientationRoot != null)
        {
            var rootSpacePosition = orientationRoot.InverseTransformPoint(nextPosition);
            rootSpacePosition.x = lockedRootSpaceX;
            nextPosition = orientationRoot.TransformPoint(rootSpacePosition);
        }

        transform.position = nextPosition;
        transform.rotation = rootRotation * Quaternion.Euler(rootSpaceEuler);
    }
}

public sealed class AdventureNarrowStanceConstraint : MonoBehaviour
{
    public Transform orientationRoot;
    public Transform leftThigh;
    public Transform rightThigh;
    public Transform leftShin;
    public Transform rightShin;
    public Transform leftFoot;
    public Transform rightFoot;
    public float upperTrackHalfWidth;
    public float lowerTrackHalfWidth;

    private void LateUpdate()
    {
        ApplyNow();
    }

    public void ApplyNow()
    {
        ApplyRootSpaceX(leftThigh, -upperTrackHalfWidth);
        ApplyRootSpaceX(rightThigh, upperTrackHalfWidth);
        ApplyRootSpaceX(leftShin, -lowerTrackHalfWidth);
        ApplyRootSpaceX(rightShin, lowerTrackHalfWidth);
        ApplyRootSpaceX(leftFoot, -lowerTrackHalfWidth);
        ApplyRootSpaceX(rightFoot, lowerTrackHalfWidth);
    }

    private void ApplyRootSpaceX(Transform bone, float rootSpaceX)
    {
        if (bone == null || orientationRoot == null)
        {
            return;
        }

        var rootSpacePosition = orientationRoot.InverseTransformPoint(bone.position);
        rootSpacePosition.x = rootSpaceX;
        bone.position = orientationRoot.TransformPoint(rootSpacePosition);
    }
}
