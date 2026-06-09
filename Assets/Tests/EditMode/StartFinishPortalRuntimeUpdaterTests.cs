using NUnit.Framework;
using TMPro;
using UnityEngine;

public class StartFinishPortalRuntimeUpdaterTests
{
    [Test]
    public void CalculatePortalPosition_UsesLapLine()
    {
        var portalPosition = StartFinishPortalRuntimeUpdater.CalculatePortalPosition();
        var lapLinePosition = CoursePath.CenterPointAtDistance(0f);

        Assert.AreEqual(lapLinePosition.x, portalPosition.x, 0.001f);
        Assert.AreEqual(lapLinePosition.z, portalPosition.z, 0.001f);
    }

    [Test]
    public void PortalDimensions_SpanEntireRoadAndPlaceTowersOutsideRoad()
    {
        Assert.Greater(StartFinishPortalRuntimeUpdater.PortalSpanWidth, 8f);
        Assert.Greater(StartFinishPortalRuntimeUpdater.TowerLateralOffset, EnvironmentPlacement.RoadHalfWidth);
    }

    [Test]
    public void EnsureStartFinishPortal_CreatesLargeBannerTowersDecorationsAndLine()
    {
        var portal = StartFinishPortalRuntimeUpdater.EnsureStartFinishPortal();

        try
        {
            Assert.AreEqual(StartFinishPortalRuntimeUpdater.PortalRootName, portal.name);
            Assert.IsNotNull(GameObject.Find("START / FINISH Banner"));
            Assert.IsNotNull(GameObject.Find("Left Race Portal Tower"));
            Assert.IsNotNull(GameObject.Find("Right Race Portal Tower"));
            Assert.IsNotNull(GameObject.Find("Start Finish Painted Line"));
            Assert.IsNotNull(GameObject.Find("Left Race Brand Panel"));
            Assert.IsNotNull(GameObject.Find("Right Race Brand Panel"));
            Assert.GreaterOrEqual(CountChildrenContaining(portal.transform, "Pennant"), 8);
            StringAssert.Contains("START / FINISH", portal.GetComponentInChildren<TextMeshPro>().text);
        }
        finally
        {
            Object.DestroyImmediate(portal);
        }
    }

    [Test]
    public void EnsureStartFinishPortal_DisablesOldSmallGateAndAvoidsDuplicates()
    {
        var oldGate = new GameObject("Start Finish Gate");
        var oldLine = new GameObject("Start Finish Line");
        var first = StartFinishPortalRuntimeUpdater.EnsureStartFinishPortal();
        var second = StartFinishPortalRuntimeUpdater.EnsureStartFinishPortal();

        try
        {
            Assert.IsFalse(oldGate.activeSelf);
            Assert.IsFalse(oldLine.activeSelf);
            Assert.AreSame(first, second);
            Assert.AreEqual(1, Object.FindObjectsByType<StartFinishPortalRuntimeUpdater>(FindObjectsSortMode.None).Length);
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(oldGate);
            Object.DestroyImmediate(oldLine);
        }
    }

    private static int CountChildrenContaining(Transform parent, string namePart)
    {
        var count = 0;
        for (var index = 0; index < parent.childCount; index++)
        {
            if (parent.GetChild(index).name.Contains(namePart))
            {
                count++;
            }
        }

        return count;
    }
}
