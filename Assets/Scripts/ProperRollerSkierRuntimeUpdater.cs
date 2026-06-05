using UnityEngine;

[DisallowMultipleComponent]
public class ProperRollerSkierRuntimeUpdater : MonoBehaviour
{
    private const string VisualRootName = "Roller Skier Visual";

    public const float EnduranceShoulderWidth = 0.66f;
    public const float EnduranceWaistWidth = 0.19f;
    public const float EnduranceHeadDiameter = 0.18f;
    public const float EnduranceLegVisualLength = 0.9f;
    public const float VisibleNeckHeight = 0.12f;

    public const float ClassicRollerSkiFrameLength = 1.14f;
    public const float ClassicRollerSkiFrameWidth = 0.046f;
    public const float ClassicRollerSkiWheelDiameter = 0.23f;
    public const float ClassicRollerSkiRearWheelZ = -0.15f;
    public const float ClassicRollerSkiFrontWheelZ = 0.98f;
    public const float ClassicRollerSkiHeelZ = -0.09f;
    public const float ClassicRollerSkiToeZ = 0.25f;
    public const float VisibleBindingHeight = 0.058f;
    public const float VisibleBootCuffHeight = 0.17f;
    public const float VisibleWheelSidewallWidth = 0.095f;

    public const float VisiblePoleGripRadius = 0.06f;
    public const float VisiblePoleShaftRadius = 0.028f;
    public const float VisiblePoleShaftLength = 1.4f;
    public const float VisiblePoleLateralOffset = 0.13f;
    public const float VisiblePoleTipLateralOffset = 0.31f;
    public static readonly Color VisiblePoleShaftColor = new Color(0.012f, 0.014f, 0.016f);
    public static readonly Color PoleGripColor = new Color(0.006f, 0.007f, 0.008f);
    public static readonly Color PoleHighlightColor = new Color(0.94f, 0.96f, 0.88f);

    private static readonly Color PoleTipColor = new Color(0.015f, 0.017f, 0.019f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<ProperRollerSkierRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Proper Roller Skier Runtime Updater");
        updater.AddComponent<ProperRollerSkierRuntimeUpdater>();
    }

    private void Start()
    {
        ApplyProperRollerSkierModel();
        Destroy(gameObject);
    }

    public static bool ApplyProperRollerSkierModel()
    {
        var visualRoot = GameObject.Find(VisualRootName);
        if (visualRoot == null)
        {
            return false;
        }

        var animator = visualRoot.GetComponentInParent<RollerSkierAnimator>();
        if (animator == null)
        {
            return false;
        }

        ClearChildren(visualRoot.transform);
        CreateProperRollerSkierVisual(visualRoot.transform, animator);
        animator.ApplyPose(0.15f);
        return true;
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
        var suitBlue = new Color(0.055f, 0.28f, 0.86f);
        var suitDark = new Color(0.035f, 0.043f, 0.06f);
        var bootBlack = new Color(0.018f, 0.02f, 0.024f);
        var bindingRed = new Color(0.82f, 0.06f, 0.035f);
        var skin = new Color(0.95f, 0.78f, 0.58f);
        var helmetColor = new Color(0.06f, 0.065f, 0.075f);
        var visorColor = new Color(0.018f, 0.021f, 0.026f);
        var skiColor = new Color(0.9f, 0.94f, 0.95f);
        var aluminiumColor = new Color(0.68f, 0.76f, 0.78f);
        var wheelColor = new Color(0.018f, 0.018f, 0.022f);

        animator.leftSki = CreateRollerSki(parent, "Left Parallel Classic Roller Ski", -0.235f, skiColor, aluminiumColor, wheelColor, bootBlack, bindingRed);
        animator.rightSki = CreateRollerSki(parent, "Right Parallel Classic Roller Ski", 0.235f, skiColor, aluminiumColor, wheelColor, bootBlack, bindingRed);

        animator.hips = AddBodyPart(parent, "Forward Hinged Athletic Hips", PrimitiveType.Capsule, new Vector3(0f, 0.96f, 0.12f), new Vector3(0.32f, 0.17f, 0.22f), suitDark, new Vector3(-16f, 0f, 90f));
        AddBodyPart(parent, "Natural Hip Shelf", PrimitiveType.Capsule, new Vector3(0f, 0.985f, 0.065f), new Vector3(0.36f, 0.075f, 0.17f), suitDark, new Vector3(-9f, 0f, 90f));

        animator.leftThigh = AddBodyPart(parent, "Left Long Athletic Thigh", PrimitiveType.Capsule, new Vector3(-0.14f, 0.68f, 0.165f), new Vector3(0.096f, 0.47f, 0.096f), suitDark, new Vector3(-31f, 0f, 2.5f));
        animator.rightThigh = AddBodyPart(parent, "Right Long Athletic Thigh", PrimitiveType.Capsule, new Vector3(0.14f, 0.68f, 0.165f), new Vector3(0.096f, 0.47f, 0.096f), suitDark, new Vector3(-31f, 0f, -2.5f));
        AddBodyPart(parent, "Left Soft Knee Bend", PrimitiveType.Sphere, new Vector3(-0.155f, 0.505f, 0.185f), new Vector3(0.082f, 0.075f, 0.082f), suitDark, Vector3.zero);
        AddBodyPart(parent, "Right Soft Knee Bend", PrimitiveType.Sphere, new Vector3(0.155f, 0.505f, 0.185f), new Vector3(0.082f, 0.075f, 0.082f), suitDark, Vector3.zero);
        animator.leftShin = AddBodyPart(parent, "Left Long Lower Leg", PrimitiveType.Capsule, new Vector3(-0.17f, 0.33f, 0.07f), new Vector3(0.074f, 0.43f, 0.074f), suitDark, new Vector3(17f, 0f, -2f));
        animator.rightShin = AddBodyPart(parent, "Right Long Lower Leg", PrimitiveType.Capsule, new Vector3(0.17f, 0.33f, 0.07f), new Vector3(0.074f, 0.43f, 0.074f), suitDark, new Vector3(17f, 0f, 2f));
        animator.leftFoot = CreateBoot(parent, "Left Boot", -0.235f, bootBlack, bindingRed);
        animator.rightFoot = CreateBoot(parent, "Right Boot", 0.235f, bootBlack, bindingRed);

        var torsoPivot = CreateChild(parent, "Torso Pivot", new Vector3(0f, 1.105f, 0.085f));
        animator.torso = torsoPivot;
        AddBodyPart(torsoPivot, "Lean Endurance Torso", PrimitiveType.Capsule, new Vector3(0f, 0.3f, -0.055f), new Vector3(0.3f, 0.66f, 0.205f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Broad V Shape Chest", PrimitiveType.Capsule, new Vector3(0f, 0.445f, -0.09f), new Vector3(EnduranceShoulderWidth, 0.26f, 0.08f), suitBlue, new Vector3(7f, 0f, 90f));
        AddBodyPart(torsoPivot, "Tapered Narrow Waist", PrimitiveType.Capsule, new Vector3(0f, 0.075f, 0.005f), new Vector3(EnduranceWaistWidth, 0.14f, 0.148f), suitDark, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Broad Relaxed Shoulder Line", PrimitiveType.Capsule, new Vector3(0f, 0.575f, -0.04f), new Vector3(EnduranceShoulderWidth, 0.075f, 0.13f), suitBlue, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Left Structured Shoulder", PrimitiveType.Sphere, new Vector3(-0.33f, 0.545f, -0.045f), new Vector3(0.115f, 0.09f, 0.105f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Right Structured Shoulder", PrimitiveType.Sphere, new Vector3(0.33f, 0.545f, -0.045f), new Vector3(0.115f, 0.09f, 0.105f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Visible Neck", PrimitiveType.Capsule, new Vector3(0f, 0.68f, -0.145f), new Vector3(0.082f, VisibleNeckHeight, 0.082f), skin, Vector3.zero);
        AddBodyPart(torsoPivot, "Left Collarbone Plane", PrimitiveType.Cube, new Vector3(-0.135f, 0.615f, -0.09f), new Vector3(0.21f, 0.026f, 0.04f), suitBlue, new Vector3(0f, 0f, -10f));
        AddBodyPart(torsoPivot, "Right Collarbone Plane", PrimitiveType.Cube, new Vector3(0.135f, 0.615f, -0.09f), new Vector3(0.21f, 0.026f, 0.04f), suitBlue, new Vector3(0f, 0f, 10f));

        var headPivot = CreateChild(torsoPivot, "Head Stabilizer", new Vector3(0f, 0.79f, -0.19f));
        animator.head = headPivot;
        AddBodyPart(headPivot, "Compact Head", PrimitiveType.Sphere, Vector3.zero, new Vector3(EnduranceHeadDiameter, 0.19f, EnduranceHeadDiameter), skin, Vector3.zero);
        AddBodyPart(headPivot, "Low Poly Helmet", PrimitiveType.Sphere, new Vector3(0f, 0.1f, 0f), new Vector3(0.215f, 0.12f, 0.215f), helmetColor, Vector3.zero);
        AddBodyPart(headPivot, "Helmet Visor", PrimitiveType.Cube, new Vector3(0f, 0.062f, -0.14f), new Vector3(0.18f, 0.036f, 0.052f), visorColor, Vector3.zero);

        animator.leftArm = CreateArm(parent, "Left Connected Double-Poling Arm", new Vector3(-0.305f, 1.49f, -0.08f), -1f, suitBlue, skin);
        animator.rightArm = CreateArm(parent, "Right Connected Double-Poling Arm", new Vector3(0.305f, 1.49f, -0.08f), 1f, suitBlue, skin);
        animator.leftPole = CreatePole(parent, "Left Ski Pole", new Vector3(-0.405f, 0.98f, 0.13f), -1f);
        animator.rightPole = CreatePole(parent, "Right Ski Pole", new Vector3(0.405f, 0.98f, 0.13f), 1f);
    }

    private static Transform CreateRollerSki(Transform parent, string name, float xPosition, Color skiColor, Color aluminiumColor, Color wheelColor, Color bootColor, Color bindingColor)
    {
        var ski = CreateChild(parent, name, new Vector3(xPosition, 0f, 0.13f));
        AddBodyPart(ski, "Slim Foot Platform", PrimitiveType.Capsule, new Vector3(0f, 0.126f, -0.03f), new Vector3(0.046f, 0.28f, 0.026f), skiColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(ski, "Slim Aluminium Roller Ski Frame", PrimitiveType.Cube, new Vector3(0f, 0.108f, 0.285f), new Vector3(ClassicRollerSkiFrameWidth, 0.021f, ClassicRollerSkiFrameLength), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Inner Slim Side Rail", PrimitiveType.Cube, new Vector3(-0.026f, 0.122f, 0.285f), new Vector3(0.008f, 0.025f, 1.17f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Outer Slim Side Rail", PrimitiveType.Cube, new Vector3(0.026f, 0.122f, 0.285f), new Vector3(0.008f, 0.025f, 1.17f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Front Aluminium Fork Bridge", PrimitiveType.Cube, new Vector3(0f, 0.114f, 0.85f), new Vector3(0.13f, 0.033f, 0.09f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Rear Aluminium Fork Bridge", PrimitiveType.Cube, new Vector3(0f, 0.114f, -0.28f), new Vector3(0.13f, 0.033f, 0.09f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Front Axle", PrimitiveType.Cylinder, new Vector3(0f, 0.078f, 0.85f), new Vector3(0.017f, 0.165f, 0.017f), bootColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Axle", PrimitiveType.Cylinder, new Vector3(0f, 0.078f, -0.28f), new Vector3(0.017f, 0.165f, 0.017f), bootColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Large Front Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, 0.85f), new Vector3(ClassicRollerSkiWheelDiameter, VisibleWheelSidewallWidth, ClassicRollerSkiWheelDiameter), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Large Rear Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, -0.28f), new Vector3(ClassicRollerSkiWheelDiameter, VisibleWheelSidewallWidth, ClassicRollerSkiWheelDiameter), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Bright Front Wheel Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, 0.85f), new Vector3(0.082f, 0.101f, 0.082f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Bright Rear Wheel Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.027f, -0.28f), new Vector3(0.082f, 0.101f, 0.082f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Front Wheel Side Highlight", PrimitiveType.Cylinder, new Vector3(0.058f, 0.027f, 0.85f), new Vector3(0.116f, 0.01f, 0.116f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Wheel Side Highlight", PrimitiveType.Cylinder, new Vector3(0.058f, 0.027f, -0.28f), new Vector3(0.116f, 0.01f, 0.116f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Visible Binding Plate", PrimitiveType.Cube, new Vector3(0f, 0.158f, -0.035f), new Vector3(0.092f, 0.03f, 0.34f), bootColor, Vector3.zero);
        AddBodyPart(ski, "Red Toe Binding Clamp", PrimitiveType.Cube, new Vector3(0f, 0.195f, 0.12f), new Vector3(0.105f, VisibleBindingHeight, 0.06f), bindingColor, Vector3.zero);
        AddBodyPart(ski, "Rear Heel Binding Cup", PrimitiveType.Cube, new Vector3(0f, 0.188f, -0.2f), new Vector3(0.082f, 0.056f, 0.072f), bindingColor, Vector3.zero);
        return ski;
    }

    private static Transform CreateBoot(Transform parent, string name, float xPosition, Color bootColor, Color bindingColor)
    {
        var boot = CreateChild(parent, name, new Vector3(xPosition, 0.15f, 0.08f));
        AddBodyPart(boot, "Roller Ski Boot Lower", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(0.12f, 0.1f, 0.34f), bootColor, Vector3.zero);
        AddBodyPart(boot, "Raised Boot Toe", PrimitiveType.Cube, new Vector3(0f, 0.045f, 0.14f), new Vector3(0.115f, 0.062f, 0.12f), bootColor, new Vector3(-7f, 0f, 0f));
        AddBodyPart(boot, "Visible Boot Cuff", PrimitiveType.Capsule, new Vector3(0f, 0.115f, -0.035f), new Vector3(0.118f, VisibleBootCuffHeight, 0.108f), bootColor, new Vector3(-7f, 0f, 90f));
        AddBodyPart(boot, "Binding Pin Detail", PrimitiveType.Cube, new Vector3(0f, -0.01f, 0.145f), new Vector3(0.132f, 0.03f, 0.035f), bindingColor, Vector3.zero);
        return boot;
    }

    private static Transform CreateArm(Transform parent, string name, Vector3 localPosition, float side, Color suitColor, Color skinColor)
    {
        var armPivot = CreateChild(parent, name, localPosition);
        AddBodyPart(armPivot, "Relaxed Upper Arm", PrimitiveType.Capsule, new Vector3(-0.012f * side, -0.255f, 0.07f), new Vector3(0.07f, 0.36f, 0.07f), suitColor, new Vector3(14f, 0f, 1f * side));
        AddBodyPart(armPivot, "Long Close Forearm", PrimitiveType.Capsule, new Vector3(0.018f * side, -0.59f, 0.245f), new Vector3(0.058f, 0.44f, 0.058f), suitColor, new Vector3(29f, 0f, -1.5f * side));
        AddBodyPart(armPivot, "Hand On Pole Grip", PrimitiveType.Sphere, new Vector3(0.04f * side, -0.895f, 0.43f), new Vector3(0.078f, 0.078f, 0.078f), skinColor, Vector3.zero);
        return armPivot;
    }

    private static Transform CreatePole(Transform parent, string name, Vector3 localPosition, float side)
    {
        var polePivot = CreateChild(parent, name, localPosition);
        AddBodyPart(polePivot, "Ergonomic Pole Grip", PrimitiveType.Capsule, new Vector3(0.018f * side, -0.035f, 0.02f), new Vector3(VisiblePoleGripRadius, 0.13f, VisiblePoleGripRadius), PoleGripColor, new Vector3(14f, 0f, 0f));
        AddBodyPart(polePivot, "Wrist Strap", PrimitiveType.Cube, new Vector3(0.065f * side, -0.12f, 0.04f), new Vector3(0.018f, 0.22f, 0.042f), PoleGripColor, new Vector3(18f, 0f, 10f * side));
        AddBodyPart(polePivot, "Dark Visible Pole Shaft", PrimitiveType.Cylinder, new Vector3(VisiblePoleLateralOffset * side, -0.71f, 0.34f), new Vector3(VisiblePoleShaftRadius, VisiblePoleShaftLength, VisiblePoleShaftRadius), VisiblePoleShaftColor, new Vector3(24f, 0f, -1.5f * side));
        AddBodyPart(polePivot, "Upper Pole Motion Marker", PrimitiveType.Cylinder, new Vector3(VisiblePoleLateralOffset * side, -0.32f, 0.16f), new Vector3(VisiblePoleShaftRadius * 1.18f, 0.045f, VisiblePoleShaftRadius * 1.18f), PoleHighlightColor, new Vector3(24f, 0f, -1.5f * side));
        AddBodyPart(polePivot, "Lower Pole Motion Marker", PrimitiveType.Cylinder, new Vector3(VisiblePoleLateralOffset * side, -1.05f, 0.58f), new Vector3(VisiblePoleShaftRadius * 1.18f, 0.045f, VisiblePoleShaftRadius * 1.18f), PoleHighlightColor, new Vector3(24f, 0f, -1.5f * side));
        AddBodyPart(polePivot, "Compact Pole Basket", PrimitiveType.Cylinder, new Vector3(VisiblePoleTipLateralOffset * side, -1.38f, 0.78f), new Vector3(0.075f, 0.012f, 0.075f), PoleTipColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(polePivot, "Pole Tip", PrimitiveType.Sphere, new Vector3(VisiblePoleTipLateralOffset * side, -1.49f, 0.84f), new Vector3(0.045f, 0.045f, 0.045f), PoleTipColor, Vector3.zero);
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
