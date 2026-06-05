# Ski-Verse SkiErg Prototype

A small Unity prototype for a SkiErg-inspired movement game.

## What is included

- A runtime-generated 5 km road with large, clearly visible sweeping training-course turns
- Visible uphill and downhill road sections generated from the course profile
- White edge lines and dashed center road markings that follow the curves and slopes
- At least 15 meters of open terrain on each side of the road before trees, hills, or mountains begin
- Roadside speed posts every 25 meters
- Low-poly turn warning signs placed before upcoming bends
- Green open grass shoulders on both sides of the road
- Set-back simple low-poly trees placed outside the open road margin
- Rolling low-poly hills moved further away from the road
- A distant mountain backdrop built from lightweight low-poly meshes only as background scenery
- Start and finish gates with road line markers
- A proper low-poly roller skier with realistic proportions, parallel roller skis, wheels, poles, helmet, and double-poling stance
- Synchronized double-poling arm and pole animation that works with the current movement controller
- A smoother speed-responsive camera with noticeable speed-based field of view, longer high-speed look-ahead, and light camera shake
- `W` increases forward speed
- `S` decreases forward speed
- A top-left TextMeshPro HUD showing speed in km/h and distance in km
- EditMode tests for speed, distance, HUD formatting, camera FOV, camera look-ahead, camera shake, dramatic course path behavior, environment clearance, and roller skier animation behavior

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
