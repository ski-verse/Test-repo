using NUnit.Framework;
using UnityEngine;

public class SkierPresenceRuntimeUpdaterTests
{
    [Test]
    public void ApplyAdditionalScreenPresence_ScalesSkierVisualAnotherTwentyFivePercent()
    {
        var visualRoot = new GameObject("Roller Skier Visual");
        visualRoot.transform.localScale = Vector3.one * 1.18f;

        var applied = SkierPresenceRuntimeUpdater.ApplyAdditionalScreenPresence();

        Assert.IsTrue(applied);
        Assert.AreEqual(1.475f, visualRoot.transform.localScale.x, 0.001f);
        Assert.AreEqual(1.475f, visualRoot.transform.localScale.y, 0.001f);
        Assert.AreEqual(1.475f, visualRoot.transform.localScale.z, 0.001f);

        Object.DestroyImmediate(visualRoot);
    }

    [Test]
    public void CameraDefaults_DoNotMoveFurtherAwayWhenSkierPresenceIncreases()
    {
        Assert.AreEqual(new Vector3(0f, 2.75f, -4.8f), FollowCamera.FocusedPlayerOffset);
        Assert.AreEqual(14f, new GameObject("Follow Camera").AddComponent<FollowCamera>().baseLookAheadDistance, 0.001f);

        Object.DestroyImmediate(GameObject.Find("Follow Camera"));
    }
}
