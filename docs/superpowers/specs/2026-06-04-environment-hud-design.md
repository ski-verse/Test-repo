# Environment and HUD Design

## Goal
Extend the SkiErg prototype with a simple outdoor environment, a placeholder skier model, a 5 km road, and live speed/distance UI.

## Environment
The runtime bootstrap creates a straight 5,000 meter road. The road remains a simple dark cube so the prototype stays lightweight. Two wide green grass strips run along the full road length, one on each side.

## Player
The capsule is replaced by a placeholder skier made from Unity primitives. The skier has a body, head, two skis, and two poles. The movement controller stays on the root object so all visible parts move together.

## HUD
A world-independent UI canvas displays speed and distance in the top-left corner. Speed is shown in km/h using meters-per-second multiplied by 3.6. Distance is shown in km based on forward Z progress from the start position.

## Components
`PlayerSpeedController` keeps movement plus exposes `SpeedKmh` and `DistanceKm` for UI and tests.

`SpeedDistanceDisplay` reads a `PlayerSpeedController` and updates two text labels.

`SkiErgGameBootstrap` creates the 5 km road, grass strips, placeholder skier, follow camera, light, and HUD.

## Testing
EditMode tests cover speed conversion and distance conversion in addition to existing speed behavior.
