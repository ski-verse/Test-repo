# Ski-Verse SkiErg Prototype

A small Unity prototype for a SkiErg-inspired movement game.

## What is included

- A runtime-generated 5 km straight road
- White edge lines and dashed center road markings
- Green grass strips on both sides of the road
- Simple trees placed along both sides of the course
- Start and finish gates with road line markers
- A primitive placeholder skier model with body, head, skis, and poles
- A speed-responsive camera that follows behind the skier and widens FOV at higher speed
- `W` increases forward speed
- `S` decreases forward speed
- A top-left TextMeshPro HUD showing speed in km/h and distance in km
- EditMode tests for speed, distance, HUD formatting, and camera FOV behavior

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
