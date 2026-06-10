using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NordicEnvironmentBakeEditor
{
    private const string MenuRoot = "Tools/Ski-Verse/";

    [MenuItem(MenuRoot + "Bake Nordic Environment To Scene")]
    public static void BakeNordicEnvironmentToScene()
    {
        if (FindBakedRoot() != null)
        {
            EditorUtility.DisplayDialog(
                "Baked Nordic Environment Exists",
                "A baked Nordic environment already exists. Use Rebuild Baked Nordic Environment From Settings if you want to replace it.",
                "OK");
            return;
        }

        BakeNewEnvironment();
    }

    [MenuItem(MenuRoot + "Clear Baked Nordic Environment")]
    public static void ClearBakedNordicEnvironment()
    {
        var bakedRoot = FindBakedRoot();
        if (bakedRoot == null)
        {
            EditorUtility.DisplayDialog("No Baked Environment", "There is no Baked Nordic Environment object in the current scene.", "OK");
            return;
        }

        Undo.DestroyObjectImmediate(bakedRoot);
        MarkActiveSceneDirty();
    }

    [MenuItem(MenuRoot + "Rebuild Baked Nordic Environment From Settings")]
    public static void RebuildBakedNordicEnvironmentFromSettings()
    {
        var bakedRoot = FindBakedRoot();
        if (bakedRoot != null)
        {
            var shouldRebuild = EditorUtility.DisplayDialog(
                "Rebuild Baked Nordic Environment",
                "This will delete the existing Baked Nordic Environment and rebuild it from the current NordicEnvironmentSettings.",
                "Rebuild",
                "Cancel");

            if (!shouldRebuild)
            {
                return;
            }

            Undo.DestroyObjectImmediate(bakedRoot);
        }

        BakeNewEnvironment();
    }

    private static void BakeNewEnvironment()
    {
        var settings = NordicEnvironmentSettings.GetOrCreateRuntimeSettings();
        var bakedRoot = new GameObject(BakedNordicEnvironmentMarker.BakedRootName);
        Undo.RegisterCreatedObjectUndo(bakedRoot, "Bake Nordic Environment");
        bakedRoot.AddComponent<BakedNordicEnvironmentMarker>();

        SkiErgGameBootstrap.CreateNordicEnvironmentDecorations(settings, bakedRoot.transform);

        Selection.activeGameObject = bakedRoot;
        MarkActiveSceneDirty();
    }

    private static GameObject FindBakedRoot()
    {
        return GameObject.Find(BakedNordicEnvironmentMarker.BakedRootName);
    }

    private static void MarkActiveSceneDirty()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
    }
}
