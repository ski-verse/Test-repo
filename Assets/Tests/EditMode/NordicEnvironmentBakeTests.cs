using NUnit.Framework;
using UnityEngine;

public class NordicEnvironmentBakeTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (var settings in Object.FindObjectsByType<NordicEnvironmentSettings>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(settings.gameObject);
        }

        var bakedRoot = GameObject.Find(BakedNordicEnvironmentMarker.BakedRootName);
        if (bakedRoot != null)
        {
            Object.DestroyImmediate(bakedRoot);
        }
    }

    [Test]
    public void BakedMarker_DetectsBakedEnvironmentRoot()
    {
        Assert.IsFalse(BakedNordicEnvironmentMarker.HasBakedEnvironment());

        var root = new GameObject(BakedNordicEnvironmentMarker.BakedRootName);
        root.AddComponent<BakedNordicEnvironmentMarker>();

        Assert.IsTrue(BakedNordicEnvironmentMarker.HasBakedEnvironment());
    }

    [Test]
    public void CreateNordicEnvironmentDecorations_GroupsEditableEnvironmentObjectsUnderParent()
    {
        var settings = new GameObject("Settings").AddComponent<NordicEnvironmentSettings>();
        var bakedRoot = new GameObject(BakedNordicEnvironmentMarker.BakedRootName);
        bakedRoot.AddComponent<BakedNordicEnvironmentMarker>();

        SkiErgGameBootstrap.CreateNordicEnvironmentDecorations(settings, bakedRoot.transform);

        Assert.IsNotNull(bakedRoot.transform.Find("Continuous Green Roadside Ground"));
        Assert.IsNotNull(bakedRoot.transform.Find("Nordic Distant Forest Bands"));
        Assert.IsNotNull(bakedRoot.transform.Find("Nordic Starter Pack Rocks"));
        Assert.IsNotNull(bakedRoot.transform.Find("Nordic Mountain Ranges"));
        Assert.IsNotNull(bakedRoot.transform.Find("Set Back Roadside Trees"));
        Assert.IsNotNull(bakedRoot.transform.Find(NordicLandscapeRuntimeUpdater.LandscapeRootName));
        Assert.IsNotNull(bakedRoot.GetComponent<BakedNordicEnvironmentMarker>());
    }
}
