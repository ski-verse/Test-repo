using UnityEngine;

[DisallowMultipleComponent]
public class ProperRollerSkierRuntimeUpdater : MonoBehaviour
{
    private const string VisualRootName = "Roller Skier Visual";

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
        var suitBlue = new Color(0.08f, 0.34f, 0.9f);
        var suitDark = new Color(0.045f, 0.055f, 0.075f);
        var skin = new Color(0.95f, 0.78f, 0.58f);
        var helmetColor = new Color(0.08f, 0.09f, 0.11f);
        var visorColor = new Color(0.02f, 0.025f, 0.03f);
        var skiColor = new Color(0.9f, 0.94f, 0.95f);
        var wheelColor = new Color(0.025f, 0.025f, 0.03f);
        var poleColor = new Color(0.035f, 0.04f, 0.045f);

        animator.leftSki = CreateRollerSki(parent, "Left Parallel Roller Ski", -0.24f, skiColor, wheelColor, suitDark);
        animator.rightSki = CreateRollerSki(parent, "Right Parallel Roller Ski", 0.24f, skiColor, wheelColor, suitDark);

        AddBodyPart(parent, "Narrow Athletic Hips", PrimitiveType.Capsule, new Vector3(0f, 0.99f, 0.055f), new Vector3(0.36f, 0.18f, 0.24f), suitDark, new Vector3(-10f, 0f, 90f));
        AddBodyPart(parent, "Left Long Athletic Thigh", PrimitiveType.Capsule, new Vector3(-0.145f, 0.71f, 0.13f), new Vector3(0.108f, 0.43f, 0.108f), suitDark, new Vector3(-22f, 0f, 3f));
        AddBodyPart(parent, "Right Long Athletic Thigh", PrimitiveType.Capsule, new Vector3(0.145f, 0.71f, 0.13f), new Vector3(0.108f, 0.43f, 0.108f), suitDark, new Vector3(-22f, 0f, -3f));
        AddBodyPart(parent, "Left Long Lower Leg", PrimitiveType.Capsule, new Vector3(-0.17f, 0.36f, 0.055f), new Vector3(0.086f, 0.39f, 0.086f), suitDark, new Vector3(8f, 0f, -2f));
        AddBodyPart(parent, "Right Long Lower Leg", PrimitiveType.Capsule, new Vector3(0.17f, 0.36f, 0.055f), new Vector3(0.086f, 0.39f, 0.086f), suitDark, new Vector3(8f, 0f, 2f));
        AddBodyPart(parent, "Left Boot", PrimitiveType.Cube, new Vector3(-0.24f, 0.15f, 0.1f), new Vector3(0.13f, 0.12f, 0.34f), suitDark, Vector3.zero);
        AddBodyPart(parent, "Right Boot", PrimitiveType.Cube, new Vector3(0.24f, 0.15f, 0.1f), new Vector3(0.13f, 0.12f, 0.34f), suitDark, Vector3.zero);

        var torsoPivot = CreateChild(parent, "Torso Pivot", new Vector3(0f, 1.14f, 0.04f));
        animator.torso = torsoPivot;
        AddBodyPart(torsoPivot, "Athletic Forward Leaning Torso", PrimitiveType.Capsule, new Vector3(0f, 0.3f, -0.055f), new Vector3(0.3f, 0.64f, 0.225f), suitBlue, Vector3.zero);
        AddBodyPart(torsoPivot, "Chest Panel", PrimitiveType.Capsule, new Vector3(0f, 0.45f, -0.095f), new Vector3(0.35f, 0.24f, 0.08f), suitBlue, new Vector3(8f, 0f, 90f));
        AddBodyPart(torsoPivot, "Narrow Waist", PrimitiveType.Capsule, new Vector3(0f, 0.08f, 0.005f), new Vector3(0.23f, 0.15f, 0.17f), suitDark, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Relaxed Shoulder Line", PrimitiveType.Capsule, new Vector3(0f, 0.57f, -0.03f), new Vector3(0.48f, 0.08f, 0.14f), suitBlue, new Vector3(0f, 0f, 90f));
        AddBodyPart(torsoPivot, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.79f, -0.19f), new Vector3(0.205f, 0.215f, 0.205f), skin, Vector3.zero);
        AddBodyPart(torsoPivot, "Low Poly Helmet", PrimitiveType.Sphere, new Vector3(0f, 0.89f, -0.19f), new Vector3(0.24f, 0.13f, 0.24f), helmetColor, Vector3.zero);
        AddBodyPart(torsoPivot, "Helmet Visor", PrimitiveType.Cube, new Vector3(0f, 0.855f, -0.345f), new Vector3(0.2f, 0.045f, 0.06f), visorColor, Vector3.zero);

        animator.leftArm = CreateArm(parent, "Left Connected Double-Poling Arm", new Vector3(-0.255f, 1.49f, -0.055f), -1f, suitBlue, skin);
        animator.rightArm = CreateArm(parent, "Right Connected Double-Poling Arm", new Vector3(0.255f, 1.49f, -0.055f), 1f, suitBlue, skin);
        animator.leftPole = CreatePole(parent, "Left Ski Pole", new Vector3(-0.39f, 0.96f, 0.14f), -1f, poleColor);
        animator.rightPole = CreatePole(parent, "Right Ski Pole", new Vector3(0.39f, 0.96f, 0.14f), 1f, poleColor);
    }

    private static Transform CreateRollerSki(Transform parent, string name, float xPosition, Color skiColor, Color wheelColor, Color bootColor)
    {
        var ski = CreateChild(parent, name, new Vector3(xPosition, 0f, 0.13f));
        AddBodyPart(ski, "Long Roller Ski Deck", PrimitiveType.Capsule, new Vector3(0f, 0.09f, 0f), new Vector3(0.075f, 0.86f, 0.04f), skiColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(ski, "Inner Side Rail", PrimitiveType.Cube, new Vector3(-0.045f, 0.105f, 0f), new Vector3(0.016f, 0.03f, 1.9f), skiColor, Vector3.zero);
        AddBodyPart(ski, "Outer Side Rail", PrimitiveType.Cube, new Vector3(0.045f, 0.105f, 0f), new Vector3(0.016f, 0.03f, 1.9f), skiColor, Vector3.zero);
        AddBodyPart(ski, "Front Axle", PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0.86f), new Vector3(0.018f, 0.13f, 0.018f), bootColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Axle", PrimitiveType.Cylinder, new Vector3(0f, 0.08f, -0.84f), new Vector3(0.018f, 0.13f, 0.018f), bootColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Front Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0.91f), new Vector3(0.115f, 0.046f, 0.115f), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Rear Roller Wheel", PrimitiveType.Cylinder, new Vector3(0f, 0.035f, -0.89f), new Vector3(0.115f, 0.046f, 0.115f), wheelColor, new Vector3(0f, 0f, 90f));
        AddBodyPart(ski, "Binding Plate", PrimitiveType.Cube, new Vector3(0f, 0.14f, 0.08f), new Vector3(0.115f, 0.035f, 0.34f), bootColor, Vector3.zero);
        AddBodyPart(ski, "Heel Stop", PrimitiveType.Cube, new Vector3(0f, 0.17f, -0.1f), new Vector3(0.095f, 0.055f, 0.08f), bootColor, Vector3.zero);
        return ski;
    }

    private static Transform CreateArm(Transform parent, string name, Vector3 localPosition, float side, Color suitColor, Color skinColor)
    {
        var armPivot = CreateChild(parent, name, localPosition);
        AddBodyPart(armPivot, "Relaxed Upper Arm", PrimitiveType.Capsule, new Vector3(0.01f * side, -0.245f, 0.09f), new Vector3(0.072f, 0.33f, 0.072f), suitColor, new Vector3(20f, 0f, 2.5f * side));
        AddBodyPart(armPivot, "Long Forearm", PrimitiveType.Capsule, new Vector3(0.038f * side, -0.56f, 0.26f), new Vector3(0.064f, 0.405f, 0.064f), suitColor, new Vector3(34f, 0f, -3.5f * side));
        AddBodyPart(armPivot, "Hand On Pole Grip", PrimitiveType.Sphere, new Vector3(0.078f * side, -0.86f, 0.46f), new Vector3(0.088f, 0.088f, 0.088f), skinColor, Vector3.zero);
        return armPivot;
    }

    private static Transform CreatePole(Transform parent, string name, Vector3 localPosition, float side, Color poleColor)
    {
        var polePivot = CreateChild(parent, name, localPosition);
        AddBodyPart(polePivot, "Ergonomic Pole Grip", PrimitiveType.Capsule, new Vector3(0.01f * side, -0.035f, 0.02f), new Vector3(0.045f, 0.11f, 0.045f), poleColor, new Vector3(14f, 0f, 0f));
        AddBodyPart(polePivot, "Wrist Strap", PrimitiveType.Cube, new Vector3(0.052f * side, -0.11f, 0.04f), new Vector3(0.014f, 0.2f, 0.04f), poleColor, new Vector3(18f, 0f, 12f * side));
        AddBodyPart(polePivot, "Pole Shaft", PrimitiveType.Cylinder, new Vector3(0.048f * side, -0.66f, 0.38f), new Vector3(0.013f, 1.28f, 0.013f), poleColor, new Vector3(24f, 0f, -2.5f * side));
        AddBodyPart(polePivot, "Compact Pole Basket", PrimitiveType.Cylinder, new Vector3(0.17f * side, -1.28f, 0.74f), new Vector3(0.055f, 0.009f, 0.055f), poleColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(polePivot, "Pole Tip", PrimitiveType.Sphere, new Vector3(0.19f * side, -1.38f, 0.8f), new Vector3(0.032f, 0.032f, 0.032f), poleColor, Vector3.zero);
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
