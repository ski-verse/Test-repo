using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Ski-Verse/Skier Visual Settings")]
public class SkierVisualSettings : MonoBehaviour
{
    public const string RuntimeSettingsName = "Skier Visual Settings";

    [Header("Animation Test")]
    [Tooltip("Uses the imported skier_doublepoling_03 FBX directly as the visible gameplay skier for testing the authored double-poling animation.")]
    public bool useImportedDoublePolingAnimationTest;
    [Tooltip("Animator Controller assigned to the runtime skier_doublepoling_03 Animator when imported double-poling animation test mode is enabled.")]
    public RuntimeAnimatorController importedDoublePolingController;

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
