using NUnit.Framework;
using UnityEngine;

public class MountainRangeSceneUpdaterTests
{
    [Test]
    public void BuildMountainRanges_CreatesOnlyDistantMountainChains()
    {
        var root = new GameObject("Nordic Mountain Ranges");

        try
        {
            MountainRangeSceneUpdater.BuildMountainRanges(root.transform);

            Assert.Greater(root.transform.childCount, 0);

            for (var index = 0; index < root.transform.childCount; index++)
            {
                var child = root.transform.GetChild(index);
                StringAssert.Contains("Mountain Chain", child.name);
                Assert.IsFalse(child.name.Contains("Low Poly Mountain"));
                Assert.IsNotNull(child.GetComponent<MeshFilter>());
                Assert.IsNotNull(child.GetComponent<MeshRenderer>());
                Assert.LessOrEqual(child.localScale.z, MountainRangeSceneUpdater.FarMountainChainLength);
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void ContainsGeneratedMountainChains_DetectsAlreadyCleanMountainSystem()
    {
        var root = new GameObject("Nordic Mountain Ranges");
        var chain = new GameObject("Left Near Mountain Chain");

        try
        {
            chain.transform.SetParent(root.transform, false);

            Assert.IsTrue(MountainRangeSceneUpdater.ContainsGeneratedMountainChains(root.transform));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void ContainsGeneratedMountainChains_IgnoresPrototypeMoundMountains()
    {
        var root = new GameObject("Nordic Mountain Ranges");
        var prototype = new GameObject("Low Poly Mountain");

        try
        {
            prototype.transform.SetParent(root.transform, false);

            Assert.IsFalse(MountainRangeSceneUpdater.ContainsGeneratedMountainChains(root.transform));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void RebuildMountainRanges_ReplacesOldGeneratedChainsWithDistantChains()
    {
        var root = new GameObject("Nordic Mountain Ranges");
        var oldChain = new GameObject("Left Near Mountain Chain");

        try
        {
            oldChain.transform.SetParent(root.transform, false);
            oldChain.transform.localScale = new Vector3(190f, 80f, 490f);

            MountainRangeSceneUpdater.RebuildMountainRanges(root.transform, null);

            Assert.Greater(root.transform.childCount, 0);
            for (var index = 0; index < root.transform.childCount; index++)
            {
                var child = root.transform.GetChild(index);
                var footprint = child.localScale.z <= MountainRangeSceneUpdater.NearMountainChainLength
                    ? CalculateFootprintRadius(EnvironmentPlacement.NearMountainHalfWidth * 2f, MountainRangeSceneUpdater.NearMountainChainLength)
                    : CalculateFootprintRadius(EnvironmentPlacement.FarMountainHalfWidth * 2f, MountainRangeSceneUpdater.FarMountainChainLength);
                Assert.IsTrue(EnvironmentPlacement.HasLoopRoadClearance(child.position, footprint + MountainRangeSceneUpdater.MountainRoadVisualClearance));
                Assert.LessOrEqual(child.localScale.z, MountainRangeSceneUpdater.FarMountainChainLength);
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static float CalculateFootprintRadius(float width, float length)
    {
        return Mathf.Sqrt(width * width + length * length) * 0.5f;
    }
}
