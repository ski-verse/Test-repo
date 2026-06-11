using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Ski-Verse/Skier Visual Settings")]
public class SkierVisualSettings : MonoBehaviour
{
    public const string RuntimeSettingsName = "Skier Visual Settings";

    [Header("Animation Test")]
    [Tooltip("Keeps the imported Adventure Character Animator enabled so imported double-poling clips such as Armature|ArmatureAction can drive the visible skier.")]
    public bool useImportedDoublePolingAnimationTest;

    public static SkierVisualSettings GetOrCreateRuntimeSettings()
    {
        var existing = FindActiveSettings();
        if (existing != null)
        {
            return existing;
        }

        var environmentSettings = NordicEnvironmentSettings.FindActiveSettings();
        if (environmentSettings != null)
        {
            return environmentSettings.gameObject.AddComponent<SkierVisualSettings>();
        }

        var settingsObject = new GameObject(RuntimeSettingsName);
        return settingsObject.AddComponent<SkierVisualSettings>();
    }

    public static SkierVisualSettings FindActiveSettings()
    {
        return Object.FindFirstObjectByType<SkierVisualSettings>();
    }
}
