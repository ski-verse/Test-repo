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
        Assert.GreaterOrEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, 0.09f);
        Assert.LessOrEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, 0.13f);
        Assert.AreEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, SkiClassicsSkierModelBuilder.SkiTrackHalfWidth, 0.001f);
        Assert.AreEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, SkiClassicsSkierModelBuilder.BootTrackHalfWidth, 0.001f);
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
    public void CreateModel_PlacesBootsBindingsAndRollerSkisOnTheSameNarrowTrack()
    {
        var parent = new GameObject("Model Parent").transform;

        var model = SkiClassicsSkierModelBuilder.CreateModel(parent);

        Assert.AreEqual(-SkiClassicsSkierModelBuilder.StanceHalfWidth, FindChildRecursive(model, "Left Classic Roller Ski").localPosition.x, 0.001f);
        Assert.AreEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, FindChildRecursive(model, "Right Classic Roller Ski").localPosition.x, 0.001f);
        Assert.AreEqual(-SkiClassicsSkierModelBuilder.StanceHalfWidth, FindChildRecursive(model, "Left Roller Ski Boot").localPosition.x, 0.001f);
        Assert.AreEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, FindChildRecursive(model, "Right Roller Ski Boot").localPosition.x, 0.001f);
        Assert.AreEqual(-SkiClassicsSkierModelBuilder.StanceHalfWidth, FindChildRecursive(model, "Left Classic Binding").localPosition.x, 0.001f);
        Assert.AreEqual(SkiClassicsSkierModelBuilder.StanceHalfWidth, FindChildRecursive(model, "Right Classic Binding").localPosition.x, 0.001f);

        Object.DestroyImmediate(parent.gameObject);
    }

    [Test]
    public void CreateModel_UsesReferenceImageRearViewArmsLegsBootsAndHelmet()
    {
        var parent = new GameObject("Model Parent").transform;

        var model = SkiClassicsSkierModelBuilder.CreateModel(parent);

        Assert.IsNotNull(FindChildRecursive(model, "Bare Upper Back"));
        Assert.IsNotNull(FindChildRecursive(model, "Red Heart Rate Strap"));
        Assert.IsNotNull(FindChildRecursive(model, "Black Bib Shorts"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Bare Upper Arm"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Bare Upper Arm"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Bare Forearm"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Bare Forearm"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Bare Thigh"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Bare Thigh"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Bare Calf"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Bare Calf"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Boot Neon Cuff"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Boot Neon Cuff"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Boot Heel Accent"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Boot Heel Accent"));
        Assert.IsNotNull(FindChildRecursive(model, "White Helmet Rear Panel"));
        Assert.IsNotNull(FindChildRecursive(model, "Black Helmet Shell"));
        Assert.IsNotNull(FindChildRecursive(model, "Left Outside Pole Silhouette"));
        Assert.IsNotNull(FindChildRecursive(model, "Right Outside Pole Silhouette"));

        var torsoRenderer = FindChildRecursive(model, "Bare Upper Back").GetComponent<Renderer>();
        var shortsRenderer = FindChildRecursive(model, "Black Bib Shorts").GetComponent<Renderer>();
        Assert.Greater(torsoRenderer.sharedMaterial.color.r, shortsRenderer.sharedMaterial.color.r * 8f);
        Assert.Less(FindChildRecursive(model, "Compact Head").localScale.x, FindChildRecursive(model, "Visible Shoulder Line").localScale.x * 0.26f);
        Assert.Greater(FindChildRecursive(model, "Left Roller Ski Boot").localScale.y, 0.08f);

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
