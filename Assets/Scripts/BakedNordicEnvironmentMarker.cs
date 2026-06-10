using UnityEngine;

[DisallowMultipleComponent]
public class BakedNordicEnvironmentMarker : MonoBehaviour
{
    public const string BakedRootName = "Baked Nordic Environment";

    public static bool HasBakedEnvironment()
    {
        return GameObject.Find(BakedRootName) != null;
    }
}
