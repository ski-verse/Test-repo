using UnityEngine;

[DisallowMultipleComponent]
public class SkierHumanSilhouetteRuntimeUpdater : MonoBehaviour
{
    public const string HumanSilhouetteAppliedMarkerName = "Skier Human Silhouette Applied";
    public const float ReducedBlueTorsoWidth = 0.22f;
    public const float ReducedBlueTorsoHeight = 0.5f;
    public const float VisibleBackPanelWidth = 0.54f;
    public const float VisibleShortsPanelWidth = 0.38f;
    public const float VisibleGluteAccentWidth = 0.18f;
    public const float VisibleGripContrastRadius = 0.081f;
    public const float VisiblePoleOutsideOffset = 0.18f;

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

        var suitDark = new Color(0.018f, 0.022f, 0.032f);
        var suitBlue = new Color(0.04f, 0.24f, 0.82f);
        var suitBackBlue = new Color(0.035f, 0.16f, 0.56f);
        var suitSeam = new Color(0.004f, 0.006f, 0.01f);
        var gloveBlack = new Color(0.006f, 0.007f, 0.009f);
        var highlight = new Color(0.92f, 0.95f, 0.88f);

        RecolorAndResize(visualRoot, "Tight Suit Endurance Torso", suitDark, new Vector3(ReducedBlueTorsoWidth, ReducedBlueTorsoHeight, 0.16f));
        RecolorAndResize(visualRoot, "Broad V Shape Chest", suitBlue, new Vector3(0.58f, 0.19f, 0.06f));
        RecolorAndResize(visualRoot, "Broad Relaxed Shoulder Line", suitBlue, new Vector3(0.72f, 0.08f, 0.14f));
        RecolorAndResize(visualRoot, "Left Defined Shoulder Cap", suitBlue, new Vector3(0.135f, 0.1f, 0.12f));
        RecolorAndResize(visualRoot, "Right Defined Shoulder Cap", suitBlue, new Vector3(0.135f, 0.1f, 0.12f));

        AddBodyPart(visualRoot, "Human Dark Back Panel", PrimitiveType.Cube, new Vector3(0f, 1.47f, 0.18f), new Vector3(VisibleBackPanelWidth, 0.42f, 0.035f), suitBackBlue, new Vector3(-8f, 0f, 0f));
        AddBodyPart(visualRoot, "Human Central Spine Seam", PrimitiveType.Cube, new Vector3(0f, 1.47f, 0.205f), new Vector3(0.035f, 0.42f, 0.04f), suitSeam, new Vector3(-8f, 0f, 0f));
        AddBodyPart(visualRoot, "Human Left Scapula Shadow", PrimitiveType.Cube, new Vector3(-0.16f, 1.58f, 0.215f), new Vector3(0.18f, 0.05f, 0.045f), suitSeam, new Vector3(-8f, 0f, -18f));
        AddBodyPart(visualRoot, "Human Right Scapula Shadow", PrimitiveType.Cube, new Vector3(0.16f, 1.58f, 0.215f), new Vector3(0.18f, 0.05f, 0.045f), suitSeam, new Vector3(-8f, 0f, 18f));

        AddBodyPart(visualRoot, "Human Black Shorts Block", PrimitiveType.Cube, new Vector3(0f, 0.99f, 0.205f), new Vector3(VisibleShortsPanelWidth, 0.26f, 0.13f), suitDark, new Vector3(-12f, 0f, 0f));
        AddBodyPart(visualRoot, "Human Left Glute Accent", PrimitiveType.Sphere, new Vector3(-0.105f, 0.94f, 0.265f), new Vector3(VisibleGluteAccentWidth, 0.13f, 0.11f), suitDark, Vector3.zero);
        AddBodyPart(visualRoot, "Human Right Glute Accent", PrimitiveType.Sphere, new Vector3(0.105f, 0.94f, 0.265f), new Vector3(VisibleGluteAccentWidth, 0.13f, 0.11f), suitDark, Vector3.zero);
        AddBodyPart(visualRoot, "Human Shorts Leg Split", PrimitiveType.Cube, new Vector3(0f, 0.83f, 0.23f), new Vector3(0.035f, 0.24f, 0.06f), suitSeam, new Vector3(-10f, 0f, 0f));

        AddGripReadability(animator.leftHand, animator.leftPole, -1f, gloveBlack, highlight);
        AddGripReadability(animator.rightHand, animator.rightPole, 1f, gloveBlack, highlight);
        MovePoleOutside(animator.leftPole, -1f);
        MovePoleOutside(animator.rightPole, 1f);

        new GameObject(HumanSilhouetteAppliedMarkerName).transform.SetParent(visualRoot, false);
        animator.ResetBasePose();
        Debug.Log("[Ski-Verse] Human silhouette pass applied: reduced blue torso, added dark shorts/back panels, clearer pole grips.");
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

        AddBodyPart(hand, "Visible Glove Grip Wrap", PrimitiveType.Capsule, new Vector3(0.012f * side, 0f, 0.012f), new Vector3(VisibleGripContrastRadius, 0.055f, VisibleGripContrastRadius), gloveColor, new Vector3(90f, 0f, 0f));
        AddBodyPart(hand, "Pole Contact Highlight", PrimitiveType.Cylinder, new Vector3(0.014f * side, 0f, 0.015f), new Vector3(0.07f, 0.015f, 0.07f), highlight, new Vector3(90f, 0f, 0f));

        if (pole != null && pole.parent != hand)
        {
            pole.SetParent(hand, false);
            pole.localPosition = Vector3.zero;
            pole.localRotation = Quaternion.identity;
        }
    }

    private static void MovePoleOutside(Transform pole, float side)
    {
        if (pole == null)
        {
            return;
        }

        var shaft = FindDescendant(pole, "Dark Visible Pole Shaft");
        if (shaft != null)
        {
            shaft.localPosition = new Vector3(VisiblePoleOutsideOffset * side, -0.72f, 0.34f);
        }

        var upper = FindDescendant(pole, "Upper Pole Motion Marker");
        if (upper != null)
        {
            upper.localPosition = new Vector3(VisiblePoleOutsideOffset * side, -0.32f, 0.16f);
        }

        var lower = FindDescendant(pole, "Lower Pole Motion Marker");
        if (lower != null)
        {
            lower.localPosition = new Vector3(VisiblePoleOutsideOffset * side, -1.06f, 0.58f);
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
