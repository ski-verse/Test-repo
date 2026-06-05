using UnityEngine;

public static class SkiClassicsSkierModelBuilder
{
    public const string DefaultModelName = "Ski Classics Roller Skier Model";
    public const string GameplayModelAppliedMarkerName = "Ski Classics Gameplay Model Applied";

    public const float ShoulderWidth = 0.7f;
    public const float WaistWidth = 0.22f;
    public const float HipWidth = 0.38f;
    public const float ThighLength = 0.5f;
    public const float CalfLength = 0.44f;
    public const float HeadDiameter = 0.17f;

    public const float ClassicRollerSkiLength = 1.12f;
    public const float ClassicRollerSkiWidth = 0.048f;
    public const float WheelDiameter = 0.23f;
    public const float PoleShaftRadius = 0.04f;
    public const float PoleAttachmentForwardOffset = 0.26f;
    public const float PoleAttachmentLateralOffset = 0.38f;

    public static Transform CreateModel(Transform parent, string modelName = DefaultModelName)
    {
        var modelRoot = CreateChild(parent, modelName, Vector3.zero);
        modelRoot.localRotation = Quaternion.identity;
        modelRoot.localScale = Vector3.one;

        var suitBlue = new Color(0.035f, 0.18f, 0.7f);
        var suitDark = new Color(0.012f, 0.015f, 0.022f);
        var suitShadow = new Color(0.006f, 0.008f, 0.012f);
        var skin = new Color(0.92f, 0.72f, 0.52f);
        var helmet = new Color(0.02f, 0.023f, 0.028f);
        var bootBlack = new Color(0.01f, 0.012f, 0.016f);
        var bindingRed = new Color(0.78f, 0.04f, 0.035f);
        var aluminium = new Color(0.72f, 0.78f, 0.78f);
        var wheelBlack = new Color(0.008f, 0.008f, 0.01f);
        var poleBlack = new Color(0f, 0.001f, 0.002f);
        var poleHighlight = new Color(0.92f, 0.95f, 0.86f);

        CreateClassicRollerSki(modelRoot, "Left", -0.22f, aluminium, wheelBlack, bootBlack, bindingRed);
        CreateClassicRollerSki(modelRoot, "Right", 0.22f, aluminium, wheelBlack, bootBlack, bindingRed);

        AddBodyPart(modelRoot, "Readable Hips", PrimitiveType.Capsule, new Vector3(0f, 0.94f, 0.12f), new Vector3(HipWidth, 0.17f, 0.22f), suitDark, new Vector3(-12f, 0f, 90f));
        AddBodyPart(modelRoot, "Left Hip Plane", PrimitiveType.Sphere, new Vector3(-0.16f, 0.94f, 0.16f), new Vector3(0.18f, 0.13f, 0.13f), suitDark, Vector3.zero);
        AddBodyPart(modelRoot, "Right Hip Plane", PrimitiveType.Sphere, new Vector3(0.16f, 0.94f, 0.16f), new Vector3(0.18f, 0.13f, 0.13f), suitDark, Vector3.zero);
        AddBodyPart(modelRoot, "Shorts Separation", PrimitiveType.Cube, new Vector3(0f, 1.04f, 0.03f), new Vector3(0.35f, 0.035f, 0.17f), suitShadow, new Vector3(-8f, 0f, 0f));

        CreateLeg(modelRoot, "Left", -0.145f, suitDark, bootBlack, bindingRed);
        CreateLeg(modelRoot, "Right", 0.145f, suitDark, bootBlack, bindingRed);

        var torsoPivot = CreateChild(modelRoot, "Forward Lean Body Pivot", new Vector3(0f, 1.08f, 0.08f));
        torsoPivot.localRotation = Quaternion.Euler(9f, 0f, 0f);
        AddBodyPart(torsoPivot, "Athletic Torso", PrimitiveType.Capsule, new Vector3(0f, 0.29f, -0.04f), new Vector3(0.3f, 0.63f, 0.18f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Visible Shoulder Line", PrimitiveType.Capsule, new Vector3(0f, 0.57f, -0.03f), new Vector3(ShoulderWidth, 0.08f, 0.13f), suitBlue, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Upper Back Plane", PrimitiveType.Capsule, new Vector3(0f, 0.48f, 0.075f), new Vector3(0.62f, 0.16f, 0.08f), suitShadow, new Vector3(4f, 0f, 90f));
        AddBodyPart(torsoPivot, "Left Shoulder Cap", PrimitiveType.Sphere, new Vector3(-0.35f, 0.54f, -0.035f), new Vector3(0.13f, 0.1f, 0.11f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Right Shoulder Cap", PrimitiveType.Sphere, new Vector3(0.35f, 0.54f, -0.035f), new Vector3(0.13f, 0.1f, 0.11f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Narrow Waist", PrimitiveType.Capsule, new Vector3(0f, 0.08f, 0.0f), new Vector3(WaistWidth, 0.14f, 0.13f), suitDark, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Visible Neck", PrimitiveType.Capsule, new Vector3(0f, 0.69f, -0.13f), new Vector3(0.075f, 0.12f, 0.075f), skin, Vector3.zero);

        var headPivot = CreateChild(torsoPivot, "Neutral Head Looking Forward", new Vector3(0f, 0.79f, -0.18f));
        AddBodyPart(headPivot, "Compact Head", PrimitiveType.Sphere, Vector3.zero, new Vector3(HeadDiameter, 0.18f, HeadDiameter), skin, Vector3.zero);
        AddBodyPart(headPivot, "Low Poly Helmet", PrimitiveType.Sphere, new Vector3(0f, 0.095f, 0f), new Vector3(0.21f, 0.115f, 0.21f), helmet, Vector3.zero);

        CreateArmAndPole(modelRoot, "Left", -1f, suitBlue, bootBlack, poleBlack, poleHighlight);
        CreateArmAndPole(modelRoot, "Right", 1f, suitBlue, bootBlack, poleBlack, poleHighlight);

        return modelRoot;
    }

    public static Transform CreateGameplayModel(Transform parent, RollerSkierAnimator animator, string modelName = DefaultModelName)
    {
        Transform modelRoot = CreateModel(parent, modelName);
        if (animator != null)
        {
            animator.hips = FindChildRecursive(modelRoot, "Readable Hips");
            animator.torso = FindChildRecursive(modelRoot, "Forward Lean Body Pivot");
            animator.head = FindChildRecursive(modelRoot, "Neutral Head Looking Forward");
            animator.leftArm = FindChildRecursive(modelRoot, "Left Pole Arm");
            animator.rightArm = FindChildRecursive(modelRoot, "Right Pole Arm");
            animator.leftHand = FindChildRecursive(modelRoot, "Left Gloved Hand");
            animator.rightHand = FindChildRecursive(modelRoot, "Right Gloved Hand");
            animator.leftPole = FindChildRecursive(modelRoot, "Left Ski Pole");
            animator.rightPole = FindChildRecursive(modelRoot, "Right Ski Pole");
            animator.leftThigh = FindChildRecursive(modelRoot, "Left Thigh");
            animator.rightThigh = FindChildRecursive(modelRoot, "Right Thigh");
            animator.leftShin = FindChildRecursive(modelRoot, "Left Calf");
            animator.rightShin = FindChildRecursive(modelRoot, "Right Calf");
            animator.leftFoot = FindChildRecursive(modelRoot, "Left Roller Ski Boot");
            animator.rightFoot = FindChildRecursive(modelRoot, "Right Roller Ski Boot");
            animator.leftSki = FindChildRecursive(modelRoot, "Left Classic Roller Ski");
            animator.rightSki = FindChildRecursive(modelRoot, "Right Classic Roller Ski");
        }

        if (parent != null && parent.Find(GameplayModelAppliedMarkerName) == null)
        {
            CreateChild(parent, GameplayModelAppliedMarkerName, Vector3.zero);
        }

        return modelRoot;
    }

    private static void CreateLeg(Transform parent, string sideName, float x, Color suitColor, Color bootColor, Color bindingColor)
    {
        var side = sideName == "Left" ? -1f : 1f;
        AddBodyPart(parent, sideName + " Thigh", PrimitiveType.Capsule, new Vector3(x, 0.67f, 0.16f), new Vector3(0.092f, ThighLength, 0.092f), suitColor, new Vector3(-28f, 0f, 3f * side));
        AddBodyPart(parent, sideName + " Knee", PrimitiveType.Sphere, new Vector3(x - 0.01f * side, 0.48f, 0.18f), new Vector3(0.078f, 0.072f, 0.078f), suitColor, Vector3.zero);
        AddBodyPart(parent, sideName + " Calf", PrimitiveType.Capsule, new Vector3(x - 0.02f * side, 0.3f, 0.075f), new Vector3(0.068f, CalfLength, 0.068f), suitColor, new Vector3(15f, 0f, -2f * side));
        AddBodyPart(parent, sideName + " Roller Ski Boot", PrimitiveType.Cube, new Vector3(x - 0.075f * side, 0.155f, 0.08f), new Vector3(0.11f, 0.095f, 0.34f), bootColor, Vector3.zero);
        AddBodyPart(parent, sideName + " Boot Cuff", PrimitiveType.Capsule, new Vector3(x - 0.075f * side, 0.27f, 0.045f), new Vector3(0.105f, 0.17f, 0.095f), bootColor, new Vector3(-8f, 0f, 90f));
        AddBodyPart(parent, sideName + " Classic Binding", PrimitiveType.Cube, new Vector3(x - 0.075f * side, 0.21f, 0.15f), new Vector3(0.11f, 0.055f, 0.07f), bindingColor, Vector3.zero);
    }

    private static void CreateClassicRollerSki(Transform parent, string sideName, float x, Color aluminiumColor, Color wheelColor, Color bootColor, Color bindingColor)
    {
        var ski = CreateChild(parent, sideName + " Classic Roller Ski", new Vector3(x, 0f, 0.16f));
        AddBodyPart(ski, "Slim Aluminium Frame", PrimitiveType.Cube, new Vector3(0f, 0.105f, 0.25f), new Vector3(ClassicRollerSkiWidth, 0.022f, ClassicRollerSkiLength), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, "Foot Platform", PrimitiveType.Cube, new Vector3(0f, 0.14f, 0f), new Vector3(0.075f, 0.025f, 0.34f), aluminiumColor, Vector3.zero);
        AddBodyPart(ski, sideName + " Front Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0.82f), new Vector3(WheelDiameter, 0.095f, WheelDiameter), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, sideName + " Rear Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, -0.32f), new Vector3(WheelDiameter, 0.095f, WheelDiameter), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, sideName + " Front Wheel Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0.82f), new Vector3(0.085f, 0.105f, 0.085f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, sideName + " Rear Wheel Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, -0.32f), new Vector3(0.085f, 0.105f, 0.085f), aluminiumColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Binding Rail", PrimitiveType.Cube, new Vector3(0f, 0.17f, 0.04f), new Vector3(0.09f, 0.035f, 0.32f), bootColor, Vector3.zero);
        AddBodyPart(ski, "Toe Binding Block", PrimitiveType.Cube, new Vector3(0f, 0.205f, 0.15f), new Vector3(0.11f, 0.055f, 0.07f), bindingColor, Vector3.zero);
    }

    private static void CreateArmAndPole(Transform modelRoot, string sideName, float side, Color suitColor, Color gloveColor, Color poleColor, Color highlightColor)
    {
        var arm = CreateChild(modelRoot, sideName + " Pole Arm", new Vector3(0.33f * side, 1.48f, -0.04f));
        arm.localRotation = Quaternion.Euler(-6f, 0f, 1.5f * -side);
        AddBodyPart(arm, sideName + " Upper Arm", PrimitiveType.Capsule, new Vector3(-0.01f * side, -0.24f, 0.06f), new Vector3(0.075f, 0.35f, 0.075f), suitColor, new Vector3(14f, 0f, 1f * side));
        AddBodyPart(arm, sideName + " Forearm", PrimitiveType.Capsule, new Vector3(0.02f * side, -0.57f, 0.23f), new Vector3(0.064f, 0.43f, 0.064f), suitColor, new Vector3(30f, 0f, -1f * side));
        var hand = AddBodyPart(arm, sideName + " Gloved Hand", PrimitiveType.Sphere, new Vector3(0.035f * side, -0.82f, 0.41f), new Vector3(0.075f, 0.068f, 0.075f), gloveColor, Vector3.zero);
        AddBodyPart(hand, sideName + " Pole Grip Attachment", PrimitiveType.Capsule, new Vector3(0.035f * side, -0.005f, 0.015f), new Vector3(0.095f, 0.07f, 0.095f), gloveColor, new Vector3(90f, 0f, 0f));

        var pole = CreateChild(modelRoot, sideName + " Ski Pole", new Vector3(PoleAttachmentLateralOffset * side, 1.02f, PoleAttachmentForwardOffset));
        AddBodyPart(pole, sideName + " Pole Grip", PrimitiveType.Capsule, new Vector3(0.02f * side, -0.03f, 0.02f), new Vector3(0.06f, 0.15f, 0.06f), gloveColor, new Vector3(12f, 0f, 0f));
        AddBodyPart(pole, sideName + " Pole Strap", PrimitiveType.Capsule, new Vector3(0.09f * side, -0.16f, 0.04f), new Vector3(0.03f, 0.25f, 0.03f), gloveColor, new Vector3(30f, 0f, 14f * side));
        AddBodyPart(pole, sideName + " Pole Shaft", PrimitiveType.Cylinder, new Vector3(0.13f * side, -0.73f, 0.31f), new Vector3(PoleShaftRadius, 1.44f, PoleShaftRadius), poleColor, new Vector3(25f, 0f, -2f * side));
        AddBodyPart(pole, sideName + " Pole Basket", PrimitiveType.Cylinder, new Vector3(0.31f * side, -1.42f, 0.75f), new Vector3(0.095f, 0.015f, 0.095f), poleColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(pole, sideName + " Pole Tip", PrimitiveType.Sphere, new Vector3(0.34f * side, -1.52f, 0.82f), new Vector3(0.045f, 0.045f, 0.045f), poleColor, Vector3.zero);
        AddBodyPart(pole, sideName + " Pole Pressure Highlight", PrimitiveType.Cylinder, new Vector3(0.22f * side, -1.02f, 0.5f), new Vector3(0.035f, 0.42f, 0.035f), highlightColor, new Vector3(25f, 0f, -2f * side));
    }

    private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        var child = new GameObject(name).transform;
        if (parent != null)
        {
            child.SetParent(parent, false);
        }

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

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
