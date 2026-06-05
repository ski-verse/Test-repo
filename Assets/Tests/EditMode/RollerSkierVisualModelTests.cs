using NUnit.Framework;
using UnityEngine;

public class RollerSkierVisualModelTests
{
    [Test]
    public void ProperRollerSkierVisual_UsesEnduranceAthleteProportions()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceShoulderWidth, 0.64f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceWaistWidth, 0.2f);
        Assert.Greater(ProperRollerSkierRuntimeUpdater.EnduranceShoulderWidth, ProperRollerSkierRuntimeUpdater.EnduranceWaistWidth * 3f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceLegVisualLength, 0.86f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceHeadDiameter, 0.19f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleNeckHeight, 0.11f);
    }

    [Test]
    public void ProperRollerSkierVisual_UsesSkierModel20HumanSilhouette()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceShoulderWidth, 0.69f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleUpperBackWidth, 0.6f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleLatWidth, 0.15f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceWaistWidth, 0.18f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.RealisticHipWidth, 0.34f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.RealisticHipWidth, 0.4f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceLegVisualLength, 0.94f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.EnduranceHeadDiameter, 0.17f);
        Assert.Greater(ProperRollerSkierRuntimeUpdater.EnduranceShoulderWidth, ProperRollerSkierRuntimeUpdater.RealisticHipWidth * 1.7f);
    }

    [Test]
    public void ProperRollerSkierVisual_UsesHumanBackHipAndGripDetails()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleShoulderCapRadius, 0.118f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleScapulaWidth, 0.17f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleGluteWidth, 0.16f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleGluteDepth, 0.12f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleShortsBandHeight, 0.03f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleGripWrapRadius, ProperRollerSkierRuntimeUpdater.VisiblePoleGripRadius * 1.05f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftRadius, 0.028f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleLateralOffset, 0.14f);
    }

    [Test]
    public void HumanSilhouettePass_ReducesBlueMannequinLookFromGameplayCamera()
    {
        Assert.LessOrEqual(SkierHumanSilhouetteRuntimeUpdater.ReducedBlueTorsoWidth, 0.22f);
        Assert.LessOrEqual(SkierHumanSilhouetteRuntimeUpdater.ReducedBlueTorsoHeight, 0.5f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisibleBackPanelWidth, 0.54f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisibleShortsPanelWidth, 0.38f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisibleGluteAccentWidth, 0.18f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisibleGripContrastRadius, ProperRollerSkierRuntimeUpdater.VisibleGloveRadius * 1.15f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisiblePoleOutsideOffset, 0.18f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.GameplayReadablePoleRadius, ProperRollerSkierRuntimeUpdater.VisiblePoleShaftRadius * 1.45f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.NaturalUpperArmRadius, 0.08f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.NaturalForearmRadius, 0.064f);
    }

    [Test]
    public void HumanSilhouettePass_UsesGameplayVisiblePoleSilhouette()
    {
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisiblePoleOutsideOffset, 0.34f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.GameplayReadablePoleRadius, 0.054f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.RearVisiblePoleRadius, 0.06f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.RearVisiblePoleOutsideOffset, 0.42f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.RearVisiblePolePlantRadius, 0.095f);
    }

    [Test]
    public void HumanSilhouettePass_RefinesGripBackAndHipRealism()
    {
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.PolePressureLineRadius, 0.04f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.VisibleHandClampRadius, 0.092f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.UpperBackRidgeWidth, 0.62f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.RearShoulderPlaneWidth, 0.26f);
        Assert.GreaterOrEqual(SkierHumanSilhouetteRuntimeUpdater.NaturalHipRoundness, 0.22f);
    }

    [Test]
    public void HumanSilhouettePass_AttachesReadableHumanDetailsToAnimatedRig()
    {
        var skier = new GameObject("Low Poly Roller Skier");
        var animator = skier.AddComponent<RollerSkierAnimator>();
        var visualRoot = new GameObject("Roller Skier Visual").transform;
        visualRoot.SetParent(skier.transform, false);
        visualRoot.localScale = Vector3.one;

        Assert.IsTrue(ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel());
        Assert.IsTrue(SkierHumanSilhouetteRuntimeUpdater.ApplyHumanSilhouettePass());

        Assert.IsNotNull(visualRoot.Find(SkierHumanSilhouetteRuntimeUpdater.HumanSilhouetteAppliedMarkerName));
        Assert.IsNotNull(FindChildRecursive(animator.torso, "Human Dark Back Panel"));
        Assert.IsNotNull(FindChildRecursive(animator.torso, "Human Central Spine Seam"));
        Assert.IsNotNull(FindChildRecursive(animator.torso, "Human Left Scapula Shadow"));
        Assert.IsNotNull(FindChildRecursive(animator.torso, "Human Lat Shadow Left"));
        Assert.IsNotNull(FindChildRecursive(animator.torso, "Gameplay Upper Back Ridge"));
        Assert.IsNotNull(FindChildRecursive(animator.torso, "Left Rear Shoulder Plane"));
        Assert.IsNotNull(FindChildRecursive(animator.hips, "Human Black Shorts Block"));
        Assert.IsNotNull(FindChildRecursive(animator.hips, "Human Left Glute Accent"));
        Assert.IsNotNull(FindChildRecursive(animator.hips, "Human Shorts Leg Split"));
        Assert.IsNotNull(FindChildRecursive(animator.hips, "Left Natural Hip Roundness"));
        Assert.IsNotNull(FindChildRecursive(animator.leftHand, "Visible Glove Grip Wrap"));
        Assert.IsNotNull(FindChildRecursive(animator.rightHand, "Visible Glove Grip Wrap"));
        Assert.IsNotNull(FindChildRecursive(animator.leftHand, "Visible Hand Clamp Around Pole"));
        Assert.IsNotNull(FindChildRecursive(animator.rightHand, "Visible Hand Clamp Around Pole"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Gameplay Rear Visible Pole Shaft"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Gameplay Rear Visible Pole Shaft"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Gameplay Rear Pole Plant Dot"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Gameplay Rear Pole Plant Dot"));
        Assert.IsNotNull(FindChildRecursive(animator.leftPole, "Visible Pole Pressure Line"));
        Assert.IsNotNull(FindChildRecursive(animator.rightPole, "Visible Pole Pressure Line"));
        Assert.AreEqual(SkierHumanSilhouetteRuntimeUpdater.NaturalUpperArmRadius, FindChildRecursive(animator.leftArm, "Relaxed Upper Arm").localScale.x, 0.001f);
        Assert.AreEqual(SkierHumanSilhouetteRuntimeUpdater.NaturalUpperArmRadius, FindChildRecursive(animator.rightArm, "Relaxed Upper Arm").localScale.x, 0.001f);
        Assert.AreEqual(animator.leftHand, animator.leftPole.parent);
        Assert.AreEqual(animator.rightHand, animator.rightPole.parent);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void DoublePolePose_CompressesBodyWhileFeetStayConnectedToSkis()
    {
        var skier = new GameObject("Animated Skier");
        var animator = skier.AddComponent<RollerSkierAnimator>();
        animator.torso = new GameObject("Torso").transform;
        animator.hips = new GameObject("Hips").transform;
        animator.leftFoot = new GameObject("Left Foot").transform;
        animator.rightFoot = new GameObject("Right Foot").transform;
        animator.leftSki = new GameObject("Left Ski").transform;
        animator.rightSki = new GameObject("Right Ski").transform;
        animator.torso.SetParent(skier.transform, false);
        animator.hips.SetParent(skier.transform, false);
        animator.leftFoot.SetParent(skier.transform, false);
        animator.rightFoot.SetParent(skier.transform, false);
        animator.leftSki.SetParent(skier.transform, false);
        animator.rightSki.SetParent(skier.transform, false);
        animator.torso.localPosition = new Vector3(0f, 1.1f, 0f);
        animator.hips.localPosition = new Vector3(0f, 0.95f, 0f);
        animator.leftFoot.localPosition = new Vector3(-0.2f, 0.15f, 0.08f);
        animator.rightFoot.localPosition = new Vector3(0.2f, 0.15f, 0.08f);
        animator.leftSki.localPosition = new Vector3(-0.2f, 0f, 0.08f);
        animator.rightSki.localPosition = new Vector3(0.2f, 0f, 0.08f);
        var leftFootToSki = animator.leftFoot.localPosition - animator.leftSki.localPosition;
        var rightFootToSki = animator.rightFoot.localPosition - animator.rightSki.localPosition;

        animator.ApplyPose(0.46f);

        Assert.Less(animator.torso.localPosition.y, 1.1f);
        Assert.Less(animator.hips.localPosition.y, 0.95f);
        Assert.AreEqual(leftFootToSki.y, (animator.leftFoot.localPosition - animator.leftSki.localPosition).y, 0.002f);
        Assert.AreEqual(leftFootToSki.z, (animator.leftFoot.localPosition - animator.leftSki.localPosition).z, 0.002f);
        Assert.AreEqual(rightFootToSki.y, (animator.rightFoot.localPosition - animator.rightSki.localPosition).y, 0.002f);
        Assert.AreEqual(rightFootToSki.z, (animator.rightFoot.localPosition - animator.rightSki.localPosition).z, 0.002f);

        Object.DestroyImmediate(skier);
    }

    [Test]
    public void ProperRollerSkierVisual_UsesTightSuitAndReadableEquipmentDetails()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleGloveRadius, 0.062f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.VisibleGloveRadius, 0.072f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleBootCuffHeight, 0.18f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleBindingHeight, 0.06f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleHelmetWidth, 0.2f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleStrapLength, 0.24f);
    }

    [Test]
    public void ProperRollerSkierVisual_UsesModernClassicRollerSkiGeometry()
    {
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiFrameLength, 1.18f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiFrameWidth, 0.05f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiWheelDiameter, 0.22f);
        Assert.Less(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiRearWheelZ, ProperRollerSkierRuntimeUpdater.ClassicRollerSkiHeelZ);
        Assert.Less(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiHeelZ - ProperRollerSkierRuntimeUpdater.ClassicRollerSkiRearWheelZ, 0.1f);
        Assert.Greater(ProperRollerSkierRuntimeUpdater.ClassicRollerSkiFrontWheelZ - ProperRollerSkierRuntimeUpdater.ClassicRollerSkiToeZ, 0.65f);
    }

    [Test]
    public void ProperRollerSkierVisual_KeepsEquipmentReadableFromGameplayCamera()
    {
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleBindingHeight, 0.055f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleBootCuffHeight, 0.16f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisibleWheelSidewallWidth, 0.09f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftRadius, 0.026f);
        Assert.LessOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleShaftRadius, 0.03f);
        Assert.GreaterOrEqual(ProperRollerSkierRuntimeUpdater.VisiblePoleTipLateralOffset, 0.28f);
    }

    [Test]
    public void ApplyProperRollerSkierModel_ReplacesBootstrapVisualWithModel20()
    {
        var skier = new GameObject("Low Poly Roller Skier");
        var animator = skier.AddComponent<RollerSkierAnimator>();
        var visualRoot = new GameObject("Roller Skier Visual").transform;
        visualRoot.SetParent(skier.transform, false);
        visualRoot.localScale = Vector3.one;
        new GameObject("Old Placeholder Torso").transform.SetParent(visualRoot, false);

        var applied = ProperRollerSkierRuntimeUpdater.ApplyProperRollerSkierModel();

        Assert.IsTrue(applied);
        Assert.IsNotNull(visualRoot.Find(ProperRollerSkierRuntimeUpdater.Model20AppliedMarkerName));
        Assert.IsNull(visualRoot.Find("Old Placeholder Torso"));
        Assert.GreaterOrEqual(visualRoot.localScale.x, ProperRollerSkierRuntimeUpdater.Model20RuntimeVisualScale);
        Assert.IsNotNull(animator.torso);
        Assert.IsNotNull(animator.leftHand);
        Assert.IsNotNull(animator.rightHand);
        Assert.AreEqual(animator.leftHand, animator.leftPole.parent);
        Assert.AreEqual(animator.rightHand, animator.rightPole.parent);

        Object.DestroyImmediate(skier);
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
