using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class StarterPackEnvironmentAssets
{
    public static readonly string[] PinePrefabPaths =
    {
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Pine 1.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Pine 2.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Pine 3.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Pine 4.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Pine 5.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Pine 6.prefab"
    };

    public static readonly string[] TreePrefabPaths =
    {
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 1.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 2.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 3.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 4.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 5.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 6.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 7.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Trees/Tree 8.prefab"
    };

    public static readonly string[] RockPrefabPaths =
    {
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 1.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 2.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 3.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 4.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 5.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 6.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 7.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 8.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 9.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 10.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 11.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock 12.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock Pile 1.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock Pile 2.prefab",
        "Assets/Low Poly Environment Starter Kit/Prefabs/Standard/Rocks/Rock Pile 3.prefab"
    };

    public static bool TryCreatePine(Transform parent, Vector3 position, float scale, float seed, out GameObject instance)
    {
        return TryInstantiatePrefab(PinePrefabPaths, parent, "Starter Pack Pine", position, scale, seed, out instance);
    }

    public static bool TryCreateMixedTree(Transform parent, Vector3 position, float scale, float seed, out GameObject instance)
    {
        var paths = seed % 2f < 1f ? PinePrefabPaths : TreePrefabPaths;
        return TryInstantiatePrefab(paths, parent, "Starter Pack Tree", position, scale, seed, out instance);
    }

    public static bool TryCreateRock(Transform parent, Vector3 position, float scale, float seed, out GameObject instance)
    {
        return TryInstantiatePrefab(RockPrefabPaths, parent, "Starter Pack Rock", position, scale, seed, out instance);
    }

    public static string SelectPath(string[] paths, float seed)
    {
        if (paths == null || paths.Length == 0)
        {
            return string.Empty;
        }

        var index = Mathf.Abs(Mathf.FloorToInt(seed * 997f)) % paths.Length;
        return paths[index];
    }

    private static bool TryInstantiatePrefab(string[] prefabPaths, Transform parent, string name, Vector3 position, float scale, float seed, out GameObject instance)
    {
        instance = null;
        var prefabPath = SelectPath(prefabPaths, seed);

#if UNITY_EDITOR
        if (string.IsNullOrEmpty(prefabPath))
        {
            return false;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return false;
        }

        instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        if (instance == null)
        {
            return false;
        }

        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, Mathf.Repeat(seed * 137.5f, 360f), 0f);
        instance.transform.localScale = Vector3.one * scale;
        DisableColliders(instance);
        return true;
#else
        return false;
#endif
    }

    private static void DisableColliders(GameObject root)
    {
        var colliders = root.GetComponentsInChildren<Collider>();
        for (var index = 0; index < colliders.Length; index++)
        {
            colliders[index].enabled = false;
        }
    }
}
