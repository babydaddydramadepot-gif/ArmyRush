# DEVLOG.md

# ArmyRush Development Log

Version: 1.0
Status: Active

---

# Purpose

This document serves as the official development history for ArmyRush.

Every major implementation, system change, architectural decision, optimization pass, bug fix, content addition, balancing update, and production milestone must be recorded here.

This document acts as long-term project memory.

When uncertainty exists regarding previous work, consult this file.

---

# Logging Rules

Every meaningful change must be logged.

A log entry should be added whenever:

* New system is completed
* Existing system is refactored
* Architecture changes
* Major bugs are fixed
* New content is added
* New levels are added
* UI changes significantly
* Performance improvements are completed
* Audio/VFX passes are completed

---

# Entry Format

Date:

System:

Files:

Summary:

Result:

Follow Up:

Example:

---

Date:

2026-05-29

System:

Crowd Combat

Files:

CrowdCombatManager.cs

EnemyCombatResolver.cs

Summary:

Implemented crowd-vs-crowd combat calculations.

Result:

Crowds now resolve encounters correctly.

Follow Up:

Add combat VFX.

---

# Project Status

Current Build Stage:

Production Vertical Slice Foundation

Current Focus:

Core gameplay feel, manual QA, iOS device readiness, and vertical-slice polish

Completion Estimate:

23%

Last Updated:

2026-05-30

---

Date:

2026-05-30

System:

Documentation Read Pass and Repository Audit

Files:

Docs/GAME_VISION.md
Docs/TECH_ARCHITECTURE.md
Docs/GAMEPLAY_LOOP.md
Docs/Visual_STYLE.md
Docs/LEVEL_DESIGN.md
Docs/PROGRESSION.md
Docs/UI_UX.md
Docs/MONETIZATION.md
Docs/CONTENT_PIPELINE.md
Docs/AI_AGENT_RULES.md
Docs/QUALITY_BAR.md
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Completed the required source-of-truth read pass before implementation. Audited the Unity project and confirmed it is a fresh Unity 6 URP template with Input System installed, one SampleScene, no ArmyRush gameplay code, no production scenes, and no vertical-slice content yet.

Result:

Implementation plan added to `TASKS.md`. Folder-structure conflict resolved by hierarchy: new work will use `Assets/_Project/` from `TECH_ARCHITECTURE.md` rather than the lower-priority `_ArmyRush` variant in `TASKS.md`.

Follow Up:

Create project structure, runtime architecture, generated assets, Boot/MainMenu/Game scenes, data-driven levels, portrait iOS settings, and validate compilation in Unity batch mode.

---

Date:

2026-05-30

System:

Production Foundation and First Playable Runner Pass

Files:

Assets/_Project/
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/ProjectSettings.asset
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Created the `Assets/_Project/` production structure and added modular runtime systems for service bootstrapping, save data, economy, upgrades, touch/mouse input, player auto-run movement, camera follow, pooled crowd units, gates, projectile targeting, enemies, obstacles, level spawning, finish/reward flow, safe-area UI, menu UI, upgrade UI, audio hooks, and haptics hooks. Added an editor builder that generated URP-ready materials, procedural low-poly mesh assets, reusable prefabs, Boot/MainMenu/Game scenes, global tuning, eight upgrade definitions, and 20 data-driven level assets.

Result:

Unity batch mode successfully executed `ArmyRushProjectBuilder.BuildProductionFoundation` with no compiler errors or warnings. Build settings now include Boot, MainMenu, and Game scenes. Player settings are portrait-only with iPhone/iPad targeting and the `com.armyrush.game` bundle identifier.

Follow Up:

Run manual play-mode QA in Unity, tune lane/gate collision feel, add boss-specific behavior, add pooled VFX manager and final VFX assets, add real audio clips, improve UI reward animations, validate all 20 levels, and perform iOS/Xcode export testing.

---

Date:

2026-05-30

System:

Foundation Validation

Files:

Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Scripts/Gates/GateController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a repeatable editor validation method that checks generated prefabs and Boot/MainMenu/Game scenes for missing scripts, verifies required scene roots, builds the current level in edit-mode validation, and confirms gates, enemy groups, obstacles, finish trigger, pool, UI, player, and starting crowd are present.

Result:

`ArmyRushProjectBuilder.ValidateProductionFoundation` passed in Unity batch mode with no compiler errors or warnings. Gate visual color updates now use `MaterialPropertyBlock` so validation does not dirty shared material assets.

Follow Up:

Manual play-mode QA is still required to validate feel, timing, touch ergonomics, and complete run flow.

---

Date:

2026-05-30

System:

Floating Feedback VFX

Files:

Assets/_Project/Scripts/VFX/FloatingText.cs
Assets/_Project/Scripts/VFX/VfxManager.cs
Assets/_Project/Scripts/Gates/GateController.cs
Assets/_Project/Scripts/Combat/Damageable.cs
Assets/_Project/Scripts/Enemies/EnemyGroup.cs
Assets/_Project/Scripts/Level/RunManager.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Prefabs/VFX/PF_FloatingText.prefab

Summary:

Added pooled floating text feedback for gate math, projectile damage, enemy clears, and coin rewards. Regenerated the Game scene so `VfxManager` and `PF_FloatingText` are wired into the runtime pool.

Result:

Unity batch generation and validation both pass with no compiler errors or warnings. Gameplay interactions now have readable on-screen feedback instead of only hidden count math.

Follow Up:

Add final particle effects for hit sparks, gate bursts, coin bursts, obstacle debris, victory fireworks, and boss explosions.

---

Date:

2026-05-30

System:

Tank Boss Foundation

Files:

Assets/_Project/Scripts/Bosses/BossController.cs
Assets/_Project/Scripts/Level/LevelManager.cs
Assets/_Project/Scripts/UI/GameplayUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Prefabs/Bosses/PF_Boss_Tank.prefab
Assets/_Project/ScriptableObjects/Levels/SO_Level_005.asset
Assets/_Project/ScriptableObjects/Levels/SO_Level_010.asset
Assets/_Project/ScriptableObjects/Levels/SO_Level_015.asset
Assets/_Project/ScriptableObjects/Levels/SO_Level_020.asset

Summary:

Added a first tank boss implementation. Boss levels now spawn a generated low-poly tank boss instead of using only a large enemy crowd. The boss is a damageable target with world health text, contact pressure, defeat feedback, and HUD boss health bar events.

Result:

Unity batch generation and validation pass with no compiler errors or warnings. Levels 5, 10, 15, and 20 now contain a boss-specific runtime object and boss health UI support.

Follow Up:

Add unique boss attacks, boss reward tuning, boss-specific SFX/VFX, and manual balance testing.

---

Date:

2026-05-30

System:

Runtime Audio Feedback

Files:

Assets/_Project/Scripts/Audio/AudioService.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Upgraded the audio hook service from silent call sites to audible runtime feedback. The service now lazily creates a small persistent AudioSource pool and procedural mobile-friendly clips for button taps, gates, shooting, hits, enemy defeat, destruction, coin rewards, upgrades, victory, and defeat.

Result:

Unity batch validation passes with no compiler errors or warnings. Core interactions now produce audio feedback without requiring external audio assets or third-party packages.

Follow Up:

Replace procedural clips with final authored SFX/music assets during the audio polish pass.

---

Date:

2026-05-30

System:

iOS Development Export

Files:

Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added `ArmyRushProjectBuilder.BuildIOSDevelopmentExport`, a repeatable Unity batch-mode export path for the iOS development build. The method configures build scenes, portrait/iPhone+iPad player settings, and exports the Unity project to an Xcode project folder.

Result:

Unity successfully exported the iOS Xcode project to `ArmyRush_iOSBuild` with build result `Success`. Signing, Xcode archive/build, simulator launch, and physical device deployment are still pending.

Follow Up:

Open/build the generated Xcode project with signing configured, then test on a physical iPhone.

---

Date:

2026-05-30

System:

Xcode Build Validation

Files:

ArmyRush_iOSBuild/
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Built the generated Xcode project with `xcodebuild -project ArmyRush_iOSBuild/Unity-iPhone.xcodeproj -scheme Unity-iPhone -configuration Debug -sdk iphonesimulator CODE_SIGNING_ALLOWED=NO build`. Xcode selected the iPhoneOS SDK/destination from the Unity export and compiled the IL2CPP output, UnityFramework, and app target.

Result:

The Xcode build completed with `** BUILD SUCCEEDED **`. Unity symbol upload reported missing Unity Cloud Diagnostics credentials because `USYM_UPLOAD_AUTH_TOKEN` is not present, but this did not fail the app build.

Follow Up:

Run simulator launch if a compatible simulator destination is available, configure Apple signing for physical-device deployment, and perform a real iPhone playthrough.

---

Date:

2026-05-30

System:

Boss Combat and Upgrade Scaling

Files:

Assets/_Project/Scripts/Bosses/BossDefinition.cs
Assets/_Project/Scripts/Bosses/BossController.cs
Assets/_Project/Scripts/Level/LevelData.cs
Assets/_Project/Scripts/Level/LevelManager.cs
Assets/_Project/Scripts/Level/RunManager.cs
Assets/_Project/Scripts/Combat/PlayerCombatController.cs
Assets/_Project/Scripts/Player/PlayerController.cs
Assets/_Project/Scripts/Audio/AudioService.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/ScriptableObjects/Bosses/SO_Boss_Tank.asset
Assets/_Project/ScriptableObjects/Levels/
Assets/_Project/Scenes/Game.unity

Summary:

Added data-driven boss definitions and connected boss levels to a tank boss definition. Boss contact now pauses forward movement into a combat beat while preserving lateral steering, player volleys continue during the pause, the tank telegraphs cannon attacks, failed dodges remove soldiers, boss defeat resumes the run, and boss levels add a boss coin bonus. Combat upgrades now actively scale boss damage, obstacle damage, critical chance, and critical damage.

Result:

Unity generation and production validation both pass with no C# compiler errors or warnings. Boss levels now behave as distinct encounters instead of pass-through health targets.

Follow Up:

Tune boss damage/health on device, add authored boss animations, polish boss explosion VFX styling, add additional boss variants, and expose late-game upgrade buttons in the UI.

---

Date:

2026-05-30

System:

Pooled Gameplay VFX

Files:

Assets/_Project/Scripts/VFX/PooledParticleVfx.cs
Assets/_Project/Scripts/VFX/VfxManager.cs
Assets/_Project/Scripts/Combat/Damageable.cs
Assets/_Project/Scripts/Gates/GateController.cs
Assets/_Project/Scripts/Level/RunManager.cs
Assets/_Project/Scripts/Bosses/BossController.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Prefabs/VFX/
Assets/_Project/Art/Materials/MAT_VFXParticle.mat
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added pooled particle VFX support with generated prefabs for hit sparks, positive gate bursts, negative gate bursts, coin reward bursts, and boss explosions. `VfxManager` now exposes cue-based particle spawning alongside floating text, and core gameplay events call those cues.

Result:

Unity generation and production validation pass with no C# compiler errors or warnings. Combat, gates, rewards, and boss defeat now have pooled visual effects instead of relying only on text feedback.

Follow Up:

Add muzzle flashes, obstacle debris, victory fireworks, defeat fade, final authored VFX styling, and device performance tuning for particle density.

---

Date:

2026-05-30

System:

Settings UI

Files:

Assets/_Project/Scripts/UI/SettingsPanelUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Scenes/MainMenu.unity
Assets/_Project/Scenes/Game.unity
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a reusable settings panel to the main menu and gameplay HUD. The panel exposes an SFX volume slider and haptics toggle backed by `PlayerSaveData`, persists changes through `SaveService`, and is generated into both production scenes with safe-area placement.

Result:

Unity generation and production validation pass with no C# compiler errors or warnings. Audio and haptics settings are now accessible to players instead of existing only in save data.

Follow Up:

Add music controls after music exists, add privacy/terms/restore entries for monetization readiness, and device-test the panel on iPhone and iPad safe areas.

---

Date:

2026-05-30

System:

Runtime Performance Baseline

Files:

Assets/_Project/Scripts/Core/GameBootstrapper.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added bootstrap-time runtime performance configuration for mobile play: vSync is disabled, `Application.targetFrameRate` is set to 60, device sleep is disabled during play sessions, and multitouch remains enabled for mobile input.

Result:

Unity production validation passes with no C# compiler errors or warnings. The app now explicitly targets the documented 60 FPS mobile baseline at runtime.

Follow Up:

Profile real iPhone frame time, crowd scaling, projectile pooling, VFX pooling, draw calls, and thermal behavior.

---

Date:

2026-05-30

System:

Bonus Run Reward Section

Files:

Assets/_Project/Scripts/Level/BonusCrateController.cs
Assets/_Project/Scripts/Level/BonusEndTrigger.cs
Assets/_Project/Scripts/Level/LevelData.cs
Assets/_Project/Scripts/Level/LevelManager.cs
Assets/_Project/Scripts/Level/RunManager.cs
Assets/_Project/Scripts/Level/FinishLineTrigger.cs
Assets/_Project/Scripts/Combat/Damageable.cs
Assets/_Project/Scripts/Combat/PlayerCombatController.cs
Assets/_Project/Scripts/Combat/TargetRegistry.cs
Assets/_Project/Scripts/Core/GameTypes.cs
Assets/_Project/Scripts/Player/PlayerController.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Prefabs/Levels/PF_BonusCrate.prefab
Assets/_Project/Prefabs/Levels/PF_BonusEnd.prefab
Assets/_Project/ScriptableObjects/Levels/
Assets/_Project/Scenes/Game.unity
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Converted the finish line from immediate victory into a no-fail bonus run. Level data now controls bonus crate count, crate health, crate rewards, and bonus section length. The level builder extends the track beyond the finish, spawns generated reward crates, then resolves victory at a claim trigger. Player movement and shooting continue through `FinishSequence`, bonus crates register as lower-priority combat targets, and destroyed crates add pooled coin VFX/audio plus extra coins into the final payout.

Result:

Unity generation and production validation both pass with no C# compiler errors or warnings. Validation now confirms spawned bonus crates and the bonus end trigger are present in the generated Game scene.

Follow Up:

Manual play-mode QA needs to tune crate health, section pacing, camera framing beyond the finish line, and reward satisfaction on device.

---

Date:

2026-05-30

System:

Full Upgrade Menu Exposure

Files:

Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Scenes/MainMenu.unity
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Expanded the generated main menu upgrade area from four wide buttons to a compact two-column grid exposing all eight progression categories: Damage, Fire Rate, Starting Soldiers, Coin Bonus, Boss Damage, Obstacle Damage, Critical Chance, and Critical Damage. The production validator now checks that the generated MainMenu scene has at least one upgrade button per defined `UpgradeType`.

Result:

Unity generation and production validation both pass with no C# compiler errors or warnings. All combat and economy upgrade definitions are now reachable from the menu instead of existing only in data and gameplay calculations.

Follow Up:

Add purchase animation juice, improve unaffordable/max-state presentation, and device-test text fit across iPhone and iPad safe areas.

---

# Completion Tracking

Gameplay Systems

36%

UI Systems

29%

Visual Systems

25%

Audio Systems

23%

Level Systems

31%

Progression Systems

42%

Boss Systems

28%

Optimization

15%

Polish

13%

Release Readiness

8%

---

# Milestone Roadmap

Milestone 1

Core Gameplay Functional

Status:

Not Started

---

Milestone 2

Core Loop Complete

Status:

Not Started

---

Milestone 3

Visual Polish Pass

Status:

Not Started

---

Milestone 4

Progression Complete

Status:

Not Started

---

Milestone 5

Content Complete

Status:

Not Started

---

Milestone 6

Optimization Complete

Status:

Not Started

---

Milestone 7

Release Candidate

Status:

Not Started

---

# Gameplay Log

All gameplay-related changes belong here.

---

## Initial State

First functional pass implemented for runner movement, crowd count, gates, shooting, enemy groups, obstacles, finish, rewards, and run state. Manual play-mode QA remains pending.

---

# UI Log

All interface-related changes belong here.

---

## Initial State

First functional pass implemented for main menu, upgrade buttons, gameplay HUD, victory panel, defeat panel, and safe-area fitting.

---

# Visual Log

All visual changes belong here.

---

## Initial State

Generated procedural low-poly materials, meshes, prefabs, road/ocean environment, soldiers, enemies, gates, obstacles, projectile tracers, and finish line. Final art polish and VFX pass remain pending.

---

# Audio Log

All audio changes belong here.

---

## Initial State

Centralized audio cue hooks and pooled procedural runtime SFX implemented. Final authored clips and music remain pending.

---

# VFX Log

All effects-related changes belong here.

---

## Initial State

Basic projectile tracer, gate pulse feedback, pooled floating text, damage feedback, clear feedback, and coin reward text implemented. Final particle assets remain pending.

---

# Progression Log

All progression updates belong here.

---

## Initial State

Save, economy, upgrade definitions, upgrade purchasing, current level progression, and coin rewards implemented.

---

# Level Design Log

All level-related work belongs here.

---

## Initial State

20 authored `LevelData` assets generated with gates, enemies, obstacles, finish distances, escalating rewards, and boss-level flags.

---

# Optimization Log

All performance improvements belong here.

---

## Initial State

No optimization work completed.

---

# Bug Fix Log

All bug fixes must be recorded.

---

## Initial State

No bug fixes recorded.

---

# Known Issues

This section tracks unresolved issues.

---

Issue ID:

AR-001

Status:

Manual Unity play-mode QA has not been completed yet after generated scene creation, including the new finish-line bonus section.

Issue ID:

AR-002

Status:

Boss levels now have a data-driven tank boss, combat pause, telegraphed cannon attacks, boss rewards, and boss audio/haptics, but authored boss animations, tuned boss balance, boss VFX, and additional boss variants are still pending.

Issue ID:

AR-003

Status:

Audio and haptic call sites exist with pooled procedural SFX playback including boss attack/defeat cues, but final authored SFX and music are not implemented yet.

Issue ID:

AR-004

Status:

Final VFX assets are not complete; projectile tracers, floating combat text, gate bursts, hit sparks, coin bursts, bonus crate reward bursts, boss attack telegraphs, and boss explosion bursts are present, but muzzle flashes, obstacle debris, victory effects, defeat fade, and final authored styling remain pending.

Issue ID:

AR-005

Status:

Generated Xcode project builds successfully with signing disabled, but simulator/device launch QA and physical iPhone deployment are still pending.

Issue ID:

AR-006

Status:

Unity Cloud Diagnostics symbol upload reports missing `USYM_UPLOAD_AUTH_TOKEN`; this is optional for local development builds and only needs configuration if cloud symbol upload is enabled.

---

# Technical Debt

Track shortcuts, temporary solutions, and future refactors.

---

## Initial State

`TASKS.md` still contains an older `_ArmyRush` folder skeleton. Implementation uses `Assets/_Project/` according to the higher-priority `TECH_ARCHITECTURE.md`.

---

# Release Blockers

Any issue preventing release must be listed here.

---

Release Blocker ID:

RB-001

Status:

Physical iPhone deployment and full-device playthrough have not been completed yet.

---

# Future Ideas

Potential future improvements.

Examples:

Additional environments

Additional bosses

Weapon variants

Seasonal content

Prestige system

Events

Clan system

Advanced cosmetics

---

# Codex Instructions

After every major implementation:

1. Update TASKS.md
2. Update DEVLOG.md
3. Record affected files
4. Record completion status
5. Record follow-up work
6. Record known issues

Never leave major work undocumented.

Never mark tasks complete without updating this file.

This document serves as the historical source of truth for ArmyRush development.
