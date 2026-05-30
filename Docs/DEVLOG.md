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

59%

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
Assets/_Project/Scripts/UI/SettingsPanelUI.cs
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

Date:

2026-05-30

System:

Upgrade Purchase Feedback

Files:

Assets/_Project/Scripts/UI/UpgradeButtonView.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added clearer upgrade button state feedback. Affordable costs remain gold, unaffordable costs turn warm red with muted title/level text, maxed upgrades use a green `MAX` state, and successful purchases briefly flash the level/cost area with `BOUGHT` before returning to the next cost.

Result:

Unity production validation passes with no C# compiler errors or warnings. Upgrade purchases now provide visual confirmation in addition to audio and haptic feedback.

Follow Up:

Device-test text fit and perceived timing, then consider adding richer coin spend/count-up animation during the broader UI polish pass.

---

Date:

2026-05-30

System:

Core Gameplay VFX Expansion

Files:

Assets/_Project/Scripts/VFX/VfxManager.cs
Assets/_Project/Scripts/Combat/PlayerCombatController.cs
Assets/_Project/Scripts/Obstacles/ObstacleController.cs
Assets/_Project/Scripts/Level/RunManager.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Prefabs/VFX/PF_VFX_MuzzleFlash.prefab
Assets/_Project/Prefabs/VFX/PF_VFX_ObstacleDebris.prefab
Assets/_Project/Prefabs/VFX/PF_VFX_VictoryBurst.prefab
Assets/_Project/Scenes/Game.unity
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Expanded pooled gameplay VFX with generated prefabs and cues for muzzle flashes, obstacle debris, and victory celebration bursts. Player volleys now spawn a muzzle flash at the aim origin, destroyed obstacles emit debris, and victory now combines coin burst feedback with a separate celebration effect.

Result:

Unity generation and production validation both pass with no C# compiler errors or warnings. The validator now requires the new VFX prefabs alongside the existing hit, gate, coin, and boss effects.

Follow Up:

Add crowd add/loss bursts, upgrade glow, defeat fade, smoke variants, and final authored VFX styling during the broader visual polish pass.

---

Date:

2026-05-30

System:

Crowd Count VFX

Files:

Assets/_Project/Scripts/Crowd/CrowdManager.cs
Assets/_Project/Scripts/VFX/VfxManager.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Prefabs/VFX/PF_VFX_CrowdGain.prefab
Assets/_Project/Prefabs/VFX/PF_VFX_CrowdLoss.prefab
Assets/_Project/Scenes/Game.unity
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added dedicated pooled VFX cues and generated prefabs for crowd gain and crowd loss. `CrowdManager` now suppresses effects for the initial level spawn count, then emits green or red bursts on later count changes from gates, combat, obstacles, and boss attacks.

Result:

Unity generation and production validation both pass with no C# compiler errors or warnings. The validator now requires the crowd gain/loss VFX prefabs.

Follow Up:

Device-test density and readability during large gate multipliers and heavy combat, then add upgrade glow and defeat fade.

---

Date:

2026-05-30

System:

Result Panel Reveal

Files:

Assets/_Project/Scripts/UI/ResultPanelAnimator.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Scenes/Game.unity
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a reusable `ResultPanelAnimator` that gives victory and defeat panels a short unscaled-time fade and scale reveal. The generated Game scene now attaches it to both result panels through the editor builder.

Result:

Unity generation and production validation both pass with no C# compiler errors or warnings. End-of-run panels no longer appear as abrupt static overlays.

Follow Up:

Add reward count-up timing and richer defeat transition polish after device UI fit checks.

---

Date:

2026-05-30

System:

Victory Reward Count-Up

Files:

Assets/_Project/Scripts/UI/GameplayUI.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a short unscaled-time count-up animation for the victory coin reward text. `GameplayUI` now counts from zero to the earned coin total with eased timing, cancels stale count routines when leaving victory state, and preserves the final reward value at the end.

Result:

Unity production validation passes with no C# compiler errors or warnings. Victory rewards now animate instead of appearing as a static final number.

Follow Up:

Device-test pacing against typical early and boss-level payouts, then tune duration or add coin icon motion if needed.

---

Date:

2026-05-30

System:

Pool Prewarm Optimization

Files:

Assets/_Project/Scripts/Utility/PoolManager.cs
Assets/_Project/Scripts/Crowd/CrowdManager.cs
Assets/_Project/Scripts/Level/LevelManager.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Improved runtime pool preparation for mobile frame stability. `PoolManager.Prewarm` now tops up the inactive queue instead of duplicating counts on repeated calls. `LevelManager` prewarms starting crowd visuals and all level enemy visual units before spawning the run, and `CrowdManager` exposes a capped visual prewarm helper.

Result:

Unity production validation passes with no C# compiler errors or warnings. Level start now does less surprise instantiation for soldier and enemy visuals.

Follow Up:

Profile real device frame time during level load, large gate multipliers, and boss encounters to tune pool sizes.

---

Date:

2026-05-30

System:

Crowd Formation Update Optimization

Files:

Assets/_Project/Scripts/Crowd/CrowdManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Reduced per-frame crowd overhead by moving player formation target recalculation off the normal Update path. The crowd now recalculates soldier local target slots only when the visible soldier count changes, while individual pooled soldiers continue interpolating toward their assigned slots.

Result:

Unity production validation passes with no C# compiler errors or warnings. Player crowd runtime work now avoids a full visible-soldier list pass every frame during steady movement.

Follow Up:

Profile large gate multipliers on device and consider centralizing per-unit animation updates if soldier Update cost becomes the next frame-time bottleneck.

---

Date:

2026-05-30

System:

Generated Material Instancing Optimization

Files:

Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Assets/_Project/Art/Materials/MAT_PlayerBlue.mat
Assets/_Project/Art/Materials/MAT_PlayerNavy.mat
Assets/_Project/Art/Materials/MAT_EnemyRed.mat
Assets/_Project/Art/Materials/MAT_EnemyCrimson.mat
Assets/_Project/Art/Materials/MAT_StylizedSkin.mat
Assets/_Project/Art/Materials/MAT_TrackGray.mat
Assets/_Project/Art/Materials/MAT_RailWhite.mat
Assets/_Project/Art/Materials/MAT_OceanBlue.mat
Assets/_Project/Art/Materials/MAT_GatePositive.mat
Assets/_Project/Art/Materials/MAT_GateNegative.mat
Assets/_Project/Art/Materials/MAT_ProjectileYellow.mat
Assets/_Project/Art/Materials/MAT_CoinGold.mat
Assets/_Project/Art/Materials/MAT_ObstacleWood.mat
Assets/_Project/Art/Materials/MAT_ObstacleMetal.mat
Assets/_Project/Art/Materials/MAT_UIBlue.mat
Assets/_Project/Art/Materials/MAT_VFXParticle.mat
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Enabled GPU instancing on the generated shared material library and updated the project builder so regenerated materials keep instancing enabled. This keeps the repeated low-poly meshes aligned with the mobile draw-call reduction pass while preserving the existing shared-material workflow.

Result:

Unity production validation passes with no C# compiler errors or warnings. Generated material draw-call optimization is in place.

Follow Up:

Profile actual device batches in Xcode/Unity profiler and tune mesh/material grouping if repeated soldiers, obstacles, or track pieces still dominate rendering cost.

---

Date:

2026-05-30

System:

Pooling Validation Coverage

Files:

Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Expanded the production validator to verify pooling-critical prefabs and scene instances. Validation now checks pooled soldier, projectile, floating text, and serialized particle VFX prefabs for `PooledObject` plus their required runtime components, and checks spawned active pooled instances for a valid `PoolManager` owner after level construction.

Result:

Unity production validation passes with no C# compiler errors or warnings. Object-pooling verification is now codified in the Unity validation path.

Follow Up:

Add device profiler captures for pool growth under large crowd, projectile, VFX, and bonus-crate reward scenarios.

---

Date:

2026-05-30

System:

Settings Legal and Restore Placeholders

Files:

Assets/_Project/Scripts/UI/SettingsPanelUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added the remaining settings-screen placeholder controls required by the UI/UX and monetization docs. The settings panel now exposes Privacy and Terms buttons that open configurable placeholder URLs, plus a Restore Purchases placeholder that provides immediate button/audio/haptic feedback. Existing generated scenes create the controls at runtime when serialized references are absent, while future generated scenes receive wired controls from the project builder.

Result:

Unity production validation passes with no C# compiler errors or warnings. Settings now cover audio, haptics, legal link placeholders, and restore placeholder architecture.

Follow Up:

Replace placeholder legal URLs with live production policy URLs and connect Restore Purchases to a real IAP service if monetization integration is added.

---

Date:

2026-05-30

System:

Soldier and Enemy Hit Reactions

Files:

Assets/_Project/Scripts/Crowd/SoldierUnitVisual.cs
Assets/_Project/Scripts/Crowd/CrowdManager.cs
Assets/_Project/Scripts/Enemies/EnemyGroup.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added procedural hit flinch feedback to reusable soldier visuals. Crowd losses now trigger readable survivor reactions, and enemy groups play hit reactions on damaged units after health changes resolve. The implementation stays pool-safe, allocation-free during reactions, and shares the existing stylized unit visual path for player and enemy teams.

Result:

Unity production validation passes with no C# compiler errors or warnings. Soldier visuals now include run, shoot, hit, death, spawn, and victory gameplay animation coverage; enemy visuals now include hit reaction coverage.

Follow Up:

Add distinct enemy shoot animation behavior if ranged enemy attacks are introduced, and tune hit amplitude on device for readability.

---

Date:

2026-05-30

System:

Gate Glow and Variant Polish

Files:

Assets/_Project/Scripts/Gates/GateController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added idle glow pulsing to gate panels through reused material property blocks, with subtle label tinting for positive and negative gates. The existing data-driven positive/negative operation colors now read as intentional variants before activation, while the activation animation and VFX continue to own the high-impact moment after the player passes through.

Result:

Unity production validation passes with no C# compiler errors or warnings. Gate visuals now cover glow, activation animation, and positive/negative variant feedback without adding per-frame allocations or new scene dependencies.

Follow Up:

Tune glow strength on device after portrait readability QA, especially on smaller iPhones with dense obstacle layouts.

---

Date:

2026-05-30

System:

Obstacle Destruction Haptics

Files:

Assets/_Project/Scripts/Obstacles/ObstacleController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Connected obstacle destruction to the existing haptics service with a medium-impact cue. This completes the documented haptic coverage pass across gate pass, upgrade purchase, boss defeat, major explosions, and the settings-controlled enable/disable path.

Result:

Unity production validation passes with no C# compiler errors or warnings. Haptics remain device-QA pending because iOS vibration behavior cannot be confirmed from batch-mode validation.

Follow Up:

Verify haptic strength and throttling on a physical iPhone during obstacle-heavy levels.

---

Date:

2026-05-30

System:

Projectile Trail Feedback

Files:

Assets/_Project/Scripts/Combat/Projectile.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added projectile trail rendering that is configured once per pooled projectile and reset every time a shot is fired or released. Existing projectile prefabs gain the trail at runtime if needed, and newly generated projectile prefabs include the TrailRenderer from the project builder.

Result:

Unity production validation passes with no C# compiler errors or warnings. Auto-shooting feedback now covers projectile pooling, bullet trails, muzzle flashes, hit impacts, and damage-number feedback.

Follow Up:

Tune trail width and lifetime during device play-mode QA so dense volleys stay readable without visual clutter.

---

Date:

2026-05-30

System:

Deterministic Endless Level Generation

Files:

Assets/_Project/Scripts/Level/LevelManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Replaced authored-level wrapping with runtime deterministic LevelData generation after the highest authored level. Endless levels now scale track length, gate values, enemy counts, obstacle health, bonus crate rewards, and boss cadence from the requested level index while preserving the authored lane/chunk spawning path. Added production validation coverage for the first post-authored level preview.

Result:

Unity production validation passes with no C# compiler errors or warnings, including the new post-authored endless level preview check. Endless support now creates designed, repeatable post-authored levels instead of replaying earlier authored layouts.

Follow Up:

Add true boss-type rotation once helicopter and mech boss variants exist; the current endless path reuses the available tank boss definition.

---

Date:

2026-05-30

System:

Tank Boss Damage States

Files:

Assets/_Project/Scripts/Bosses/BossController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added two mid-fight boss damage thresholds at roughly two-thirds and one-third health. The tank now emits smoke, hit sparks, floating state callouts, a restrained impact shake, and medium haptic feedback when its armor cracks and when it enters critical damage.

Result:

Unity production validation passes with no C# compiler errors or warnings. Tank boss combat now has readable damage-state escalation before the existing defeat animation and explosion sequence.

Follow Up:

Tune threshold feedback during boss balance QA so the callouts remain visible during dense projectile volleys.

---

Date:

2026-05-30

System:

Obstacle Reward Payouts

Files:

Assets/_Project/Scripts/Obstacles/ObstacleController.cs
Assets/_Project/Scripts/Level/LevelManager.cs
Assets/_Project/Scripts/Level/RunManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added obstacle coin rewards that are configured from level/tuning context and paid into the run's pending reward pool when obstacles are destroyed. Obstacle clears now emit coin floating text and coin-burst VFX, and those earned coins are included in the final victory reward calculation.

Result:

Unity production validation passes with no C# compiler errors or warnings. The obstacle system now covers health, damage receiving, destruction, VFX/audio/haptic feedback, data-driven placement, and reward payouts.

Follow Up:

Balance obstacle reward values against run length and upgrade costs during device playtesting.

---

Date:

2026-05-30

System:

Upgrade Purchase Glow Feedback

Files:

Assets/_Project/Scripts/UI/UpgradeButtonView.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a reusable non-interactive glow layer to upgrade buttons and animated it during successful purchases. The upgrade feedback now combines button color pulse, bought text, level/cost flash, audio, haptics, and a short UI glow burst.

Result:

Unity production validation passes with no C# compiler errors or warnings. The documented upgrade glow VFX now has concrete runtime coverage.

Follow Up:

Tune glow size and alpha during device layout QA so dense upgrade panels remain readable.

---

Date:

2026-05-30

System:

Rewarded and Revive Placeholder UI

Files:

Assets/_Project/Scripts/UI/GameplayUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added future-ready rewarded/revive placeholder controls to the result panels without adding an ad SDK dependency. Victory now has a 2X Reward placeholder, defeat has a Revive placeholder, and both provide button audio, light haptics, and status text feedback when ads are unavailable.

Result:

Unity production validation passes with no C# compiler errors or warnings. Generated scenes receive wired placeholder controls from the builder, while existing scenes create or reposition the controls at runtime.

Follow Up:

Connect the buttons to a real rewarded-ad availability service if monetization is introduced after core gameplay/device QA.

---

Date:

2026-05-30

System:

Defeat Partial Reward Payout

Files:

Assets/_Project/Scripts/Level/RunManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Fixed defeat reward handling so coins earned before failure are not discarded. Combat/obstacle rewards accumulated during a run are now multiplied by the coin-reward upgrade, added to the economy on defeat, and shown by the defeat panel.

Result:

Unity production validation passes with no C# compiler errors or warnings. Defeat UI now has a real coins-earned path instead of only displaying zero unless the run reached victory.

Follow Up:

Tune whether partial defeat rewards should be generous or conservative once obstacle reward balance is tested on-device.

---

Date:

2026-05-30

System:

Procedural Unit Animation Polish

Files:

Assets/_Project/Scripts/Crowd/SoldierUnitVisual.cs
Assets/_Project/Scripts/Crowd/CrowdManager.cs
Assets/_Project/Scripts/Combat/PlayerCombatController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added pooled procedural animation polish for crowd and enemy units. Soldier visuals now reset cleanly on spawn, pop in from a small scale, kick their weapon/body during sampled firing volleys, and play a short detached death tumble before returning to the pool. Crowd shooting now triggers sampled unit recoil instead of only spawning muzzle flashes and projectiles.

Result:

Unity production validation passes with no C# compiler errors or warnings. Unit removal is no longer instant, combat has more readable motion, and pooling remains intact because death animation releases units back through `PooledObject`.

Follow Up:

Add explicit hit reactions, victory celebration poses, and authored animation replacements where final production assets require them.

---

Date:

2026-05-30

System:

Gate Punch Animation Polish

Files:

Assets/_Project/Scripts/Gates/GateController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added production-safe gate activation animation. Gates now use a cached `MaterialPropertyBlock` for activation color fades instead of runtime material instancing, pulse briefly when triggered, fade their panel and label into a used state, and reset transform/label state cleanly when reused or disabled.

Result:

Unity production validation passes with no C# compiler errors or warnings. Gate interactions now provide the documented punch animation without creating per-hit renderer materials.

Follow Up:

Tune gate punch amplitude during device QA and add authored glow/VFX styling in the final visual polish pass.

---

Date:

2026-05-30

System:

Boss Defeat Animation Polish

Files:

Assets/_Project/Scripts/Bosses/BossController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a short procedural tank boss defeat sequence. Boss defeat now disables the trigger, stops boss combat behavior, shakes the tank into a tilted sinking pose, fades the world health label, layers smoke during the tumble, resumes the run after the payoff beat, and then deactivates the boss cleanly.

Result:

Unity production validation passes with no C# compiler errors or warnings. Boss defeat no longer disappears instantly after VFX.

Follow Up:

Add boss damage-state reactions, tune boss balance on device, and replace procedural motion with authored boss animation clips if final art production supplies them.

---

Date:

2026-05-30

System:

Victory Celebration Animation Polish

Files:

Assets/_Project/Scripts/Crowd/SoldierUnitVisual.cs
Assets/_Project/Scripts/Crowd/CrowdManager.cs
Assets/_Project/Scripts/Level/RunManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Connected victory animation feedback to the real win flow. `RunManager.WinRun` now asks the crowd to celebrate when the run reaches victory, `CrowdManager` fans that request across active pooled soldiers, and soldier visuals play short staggered cheer hops with raised weapon motion while preserving their existing formation and pooling behavior.

Result:

Unity production validation passes with no C# compiler errors or warnings. Victory now has crowd character motion in addition to coins, VFX, audio, haptics, and camera shake.

Follow Up:

Tune the celebration against the victory panel framing on phone and iPad, then replace procedural cheer motion with authored clips if final character animation assets become available.

---

Date:

2026-05-30

System:

Safe Area Fitter Hardening

Files:

Assets/_Project/Scripts/UI/SafeAreaFitter.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Hardened the runtime safe-area fitter for iPhone and iPad layouts. The fitter now reapplies when screen dimensions change, clamps normalized safe-area anchors, handles zero-size startup frames defensively, and resets offsets after applying anchors.

Result:

Unity production validation passes with no C# compiler errors or warnings. Generated UI roots remain better prepared for notch, Dynamic Island, home indicator, and iPad safe-area changes.

Follow Up:

Run the documented device/simulator safe-area matrix to verify layout on actual target aspect ratios.

---

Date:

2026-05-30

System:

Result Screen Upgrade Shortcuts

Files:

Assets/_Project/Scripts/UI/GameplayUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added upgrade shortcut buttons to both victory and defeat result panels. Existing generated scenes create the shortcut buttons at runtime and resize the primary result action into a two-button layout, while future generated scenes receive serialized upgrade buttons from the builder. Runtime-created UI labels now preserve their default font if Unity's built-in font lookup is unavailable.

Result:

Unity production validation passes with no C# compiler errors or warnings. Win/loss flow now gives players a fast path back to upgrades instead of only next/retry.

Follow Up:

Add rewarded-ad revive/reward architecture once monetization services are selected.

---

Date:

2026-05-30

System:

Procedural Music and Settings Control

Files:

Assets/_Project/Scripts/Audio/AudioService.cs
Assets/_Project/Scripts/Core/GameBootstrapper.cs
Assets/_Project/Scripts/UI/SettingsPanelUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added procedural looping menu/game music, persisted music volume control, and a music slider in the settings panel. `AudioService` now owns a looping music source, `GameBootstrapper` starts music once services are available, and `SettingsPanelUI` creates the music slider at runtime for existing generated scenes while the builder wires it for future scenes.

Result:

Unity production validation passes with no C# compiler errors or warnings. Audio settings now cover both music and SFX without requiring external music assets.

Follow Up:

Replace the procedural loop with authored mobile-ready music and add legal/settings links before release readiness.

---

Date:

2026-05-30

System:

Smoke VFX Variants

Files:

Assets/_Project/Scripts/VFX/VfxManager.cs
Assets/_Project/Scripts/VFX/PooledParticleVfx.cs
Assets/_Project/Scripts/Obstacles/ObstacleController.cs
Assets/_Project/Scripts/Bosses/BossController.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added pooled smoke VFX cues for obstacle destruction and boss defeat. `VfxManager` now supports light and heavy smoke prefabs, creates runtime smoke templates for existing scenes, and exposes builder-created smoke prefabs for future generated scenes. Obstacle breaks spawn a dust puff, while boss defeats layer heavy smoke with offset smaller puffs.

Result:

Unity production validation passes with no C# compiler errors or warnings. The required VFX list now has a functional pass for smoke coverage without runtime allocation spikes at impact sites.

Follow Up:

Replace procedural smoke styling with authored mobile-optimized particle art during the final VFX pass.

---

Date:

2026-05-30

System:

Defeat Fade Overlay

Files:

Assets/_Project/Scripts/UI/GameplayUI.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a full-screen defeat fade that eases in behind the retry panel using unscaled time. `GameplayUI` now creates a resilient runtime overlay when older generated scenes do not have the serialized reference, and the scene builder wires the overlay for future generated scenes.

Result:

Unity production validation passes with no C# compiler errors or warnings. Defeat now has a clearer visual transition while keeping the retry controls readable and free of debug UI.

Follow Up:

Device-test the fade opacity against combat-heavy loss states, then tune the final defeat composition during the authored UI art pass.

---

Date:

2026-05-30

System:

Upgrade Glow Feedback

Files:

Assets/_Project/Scripts/UI/UpgradeButtonView.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added immediate upgrade purchase feedback directly to upgrade buttons. Buttons now pulse the cost and level text, show a short bought state, flash the button background on purchase, and use distinct blocked/maxed background states for faster readability.

Result:

Unity production validation passes with no C# compiler errors or warnings. Upgrade purchases now provide visible confirmation without adding debug UI or scene references.

Follow Up:

Device-test button readability on iPhone and iPad, then consider coin icon motion once the menu art pass is underway.

---

Date:

2026-05-30

System:

Camera Impact Feedback

Files:

Assets/_Project/Scripts/Camera/CameraFollowRig.cs
Assets/_Project/Scripts/Obstacles/ObstacleController.cs
Assets/_Project/Scripts/Bosses/BossController.cs
Assets/_Project/Scripts/Level/RunManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a cue-based camera shake API to the follow rig and connected it to high-impact gameplay moments. Obstacle destruction now produces a small shake, successful boss attacks produce a stronger hit shake, boss defeat produces a heavier burst, and victory adds a brief celebration shake.

Result:

Unity production validation passes with no C# compiler errors or warnings. Camera impact feedback now supports the documented destruction, boss, and victory polish requirements without adding per-event camera references.

Follow Up:

Tune amplitudes on device to keep the effect satisfying without hurting portrait readability or causing motion discomfort.

---

Date:

2026-05-30

System:

Automated Level Data QA Baseline

Files:

Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Expanded the production validator with an authored-level data QA pass. Validation now checks level count, duplicate indices, track/reward/bonus data, gate values and positions, enemy and obstacle combat data, boss definition coverage, boss-level count, and a conservative positive gate-path estimate from the starting troop count.

Result:

Unity production validation passes with no C# compiler errors or warnings, including the new authored-level data QA pass. Manual playthrough QA is still required, but malformed level data and several impossible-layout risks are now covered by repeatable editor validation.

Follow Up:

Add play-mode or device automation around actual victory/defeat/reward flows once Unity runtime test scaffolding is introduced.

---

Date:

2026-05-30

System:

Boss Intro Audio Cue

Files:

Assets/_Project/Scripts/Audio/AudioService.cs
Assets/_Project/Scripts/Bosses/BossController.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a dedicated procedural boss-intro SFX cue to the pooled audio service and play it when a boss encounter engages. The task board now reflects the existing pooled SFX source implementation and the new boss-intro hook.

Result:

Unity production validation passes with no C# compiler errors or warnings. Boss encounters now combine intro text, warning haptics, boss-intro audio, attack audio, hit audio, and defeat audio.

Follow Up:

Replace procedural boss SFX with authored clips during the final audio pass and verify mix balance on device speakers.

---

Date:

2026-05-30

System:

Obstacle Explosion VFX

Files:

Assets/_Project/Scripts/VFX/VfxManager.cs
Assets/_Project/Scripts/Obstacles/ObstacleController.cs
Assets/_Project/Editor/ArmyRushProjectBuilder.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added a dedicated obstacle explosion VFX cue. Obstacle destruction now spawns an explosion burst before debris and smoke, adds heavier smoke for high-penalty obstacles, and uses a pooled runtime fallback when an authored obstacle-explosion prefab is not present in existing scenes. The project builder can wire a dedicated `PF_VFX_ObstacleExplosion` prefab for newly generated scenes.

Result:

Unity production validation passes with no C# compiler errors or warnings. Destruction VFX now have functional spark, smoke, explosion, debris, and camera-shake coverage.

Follow Up:

Run device QA for explosion scale/readability and generate the dedicated authored prefab asset during the next full foundation rebuild if the scene is regenerated.

---

Date:

2026-05-30

System:

Enemy Shoot Animation Feedback

Files:

Assets/_Project/Scripts/Enemies/EnemyGroup.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added enemy attack animation feedback to combat. Enemy units now face back toward the player, pulse sampled front-line soldiers with the existing weapon kick when damaged, force an attack pulse during collision attrition, and emit a pooled muzzle flash at the enemy firing line.

Result:

Unity production validation passes with no C# compiler errors or warnings. Enemy visual coverage now includes run, death, hit reaction, and shoot/attack feedback; final authored character art and stronger enemy silhouette work remain open.

Follow Up:

Tune attack cadence and enemy silhouette readability during manual device combat QA.

---

Date:

2026-05-30

System:

Enemy Defeat Rewards

Files:

Assets/_Project/Scripts/Enemies/EnemyGroup.cs
Assets/_Project/Scripts/Level/LevelManager.cs
Docs/TASKS.md
Docs/DEVLOG.md

Summary:

Added enemy-group coin rewards to the combat loop. Level spawning now calculates a scaled coin reward per enemy group from `GlobalTuning.enemyCoinValue` and level index, passes it into `EnemyGroup`, and cleared groups add those coins through the run reward pipeline with coin-burst feedback.

Result:

Unity production validation passes with no C# compiler errors or warnings. Reward calculation now includes level completion, enemy clears, obstacle clears, boss rewards, bonus crates, survivor bonus, and coin multiplier upgrades.

Follow Up:

Balance enemy reward values against enemy density and early upgrade costs during manual level QA.

---

# Completion Tracking

Gameplay Systems

45%

UI Systems

40%

Visual Systems

45%

Audio Systems

27%

Level Systems

35%

Progression Systems

47%

Boss Systems

32%

Optimization

24%

Polish

35%

Release Readiness

12%

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

First functional pass implemented for runner movement, crowd count, gates, shooting with sampled unit recoil, enemy groups, animated unit removal, obstacles, finish, rewards, and run state. Manual play-mode QA remains pending.

---

# UI Log

All interface-related changes belong here.

---

## Initial State

First functional pass implemented for main menu, upgrade buttons with purchase feedback, gameplay HUD, victory panel with upgrade shortcut, defeat panel with fade overlay and upgrade shortcut, hardened safe-area fitting, and settings legal/restore placeholder controls.

---

# Visual Log

All visual changes belong here.

---

## Initial State

Generated procedural low-poly materials, meshes, prefabs, road/ocean environment, soldiers, enemies, gates, obstacles, projectile tracers, finish line, procedural unit spawn/shoot/death/victory motion, and procedural boss defeat motion. Final art polish and VFX pass remain pending.

---

# Audio Log

All audio changes belong here.

---

## Initial State

Centralized audio cue hooks, pooled procedural runtime SFX, procedural music loop, and persisted music/SFX volume settings implemented. Final authored clips and music remain pending.

---

# VFX Log

All effects-related changes belong here.

---

## Initial State

Projectile tracers, gate pulse feedback, pooled floating text, damage feedback, clear feedback, muzzle flashes, crowd gain/loss bursts, obstacle debris, smoke variants, coin reward bursts, victory bursts, upgrade glow feedback, and defeat fade implemented. Final authored particle styling remains pending.

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

Runtime frame pacing setup, pool prewarm top-ups, player-crowd formation update reduction, generated material instancing, and pooling validation coverage are implemented. Device profiling remains pending.

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

Manual Unity play-mode QA has not been completed yet after generated scene creation, including the new finish-line bonus section. Authored level data now has automated production-validator coverage, but hands-on playthrough remains pending.

Issue ID:

AR-002

Status:

Boss levels now have a data-driven tank boss, combat pause, telegraphed cannon attacks, boss rewards, boss audio/haptics, and procedural defeat motion, but damage-state reactions, tuned boss balance, final authored boss VFX, and additional boss variants are still pending.

Issue ID:

AR-003

Status:

Audio and haptic call sites exist with pooled procedural SFX playback including boss intro/attack/defeat cues, plus a procedural music loop with settings control, but final authored SFX and music are not implemented yet.

Issue ID:

AR-004

Status:

Final VFX assets are not complete; projectile tracers, floating combat text, gate bursts, hit sparks, muzzle flashes, crowd gain/loss bursts, obstacle debris, smoke variants, coin bursts, bonus crate reward bursts, victory bursts, boss attack telegraphs, boss explosion bursts, upgrade glow feedback, and defeat fade are present, but final authored styling remains pending.

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
