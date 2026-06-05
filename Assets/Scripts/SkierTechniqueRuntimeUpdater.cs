using UnityEngine;

[DisallowMultipleComponent]
public class SkierTechniqueRuntimeUpdater : MonoBehaviour
{
    private bool configured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallTechniqueUpdater()
    {
        if (Object.FindFirstObjectByType<SkierTechniqueRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Skier Technique Runtime Updater");
        updater.AddComponent<SkierTechniqueRuntimeUpdater>();
    }

    private void Start()
    {
        configured = ConfigureIfAvailable();
    }

    private void Update()
    {
        if (!configured)
        {
            configured = ConfigureIfAvailable();
        }
    }

    public static bool ConfigureAnimator(RollerSkierAnimator animator)
    {
        if (animator == null)
        {
            return false;
        }

        var root = animator.transform;
        animator.hips = animator.hips != null ? animator.hips : FindDescendant(root, "Forward Hinged Athletic Hips", "Hips");
        animator.leftThigh = animator.leftThigh != null ? animator.leftThigh : FindDescendant(root, "Left Long Athletic Thigh", "Left Thigh");
        animator.rightThigh = animator.rightThigh != null ? animator.rightThigh : FindDescendant(root, "Right Long Athletic Thigh", "Right Thigh");
        animator.leftShin = animator.leftShin != null ? animator.leftShin : FindDescendant(root, "Left Long Lower Leg", "Left Shin");
        animator.rightShin = animator.rightShin != null ? animator.rightShin : FindDescendant(root, "Right Long Lower Leg", "Right Shin");
        animator.leftFoot = animator.leftFoot != null ? animator.leftFoot : FindDescendant(root, "Left Boot", "Left Foot");
        animator.rightFoot = animator.rightFoot != null ? animator.rightFoot : FindDescendant(root, "Right Boot", "Right Foot");

        animator.leftArm = animator.leftArm != null ? animator.leftArm : FindDescendant(root, "Left Connected Double-Poling Arm", "Left Double-Poling Arm");
        animator.rightArm = animator.rightArm != null ? animator.rightArm : FindDescendant(root, "Right Connected Double-Poling Arm", "Right Double-Poling Arm");
        animator.leftHand = animator.leftHand != null ? animator.leftHand : FindDescendant(animator.leftArm, "Hand On Pole Grip", "Hand");
        animator.rightHand = animator.rightHand != null ? animator.rightHand : FindDescendant(animator.rightArm, "Hand On Pole Grip", "Hand");
        animator.leftPole = animator.leftPole != null ? animator.leftPole : FindDescendant(root, "Left Ski Pole", "Left Carbon Pole");
        animator.rightPole = animator.rightPole != null ? animator.rightPole : FindDescendant(root, "Right Ski Pole", "Right Carbon Pole");

        AttachPoleToHand(animator.leftPole, animator.leftHand);
        AttachPoleToHand(animator.rightPole, animator.rightHand);

        var rigReady = animator.leftHand != null && animator.rightHand != null && animator.leftPole != null && animator.rightPole != null;
        if (rigReady)
        {
            animator.ResetBasePose();
        }

        return rigReady;
    }

    private static bool ConfigureIfAvailable()
    {
        var animator = Object.FindFirstObjectByType<RollerSkierAnimator>();
        return ConfigureAnimator(animator);
    }

    private static void AttachPoleToHand(Transform pole, Transform hand)
    {
        if (pole == null || hand == null)
        {
            return;
        }

        if (pole.parent != hand)
        {
            pole.SetParent(hand, false);
        }

        pole.localPosition = Vector3.zero;
        pole.localRotation = Quaternion.identity;
    }

    private static Transform FindDescendant(Transform root, params string[] names)
    {
        if (root == null || names == null)
        {
            return null;
        }

        for (var i = 0; i < names.Length; i++)
        {
            if (root.name == names[i])
            {
                return root;
            }
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var match = FindDescendant(root.GetChild(i), names);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
