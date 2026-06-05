# Ski-Verse SkiErg Prototype

A small Unity prototype for a SkiErg-inspired movement game.

## What is included

- A runtime-generated 5 km road with large, clearly visible sweeping training-course turns
- Visible uphill and downhill road sections generated from the course profile
- White edge lines and dashed center road markings that follow the curves and slopes
- At least 15 meters of open terrain on each side of the road before trees, hills, forests, or mountains begin
- Roadside speed posts every 25 meters
- Low-poly turn warning signs placed before upcoming bends
- Green open grass shoulders on both sides of the road
- Set-back simple low-poly trees placed outside the open road margin
- Rolling low-poly hills moved further away from the road
- Distant low-poly forest bands between the road corridor and mountain ranges
- Rounded low-poly Scandinavian mountain chains with reduced height, smoother silhouettes, and a supporting horizon role while staying outside the road corridor
- Start and finish gates with road line markers
- A proper low-poly roller skier with realistic proportions, parallel roller skis, wheels, poles, helmet, and double-poling stance
- Larger on-screen skier presence with a scaled visual rig, an extra 25% runtime presence boost, and a higher simulator-style follow camera that keeps the road horizon visible
- Stable SkiErg-inspired double-poling animation with fixed body-part pivots, safe torso lean, closer arms, reduced outward swing, parallel pole motion, and speed-scaled cycle timing
- A lightweight workout session flow that starts on Play, tracks elapsed time, distance, average speed, and max speed, then shows a 5.0 km finish summary with a restart button
- A smoother speed-responsive camera with noticeable speed-based field of view, longer high-speed look-ahead, higher composition, and light camera shake
- `W` increases forward speed
- `S` decreases forward speed
- A top-left TextMeshPro HUD showing speed in km/h, distance in km, and elapsed time
- EditMode tests for speed, distance, HUD formatting, camera FOV, focused player camera framing, camera look-ahead, camera shake, dramatic course path behavior, environment clearance, visible mountain placement, rounded mountain range mesh shape, skier screen presence, stable double-poling animation hierarchy, narrowed arm technique, workout session flow, and roller skier animation behavior

## Open in Unity

1. Clone this repository.
2. Open the repository folder with Unity Hub.
3. Use Unity 6.4 or newer.
4. Open any empty scene or create a new one.
5. Press Play.

The prototype scene is created automatically at runtime by `Assets/Scripts/SkiErgGameBootstrap.cs`, with `Assets/Scripts/MountainRangeSceneUpdater.cs` replacing the placeholder mountains with Nordic low-poly mountain chains, `Assets/Scripts/SkierPresenceRuntimeUpdater.cs` increasing the skier's visual presence, and `Assets/Scripts/WorkoutSessionController.cs` adding the lightweight workout session flow.

## Controls

- `W`: Increase speed
- `S`: Decrease speed

## Tests

Open Unity Test Runner and run EditMode tests. The tests are in `Assets/Tests/EditMode`.
