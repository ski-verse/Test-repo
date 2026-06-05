using NUnit.Framework;
using UnityEngine;

public class SkiClassicsSkierModelBuilderTests
{
    [Test]
    public void ModelConstants_UseHumanSkiClassicsProportions()
    {
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.ShoulderWidth, 0.66f);
        Assert.LessOrEqual(SkiClassicsSkierModelBuilder.WaistWidth, 0.24f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.HipWidth, 0.36f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.ThighLength, 0.48f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.CalfLength, 0.42f);
        Assert.LessOrEqual(SkiClassicsSkierModelBuilder.HeadDiameter, 0.18f);
        Assert.Greater(SkiClassicsSkierModelBuilder.ShoulderWidth, SkiClassicsSkierModelBuilder.HipWidth * 1.6f);
    }

    [Test]
    public void ModelConstants_UseReadableClassicRollerSkiEquipment()
    {
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.ClassicRollerSkiLength, 1.08f);
        Assert.LessOrEqual(SkiClassicsSkierModelBuilder.ClassicRollerSkiWidth, 0.052f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.WheelDiameter, 0.22f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.PoleShaftRadius, 0.038f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.PoleAttachmentForwardOffset, 0.22f);
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.PoleAttachmentLateralOffset, 0.34f);
    }

    [Test]
    public void CreateModel_BuildsIndependentSkierWithReadableHumanAndEquipmentParts()
    {
        var parent = new GameObject("Model Parent").transform;

        var model = SkiClassicsSkierModelBuilder.CreateModel(parent);

        Assert.IsNotNull(model);
        Assert.AreEqual(parent, model.parent);
        Assert.AreEqual("Ski Classics Roller Skier Model", model.name);
        Assert.IsNotNull(FindChildRecursive(model, "Athletic Torso"));
        Assert.IsNotNull(FindChildRecursive(model, "Visible Shoulder Line"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Shoulder Cap"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Shoulder Cap"));
        Assert.IsNotNull(FindChildRecursive(model, "Narrow Waist"));
        Assert.IsNotNull(FindChildRecursive(model, "Readable Hips"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Thigh"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Thigh"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Calf"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Calf"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Roller Ski Boot"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Roller Ski Boot"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Classic Binding"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Classic Binding"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Classic Roller Ski"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Classic Roller Ski"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Front Wheel"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Rear Wheel"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Front Wheel"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Rear Wheel"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Pole Grip Attachment"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Pole Grip Attachment"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Ski Pole"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Ski Pole"));

        Object.DestroyImmediate(parent.gameObject);
    }

    [Test]
    public void CreateModel_UsesReferenceVideoRearViewSilhouette()
    {
        var parent = new GameObject("Model Parent").transform;

        var model = SkiClassicsSkierModelBuilder.CreateModel(parent);

        Assert.IsNotNull(FindChildRecursive(model, "Dark Rear Racing Suit Panel"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Shoulder Blade"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Shoulder Blade"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Glute Shape"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Glute Shape"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Outside Pole Silhouette"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Outside Pole Silhouette"));

        var torsoRenderer = FindChildRecursive(model, "Athletic Torso").GetComponent<Renderer>();
        Assert.Less(torsoRenderer.sharedMaterial.color.b, 0.16f);
        Assert.LessOrEqual(FindChildRecursive(model, "Compact Head").localScale.x, 0.16f);
        Assert.Less(FindChildRecursive(model, "Narrow Waist").localScale.x, FindChildRecursive(model, "Visible Shoulder Line").localScale.x * 0.4f);

        Object.DestroyImmediate(parent.gameObject);
    }

    [Test]
    public void CreateModel_DoesNotRequireOrModifyExistingGameplaySkier()
    {
        var existingSkier = new GameObject("Low Poly Roller Skier");
        var existingVisual = new GameObject("Roller Skier Visual").transform;
        existingVisual.SetParent(existingSkier.transform, false);
        new GameObject("Existing Skier Part").transform.SetParent(existingVisual, false);
        var parent = new GameObject("Model Parent").transform;

        var model = SkiClassicsSkierModelBuilder.CreateModel(parent, "Independent Ski Classics Model");

        Assert.IsNotNull(model);
        Assert.AreEqual("Independent Ski Classics Model", model.name);
        Assert.IsNotNull(existingVisual.Find("Existing Skier Part"));
        Assert.IsNull(existingVisual.Find("Athletic Torso"));

        Object.DestroyImmediate(parent.gameObject);
        Object.DestroyImmediate(existingSkier);
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var match = FindChildRecursive(root.GetChild(i), childName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
