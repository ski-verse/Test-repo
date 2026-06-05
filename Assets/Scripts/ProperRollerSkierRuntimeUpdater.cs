using UnityEngine;

[DisallowMultipleComponent]
public class ProperRollerSkierRuntimeUpdater : MonoBehaviour
{
    private const string VisualRootName = "Roller Skier Visual";

    public const string Model20AppliedMarkerName = "Skier Model 2.0 Applied";
    public const float Model20RuntimeVisualScale = 1.32f;

    public const float EnduranceShoulderWidth = 0.7f;
    public const float EnduranceWaistWidth = 0.18f;
    public const float EnduranceHeadDiameter = 0.17f;
    public const float EnduranceLegVisualLength = 0.96f;
    public const float VisibleNeckHeight = 0.13f;
    public const float VisibleUpperBackWidth = 0.64f;
    public const float VisibleLatWidth = 0.16f;
    public const float RealisticHipWidth = 0.38f;
    public const float VisibleShoulderCapRadius = 0.122f;
    public const float VisibleScapulaWidth = 0.18f;
    public const float VisibleGluteWidth = 0.17f;
    public const float VisibleGluteDepth = 0.13f;
    public const float VisibleShortsBandHeight = 0.035f;

    public const float ClassicRollerSkiFrameLength = 1.12f;
    public const float ClassicRollerSkiFrameWidth = 0.044f;
    public const float ClassicRollerSkiWheelDiameter = 0.235f;
    public const float ClassicRollerSkiRearWheelZ = -0.28f;
    public const float ClassicRollerSkiFrontWheelZ = 0.86f;
    public const float ClassicRollerSkiHeelZ = -0.22f;
    public const float ClassicRollerSkiToeZ = 0.18f;
    public const float VisibleBindingHeight = 0.064f;
    public const float VisibleBootCuffHeight = 0.185f;
    public const float VisibleWheelSidewallWidth = 0.1f;
    public const float VisibleGloveRadius = 0.068f;
    public const float VisibleHelmetWidth = 0.215f;
    public const float VisiblePoleStrapLength = 0.25f;
    public const float VisibleGripWrapRadius = 0.073f;

    public const float VisiblePoleGripRadius = 0.06f;
    public const float VisiblePoleShaftRadius = 0.028f;
    public const float VisiblePoleShaftLength = 1.42f;
    public const float VisiblePoleLateralOffset = 0.145f;
    public const float VisiblePoleTipLateralOffset = 0.33f;
    public static readonly Color VisiblePoleShaftColor = new Color(0.012f, 0.014f, 0.016f);
    public static readonly Color PoleGripColor = new Color(0.006f, 0.007f, 0.008f);
    public static readonly Color PoleHighlightColor = new Color(0.94f, 0.96f, 0.88f);

    private static readonly Color PoleTipColor = new Color(0.015f, 0.017f, 0.019f);

    private bool applied;
    private int applyAttempts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<ProperRollerSkierRuntimeUpdater>() != null)
        {
            return;
        }

        Debug.Log("[Ski-Verse] ProperRollerSkierRuntimeUpdater started. Waiting for SkiErgGameBootstrap skier visual.");
        var updater = new GameObject("Proper Roller Skier Runtime Updater");
        updater.AddComponent<ProperRollerSkierRuntimeUpdater>();
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
        applied = ApplyProperRollerSkierModel();
        if (applied)
        {
            Destroy(gameObject);
            return;
        }

        applyAttempts++;
        if (applyAttempts == 1 || applyAttempts % 60 == 0)
        {
            Debug.Log("[Ski-Verse] ProperRollerSkierRuntimeUpdater waiting for Low Poly Roller Skier/Roller Skier Visual.");
        }
    }

    public static bool ApplyProperRollerSkierModel()
    {
        var visualRoot = FindSkierVisualRoot();
        if (visualRoot == null)
        {
            return false;
        }

        var animator = visualRoot.GetComponentInParent<RollerSkierAnimator>();
        if (animator == null)
        {
            return false;
        }

        if (visualRoot.Find(Model20AppliedMarkerName) != null)
        {
            Debug.Log("[Ski-Verse] Skier Model 2.0 already applied to bootstrap skier visual.");
            return true;
        }

        ClearChildren(visualRoot);
        var scale = Mathf.Max(visualRoot.localScale.x, Model20RuntimeVisualScale);
        visualRoot.localScale = Vector3.one * scale;
        CreateProperRollerSkierVisual(visualRoot, animator);
        CreateChild(visualRoot, Model20AppliedMarkerName, Vector3.zero);
        animator.ResetBasePose();
        SkierTechniqueRuntimeUpdater.ConfigureAnimator(animator);
        animator.ResetBasePose();
        animator.ApplyPose(0.15f);
        Debug.Log($"[Ski-Verse] Skier Model 2.0 applied to {animator.gameObject.name}/{visualRoot.name} with visual scale {visualRoot.localScale.x:0.00}.");
        return true;
    }

    private static Transform FindSkierVisualRoot()
    {
        var animators = Object.FindObjectsByType<RollerSkierAnimator>(FindObjectsSortMode.None);
        for (var i = 0; i < animators.Length; i++)
        {
            var visualRoot = animators[i].transform.Find(VisualRootName);
            if (visualRoot != null)
            {
                return visualRoot;
            }
        }

        var fallback = GameObject.Find(VisualRootName);
        return fallback != null ? fallback.transform : null;
    }

    private static void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Object.Destroy(child);
            }
            else
            {
                Object.DestroyImmediate(child);
            }
        }
    }

    private static void CreateProperRollerSkierVisual(Transform parent, RollerSkierAnimator animator)
    {
        var suitBlue = new Color(0.045f, 0.26f, 0.84f);
        var suitDark = new Color(0.028f, 0.035f, 0.05f);
        var suitBackShadow = new Color(0.028f, 0.13f, 0.42f);
        var suitSeam = new Color(0.012f, 0.015f, 0.022f);
        var bootBlack = new Color(0.015f, 0.017f, 0.022f);
        var gloveBlack = new Color(0.008f, 0.009f, 0.011f);
        var bindingRed = new Color(0.82f, 0.06f, 0.035f);
        var skin = new Color(0.95f, 0.78f, 0.58f);
        var helmetColor = new Color(0.055f, 0.06f, 0.07f);
        var visorColor = new Color(0.018f, 0.021f, 0.026f);
        var skiColor = new Color(0.9f, 0.94f, 0.95f);
        var aluminiumColor = new Color(0.68f, 0.76f, 0.78f);
        var wheelColor = new Color(0.016f, 0.016f, 0.02f);

        animator.leftSki = CreateRollerSki(parent, "Left Parallel Classic Roller Ski", -0.235f, skiColor, aluminiumColor, wheelColor, bootBlack, bindingRed);
        animator.rightSki = CreateRollerSki(parent, "Right Parallel Classic Roller Ski", 0.235f, skiColor, aluminiumColor, wheelColor, bootBlack, bindingRed);

        animator.hips = AddBodyPart(parent, "Forward Hinged Athletic Hips", PrimitiveType.Capsule, new Vector3(0f, 0.96f, 0.12f), new Vector3(RealisticHipWidth, 0.17f, 0.22f), suitDark, new Vector3(-16f, 0f, 90f));
        AddBodyPart(parent, "Natural Hip Shelf", PrimitiveType.Capsule, new Vector3(0f, 0.988f, 0.065f), new Vector3(0.38f, 0.07f, 0.16f), suitDark, new Vector3(-9f, 0f, 90f));
        AddBodyPart(parent, "Left Glute Shape", PrimitiveType.Sphere, new Vector3(-0.095f, 0.93f, 0.22f), new Vector3(VisibleGluteWidth, 0.14f, VisibleGluteDepth), suitDark, Vector3.zero);
        AddBodyPart(parent, "Right Glute Shape", PrimitiveType.Sphere, new Vector3(0.095f, 0.93f, 0.22f), new Vector3(VisibleGluteWidth, 0.14f, VisibleGluteDepth), suitDark, Vector3.zero);
        AddBodyPart(parent, "Dark Shorts Rear Panel", PrimitiveType.Cube, new Vector3(0f, 0.92f, 0.245f), new Vector3(0.34f, 0.16f, 0.035f), suitSeam, new Vector3(-10f, 0f, 0f));
        AddBodyPart(parent, "Racing Suit Waist Separation", PrimitiveType.Cube, new Vector3(0f, 1.045f, 0.035f), new Vector3(0.33f, VisibleShortsBandHeight, 0.17f), suitSeam, new Vector3(-9f, 0f, 0f));

        animator.leftThigh = AddBodyPart(parent, "Left Long Athletic Thigh", PrimitiveType.Capsule, new Vector3(-0.14f, 0.675f, 0.165f), new Vector3(0.092f, 0.49f, 0.092f), suitDark, new Vector3(-31f, 0f, 2.5f));
        animator.rightThigh = AddBodyPart(parent, "Right Long Athletic Thigh", PrimitiveType.Capsule, new Vector3(0.14f, 0.675f, 0.165f), new Vector3(0.092f, 0.49f, 0.092f), suitDark, new Vector3(-31f, 0f, -2.5f));
        AddBodyPart(parent, "Left Shorts Leg Cuff", PrimitiveType.Cube, new Vector3(-0.145f, 0.78f, 0.175f), new Vector3(0.14f, 0.035f, 0.125f), suitSeam, new Vector3(-24f, 0f, 2f));
        AddBodyPart(parent, "Right Shorts Leg Cuff", PrimitiveType.Cube, new Vector3(0.145f, 0.78f, 0.175f), new Vector3(0.14f, 0.035f, 0.125f), suitSeam, new Vector3(-24f, 0f, -2f));
        AddBodyPart(parent, "Left Soft Knee Bend", PrimitiveType.Sphere, new Vector3(-0.155f, 0.498f, 0.185f), new Vector3(0.078f, 0.072f, 0.078f), suitDark, Vector3.zero);
        AddBodyPart(parent, "Right Soft Knee Bend", PrimitiveType.Sphere, new Vector3(0.155f, 0.498f, 0.185f), new Vector3(0.078f, 0.072f, 0.078f), suitDark, Vector3.zero);
        animator.leftShin = AddBodyPart(parent, "Left Long Lower Leg", PrimitiveType.Capsule, new Vector3(-0.17f, 0.32f, 0.07f), new Vector3(0.07f, 0.45f, 0.07f), suitDark, new Vector3(17f, 0f, -2f));
        animator.rightShin = AddBodyPart(parent, "Right Long Lower Leg", PrimitiveType.Capsule, new Vector3(0.17f, 0.32f, 0.07f), new Vector3(0.07f, 0.45f, 0.07f), suitDark, new Vector3(17f, 0f, 2f));
        animator.leftFoot = CreateBoot(parent, "Left Boot", -0.235f, bootBlack, bindingRed);
        animator.rightFoot = CreateBoot(parent, "Right Boot", 0.235f, bootBlack, bindingRed);

        var torsoPivot = CreateChild(parent, "Torso Pivot", new Vector3(0f, 1.105f, 0.085f));
        animator.torso = torsoPivot;
        AddBodyPart(torsoPivot, "Tight Suit Endurance Torso", PrimitiveType.Capsule, new Vector3(0f, 0.3f, -0.055f), new Vector3(0.285f, 0.66f, 0.198f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Visible Upper Back", PrimitiveType.Capsule, new Vector3(0f, 0.49f, 0.02f), new Vector3(VisibleUpperBackWidth, 0.2f, 0.09f), suitBackShadow, new Vector3(5f, 0f, 90f));
        AddBodyPart(torsoPivot, "Left Shoulder Blade", PrimitiveType.Cube, new Vector3(-0.16f, 0.505f, 0.078f), new Vector3(VisibleScapulaWidth, 0.045f, 0.065f), suitSeam, new Vector3(3f, 0f, -16f));
        AddBodyPart(torsoPivot, "Right Shoulder Blade", PrimitiveType.Cube, new Vector3(0.16f, 0.505f, 0.078f), new Vector3(VisibleScapulaWidth, 0.045f, 0.065f), suitSeam, new Vector3(3f, 0f, 16f));
        AddBodyPart(torsoPivot, "Left Lat Taper", PrimitiveType.Capsule, new Vector3(-0.212f, 0.32f, 0.0f), new Vector3(VisibleLatWidth, 0.39f, 0.082f), suitBackShadow, new Vector3(-7f, 0f, 18f));
        AddBodyPart(torsoPivot, "Right Lat Taper", PrimitiveType.Capsule, new Vector3(0.212f, 0.32f, 0.0f), new Vector3(VisibleLatWidth, 0.39f, 0.082f), suitBackShadow, new Vector3(-7f, 0f, -18f));
        AddBodyPart(torsoPivot, "Broad V Shape Chest", PrimitiveType.Capsule, new Vector3(0f, 0.445f, -0.09f), new Vector3(EnduranceShoulderWidth, 0.245f, 0.075f), suitBlue, new Vector3(7f, 0f, 90f));
        AddBodyPart(torsoPivot, "Tapered Narrow Waist", PrimitiveType.Capsule, new Vector3(0f, 0.075f, 0.005f), new Vector3(EnduranceWaistWidth, 0.135f, 0.142f), suitDark, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Torso To Shorts Suit Cut", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.03f), new Vector3(0.27f, VisibleShortsBandHeight, 0.16f), suitSeam, Vector3.zero);
        AddBodyPart(torsoPivot, "Broad Relaxed Shoulder Line", PrimitiveType.Capsule, new Vector3(0f, 0.575f, -0.04f), new Vector3(EnduranceShoulderWidth, 0.075f, 0.13f), suitBlue, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Left Defined Shoulder Cap", PrimitiveType.Sphere, new Vector3(-0.35f, 0.545f, -0.045f), new Vector3(VisibleShoulderCapRadius, 0.092f, 0.108f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Right Defined Shoulder Cap", PrimitiveType.Sphere, new Vector3(0.35f, 0.545f, -0.045f), new Vector3(VisibleShoulderCapRadius, 0.092f, 0.108f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Visible Neck", PrimitiveType.Capsule, new Vector3(0f, 0.685f, -0.145f), new Vector3(0.078f, VisibleNeckHeight, 0.078f), skin, Vector3.zero);
        AddBodyPart(torsoPivot, "Left Collarbone Plane", PrimitiveType.Cube, new Vector3(-0.145f, 0.617f, -0.092f), new Vector3(0.22f, 0.022f, 0.038f), suitBlue, new Vector3(0f, 0f, -10f));
        AddBodyPart(torsoPivot, "Right Collarbone Plane", PrimitiveType.Cube, new Vector3(0.145f, 0.617f, -0.092f), new Vector3(0.22f, 0.022f, 0.038f), suitBlue, new Vector3(0f, 0f, 10f));

        var headPivot = CreateChild(torsoPivot, "Head Stabilizer", new Vector3(0f, 0.79f, -0.19f));
        animator.head = headPivot;
        AddBodyPart(headPivot, "Compact Head", PrimitiveType.Sphere, Vector3.zero, new Vector3(EnduranceHeadDiameter, 0.182f, EnduranceHeadDiameter), skin, Vector3.zero);
        AddBodyPart(headPivot, "Low Poly Helmet", PrimitiveType.Sphere, new Vector3(0f, 0.095f, 0f), new Vector3(VisibleHelmetWidth, 0.116f, VisibleHelmetWidth), helmetColor, Vector3.zero);
        AddBodyPart(headPivot, "Helmet Visor", PrimitiveType.Cube, new Vector3(0f, 0.058f, -0.135f), new Vector3(0.168f, 0.034f, 0.048f), visorColor, Vector3.zero);

        animator.leftArm = CreateArm(parent, "Left Connected Double-Poling Arm", new Vector3(-0.33f, 1.49f, -0.08f), -1f, suitBlue, gloveBlack);
        animator.rightArm = CreateArm(parent, "Right Connected Double-Poling Arm", new Vector3(0.33f, 1.49f, -0.08f), 1f, suitBlue, gloveBlack);
        animator.leftPole = CreatePole(parent, "Left Ski Pole", new Vector3(-0.405f, 0.98f, 0.13f), -1f);
        animator.rightPole = CreatePole(parent, "Right Ski Pole", new Vector3(0.405f, 0.98f, 0.13f), 1f);
    }

    private static Transform CreateRollerSki(Transform parent, string name, float xPosition, Color skiColor, Color aluminiumColor, Color wheelColor, Color bootColor, Color bindingColor)
    {
        var ski = CreateChild(parent, name, new Vector3(xPosition, 0f, 0.13f));
        AddBodyPart(ski, "Slim Foot Platform", PrimitiveType.Capsule, new Vector3(0f, 0.126f, -0.03f), new Vector3(0.044f, 0.27f, 0.026f), skiColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(ski, "Slim Aluminium Roller Ski Frame", PrimitiveType.Cube, new Vector3(0f, 0.108f, 0.29f), new Vector3(ClassicRollerSkiFrameWidth, 0.021f, ClassicRollerSkiFrameLength), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Inner Slim Side Rail", PrimitiveType.Cube, new Vector3(-0.025f, 0.122f, 0.29f), new Vector3(0.007f, 0.025f, 1.15f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Outer Slim Side Rail", PrimitiveType.Cube, new Vector3(0.025f, 0.122f, 0.29f), new Vector3(0.007f, 0.025f, 1.15f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Front Aluminium Fork Bridge", PrimitiveType.Cube, new Vector3(0f, 0.114f, ClassicRollerSkiFrontWheelZ), new Vector3(0.13f, 0.033f, 0.09f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Rear Aluminium Fork Bridge", PrimitiveType.Cube, new Vector3(0f, 0.114f, ClassicRollerSkiRearWheelZ), new Vector3(0.13f, 0.033f, 0.09f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Front Axle", PrimitiveType.Cylinder, new Vector3(0f, 0.078f, ClassicRollerSkiFrontWheelZ), new Vector3(0.017f, 0.168f, 0.017f), bootColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Axle", PrimitiveType.Cylinder, new Vector3(0f, 0.078f, ClassicRollerSkiRearWheelZ), new Vector3(0.017f, 0.168f, 0.017f), bootColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Large Front Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, ClassicRollerSkiFrontWheelZ), new Vector3(ClassicRollerSkiWheelDiameter, VisibleWheelSidewallWidth, ClassicRollerSkiWheelDiameter), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Large Rear Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, ClassicRollerSkiRearWheelZ), new Vector3(ClassicRollerSkiWheelDiameter, VisibleWheelSidewallWidth, ClassicRollerSkiWheelDiameter), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Bright Front Wheel Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, ClassicRollerSkiFrontWheelZ), new Vector3(0.084f, 0.105f, 0.084f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Bright Rear Wheel Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, ClassicRollerSkiRearWheelZ), new Vector3(0.084f, 0.105f, 0.084f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Front Wheel Side Highlight", PrimitiveType.Cylinder, new Vector3(0.06f, 0.027f, ClassicRollerSkiFrontWheelZ), new Vector3(0.118f, 0.01f, 0.118f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Wheel Side Highlight", PrimitiveType.Cylinder, new Vector3(0.06f, 0.027f, ClassicRollerSkiRearWheelZ), new Vector3(0.118f, 0.01f, 0.118f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Visible Binding Plate", PrimitiveType.Cube, new Vector3(0f, 0.158f, -0.035f), new Vector3(0.092f, 0.03f, 0.34f), bootColor, Vector3.zero);
        AddBodyPart(ski, "Red Toe Binding Clamp", PrimitiveType.Cube, new Vector3(0f, 0.198f, 0.12f), new Vector3(0.105f, VisibleBindingHeight, 0.06f), bindingColor, Vector3.zero);
        AddBodyPart(ski, "Rear Heel Binding Cup", PrimitiveType.Cube, new Vector3(0f, 0.188f, ClassicRollerSkiHeelZ), new Vector3(0.082f, 0.058f, 0.072f), bindingColor, Vector3.zero);
        return ski;
    }

    private static Transform CreateBoot(Transform parent, string name, float xPosition, Color bootColor, Color bindingColor)
    {
        var boot = CreateChild(parent, name, new Vector3(xPosition, 0.15f, 0.08f));
        AddBodyPart(boot, "Roller Ski Boot Lower", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(0.112f, 0.096f, 0.34f), bootColor, Vector3.zero);
        AddBodyPart(boot, "Raised Boot Toe", PrimitiveType.Cube, new Vector3(0f, 0.045f, 0.135f), new Vector3(0.108f, 0.06f, 0.118f), bootColor, new Vector3(-7f, 0f, 0f));
        AddBodyPart(boot, "Visible Boot Cuff", PrimitiveType.Capsule, new Vector3(0f, 0.12f, -0.035f), new Vector3(0.112f, VisibleBootCuffHeight, 0.104f), bootColor, new Vector3(-7f, 0f, 90f));
        AddBodyPart(boot, "Boot Heel Pocket", PrimitiveType.Cube, new Vector3(0f, 0.052f, ClassicRollerSkiHeelZ - 0.08f), new Vector3(0.105f, 0.07f, 0.08f), bootColor, Vector3.zero);
        AddBodyPart(boot, "Binding Pin Detail", PrimitiveType.Cube, new Vector3(0f, -0.01f, 0.145f), new Vector3(0.128f, 0.03f, 0.035f), bindingColor, Vector3.zero);
        return boot;
    }

    private static Transform CreateArm(Transform parent, string name, Vector3 localPosition, float side, Color suitColor, Color gloveColor)
    {
        var armPivot = CreateChild(parent, name, localPosition);
        AddBodyPart(armPivot, "Relaxed Upper Arm", PrimitiveType.Capsule, new Vector3(-0.012f * side, -0.255f, 0.07f), new Vector3(0.072f, 0.365f, 0.072f), suitColor, new Vector3(14f, 0f, 1f * side));
        AddBodyPart(armPivot, "Long Close Forearm", PrimitiveType.Capsule, new Vector3(0.014f * side, -0.592f, 0.245f), new Vector3(0.058f, 0.445f, 0.058f), suitColor, new Vector3(29f, 0f, -1f * side));
        AddBodyPart(armPivot, "Glove Cuff", PrimitiveType.Capsule, new Vector3(0.026f * side, -0.8f, 0.36f), new Vector3(0.058f, 0.1f, 0.058f), gloveColor, new Vector3(22f, 0f, -1f * side));
        AddBodyPart(armPivot, "Hand On Pole Grip", PrimitiveType.Sphere, new Vector3(0.03f * side, -0.895f, 0.43f), new Vector3(VisibleGloveRadius, VisibleGloveRadius, VisibleGloveRadius), gloveColor, Vector3.zero);
        AddBodyPart(armPivot, "Glove Wrapped Around Grip", PrimitiveType.Capsule, new Vector3(0.033f * side, -0.9f, 0.44f), new Vector3(VisibleGripWrapRadius, 0.055f, VisibleGripWrapRadius), gloveColor, new Vector3(90f, 0f, 0f));
        return armPivot;
    }

    private static Transform CreatePole(Transform parent, string name, Vector3 localPosition, float side)
    {
        var polePivot = CreateChild(parent, name, localPosition);
        AddBodyPart(polePivot, "Ergonomic Pole Grip", PrimitiveType.Capsule, new Vector3(0.016f * side, -0.035f, 0.02f), new Vector3(VisiblePoleGripRadius, 0.14f, VisiblePoleGripRadius), PoleGripColor, new Vector3(14f, 0f, 0f));
        AddBodyPart(polePivot, "Hand Contact Ring", PrimitiveType.Cylinder, new Vector3(0.016f * side, -0.035f, 0.02f), new Vector3(VisibleGripWrapRadius, 0.018f, VisibleGripWrapRadius), PoleHighlightColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(polePivot, "Wrist Strap", PrimitiveType.Cube, new Vector3(0.07f * side, -0.13f, 0.04f), new Vector3(0.018f, VisiblePoleStrapLength, 0.045f), PoleGripColor, new Vector3(18f, 0f, 10f * side));
        AddBodyPart(polePivot, "Strap Loop", PrimitiveType.Capsule, new Vector3(0.088f * side, -0.22f, 0.065f), new Vector3(0.023f, 0.19f, 0.023f), PoleGripColor, new Vector3(32f, 0f, 12f * side));
        AddBodyPart(polePivot, "Dark Visible Pole Shaft", PrimitiveType.Cylinder, new Vector3(VisiblePoleLateralOffset * side, -0.72f, 0.34f), new Vector3(VisiblePoleShaftRadius, VisiblePoleShaftLength, VisiblePoleShaftRadius), VisiblePoleShaftColor, new Vector3(24f, 0f, -1.5f * side));
        AddBodyPart(polePivot, "Upper Pole Motion Marker", PrimitiveType.Cylinder, new Vector3(VisiblePoleLateralOffset * side, -0.32f, 0.16f), new Vector3(VisiblePoleShaftRadius * 1.18f, 0.045f, VisiblePoleShaftRadius * 1.18f), PoleHighlightColor, new Vector3(24f, 0f, -1.5f * side));
        AddBodyPart(polePivot, "Lower Pole Motion Marker", PrimitiveType.Cylinder, new Vector3(VisiblePoleLateralOffset * side, -1.06f, 0.58f), new Vector3(VisiblePoleShaftRadius * 1.18f, 0.045f, VisiblePoleShaftRadius * 1.18f), PoleHighlightColor, new Vector3(24f, 0f, -1.5f * side));
        AddBodyPart(polePivot, "Compact Pole Basket", PrimitiveType.Cylinder, new Vector3(VisiblePoleTipLateralOffset * side, -1.4f, 0.78f), new Vector3(0.075f, 0.012f, 0.075f), PoleTipColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(polePivot, "Pole Tip", PrimitiveType.Sphere, new Vector3(VisiblePoleTipLateralOffset * side, -1.51f, 0.84f), new Vector3(0.045f, 0.045f, 0.045f), PoleTipColor, Vector3.zero);
        return polePivot;
    }

    private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent, false);
        child.localPosition = localPosition;
        return child;
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
