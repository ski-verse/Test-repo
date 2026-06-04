# SkiErg Prototype Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a minimal Unity SkiErg prototype with a road, player, trailing camera, and W/S speed controls.

**Architecture:** Keep the gameplay in small MonoBehaviour scripts. `PlayerSpeedController` handles speed and movement, `FollowCamera` handles camera tracking, and `SkiErgGameBootstrap` creates a playable prototype scene automatically at runtime.

**Tech Stack:** Unity C#, Unity Test Framework, EditMode tests.

---

### Task 1: Speed Controller Tests

**Files:**
- Create: `Assets/Tests/EditMode/PlayerSpeedControllerTests.cs`

- [ ] Create EditMode tests for increasing speed, decreasing speed, clamping speed, and calculating forward movement.
- [ ] Expected verification in Unity: tests fail before `PlayerSpeedController` exists.

### Task 2: Player Movement

**Files:**
- Create: `Assets/Scripts/PlayerSpeedController.cs`

- [ ] Implement configurable `acceleration`, `deceleration`, `maxSpeed`, `minSpeed`, and `CurrentSpeed`.
- [ ] Implement `IncreaseSpeed`, `DecreaseSpeed`, and `CalculateNextPosition` so tests pass.
- [ ] Implement `Update` to read W/S and move the transform forward.

### Task 3: Camera Follow

**Files:**
- Create: `Assets/Scripts/FollowCamera.cs`

- [ ] Implement a smooth camera follow behaviour using a target transform, offset, and follow speed.

### Task 4: Runtime Prototype Scene

**Files:**
- Create: `Assets/Scripts/SkiErgGameBootstrap.cs`
- Create: `ProjectSettings/ProjectVersion.txt`
- Create: `README.md`

- [ ] Create road, player capsule, trailing camera, and light at runtime.
- [ ] Attach `PlayerSpeedController` to the player and `FollowCamera` to the camera.
- [ ] Document how to open the project and run the prototype.

### Verification

Run the Unity Test Runner EditMode tests after opening the repository as a Unity project. In this environment Unity is not available, so verification is limited to code review and repository creation through the GitHub connector.
