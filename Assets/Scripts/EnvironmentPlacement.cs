public static class EnvironmentPlacement
{
    public const float RoadHalfWidth = 4f;
    public const float OpenTerrainMargin = 15f;

    public const float NearTreeOffset = 23.5f;
    public const float MidTreeOffset = 32f;
    public const float FarTreeOffset = 44f;
    public const float MaxTreeRadius = 1.9f;

    public const float NearForestOffset = 56f;
    public const float FarForestOffset = 78f;
    public const float MaxForestTreeRadius = 2.4f;

    public const float TurnSignOffset = 20.5f;

    public const float NearHillOffset = 58f;
    public const float FarHillOffset = 72f;
    public const float NearHillHalfWidth = 17f;
    public const float FarHillHalfWidth = 20f;

    public const float MountainFirstDistance = 520f;
    public const float MountainSpacing = 620f;
    public const float NearMountainOffset = 180f;
    public const float FarMountainOffset = 275f;
    public const float NearMountainHalfWidth = 95f;
    public const float FarMountainHalfWidth = 125f;

    public static bool HasOpenRoadMargin(float centerOffset, float halfWidth)
    {
        return centerOffset - halfWidth >= RoadHalfWidth + OpenTerrainMargin;
    }
}
