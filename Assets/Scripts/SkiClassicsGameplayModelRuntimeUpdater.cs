using UnityEngine;

[DisallowMultipleComponent]
public class SkiClassicsGameplayModelRuntimeUpdater : MonoBehaviour
{
    private const string VisualRootName = "Roller Skier Visual";

    private bool applied;
    private int attempts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<SkiClassicsGameplayModelRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Ski Classics Gameplay Model Runtime Updater");
        updater.AddComponent<SkiClassicsGameplayModelRuntimeUpdater>();
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
        applied = ApplyGameplayModelSwap();
        attempts++;
        if (!applied && (attempts == 1 || attempts % 60 == 0))
        {
            Debug.Log("[Ski-Verse] SkiClassicsGameplayModelRuntimeUpdater waiting for bootstrapped skier visual.");
        }
    }

    public static bool ApplyGameplayModelSwap()
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

        if (visualRoot.Find(SkiClassicsSkierModelBuilder.GameplayModelAppliedMarkerName) != null)
        {
            return true;
        }

        if (visualRoot.Find(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName) == null)
        {
            return false;
        }

        ClearChildren(visualRoot);
        visualRoot.localScale = Vector3.one * ProperRollerSkierRuntimeUpdater.Model20RuntimeVisualScale;

        SkiClassicsSkierModelBuilder.CreateGameplayModel(visualRoot, animator);
        if (visualRoot.Find(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName) == null)
        {
            new GameObject(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName).transform.SetParent(visualRoot, false);
        }

        SkierTechniqueRuntimeUpdater.ConfigureAnimator(animator);
        animator.ResetBasePose();
        animator.ApplyPose(0f);
        PoleVisibilityRuntimeUpdater.ApplyPoleVisibilityPass();

        Debug.Log("[Ski-Verse] SkiClassicsGameplayModelRuntimeUpdater applied Ski Classics gameplay skier model to Low Poly Roller Skier/Roller Skier Visual.");
        return true;
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
}
