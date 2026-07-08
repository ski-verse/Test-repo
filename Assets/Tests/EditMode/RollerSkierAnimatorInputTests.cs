using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

public class RollerSkierAnimatorInputTests
{
    [Test]
    public void PlayerMovementInput_CarriesFuturePropulsionWatts()
    {
        var input = new PlayerMovementInput(0f, 95f);

        Assert.AreEqual(0f, input.SpeedAxis, 0.001f);
        Assert.AreEqual(95f, input.PropulsionWatts, 0.001f);
    }

    [Test]
    public void PlayerMovementInput_SeparatesPropulsionWattsFromActivePoling()
    {
        var wattsWithoutStroke = new PlayerMovementInput(0f, 140f, false);
        var strokePulse = new PlayerMovementInput(0f, 140f, true);

        Assert.AreEqual(140f, wattsWithoutStroke.PropulsionWatts, 0.001f);
        Assert.IsFalse(wattsWithoutStroke.IsActivelyPoling);
        Assert.IsTrue(strokePulse.IsActivelyPoling);
        Assert.IsTrue(PlayerMovementInput.Accelerate.IsActivelyPoling);
    }

    [Test]
    public void ShouldDoublePole_UsesActivePolingSignalInsteadOfWattsAlone()
    {
        Assert.IsTrue(RollerSkierAnimator.ShouldDoublePole(PlayerMovementInput.Accelerate, 25f));
        Assert.IsTrue(RollerSkierAnimator.ShouldDoublePole(new PlayerMovementInput(0f, 140f, true), 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(new PlayerMovementInput(0f, 140f, false), 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(PlayerMovementInput.None, 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(new PlayerMovementInput(0f, 20f), 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(PlayerMovementInput.Decelerate, 25f));
    }

    [Test]
    public void ImportedDoublePolingAnimationInputDriver_UsesActivePolingSignalNotWattsAlone()
    {
        Assert.IsTrue(ImportedDoublePolingAnimationInputDriver.HasActivePropulsionInput(PlayerMovementInput.Accelerate, 0f));
        Assert.IsTrue(ImportedDoublePolingAnimationInputDriver.HasActivePropulsionInput(new PlayerMovementInput(0f, 0.5f, true), 0f));
        Assert.IsFalse(ImportedDoublePolingAnimationInputDriver.HasActivePropulsionInput(new PlayerMovementInput(0f, 140f, false), 0f));
        Assert.IsFalse(ImportedDoublePolingAnimationInputDriver.HasActivePropulsionInput(PlayerMovementInput.None, 0f));
        Assert.IsFalse(ImportedDoublePolingAnimationInputDriver.HasActivePropulsionInput(PlayerMovementInput.Decelerate, 0f));
    }

    [Test]
    public void ImportedDoublePolingAnimationInputDriver_StopsAnimatorWhenPropulsionIsIdle()
    {
        var visual = new GameObject("Imported Double Poling Test Skier");

        try
        {
            var animator = visual.AddComponent<Animator>();
            animator.speed = 1f;
            var driver = visual.AddComponent<ImportedDoublePolingAnimationInputDriver>();
            driver.animator = animator;

            driver.ApplyPolingState(false);

            Assert.AreEqual(0f, animator.speed, 0.001f);

            driver.ApplyPolingState(true);

            Assert.AreEqual(1f, animator.speed, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(visual);
        }
    }

#if UNITY_EDITOR
    [Test]
    public void ImportedDoublePolingController_ExposesIsPolingBoolParameter()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Player animator controller.controller");

        Assert.IsNotNull(controller);
        Assert.IsTrue(HasBoolParameter(controller, ImportedDoublePolingAnimationInputDriver.IsPolingParameterName));
    }
#endif

    [Test]
    public void CalculateNextPhase_DoesNotAdvanceDoublePoleWithoutPropulsion()
    {
        var currentPhase = 0.5f;

        var nextPhase = RollerSkierAnimator.CalculateNextPhase(
            currentPhase,
            24f,
            0.1f,
            0.65f,
            0.018f,
            false,
            2.5f);

        Assert.Less(nextPhase, currentPhase);
        Assert.GreaterOrEqual(nextPhase, 0f);
    }

    [Test]
    public void PlayerSpeedController_CanCoastWhileAnimationReturnsToIdle()
    {
        var player = new GameObject("Player").AddComponent<PlayerSpeedController>();
        player.CurrentSpeed = 6f;

        var glideInput = new PlayerMovementInput(0f, 0f, false);
        player.ApplyMovementInputAndGradientResistance(glideInput, 0.5f);

        Assert.Less(player.CurrentSpeed, 6f);
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(player.LastMovementInput, 25f));
        Object.DestroyImmediate(player.gameObject);
    }

    [Test]
    public void CalculateNextPhase_AdvancesWhenPropulsionIsActive()
    {
        var currentPhase = 0.5f;

        var nextPhase = RollerSkierAnimator.CalculateNextPhase(
            currentPhase,
            24f,
            0.1f,
            0.65f,
            0.018f,
            true,
            2.5f);

        Assert.Greater(nextPhase, currentPhase);
    }

    [Test]
    public void PlayerSpeedController_RemembersLastMovementInputForAnimation()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();

        controller.ApplyMovementInputAndGradientResistance(PlayerMovementInput.Accelerate, 0.1f);

        Assert.Greater(controller.LastMovementInput.SpeedAxis, 0f);
        Object.DestroyImmediate(controller.gameObject);
    }

#if UNITY_EDITOR
    private static bool HasBoolParameter(AnimatorController controller, string parameterName)
    {
        for (var i = 0; i < controller.parameters.Length; i++)
        {
            if (controller.parameters[i].type == AnimatorControllerParameterType.Bool && controller.parameters[i].name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
#endif
}
