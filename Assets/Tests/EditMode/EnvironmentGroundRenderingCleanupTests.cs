using NUnit.Framework;
using UnityEngine;

public class EnvironmentGroundRenderingCleanupTests
{
    [Test]
    public void IsExtraneousGroundOrTerrain_DisablesImportedTerrainButKeepsGeneratedGrassAndRoad()
    {
        var terrain = new GameObject("Transparent Ground Terrain Strip");
        var shoulder = new GameObject("Roadside Embankment Shoulders");
        var shoulderPart = new GameObject("Left Road Shoulder");
        var road = new GameObject("Sweeping 3 km Loop Road");

        try
        {
            shoulderPart.transform.SetParent(shoulder.transform, false);

            Assert.IsTrue(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(terrain));
            Assert.IsFalse(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(shoulderPart));
            Assert.IsFalse(EnvironmentGroundRenderingCleanup.IsExtraneousGroundOrTerrain(road));
        }
        finally
        {
            Object.DestroyImmediate(terrain);
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
}
