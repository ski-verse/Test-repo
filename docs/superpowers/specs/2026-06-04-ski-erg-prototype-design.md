# SkiErg Prototype Design

## Goal
Create a tiny Unity prototype for a SkiErg-inspired movement game.

## Gameplay
The player is represented by a simple capsule moving forward on a straight road. Pressing W increases forward speed, and pressing S decreases forward speed. Speed is clamped between configurable minimum and maximum values so the player cannot move backward or accelerate forever.

## Scene Setup
A runtime bootstrap component creates the prototype objects automatically when play mode starts:

- a long road cube
- a capsule player
- a camera positioned behind and above the player
- a directional light

This avoids fragile hand-authored Unity scene YAML while the project is still a small prototype.

## Components
`PlayerSpeedController` owns speed changes and forward movement. It exposes small methods that can be tested without requiring a full scene.

`FollowCamera` keeps the camera behind the player with a configurable offset and smooth follow speed.

`SkiErgGameBootstrap` builds the default prototype scene if no player already exists.

## Testing
EditMode tests cover speed increase, speed decrease, speed clamping, and forward-position calculation. Unity Test Runner can run these tests from the Unity editor.
