using UnityEngine;

public class SkierPresenceRuntimeUpdater : MonoBehaviour
{
    public const float AdditionalScreenPresenceScale = 1.25f;
    private const string VisualRootName = "Roller Skier Visual";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRuntimeUpdater()
    {
        if (Object.FindFirstObjectByType<SkierPresenceRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Skier Presence Runtime Updater");
        updater.AddComponent<SkierPresenceRuntimeUpdater>();
    }

    private void Start()
    {
        ApplyAdditionalScreenPresence();
        Destroy(gameObject);
    }

    public static bool ApplyAdditionalScreenPresence()
    {
        var visualRoot = GameObject.Find(VisualRootName);

        if (visualRoot == null)
        {
            return false;
        }

        visualRoot.transform.localScale *= AdditionalScreenPresenceScale;
        return true;
    }
}
