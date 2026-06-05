# Ski-Verse SkiErg Prototype

A small Unity prototype for a SkiErg-inspired movement game.

## What is included

- A runtime-generated loopable 5 km road with large, clearly visible sweeping training-course turns
- Visible uphill and downhill road sections generated from the course profile
- A major 700 meter climb at about 6.5% grade, followed by a descent so the 5 km course loops back to its start height
- Gradient-based speed reduction that slows actual forward movement on climbs while preserving the current W/S controls
- White edge lines and dashed center road markings that follow the curves and slopes
- At least 15 meters of open terrain on each side of the road before trees, hills, forests, or mountains begin
- Roadside speed posts every 25 meters
- Low-poly turn warning signs placed before upcoming bends
- Dense roadside motion cues and low road-edge flow cues that pass near the player for a stronger sense of speed while travelling through the landscape
- Green open grass shoulders on both sides of the road
- Set-back simple low-poly trees placed outside the open road margin
- Rolling low-poly hills moved further away from the road
- Distant low-poly forest bands between the road corridor and mountain ranges
- Rounded low-poly Scandinavian mountain chains with reduced height, smoother silhouettes, and a supporting horizon role while staying outside the road corridor
- Start and finish gates with road line markers
- A more realistic low-poly roller skier resembling an endurance Vasalopp/Ski Classics athlete, with longer legs, a narrower waist, broader relaxed shoulders, a slight V-shape torso, smaller head, small forward lean from the hips, slight knee bend, forward-shifted athletic stance ready for pole power, neutral head position, modern classic roller skis with shorter slimmer aluminium frames, larger visible wheels, heel-close rear wheel placement, realistic binding placement, and longer slender poles
- Larger on-screen skier presence with a scaled visual rig, an extra 25% runtime presence boost, and a higher simulator-style follow camera that keeps the road horizon visible
- Stable SkiErg-inspired double-poling animation with fixed body-part pivots, safe torso lean, closer arms, reduced outward swing, parallel pole motion, and speed-scaled cycle timing
- A lightweight workout session flow that starts on Play, tracks elapsed time, distance, average speed, and max speed, stops the player at 5.0 km, then shows a finish screen with Restart and Return to start buttons
- A smoother speed-responsive camera with a 15% higher view, preserved look-ahead behavior, stronger speed-based field of view, longer road readability, and no camera shake
- A lightweight player input abstraction where keyboard input is the current source and future PM5 input can be added without changing movement logic
- `W` increases forward speed
- `S` decreases forward speed
- A top-left TextMeshPro HUD showing speed in km/h, distance in km, elapsed time, and current gradient
- EditMode tests for speed, distance, HUD formatting, gradient HUD creation, camera FOV, focused player camera framing, camera look-ahead, camera shake, dramatic course path behavior, major climb profile, gradient-based speed reduction, environment clearance, visible mountain placement, rounded mountain range mesh shape, skier screen presence, realistic skier silhouette, improved skier proportions, Vasalopp-style athletic torso shape, athletic lower body stance, proper roller skier model replacement, modern slim classic roller ski proportions, classic roller ski wheel and binding placement, refined pole proportions, stable double-poling animation hierarchy, narrowed arm technique, workout session flow, session completion buttons, stronger speed feeling runtime updates, road-edge flow cues, player input abstraction, and roller skier animation behavior

## Open in Unity

1. Clone this repository.
2. Open the repository folder with Unity Hub.
3. Use Unity 6.4 or newer.
4. Open any empty scene or create a new one.
5. Press Play.

The prototype scene is created automatically at runtime by `Assets/Scripts/SkiErgGameBootstrap.cs`, with `Assets/Scripts/ProperRollerSkierRuntimeUpdater.cs` replacing the older placeholder visual with the more recognizable roller skier, `Assets/Scripts/MountainRangeSceneUpdater.cs` replacing the placeholder mountains with Nordic low-poly mountain chains, `Assets/Scripts/SkierPresenceRuntimeUpdater.cs` increasing the skier's visual presence, `Assets/Scripts/WorkoutSessionController.cs` adding the lightweight workout session flow and finish screen, `Assets/Scripts/SpeedFeelingRuntimeUpdater.cs` improving camera composition with stronger speed-based FOV plus denser roadside and road-edge motion cues, `Assets/Scripts/GradientHudRuntimeUpdater.cs` adding the current gradient HUD line, and `Assets/Scripts/KeyboardPlayerInputSource.cs` providing today's keyboard controls through the input abstraction used by `Assets/Scripts/PlayerSpeedController.cs`.

## Controls

- `W`: Increase speed
- `S`: Decrease speed

## Tests

Open Unity Test Runner and run EditMode tests. The tests are in `Assets/Tests/EditMode`.
