using NUnit.Framework;
using TMPro;
using UnityEngine;

public class KilometerMarkerSignRuntimeUpdaterTests
{
    [Test]
    public void CalculateMarkerCount_StartsAtOneKilometerAndUsesCourseLength()
    {
        Assert.AreEqual(0, KilometerMarkerSignRuntimeUpdater.CalculateMarkerCount(999f));
        Assert.AreEqual(1, KilometerMarkerSignRuntimeUpdater.CalculateMarkerCount(1000f));
        Assert.AreEqual(3, KilometerMarkerSignRuntimeUpdater.CalculateMarkerCount(3000f));
        Assert.AreEqual(5, KilometerMarkerSignRuntimeUpdater.CalculateMarkerCount(5200f));
    }

    [Test]
    public void CalculateMarkerPosition_PlacesSignsSymmetricallyOutsideRoad()
    {
        var left = KilometerMarkerSignRuntimeUpdater.CalculateMarkerPosition(1000f, -1f);
        var right = KilometerMarkerSignRuntimeUpdater.CalculateMarkerPosition(1000f, 1f);
        var center = CoursePath.CenterPointAtDistance(1000f);
        var roadRight = CoursePath.RightAtDistance(1000f);

        var leftOffset = Vector3.Dot(left - center, roadRight);
        var rightOffset = Vector3.Dot(right - center, roadRight);

        Assert.Less(leftOffset, -EnvironmentPlacement.RoadHalfWidth);
        Assert.Greater(rightOffset, EnvironmentPlacement.RoadHalfWidth);
        Assert.AreEqual(Mathf.Abs(leftOffset), Mathf.Abs(rightOffset), 0.001f);
        Assert.AreEqual(left.y, right.y, 0.001f);
    }

    [Test]
    public void EnsureKilometerMarkers_CreatesTwoSignsPerCompletedKilometer()
    {
        var root = KilometerMarkerSignRuntimeUpdater.EnsureKilometerMarkers();

        try
        {
            var expectedKilometers = KilometerMarkerSignRuntimeUpdater.CalculateMarkerCount(CoursePath.CourseLengthMeters);
            Assert.AreEqual(expectedKilometers * 2, root.transform.childCount);
            Assert.IsNotNull(GameObject.Find("Left 1 km Marker"));
            Assert.IsNotNull(GameObject.Find("Right 1 km Marker"));
            Assert.IsNotNull(GameObject.Find("Left 3 km Marker"));
            Assert.IsNotNull(GameObject.Find("Right 3 km Marker"));
            Assert.IsNotNull(GameObject.Find("Left 1 km Marker").GetComponentInChildren<TextMeshPro>());
            Assert.AreEqual("1 km", GameObject.Find("Left 1 km Marker").GetComponentInChildren<TextMeshPro>().text);
            Assert.AreEqual(2, GameObject.Find("Left 1 km Marker").GetComponentsInChildren<TextMeshPro>().Length);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void EnsureKilometerMarkers_DoesNotDuplicateExistingMarkers()
    {
        var first = KilometerMarkerSignRuntimeUpdater.EnsureKilometerMarkers();
        var second = KilometerMarkerSignRuntimeUpdater.EnsureKilometerMarkers();

        try
        {
            Assert.AreSame(first, second);
            Assert.AreEqual(1, Object.FindObjectsByType<KilometerMarkerSignRuntimeUpdater>(FindObjectsSortMode.None).Length);
        }
        finally
        {
            Object.DestroyImmediate(first);
        }
    }
}
