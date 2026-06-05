using NUnit.Framework;
using UnityEngine;

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
    public void ShouldDoublePole_UsesAccelerationInputOrWattsThreshold()
    {
        Assert.IsTrue(RollerSkierAnimator.ShouldDoublePole(PlayerMovementInput.Accelerate, 25f));
        Assert.IsTrue(RollerSkierAnimator.ShouldDoublePole(new PlayerMovementInput(0f, 140f), 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(PlayerMovementInput.None, 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(new PlayerMovementInput(0f, 20f), 25f));
        Assert.IsFalse(RollerSkierAnimator.ShouldDoublePole(PlayerMovementInput.Decelerate, 25f));
    }

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
}
