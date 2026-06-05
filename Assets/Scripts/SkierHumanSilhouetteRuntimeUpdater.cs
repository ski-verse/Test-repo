using UnityEngine;

[DisallowMultipleComponent]
public class SkierHumanSilhouetteRuntimeUpdater : MonoBehaviour
{
    public const string HumanSilhouetteAppliedMarkerName = "Skier Human Silhouette Applied";
    public const float ReducedBlueTorsoWidth = 0.2f;
    public const float ReducedBlueTorsoHeight = 0.46f;
    public const float VisibleBackPanelWidth = 0.58f;
    public const float VisibleShortsPanelWidth = 0.42f;
    public const float VisibleGluteAccentWidth = 0.2f;
    public const float VisibleGripContrastRadius = 0.083f;
    public const float VisiblePoleOutsideOffset = 0.2f;
    public const float GameplayReadablePoleRadius = 0.042f;
    public const float NaturalUpperArmRadius = 0.086f;
    public const float NaturalForearmRadius = 0.067f;

    private const string VisualRootName = "Roller Skier Visual";

    private bool applied;
    private int attempts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<SkierHumanSilhouetteRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Skier Human Silhouette Runtime Updater");
        updater.AddComponent<SkierHumanSilhouetteRuntimeUpdater>();
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
        applied = ApplyHumanSilhouettePass();
        attempts++;
        if (!applied && (attempts == 1 || attempts % 60 == 0))
        {
            Debug.Log("[Ski-Verse] SkierHumanSilhouetteRuntimeUpdater waiting for Skier Model 2.0 visual.");
        }
    }

    public static bool ApplyHumanSilhouettePass()
    {
        var animator = Object.FindFirstObjectByType<RollerSkierAnimator>();
        if (animator == null)
        {
            return false;
        }

        var visualRoot = animator.transform.Find(VisualRootName);
        if (visualRoot == null || visualRoot.Find(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName) == null)
        {
            return false;
        }

        if (visualRoot.Find(HumanSilhouetteAppliedMarkerName) != null)
        {
            return true;
        }

        var suitDark = new Color(0.014f, 0.017f, 0.026f);
        var suitBlue = new Color(0.035f, 0.21f, 0.74f);
        var suitBackBlue = new Color(0.018f, 0.09f, 0.34f);
        var suitSeam = new Color(0.004f, 0.006f, 0.01f);
        var gloveBlack = new Color(0.006f, 0.007f, 0.009f);
        var highlight = new Color(0.92f, 0.95f, 0.88f);

        RecolorAndResize(visualRoot, "Tight Suit Endurance Torso", suitDark, new Vector3(ReducedBlueTorsoWidth, ReducedBlueTorsoHeight, 0.14f));
        RecolorAndResize(visualRoot, "Broad V Shape Chest", suitBlue, new Vector3(0.6f, 0.17f, 0.055f));
        RecolorAndResize(visualRoot, "Broad Relaxed Shoulder Line", suitBlue, new Vector3(0.76f, 0.085f, 0.14f));
        RecolorAndResize(visualRoot, "Left Defined Shoulder Cap", suitBlue, new Vector3(0.145f, 0.105f, 0.122f));
        RecolorAndResize(visualRoot, "Right Defined Shoulder Cap", suitBlue, new Vector3(0.145f, 0.105f, 0.122f));
        RecolorAndResize(visualRoot, "Relaxed Upper Arm", suitBlue, new Vector3(NaturalUpperArmRadius, 0.365f, NaturalUpperArmRadius));
        RecolorAndResize(visualRoot, "Long Close Forearm", suitBlue, new Vector3(NaturalForearmRadius, 0.445f, NaturalForearmRadius));
        RecolorAndResize(visualRoot, "Hand On Pole Grip", gloveBlack, new Vector3(0.078f, 0.068f, 0.078f));
        RecolorAndResize(visualRoot, "Glove Wrapped Around Grip", gloveBlack, new Vector3(VisibleGripContrastRadius, 0.06f, VisibleGripContrastRadius));

        var torsoParent = animator.torso != null ? animator.torso : visualRoot;
        AddBodyPart(torsoParent, "Human Dark Back Panel", PrimitiveType.Cube, new Vector3(0f, 0.43f, 0.11f), new Vector3(VisibleBackPanelWidth, 0.46f, 0.04f), suitBackBlue, new Vector3(-4f, 0f, 0f));
        AddBodyPart(torsoParent, "Human Central Spine Seam", PrimitiveType.Cube, new Vector3(0f, 0.43f, 0.137f), new Vector3(0.034f, 0.45f, 0.045f), suitSeam, new Vector3(-4f, 0f, 0f));
        AddBodyPart(torsoParent, "Human Left Scapula Shadow", PrimitiveType.Cube, new Vector3(-0.17f, 0.54f, 0.145f), new Vector3(0.2f, 0.055f, 0.05f), suitSeam, new Vector3(-5f, 0f, -18f));
        AddBodyPart(torsoParent, "Human Right Scapula Shadow", PrimitiveType.Cube, new Vector3(0.17f, 0.54f, 0.145f), new Vector3(0.2f, 0.055f, 0.05f), suitSeam, new Vector3(-5f, 0f, 18f));
        AddBodyPart(torsoParent, "Human Lat Shadow Left", PrimitiveType.Capsule, new Vector3(-0.235f, 0.35f, 0.115f), new Vector3(0.11f, 0.31f, 0.055f), suitSeam, new Vector3(-5f, 0f, 16f));
        AddBodyPart(torsoParent, "Human Lat Shadow Right", PrimitiveType.Capsule, new Vector3(0.235f, 0.35f, 0.115f), new Vector3(0.11f, 0.31f, 0.055f), suitSeam, new Vector3(-5f, 0f, -16f));

        var hipParent = animator.hips != null ? animator.hips : visualRoot;
        AddBodyPart(hipParent, "Human Black Shorts Block", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.105f), new Vector3(VisibleShortsPanelWidth, 0.22f, 0.12f), suitDark, new Vector3(4f, 0f, -90f));
        AddBodyPart(hipParent, "Human Left Glute Accent", PrimitiveType.Sphere, new Vector3(-0.12f, -0.04f, 0.15f), new Vector3(VisibleGluteAccentWidth, 0.13f, 0.12f), suitDark, Vector3.zero);
        AddBodyPart(hipParent, "Human Right Glute Accent", PrimitiveType.Sphere, new Vector3(0.12f, -0.04f, 0.15f), new Vector3(VisibleGluteAccentWidth, 0.13f, 0.12f), suitDark, Vector3.zero);
        AddBodyPart(hipParent, "Human Shorts Leg Split", PrimitiveType.Cube, new Vector3(0f, -0.12f, 0.14f), new Vector3(0.035f, 0.2f, 0.06f), suitSeam, new Vector3(4f, 0f, -90f));

        AddGripReadability(animator.leftHand, animator.leftPole, -1f, gloveBlack, highlight);
        AddGripReadability(animator.rightHand, animator.rightPole, 1f, gloveBlack, highlight);
        MovePoleOutside(animator.leftPole, -1f, gloveBlack, highlight);
        MovePoleOutside(animator.rightPole, 1f, gloveBlack, highlight);

        new GameObject(HumanSilhouetteAppliedMarkerName).transform.SetParent(visualRoot, false);
        animator.ResetBasePose();
        Debug.Log("[Ski-Verse] Human silhouette pass applied: reduced blue mannequin mass, added animated back/shorts panels, clearer hands and poles.");
        return true;
    }

    private static void RecolorAndResize(Transform root, string partName, Color color, Vector3 localScale)
    {
        var part = FindDescendant(root, partName);
        if (part == null)
        {
            return;
        }

        part.localScale = localScale;
        var renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private static void AddGripReadability(Transform hand, Transform pole, float side, Color gloveColor, Color highlight)
    {
        if (hand == null)
        {
            return;
        }

        AddBodyPart(hand, "Visible Glove Grip Wrap", PrimitiveType.Capsule, new Vector3(0.012f * side, 0f, 0.012f), new Vector3(VisibleGripContrastRadius, 0.06f, VisibleGripContrastRadius), gloveColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(hand, "Pole Contact Highlight", PrimitiveType.Cylinder, new Vector3(0.014f * side, 0f, 0.015f), new Vector3(0.074f, 0.016f, 0.074f), highlight, new Vector3(90f, 0f, 0f));

        if (pole != null && pole.parent != hand)
        {
            pole.SetParent(hand, false);
            pole.localPosition = Vector3.zero;
            pole.localRotation = Quaternion.identity;
        }
    }

    private static void MovePoleOutside(Transform pole, float side, Color poleColor, Color highlightColor)
    {
        if (pole == null)
        {
            return;
        }

        var shaft = FindDescendant(pole, "Dark Visible Pole Shaft");
        if (shaft != null)
        {
            shaft.localPosition = new Vector3(VisiblePoleOutsideOffset * side, -0.72f, 0.34f);
            shaft.localScale = new Vector3(GameplayReadablePoleRadius, ProperRollerSkierRuntimeUpdater.VisiblePoleShaftLength, GameplayReadablePoleRadius);
            SetColor(shaft, poleColor);
        }

        var upper = FindDescendant(pole, "Upper Pole Motion Marker");
        if (upper != null)
        {
            upper.localPosition = new Vector3(VisiblePoleOutsideOffset * side, -0.32f, 0.16f);
            upper.localScale = new Vector3(GameplayReadablePoleRadius * 1.2f, 0.052f, GameplayReadablePoleRadius * 1.2f);
            SetColor(upper, highlightColor);
        }

        var lower = FindDescendant(pole, "Lower Pole Motion Marker");
        if (lower != null)
        {
            lower.localPosition = new Vector3(VisiblePoleOutsideOffset * side, -1.06f, 0.58f);
            lower.localScale = new Vector3(GameplayReadablePoleRadius * 1.2f, 0.052f, GameplayReadablePoleRadius * 1.2f);
            SetColor(lower, highlightColor);
        }
    }

    private static void SetColor(Transform part, Color color)
    {
        var renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private static Transform FindDescendant(Transform root, string partName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == partName)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var match = FindDescendant(root.GetChild(i), partName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
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
