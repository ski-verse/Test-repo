using NUnit.Framework;
using System.Reflection;
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

    [Test]
    public void Pm5PlayerInputSource_RetriesDiscoveryWhenWorkoutMetricsSourceAppearsLater()
    {
        var inputObject = new GameObject("PM5 Input");
        var input = inputObject.AddComponent<Pm5PlayerInputSource>();

        Assert.AreEqual(0f, input.ReadMovementInput().PropulsionWatts, 0.001f);

        var metricsObject = new GameObject("PM5 Metrics Source");
        var metrics = metricsObject.AddComponent<Pm5WorkoutDataSource>();
        var latestMetricsField = typeof(Pm5WorkoutDataSource).GetField("latestMetrics", BindingFlags.NonPublic | BindingFlags.Instance);
        latestMetricsField.SetValue(metrics, new Pm5WorkoutMetrics
        {
            HasWatts = true,
            Watts = 165f
        });

        var movementInput = input.ReadMovementInput();

        Assert.AreEqual(165f, movementInput.PropulsionWatts, 0.001f);

        Object.DestroyImmediate(inputObject);
        Object.DestroyImmediate(metricsObject);
    }

    [Test]
    public void Pm5PlayerInputSource_UsesStrokeCountDeltaForActivePolingAndTimesOut()
    {
        var inputObject = new GameObject("PM5 Input");
        var input = inputObject.AddComponent<Pm5PlayerInputSource>();
        input.strokePolingTimeoutSeconds = 1.75f;
        var metrics = inputObject.AddComponent<FakeWorkoutMetricsSource>();
        metrics.HasWorkoutMetricsValue = true;
        metrics.WattsValue = 180f;
        metrics.HasStrokeMetricsValue = true;
        metrics.TotalStrokesValue = 10;
        input.workoutMetricsSourceBehaviour = metrics;

        var baseline = input.ReadMovementInput(0f);
        Assert.AreEqual(180f, baseline.PropulsionWatts, 0.001f);
        Assert.IsFalse(baseline.IsActivelyPoling);

        metrics.TotalStrokesValue = 11;
        var strokePulse = input.ReadMovementInput(0.1f);
        Assert.IsTrue(strokePulse.IsActivelyPoling);

        var withinTimeout = input.ReadMovementInput(1.5f);
        Assert.IsTrue(withinTimeout.IsActivelyPoling);

        var afterTimeout = input.ReadMovementInput(2.0f);
        Assert.AreEqual(180f, afterTimeout.PropulsionWatts, 0.001f);
        Assert.IsFalse(afterTimeout.IsActivelyPoling);

        Object.DestroyImmediate(inputObject);
    }

    [Test]
    public void Bootstrap_EnsuresExistingPlayerUsesPm5InputWithKeyboardFallback()
    {
        var player = new GameObject("Existing Player");
        var controller = player.AddComponent<PlayerSpeedController>();
        var keyboard = player.AddComponent<KeyboardPlayerInputSource>();
        controller.InputSource = keyboard;

        var pm5Input = SkiErgGameBootstrap.EnsurePm5PlayerInput(controller);

        Assert.IsNotNull(pm5Input);
        Assert.AreSame(pm5Input, controller.InputSource);
        Assert.AreEqual(KeyCode.W, pm5Input.accelerateKey);
        Assert.AreEqual(KeyCode.S, pm5Input.decelerateKey);

        Object.DestroyImmediate(player);
    }

    private sealed class TestInputSource : IPlayerInputSource
    {
        public PlayerMovementInput MovementInput;

        public PlayerMovementInput ReadMovementInput()
        {
            return MovementInput;
        }
    }

    private sealed class FakeWorkoutMetricsSource : MonoBehaviour, IWorkoutMetricsSource, IStrokeMetricsSource
    {
        public bool HasWorkoutMetricsValue;
        public float WattsValue;
        public bool HasStrokeMetricsValue;
        public int TotalStrokesValue;

        public bool HasWorkoutMetrics => HasWorkoutMetricsValue;

        public float Watts => WattsValue;

        public float HeartRateBpm => 0f;

        public bool HasHeartRateBpm => false;

        public bool HasStrokeMetrics => HasStrokeMetricsValue;

        public float StrokeRateSpm => 0f;

        public int TotalStrokes => TotalStrokesValue;
    }

}
