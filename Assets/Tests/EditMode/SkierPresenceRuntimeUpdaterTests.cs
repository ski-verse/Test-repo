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
    public void CameraDefaults_KeepSkierLargeWhileImprovingForwardVisibility()
    {
        var cameraObject = new GameObject("Follow Camera");
        var followCamera = cameraObject.AddComponent<FollowCamera>();

        Assert.AreEqual(new Vector3(0f, 3.25f, -4.8f), FollowCamera.FocusedPlayerOffset);
        Assert.AreEqual(22f, followCamera.baseLookAheadDistance, 0.001f);
        Assert.AreEqual(62f, followCamera.maxLookAheadDistance, 0.001f);

        Object.DestroyImmediate(cameraObject);
    }
}
