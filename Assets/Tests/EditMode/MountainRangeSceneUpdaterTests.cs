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
}
