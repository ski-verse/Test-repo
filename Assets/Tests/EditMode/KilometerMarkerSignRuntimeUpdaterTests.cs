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
    public void CalculateApproachFacingRotation_PointsSignTowardApproachingPlayer()
    {
        var rotation = KilometerMarkerSignRuntimeUpdater.CalculateApproachFacingRotation(1000f);
        var courseDirection = CoursePath.DirectionAtDistance(1000f);
        courseDirection.y = 0f;

        Assert.Greater(Vector3.Dot(rotation * Vector3.forward, courseDirection.normalized), 0.99f);
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
            Assert.AreEqual("1<size=50%> km</size>", GameObject.Find("Left 1 km Marker").GetComponentInChildren<TextMeshPro>().text);
            Assert.AreEqual(2, GameObject.Find("Left 1 km Marker").GetComponentsInChildren<TextMeshPro>().Length);
            Assert.Greater(GameObject.Find("Left 1 km Marker").transform.position.y, CoursePath.HeightAtDistance(1000f) + 1.5f);
            var board = GameObject.Find("Left 1 km Marker").transform.Find("Marker Board");
            var post = GameObject.Find("Left 1 km Marker").transform.Find("Marker Post");
            Assert.Greater(board.localScale.x, 3f);
            Assert.GreaterOrEqual(board.localScale.y, 2.7f);
            Assert.AreEqual(Color.white, board.GetComponent<Renderer>().sharedMaterial.color);
            Assert.LessOrEqual(post.localPosition.y + post.localScale.y * 0.5f, board.localPosition.y - board.localScale.y * 0.5f + 0.001f);
            Assert.AreEqual(Color.black, GameObject.Find("Left 1 km Marker").GetComponentInChildren<TextMeshPro>().color);
            var approachText = GameObject.Find("Left 1 km Marker").transform.Find("Marker Text Approach Face");
            Assert.IsNotNull(approachText);
            Assert.Less(approachText.localPosition.z, -0.15f);
            Assert.AreEqual(Quaternion.identity, approachText.localRotation);
            Assert.AreEqual("2<size=50%> km</size>", KilometerMarkerSignRuntimeUpdater.FormatMarkerLabel("2 km"));
            Assert.Greater(approachText.GetComponent<TextMeshPro>().fontSize, 23f);
            Assert.GreaterOrEqual(approachText.localScale.x, 1f);
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

    [Test]
    public void EnsureKilometerMarkers_RebuildsExistingSmallMarkers()
    {
        var root = new GameObject(KilometerMarkerSignRuntimeUpdater.MarkerRootName);
        var oldMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        oldMarker.name = "Tiny Old Marker";
        oldMarker.transform.SetParent(root.transform, false);

        var rebuilt = KilometerMarkerSignRuntimeUpdater.EnsureKilometerMarkers();

        try
        {
            Assert.AreSame(root, rebuilt);
            Assert.IsNull(GameObject.Find("Tiny Old Marker"));
            Assert.IsNotNull(GameObject.Find("Left 1 km Marker"));
            Assert.IsNotNull(GameObject.Find("Right 1 km Marker"));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
