using NUnit.Framework;
using UnityEngine;

public class PlayerInputAbstractionTests
{
    [Test]
    public void PlayerMovementInput_ClampsSpeedAxisForProviderSafety()
    {
        Assert.AreEqual(1f, new PlayerMovementInput(3f).SpeedAxis, 0.001f);
        Assert.AreEqual(-1f, new PlayerMovementInput(-2f).SpeedAxis, 0.001f);
        Assert.AreEqual(0f, PlayerMovementInput.None.SpeedAxis, 0.001f);
    }

    [Test]
    public void ApplyMovementInput_AcceleratesAndDeceleratesFromAbstractInput()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        controller.acceleration = 4f;
        controller.deceleration = 2f;
        controller.CurrentSpeed = 1f;

        controller.ApplyMovementInput(new PlayerMovementInput(1f), 0.5f);
        Assert.AreEqual(3f, controller.CurrentSpeed, 0.001f);

        controller.ApplyMovementInput(new PlayerMovementInput(-0.5f), 1f);
        Assert.AreEqual(2f, controller.CurrentSpeed, 0.001f);

        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void ApplyInputSource_UsesInjectedInputWithoutReadingKeyboardDirectly()
    {
        var controller = new GameObject("Player").AddComponent<PlayerSpeedController>();
        var testInput = new TestInputSource { MovementInput = new PlayerMovementInput(1f) };
        controller.InputSource = testInput;
        controller.AlignToCourse(900f);
        controller.acceleration = 6f;
        controller.CurrentSpeed = 2f;

        controller.ApplyInputSource(0.5f);

        Assert.LessOrEqual(controller.CurrentGradientPercent, 0f);
        Assert.AreEqual(5f, controller.CurrentSpeed, 0.001f);
        Object.DestroyImmediate(controller.gameObject);
    }

    [Test]
    public void EnsureInputSource_AddsKeyboardSourceWhenNoSourceIsConfigured()
    {
        var player = new GameObject("Player");
        var controller = player.AddComponent<PlayerSpeedController>();

        controller.EnsureInputSource();

        Assert.IsInstanceOf<KeyboardPlayerInputSource>(controller.InputSource);
        Assert.IsNotNull(player.GetComponent<KeyboardPlayerInputSource>());
        Object.DestroyImmediate(player);
    }

    [Test]
    public void KeyboardInputSource_DefaultsToCurrentControls()
    {
        var keyboard = new GameObject("Keyboard Input").AddComponent<KeyboardPlayerInputSource>();

        Assert.AreEqual(KeyCode.W, keyboard.accelerateKey);
        Assert.AreEqual(KeyCode.S, keyboard.decelerateKey);

        Object.DestroyImmediate(keyboard.gameObject);
    }

    [Test]
    public void Pm5PlayerInputSource_ReadsWattsFromWorkoutMetricsSource()
    {
        var inputObject = new GameObject("PM5 Input");
        var input = inputObject.AddComponent<Pm5PlayerInputSource>();
        var metrics = inputObject.AddComponent<FakeWorkoutMetricsSource>();
        metrics.HasWorkoutMetricsValue = true;
        metrics.WattsValue = 143f;
        input.workoutMetricsSourceBehaviour = metrics;

        var movementInput = input.ReadMovementInput();

        Assert.AreEqual(0f, movementInput.SpeedAxis, 0.001f);
        Assert.AreEqual(143f, movementInput.PropulsionWatts, 0.001f);

        Object.DestroyImmediate(inputObject);
    }

    [Test]
    public void Pm5PlayerInputSource_ReturnsZeroWattsWhenWorkoutMetricsAreMissing()
    {
        var inputObject = new GameObject("PM5 Input");
        var input = inputObject.AddComponent<Pm5PlayerInputSource>();
        var metrics = inputObject.AddComponent<FakeWorkoutMetricsSource>();
        metrics.HasWorkoutMetricsValue = false;
        metrics.WattsValue = 143f;
        input.workoutMetricsSourceBehaviour = metrics;

        var movementInput = input.ReadMovementInput();

        Assert.AreEqual(0f, movementInput.PropulsionWatts, 0.001f);

        Object.DestroyImmediate(inputObject);
    }

    private sealed class TestInputSource : IPlayerInputSource
    {
        public PlayerMovementInput MovementInput;

        public PlayerMovementInput ReadMovementInput()
        {
            return MovementInput;
        }
    }

    private sealed class FakeWorkoutMetricsSource : MonoBehaviour, IWorkoutMetricsSource
    {
        public bool HasWorkoutMetricsValue;
        public float WattsValue;

        public bool HasWorkoutMetrics => HasWorkoutMetricsValue;

        public float Watts => WattsValue;

        public float HeartRateBpm => 0f;

        public bool HasHeartRateBpm => false;
    }
}
