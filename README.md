# Ski-Verse SkiErg Prototype

A small Unity prototype for a SkiErg-inspired movement game.

## What is included

- A runtime-generated 5 km road with large, clearly visible sweeping training-course turns
- Visible uphill and downhill road sections generated from the course profile
- White edge lines and dashed center road markings that follow the curves and slopes
- Roadside speed posts every 25 meters
- Low-poly turn warning signs placed before upcoming bends
- Green grass shoulders on both sides of the road
- Dense simple low-poly trees placed along both sides of the course
- Obvious low-poly rolling hills close to the road
- A huge 3x mountain backdrop built from lightweight low-poly meshes
- Start and finish gates with road line markers
- A better-proportioned primitive placeholder skier with torso, head, arms, legs, skis, and poles
- A smoother speed-responsive camera with noticeable speed-based field of view, longer high-speed look-ahead, and light camera shake
- `W` increases forward speed
- `S` decreases forward speed
- A top-left TextMeshPro HUD showing speed in km/h and distance in km
- EditMode tests for speed, distance, HUD formatting, camera FOV, camera look-ahead, camera shake, and dramatic course path behavior

## Open in Unity

1. Clone this repository.
2. Open the repository folder with Unity Hub.
3. Use Unity 6.4 or newer.
4. Open any empty scene or create a new one.
5. Press Play.

The prototype scene is created automatically at runtime by `Assets/Scripts/SkiErgGameBootstrap.cs`.

## Controls

- `W`: Increase speed
- `S`: Decrease speed

## Tests

Open Unity Test Runner and run EditMode tests. The tests are in `Assets/Tests/EditMode/PlayerSpeedControllerTests.cs`.
