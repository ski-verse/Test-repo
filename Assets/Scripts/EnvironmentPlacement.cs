public static class EnvironmentPlacement
{
    public const float RoadHalfWidth = 4f;
    public const float OpenTerrainMargin = 15f;

    public const float NearTreeOffset = 23.5f;
    public const float MidTreeOffset = 32f;
    public const float FarTreeOffset = 44f;
    public const float MaxTreeRadius = 1.9f;

    public const float TurnSignOffset = 20.5f;

    public const float NearHillOffset = 58f;
    public const float FarHillOffset = 72f;
    public const float NearHillHalfWidth = 17f;
    public const float FarHillHalfWidth = 20f;

    public const float MountainFirstDistance = 1200f;
    public const float MountainSpacing = 950f;
    public const float NearMountainOffset = 520f;
    public const float FarMountainOffset = 640f;
    public const float NearMountainHalfWidth = 138f;
    public const float FarMountainHalfWidth = 177f;

    public static bool HasOpenRoadMargin(float centerOffset, float halfWidth)
    {
        return centerOffset - halfWidth >= RoadHalfWidth + OpenTerrainMargin;
    }
}
