using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public class EnvironmentGroundRenderingCleanupTests
{
    [Test]
    public void IsExtraneousGroundOrTerrain_DisablesImportedTerrainButKeepsGeneratedGrassAndRoad()
    {
        var terrain = new GameObject("Transparent Ground Terrain Strip");
        var forestTerrain = new GameObject("Low Poly Forest Terrain");
        var shoulder = new GameObject("Roadside Embankment Shoulders");
        var shoulderPart = new GameObject("Left Road Shoulder");
        var road = new GameObject("Sweeping 3 km Loop Road");

        try
        {
            shoulderPart.transform.SetParent(shoulder.transform, false);

            Assert.IsTrue(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(terrain));
            Assert.IsTrue(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(forestTerrain));
            Assert.IsFalse(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(shoulderPart));
            Assert.IsFalse(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(road));
        }
        finally
        {
            Object.DestroyImmediate(terrain);
            Object.DestroyImmediate(forestTerrain);
            Object.DestroyImmediate(shoulder);
            Object.DestroyImmediate(road);
        }
    }

    [Test]
    public void ApplyOpaqueGrassMaterial_ReplacesTransparentBrownWithSolidGrass()
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "Brown Roadside Ground Strip";
        var renderer = strip.GetComponent<Renderer>();
        renderer.material.color = new Color(0.38f, 0.22f, 0.08f, 0.35f);

        try
        {
            EnvironmentGroundRenderingCleanup.ApplyOpaqueGrassMaterial(renderer, EnvironmentGroundRenderingCleanup.OpenTerrainGrassColor);

            Assert.AreEqual(1f, renderer.material.color.a, 0.001f);
            Assert.Greater(renderer.material.color.g, renderer.material.color.r);
            Assert.Greater(renderer.material.color.g, renderer.material.color.b);
            Assert.AreEqual((int)UnityEngine.Rendering.RenderQueue.Geometry, renderer.material.renderQueue);
        }
        finally
        {
            Object.DestroyImmediate(strip);
        }
    }

    [Test]
    public void CleanupSceneGround_DisablesExtraGroundAndKeepsGeneratedGrass()
    {
        var terrain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        terrain.name = "Transparent Ground Terrain Strip";
        var grassRoot = new GameObject("Open Grass Shoulders");
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "Left Open Grass Segment";
        grass.transform.SetParent(grassRoot.transform, false);

        try
        {
            EnvironmentGroundRenderingCleanup.CleanupSceneGround();

            Assert.IsFalse(terrain.activeSelf);
            Assert.IsTrue(grass.activeSelf);
            Assert.Greater(grass.GetComponent<Renderer>().material.color.g, grass.GetComponent<Renderer>().material.color.r);
        }
        finally
        {
            Object.DestroyImmediate(terrain);
            Object.DestroyImmediate(grassRoot);
            DestroyRuntimeGround();
        }
    }

    [Test]
    public void CleanupSceneGroundWithStats_ReportsSinglePassWork()
    {
        var terrain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        terrain.name = "Transparent Ground Terrain Strip";
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "Left Open Grass Segment";

        try
        {
            var stats = EnvironmentGroundRenderingCleanup.CleanupSceneGroundWithStats();

            Assert.GreaterOrEqual(stats.RenderersScanned, 2);
            Assert.GreaterOrEqual(stats.DisabledObjects, 1);
            Assert.GreaterOrEqual(stats.RecoloredObjects, 1);
            Assert.IsFalse(terrain.activeSelf);
            Assert.IsTrue(grass.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(terrain);
            Object.DestroyImmediate(grass);
            DestroyRuntimeGround();
        }
    }

    [Test]
    public void EnvironmentGroundRenderingCleanup_DoesNotRunCleanupEveryUpdateFrame()
    {
        var updateMethod = typeof(EnvironmentGroundRenderingCleanup).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.IsNull(updateMethod);
    }

    [Test]
    public void EnsureContinuousGreenRoadsideGroundExists_CreatesRuntimeGroundForExistingScenes()
    {
        var root = EnvironmentGroundRenderingCleanup.EnsureContinuousGreenRoadsideGroundExists();

        try
        {
            Assert.AreEqual(EnvironmentGroundRenderingCleanup.RuntimeGroundRootName, root.name);
            Assert.AreEqual(3, root.transform.childCount);

            for (var index = 0; index < root.transform.childCount; index++)
            {
                var renderer = root.transform.GetChild(index).GetComponent<MeshRenderer>();
                var meshFilter = root.transform.GetChild(index).GetComponent<MeshFilter>();

                Assert.IsNotNull(renderer);
                Assert.IsNotNull(meshFilter);
                Assert.Greater(renderer.material.color.g, renderer.material.color.r);
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void EnsureContinuousGreenRoadsideGroundExists_RebuildsExistingSavedGroundMeshes()
    {
        var root = new GameObject("Continuous Green Roadside Ground");
        var oldLeft = new GameObject("Left Continuous Grass Terrain");
        oldLeft.transform.SetParent(root.transform, false);
        oldLeft.AddComponent<MeshFilter>().mesh = new Mesh
        {
            vertices = new[] { Vector3.zero, Vector3.right, Vector3.forward },
            triangles = new[] { 0, 1, 2 }
        };
        oldLeft.AddComponent<MeshRenderer>().material.color = new Color(0.35f, 0.25f, 0.16f, 0.45f);

        try
        {
            var refreshed = EnvironmentGroundRenderingCleanup.EnsureContinuousGreenRoadsideGroundExists();
            var left = refreshed.transform.Find("Left Runtime Grass Terrain");
            var right = refreshed.transform.Find("Right Runtime Grass Terrain");

            Assert.AreSame(root, refreshed);
            Assert.IsNotNull(refreshed.transform.Find("Full Course Green Ground Coverage"));
            Assert.IsNotNull(left);
            Assert.IsNotNull(right);
            Assert.Greater(left.GetComponent<MeshFilter>().sharedMesh.vertexCount, 3);
            Assert.AreEqual(1f, left.GetComponent<MeshRenderer>().material.color.a, 0.001f);
            Assert.Greater(left.GetComponent<MeshRenderer>().material.color.g, left.GetComponent<MeshRenderer>().material.color.r);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void IsFlatRoadsideGroundCandidate_DetectsLargeBrownOrGreyStripsButKeepsRoad()
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "Old Brown Side Surface";
        strip.transform.localScale = new Vector3(12f, 0.08f, 80f);
        strip.GetComponent<Renderer>().material.color = new Color(0.34f, 0.25f, 0.16f, 1f);

        var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Sweeping 3 km Loop Road";
        road.transform.localScale = new Vector3(8f, 0.08f, 80f);
        road.GetComponent<Renderer>().material.color = new Color(0.32f, 0.32f, 0.32f, 1f);

        try
        {
            Assert.IsTrue(EnvironmentGroundRenderingCleanup.IsFlatRoadsideGroundCandidate(strip.GetComponent<Renderer>()));
            Assert.IsFalse(EnvironmentGroundRenderingCleanup.IsFlatRoadsideGroundCandidate(road.GetComponent<Renderer>()));
        }
        finally
        {
            Object.DestroyImmediate(strip);
            Object.DestroyImmediate(road);
        }
    }

    [Test]
    public void RoadVisualConstants_KeepCleanAsphaltAndWhiteMarkings()
    {
        Assert.Less(SkiErgGameBootstrap.RoadAsphaltColor.r, 0.13f);
        Assert.Less(SkiErgGameBootstrap.CountryRoadCenterDashWidthMeters, 0.2f);
        Assert.Less(SkiErgGameBootstrap.CountryRoadCenterDashLengthMeters, 10f);
        Assert.Greater(SkiErgGameBootstrap.RoadMarkingColor.r, SkiErgGameBootstrap.RoadAsphaltColor.r);
        Assert.AreEqual(Color.white, SkiErgGameBootstrap.RoadMarkingColor);
    }

    [Test]
    public void IsFlatRoadsideGroundCandidate_DetectsFlatSurfaceEvenUnderTreeParent()
    {
        var treeParent = new GameObject("Set Back Roadside Trees");
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "Old Brown Side Surface";
        strip.transform.SetParent(treeParent.transform, false);
        strip.transform.localScale = new Vector3(12f, 0.08f, 80f);
        strip.GetComponent<Renderer>().material.color = new Color(0.34f, 0.25f, 0.16f, 1f);

        try
        {
            Assert.IsTrue(EnvironmentGroundRenderingCleanup.IsFlatRoadsideGroundCandidate(strip.GetComponent<Renderer>()));
        }
        finally
        {
            Object.DestroyImmediate(treeParent);
        }
    }

    [Test]
    public void CleanupSceneGround_RecolorsRoadLikeSideStripsToGrass()
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "Old Grey Roadside Surface";
        strip.transform.localScale = new Vector3(14f, 0.08f, 90f);
        strip.GetComponent<Renderer>().material.color = new Color(0.38f, 0.38f, 0.36f, 1f);

        try
        {
            EnvironmentGroundRenderingCleanup.CleanupSceneGround();

            var color = strip.GetComponent<Renderer>().material.color;
            Assert.IsTrue(strip.activeSelf);
            Assert.Greater(color.g, color.r);
            Assert.Greater(color.g, color.b);
        }
        finally
        {
            Object.DestroyImmediate(strip);
            DestroyRuntimeGround();
        }
    }

    [Test]
    public void IsLargeBrownGroundCandidate_DetectsWideBrownTerrainEvenWhenNotFlat()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Wide Brown Terrain Surface";
        ground.transform.localScale = new Vector3(80f, 2.2f, 140f);
        ground.GetComponent<Renderer>().material.color = new Color(0.34f, 0.27f, 0.2f, 1f);

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Tree Trunk";
        trunk.transform.localScale = new Vector3(0.35f, 3f, 0.35f);
        trunk.GetComponent<Renderer>().material.color = new Color(0.34f, 0.22f, 0.1f, 1f);

        try
        {
            Assert.IsTrue(EnvironmentGroundRenderingCleanup.IsLargeBrownGroundCandidate(ground.GetComponent<Renderer>()));
            Assert.IsFalse(EnvironmentGroundRenderingCleanup.IsLargeBrownGroundCandidate(trunk.GetComponent<Renderer>()));
        }
        finally
        {
            Object.DestroyImmediate(ground);
            Object.DestroyImmediate(trunk);
        }
    }

    [Test]
    public void CleanupSceneGround_RecolorsWideBrownTerrainToGrass()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Wide Brown Terrain Surface";
        ground.transform.localScale = new Vector3(80f, 2.2f, 140f);
        ground.GetComponent<Renderer>().material.color = new Color(0.34f, 0.27f, 0.2f, 1f);

        try
        {
            EnvironmentGroundRenderingCleanup.CleanupSceneGround();

            var color = ground.GetComponent<Renderer>().material.color;
            Assert.Greater(color.g, color.r);
            Assert.Greater(color.g, color.b);
        }
        finally
        {
            Object.DestroyImmediate(ground);
            DestroyRuntimeGround();
        }
    }

    [Test]
    public void EdgeFlowCueColors_ReadAsGrassNotRoadStrips()
    {
        AssertGrassColor(SpeedFeelingRuntimeUpdater.EdgeFlowGrassCueColorA);
        AssertGrassColor(SpeedFeelingRuntimeUpdater.EdgeFlowGrassCueColorB);
    }

    private static void AssertGrassColor(Color color)
    {
        Assert.AreEqual(1f, color.a, 0.001f);
        Assert.Greater(color.g, color.r);
        Assert.Greater(color.g, color.b);
    }

    private static void DestroyRuntimeGround()
    {
        var runtimeGround = GameObject.Find(EnvironmentGroundRenderingCleanup.RuntimeGroundRootName);

        if (runtimeGround != null)
        {
            Object.DestroyImmediate(runtimeGround);
        }
    }
}
