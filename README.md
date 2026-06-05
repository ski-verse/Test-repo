# Ski-Verse SkiErg Prototype

A small Unity prototype for a SkiErg-inspired movement game.

## What is included

- A runtime-generated 5 km road with clearly visible, realistic training-course curves
- White edge lines and dashed center road markings that follow the curve
- Roadside speed posts every 25 meters
- Green grass shoulders on both sides of the road
- More simple low-poly trees placed along both sides of the course
- Obvious low-poly rolling hills close to the road
- A much larger distant mountain backdrop
- Start and finish gates with road line markers
- A better-proportioned primitive placeholder skier with torso, head, arms, legs, skis, and poles
- A smoother speed-responsive camera with noticeable speed-based field of view, high-speed look-ahead, and light camera shake
- `W` increases forward speed
- `S` decreases forward speed
- A top-left TextMeshPro HUD showing speed in km/h and distance in km
- EditMode tests for speed, distance, HUD formatting, camera FOV, camera look-ahead, camera shake, and course path behavior

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
