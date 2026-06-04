# Environment and HUD Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a simple 5 km outdoor environment, placeholder skier model, and HUD speed/distance display.

**Architecture:** Continue using runtime bootstrap objects so no fragile Unity scene YAML is needed. Keep movement state in `PlayerSpeedController`, UI formatting in `SpeedDistanceDisplay`, and object creation in `SkiErgGameBootstrap`.

**Tech Stack:** Unity C#, UnityEngine.UI, Unity Test Framework EditMode tests.

---

### Task 1: Extend Movement Tests

**Files:**
- Modify: `Assets/Tests/EditMode/PlayerSpeedControllerTests.cs`

- [ ] Add tests for `SpeedKmh` and `DistanceKm`.
- [ ] Expected Unity result before implementation: tests fail because the properties do not exist.

### Task 2: Extend Player Movement Data

**Files:**
- Modify: `Assets/Scripts/PlayerSpeedController.cs`

- [ ] Track starting forward position.
- [ ] Expose speed in km/h.
- [ ] Expose distance in km.

### Task 3: Add HUD Script

**Files:**
- Create: `Assets/Scripts/SpeedDistanceDisplay.cs`

- [ ] Read controller values each frame.
- [ ] Format speed as `Speed: 14.4 km/h`.
- [ ] Format distance as `Distance: 0.12 km`.

### Task 4: Expand Runtime Environment

**Files:**
- Modify: `Assets/Scripts/SkiErgGameBootstrap.cs`

- [ ] Make the road 5,000 meters long.
- [ ] Add grass strips on both sides.
- [ ] Replace capsule with a primitive placeholder skier hierarchy.
- [ ] Add a top-left HUD canvas.

### Task 5: Docs

**Files:**
- Modify: `README.md`

- [ ] Document grass, 5 km road, placeholder skier, and HUD.

### Verification

Open the repo in Unity 2022.3 LTS or newer, press Play, and run EditMode tests in Unity Test Runner. In this shell environment Unity is not installed, so final verification here is limited to fetching the written files back from GitHub.
