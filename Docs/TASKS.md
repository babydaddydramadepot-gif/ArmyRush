# TASKS.md

# ArmyRush Production Task Board
Version: 1.0
Status: Active Build Plan
Owner: Codex Goal Mode
Project: ArmyRush
Platform: Unity 6, URP, iOS/iPadOS

---

# Purpose

This file is the master task board for ArmyRush.

Codex must use this document to decide what to build, in what order, and how to determine whether a task is complete.

This file is not a loose checklist.

This file is the production execution plan.

Every major implementation step must update this file.

Every completed feature must be marked clearly.

Every blocked feature must be documented.

Every deferred feature must be moved into a future phase.

---

# Mandatory Codex Rules

Codex must follow these rules at all times:

1. Read all files inside `Docs/` before starting implementation.
2. Treat `GAME_VISION.md` as the creative authority.
3. Treat `TECH_ARCHITECTURE.md` as the engineering authority.
4. Treat `GAMEPLAY_LOOP.md` as the gameplay authority.
5. Treat `VISUAL_STYLE.md` as the art and polish authority.
6. Treat `LEVEL_DESIGN.md` as the level creation authority.
7. Treat `PROGRESSION.md` as the economy and upgrade authority.
8. Treat `UI_UX.md` as the interface and user experience authority.
9. Treat `MONETIZATION.md` as the future monetization authority.
10. Update `TASKS.md` after every major completed task.
11. Update `DEVLOG.md` after every meaningful change.
12. Do not leave placeholder systems marked as complete.
13. Do not ship Unity primitives as final visuals.
14. Do not create undocumented systems.
15. Do not ignore iPhone/iPad compatibility.
16. Do not break existing gameplay to add new systems.
17. Do not create major systems without connecting them into the real game loop.
18. Do not hardcode balancing values when ScriptableObjects or config assets are appropriate.
19. Do not leave debug UI visible in production gameplay.
20. Do not stop after creating a prototype if the feature requires polish.

---

# Definition of Done

A feature is only complete when all of the following are true:

- The feature works in play mode.
- The feature is connected to the real game loop.
- The feature has production-quality visuals or documented production-ready generated assets.
- The feature has appropriate animation.
- The feature has appropriate VFX.
- The feature has appropriate audio hooks.
- The feature respects iPhone and iPad safe areas.
- The feature performs smoothly.
- The feature has no visible debug placeholders.
- The feature has no unhandled errors.
- The feature has no compiler errors.
- The feature is organized according to `TECH_ARCHITECTURE.md`.
- The feature follows naming conventions.
- The feature updates persistent save data if required.
- The feature is documented in `DEVLOG.md`.
- The task status is updated in this file.

If any of these are missing, the feature is not done.

---

# Forbidden Completion Claims

Codex may not mark a task complete if:

- The scene only contains capsules, cubes, or spheres as final art.
- A system exists only as a script but is not playable.
- UI uses default Unity styling.
- Buttons do not animate.
- Gameplay has no feedback.
- Combat works numerically but has no visible impact.
- A level can be completed but feels empty.
- A reward system exists but does not animate or feel satisfying.
- The feature only works in the editor and not in an iOS-targeted build.
- There are console errors.
- There are missing references.
- There are unassigned serialized fields.
- There are TODO comments for core behavior.
- The implementation ignores any source-of-truth doc.

---

# Current Build Phase

Current Phase:

Phase 1: Production Vertical Slice

Goal:

Build a polished, playable, iOS-ready Last War-style crowd combat runner with:

- Main menu
- Upgrade screen
- Real gameplay scene
- Player crowd
- Gates
- Enemies
- Shooting
- Obstacles
- Boss encounter
- Reward flow
- At least 20 polished levels
- Audio hooks
- VFX hooks
- Save system
- iPhone and iPad safe area support
- No placeholder final visuals

---

# Active Implementation Plan

Date:

2026-05-30

Source-of-Truth Notes:

- Documentation read pass is complete for all Markdown files in `Docs/`.
- `Docs/.DS_Store` is a macOS metadata file and is not a design or engineering source.
- Folder-structure conflict resolved by hierarchy: `TECH_ARCHITECTURE.md` outranks this task board, so new production work will use `Assets/_Project/` rather than the lower-priority `_ArmyRush` variant listed below.
- Temporary construction assets may be used only while building systems. They must not be marked complete until replaced or upgraded to production-ready stylized assets with materials, animation, VFX, audio hooks, and QA.

Initial Audit Summary:

- Unity version: `6000.4.9f1`.
- Render pipeline: URP package `17.4.0` is installed and active.
- Input System package is installed.
- Current project content is the default URP template with `Assets/Scenes/SampleScene.unity`.
- No ArmyRush gameplay scripts, prefabs, level data, UI flow, save system, or production scenes exist yet.
- iOS/iPad target data exists, but orientation still allows autorotation and landscape. Portrait-only setup is required.
- Existing uncommitted settings changes are present and must be preserved unless directly superseded by production requirements.

First Implementation Pass:

1. Create `Assets/_Project/` production folder structure.
2. Add core runtime scripts for services, save/economy/progression, run state, input, camera, crowd, gates, combat, enemies, obstacles, level generation, UI, audio, haptics, pooling, and utility.
3. Add editor bootstrap tooling to generate scenes, prefabs, materials, tuning assets, upgrade assets, and 20 authored level data assets.
4. Configure Boot, MainMenu, and Game scenes in build settings.
5. Configure portrait orientation and mobile-safe project settings.
6. Validate compilation through Unity batch mode.
7. Run the generated Game scene in a form that is ready for manual Unity play-mode testing.
8. Update `TASKS.md` and `DEVLOG.md`, then commit the milestone.

Implementation Pass 1 Result:

- Created `Assets/_Project/` folder structure, runtime scripts, generated materials, generated meshes, prefabs, three scenes, upgrade data, tuning data, and 20 level data assets.
- Configured build scene order: Boot, MainMenu, Game.
- Configured portrait-only orientation and iPhone/iPad target support.
- Unity batch mode generation completed with no compiler errors or warnings in `/tmp/armyrush_unity_build.log`.
- Unity batch mode validation completed with no compiler errors or warnings in `/tmp/armyrush_unity_validate.log`.
- Added pooled floating text feedback for gate changes, damage, enemy clears, and coin rewards.
- Added first tank boss pass with generated boss prefab, boss spawning on levels 5/10/15/20, damageable boss health, collision pressure, world label, and HUD boss health bar.
- Added runtime procedural SFX playback through pooled AudioSources for buttons, gates, shooting, hits, clears, destruction, coin rewards, upgrades, victory, and defeat.
- Added repeatable iOS development export command and successfully generated an Xcode project at `ArmyRush_iOSBuild`.
- Confirmed the generated Xcode project builds successfully with `CODE_SIGNING_ALLOWED=NO` for a Debug `iphoneos` build.
- Added data-driven tank boss definitions, boss-level references, contact-triggered combat pause, telegraphed cannon attacks, boss defeat resume flow, boss reward bonus, and active boss/obstacle/critical upgrade scaling.
- Added pooled particle VFX prefabs and cue-based spawning for hit sparks, gate bursts, coin bursts, and boss explosions.
- Added finish-line bonus run flow with data-driven bonus crate counts, crate health, crate coin rewards, generated crate/claim prefabs, pooled reward VFX/audio, and validation coverage for spawned bonus objects.
- Added pooled muzzle flash, obstacle debris, and victory burst VFX cues with generated particle prefabs and validation coverage.
- Added pooled crowd gain/loss VFX bursts for army count changes after the initial spawn count.
- Manual Unity play-mode QA, simulator/device launch testing, physical device signing/deployment, final audio assets, final VFX assets, boss animation/balance polish, and full art polish remain pending.

---

# Phase 0: Project Foundation

## 0.1 Documentation Read Pass

Status: Completed
Priority: Critical

Tasks:

- [x] Read every file inside `Docs/`.
- [x] Build an internal understanding of the project scope.
- [x] Identify conflicts between documents.
- [x] Follow the highest-priority source of truth when conflicts exist.
- [x] Do not implement until documentation is understood.

Acceptance Criteria:

- [x] Codex can summarize project scope.
- [x] Codex has identified all required systems.
- [x] No implementation begins before docs are read.

---

## 0.2 Unity Project Validation

Status: Completed for Editor Foundation; Signing-Independent Xcode Build Passed; Device QA Pending
Priority: Critical

Tasks:

- [x] Confirm Unity version is Unity 6 LTS or compatible.
- [x] Confirm project uses Universal Render Pipeline.
- [x] Confirm iOS is selected as target platform.
- [x] Confirm Portrait orientation is active.
- [x] Confirm iPhone and iPad compatibility.
- [x] Confirm safe area support requirements.
- [x] Confirm no broken packages.
- [x] Confirm no compiler errors before starting.

Acceptance Criteria:

- [x] Project opens without compile errors.
- [x] iOS target exists.
- [x] Portrait orientation is configured.
- [x] URP assets are valid.

---

## 0.3 Folder Structure Creation

Status: Completed using `Assets/_Project/` per `TECH_ARCHITECTURE.md`
Priority: Critical

Create or validate this structure:

```text
Assets/
  _ArmyRush/
    Art/
      Characters/
      Enemies/
      Bosses/
      Obstacles/
      Environments/
      UI/
      VFX/
      Materials/
    Audio/
      Music/
      SFX/
    Prefabs/
      Player/
      Enemies/
      Gates/
      Obstacles/
      Bosses/
      UI/
      VFX/
      LevelChunks/
    Scenes/
      Boot.unity
      MainMenu.unity
      Gameplay.unity
    Scripts/
      Core/
      Gameplay/
      Player/
      Crowd/
      Combat/
      Enemies/
      Gates/
      Obstacles/
      Bosses/
      Levels/
      Progression/
      Economy/
      UI/
      Audio/
      VFX/
      Save/
      Utilities/
    ScriptableObjects/
      Levels/
      Gates/
      Enemies/
      Obstacles/
      Bosses/
      Progression/
      Economy/
      Audio/
      VFX/
    Settings/
    ThirdParty/
```

Acceptance Criteria:

- [x] All new production files live inside `Assets/_Project/` by source-of-truth hierarchy.
- [x] No random scripts are placed directly in `Assets/`.
- [x] No duplicate architecture folders are created.

---

## 0.4 Scene Foundation

Status: Functional Pass Implemented; Play-Mode QA Pending
Priority: Critical

Tasks:

- Create Boot scene.
- Create MainMenu scene.
- Create Gameplay scene.
- Add scene loading flow.
- Configure build scene order.
- Ensure Boot initializes global systems.
- Ensure MainMenu routes to Gameplay.
- Ensure Gameplay can return to MainMenu.

Acceptance Criteria:

- Game starts from Boot.
- Main menu appears.
- Play button loads Gameplay.
- Gameplay can complete and return to reward flow.

---

## 0.5 Core Manager Setup

Status: Functional Pass Implemented; Architecture Polish Pending
Priority: Critical

Implement core managers:

- GameManager
- SceneLoadManager
- SaveManager
- AudioManager
- VFXManager
- UIManager
- EconomyManager
- ProgressionManager
- LevelManager
- PoolManager
- InputManager

Acceptance Criteria:

- Managers initialize in correct order.
- No duplicate manager instances.
- Managers survive scene changes only where appropriate.
- Managers are not God objects.
- Responsibilities are separated cleanly.

---

# Phase 1: Core Gameplay Foundation

## 1.1 Player Runner Controller

Status: Functional Pass Implemented; Device Feel QA Pending
Priority: Critical

Tasks:

- Implement automatic forward movement.
- Implement touch drag left/right movement.
- Clamp player to road boundaries.
- Smooth horizontal movement.
- Prevent reverse movement.
- Support editor mouse simulation.
- Support mobile touch input.

Acceptance Criteria:

- Player moves forward automatically.
- Drag controls feel immediate.
- Player cannot leave road.
- Controls work in editor and mobile build.
- No keyboard-only dependency.

---

## 1.2 Follow Camera

Status: Functional Pass Implemented with Impact Shake; Framing QA Pending
Priority: Critical

Tasks:

- Implement fixed follow camera.
- Camera follows crowd center.
- Camera uses portrait-friendly angle.
- Camera shows upcoming gates and enemies.
- Camera does not rotate freely.
- Camera avoids jitter.
- Add camera shake for obstacle, boss, and victory moments.

Acceptance Criteria:

- Camera feels similar to Last War-style runner view.
- Player sees at least 8 seconds ahead.
- UI and gameplay remain readable.
- Camera never clips through level geometry.

---

## 1.3 Crowd Unit System

Status: Functional Pass Implemented; Performance QA Pending
Priority: Critical

Tasks:

- Implement player crowd container.
- Implement individual soldier units.
- Implement dynamic formation layout.
- Implement unit add/remove logic.
- Implement unit pooling.
- Implement crowd count tracking.
- Implement above-crowd count label.

Acceptance Criteria:

- Crowd starts with configured soldier count.
- Units form clean readable formation.
- Units can be added by gates.
- Units can be removed by combat.
- Count label updates immediately.
- System supports at least 300 visible units smoothly.

---

## 1.4 Production Soldier Visuals

Status: In Progress; Procedural Low-Poly Visual and Full Gameplay Animation Pass Implemented; Final Authored Polish Pending
Priority: Critical

Tasks:

- Create stylized blue soldier prefab.
- Soldier must include body, head, helmet, vest, weapon, boots, and team color material.
- [x] Add run animation.
- [x] Add shooting animation.
- [x] Add hit reaction animation.
- [x] Add death animation.
- [x] Add spawn animation.
- [x] Add victory animation.

Acceptance Criteria:

- No capsule soldier final visuals.
- Soldier reads clearly from gameplay camera.
- Soldier is visibly blue team.
- Soldier animations work in gameplay.
- Soldier prefab is optimized and reusable.

---

## 1.5 Gate System

Status: Functional Pass Implemented with Activation Animation; Feedback QA Pending
Priority: Critical

Tasks:

- Implement gate base class.
- Implement addition gates.
- Implement multiplication gates.
- Implement subtraction gates.
- Implement division gates for later levels.
- Implement gate trigger detection.
- [x] Implement gate visual activation.
- [x] Implement floating result feedback.

Acceptance Criteria:

- Gates modify crowd count correctly.
- Gate values are readable from camera distance.
- Positive gates are visually distinct from negative gates.
- Gate pass creates sound, VFX, and animation feedback.
- Gate logic uses configurable data assets.

---

## 1.6 Gate Visuals

Status: Functional Pass Implemented with Glow, Activation Animation, and Positive/Negative Variants; Device Readability QA Pending
Priority: Critical

Tasks:

- Create production gate prefab.
- Use bright translucent panels.
- Use bold readable text.
- [x] Add glow effects.
- [x] Add activation animation.
- [x] Add positive and negative variants.

Acceptance Criteria:

- Gates do not look like Unity primitives.
- Gate text is readable on iPhone.
- Gate colors match `VISUAL_STYLE.md`.
- Gate interaction feels satisfying.

---

## 1.7 Enemy Crowd System

Status: Functional Pass Implemented; Combat QA Pending
Priority: Critical

Tasks:

- Implement enemy crowd prefab.
- Implement red enemy soldier units.
- Implement enemy count display.
- Implement enemy formation.
- Implement enemy encounter data.
- Implement collision/combat trigger with player crowd.

Acceptance Criteria:

- Enemy groups spawn from level data.
- Enemy units are visually red team.
- Enemy count is readable.
- Enemy crowd combat works reliably.
- Enemy system supports pooling.

---

## 1.8 Production Enemy Visuals

Status: In Progress; Procedural Low-Poly Visual and Run/Death/Hit Animation Pass Implemented
Priority: Critical

Tasks:

- Create stylized red enemy soldier prefab.
- [x] Add enemy run and death animations.
- Add enemy shoot animation.
- [x] Add enemy hit reaction animation.
- Add team color material.
- Ensure enemy silhouette differs from player.

Acceptance Criteria:

- No capsule enemies.
- Enemies are visually distinct from player soldiers.
- Enemy animation and feedback are complete.

---

## 1.9 Crowd Combat Resolution

Status: Functional Pass Implemented with Animated Unit Removal; QA Pending
Priority: Critical

Tasks:

- Implement collision-based crowd combat.
- Resolve player vs enemy count correctly.
- [x] Remove defeated units with animation.
- Continue level if player survives.
- Trigger failure if player reaches zero.
- Add VFX and audio.

Acceptance Criteria:

- Combat is readable.
- Combat is fast and satisfying.
- Unit removal is visually clear.
- No invisible math-only combat.
- Defeat triggers proper game state.

---

## 1.10 Auto Shooting System

Status: Functional Pass Implemented with Projectile Trails and Impact Feedback; Device Feel QA Pending
Priority: High

Tasks:

- Implement automatic weapon firing.
- Units target nearest valid enemy or obstacle.
- [x] Add projectile pooling.
- [x] Add bullet trails.
- [x] Add muzzle flashes.
- [x] Add hit impacts.
- [x] Add damage numbers.

Acceptance Criteria:

- Shooting happens automatically.
- Projectiles are visually readable.
- Performance remains smooth.
- Damage feedback appears correctly.

---

# Phase 2: Obstacles and Destruction

## 2.1 Obstacle System

Status: Functional Pass Implemented with Reward Payouts and Destruction Feedback; Device Balance QA Pending
Priority: High

Tasks:

- [x] Implement obstacle base class.
- [x] Add health system.
- [x] Add damage receiving.
- [x] Add destruction trigger.
- [x] Add obstacle rewards.
- [x] Add obstacle data assets.

Acceptance Criteria:

- Obstacles can be damaged and destroyed.
- Health values are configurable.
- Destruction is satisfying.
- Obstacles never create impossible layouts.

---

## 2.2 Production Obstacle Visuals

Status: In Progress; Barricade/Crate Visual Pass Implemented
Priority: High

Tasks:

Create production visuals for:

- Crates
- Barrels
- Barricades
- Concrete blocks
- Military trucks
- Turrets
- Fuel tanks

Acceptance Criteria:

- No cubes used as final obstacles.
- Obstacles visually communicate durability.
- Destruction VFX are present.
- Obstacles fit the stylized military bridge theme.

---

## 2.3 Destruction VFX

Status: Functional Pass Implemented; Explosive Variants Pending
Priority: High

Tasks:

- Add impact sparks.
- [x] Add smoke puffs.
- Add explosion effects for explosive obstacles.
- Add debris bursts.
- Add camera shake for large destruction.

Acceptance Criteria:

- Destroying obstacles feels satisfying.
- Effects are optimized for mobile.
- VFX match art style.

---

# Phase 3: Level System

## 3.1 Level Data Architecture

Status: Functional Pass Implemented; Chunk/Boss Data Pending
Priority: Critical

Tasks:

- Create LevelData ScriptableObject.
- Create LevelChunkData ScriptableObject.
- Create gate placement data.
- Create enemy placement data.
- Create obstacle placement data.
- Create boss placement data.
- Create reward data.

Acceptance Criteria:

- Levels are data-driven.
- Codex can add levels without hardcoding scenes.
- Level data references reusable prefabs.
- Level balancing can be edited from assets.

---

## 3.2 Level Builder

Status: Functional Pass Implemented with Bonus Section; Layout QA Pending
Priority: Critical

Tasks:

- Build level from LevelData.
- Spawn road chunks.
- Spawn gates.
- Spawn enemies.
- Spawn obstacles.
- Spawn finish zone.
- Spawn bonus section with reward crates and claim trigger.

Acceptance Criteria:

- Gameplay scene can load different levels.
- Level construction is reliable.
- Objects spawn at correct positions.
- No overlapping broken layouts.

---

## 3.3 Road and Bridge Visuals

Status: In Progress; Generated Road/Ocean/Bonus Pass Implemented
Priority: Critical

Tasks:

- Create production road chunks.
- Create bridge segments.
- Create side rails.
- Create ocean environment.
- Create military coastline background.
- Create finish platform.
- Create bonus run platform.

Acceptance Criteria:

- Level environment resembles polished mobile military runner.
- Road chunks are visually clean.
- Environment supports readability.
- No graybox final geometry.

---

## 3.4 First 20 Designed Levels

Status: In Progress; 20 Data-Driven Levels Generated with Boss Levels
Priority: Critical

Tasks:

- Create Level 1 tutorial.
- Create Level 2 negative gate intro.
- Create Level 3 enemy group intro.
- Create Level 4 obstacle intro.
- Create Level 5 tank boss.
- Create Levels 6-10 advanced gates.
- Create Levels 11-15 heavier combat.
- Create Level 15 boss variant.
- Create Levels 16-20 risk/reward mastery.
- Create Level 20 major boss.

Acceptance Criteria:

- At least 20 levels are playable.
- Levels follow `LEVEL_DESIGN.md`.
- Difficulty scales fairly.
- No random unbalanced level layouts.
- All levels are beatable.

---

## 3.5 Endless Level Support

Status: Functional Deterministic Runtime Generation Implemented; Boss Variant Rotation Pending
Priority: Medium

Tasks:

- [x] Add procedural level generation after authored levels.
- [x] Use chunk system.
- [x] Scale difficulty.
- Rotate boss types.
- [x] Scale rewards.

Acceptance Criteria:

- Game continues beyond authored levels.
- Procedural levels remain fair.
- Repetition is minimized.

---

# Phase 4: Boss System

## 4.1 Boss Architecture

Status: Functional Combat and Defeat Animation Pass Implemented; Balance/Variants Pending
Priority: High

Tasks:

- [x] Create BossController.
- [x] Create BossData ScriptableObject.
- [x] Add boss health system.
- [x] Add boss attack system.
- [x] Add boss damage receiving.
- [x] Add boss defeat state.
- [x] Add boss reward flow.

Acceptance Criteria:

- Bosses are data-driven.
- Boss fights are readable.
- Boss defeat resumes the run after the combat pause.

---

## 4.2 Tank Boss

Status: Functional Pass Implemented with Damage States and Defeat Animation; Balance QA Pending
Priority: High

Tasks:

- Create production tank boss model/prefab.
- [x] Add movement or idle combat behavior.
- [x] Add cannon attack behavior.
- [x] Add damage states.
- [x] Add destruction animation.
- [x] Add explosion VFX.

Acceptance Criteria:

- Tank boss feels like a major moment.
- No placeholder boss visuals.
- Boss fight is beatable and satisfying.

---

## 4.3 Helicopter Boss

Status: Not Started
Priority: Medium

Tasks:

- Create helicopter boss prefab.
- Add lateral movement.
- Add missile attack.
- Add hit effects.
- Add crash/destruction animation.

Acceptance Criteria:

- Helicopter boss is visually distinct.
- Fight differs from tank boss.

---

## 4.4 Mech Boss

Status: Not Started
Priority: Medium

Tasks:

- Create mech boss prefab.
- Add multi-phase attack behavior.
- Add heavy impact VFX.
- Add unique defeat animation.

Acceptance Criteria:

- Mech boss feels like later-game escalation.
- Fight is readable and polished.

---

# Phase 5: Economy and Progression

## 5.1 Currency System

Status: Functional Pass Implemented; UI/QA Pending
Priority: Critical

Tasks:

- Implement coin balance.
- Implement gem balance for future use.
- Implement reward transactions.
- Implement spend transactions.
- Save currencies persistently.

Acceptance Criteria:

- Coins persist between sessions.
- Spending and earning are reliable.
- UI updates instantly.

---

## 5.2 Upgrade System

Status: Functional Pass Implemented; Balance QA Pending
Priority: Critical

Tasks:

Implement upgrades:

- Damage
- Fire Rate
- Starting Soldiers
- Coin Bonus
- Boss Damage
- Obstacle Damage
- Critical Chance
- Critical Damage

Acceptance Criteria:

- Upgrades use formulas from `PROGRESSION.md`.
- Upgrades persist.
- Upgrades affect gameplay.
- Upgrade UI reflects cost and level.

---

## 5.3 Reward Calculation

Status: Functional Pass Implemented; Enemy/Obstacle Reward Balance Pending
Priority: High

Tasks:

- Calculate level completion reward.
- Calculate enemy rewards.
- Calculate obstacle rewards.
- Calculate boss rewards.
- Calculate bonus run crate rewards.
- Apply coin multipliers.

Acceptance Criteria:

- Rewards feel generous early.
- Rewards scale correctly.
- Reward screen matches actual payout.

---

## 5.4 Save System

Status: Functional Pass Implemented; Migration QA Pending
Priority: Critical

Tasks:

Save:

- Current level
- Coins
- Gems
- Upgrade levels
- Settings
- Unlocked cosmetics future data
- Tutorial completion

Acceptance Criteria:

- Data persists after app restart.
- Save system handles missing fields safely.
- Save data can evolve across versions.

---

# Phase 6: UI/UX

## 6.1 HUD

Status: Functional Pass Implemented; Device Layout QA Pending
Priority: Critical

Tasks:

- Add level label.
- Add progress bar.
- Add coin counter.
- Add soldier count indicator.
- Add settings button.
- Respect safe areas.

Acceptance Criteria:

- HUD does not block gameplay.
- HUD is readable on iPhone and iPad.
- HUD updates in real time.
- HUD is not default Unity UI.

---

## 6.2 Main Menu

Status: Functional Pass Implemented; Character Showcase Polish Pending
Priority: Critical

Tasks:

- Create polished main menu.
- Add play button.
- Add upgrade button.
- Add settings button.
- Add currency display.
- Add animated background or character showcase.

Acceptance Criteria:

- Main menu feels production-ready.
- Player can start gameplay within one tap.
- Buttons animate and respond.

---

## 6.3 Upgrade Screen

Status: Functional Pass Implemented with Purchase Animation and Glow Feedback; Device Fit QA Pending
Priority: Critical

Tasks:

- Create upgrade panel.
- Add all upgrade categories.
- Add level display.
- Add cost display.
- Add purchase buttons.
- Add disabled state when unaffordable.
- [x] Add purchase animation.

Acceptance Criteria:

- Upgrade screen feels satisfying.
- Upgrade purchases are clear.
- No default UI styling.

---

## 6.4 Victory Screen

Status: Functional Pass Implemented with Animated Reveal, Rewarded Placeholder, and Upgrade Shortcut; Device Fit QA Pending
Priority: Critical

Tasks:

- Create victory screen.
- Show coins earned.
- Show bonus rewards in earned coin total.
- Add next level button.
- [x] Add upgrade shortcut.
- [x] Add optional rewarded ad placeholder architecture.

Acceptance Criteria:

- Victory screen celebrates success.
- Player understands rewards.
- Flow to next level is fast.

---

## 6.5 Defeat Screen

Status: Functional Pass Implemented with Animated Reveal, Partial Rewards, Revive Placeholder, and Upgrade Shortcut; Device Fit QA Pending
Priority: High

Tasks:

- Create defeat screen.
- Add retry button.
- [x] Add upgrade button.
- [x] Add coins earned.
- [x] Add revive architecture for rewarded ads future.

Acceptance Criteria:

- Defeat does not feel overly punishing.
- Player is encouraged to retry or upgrade.

---

## 6.6 Settings Screen

Status: Functional Pass Implemented with Audio, Haptics, Legal Link, and Restore Placeholder Controls; Final Legal URLs Pending
Priority: Medium

Tasks:

- [x] Add music toggle/slider.
- [x] Add SFX toggle/slider.
- [x] Add haptics toggle.
- [x] Add privacy policy placeholder link.
- [x] Add terms placeholder link.
- [x] Add restore purchases placeholder.

Acceptance Criteria:

- Settings save persistently.
- Settings are accessible.
- UI respects safe areas.

---

# Phase 7: Audio, VFX, and Juice

## 7.1 Audio System

Status: Functional Pass Implemented with Procedural Music; Final Audio Assets Pending
Priority: High

Tasks:

- Implement AudioManager.
- Add SFX categories.
- [x] Add music support.
- [x] Add SFX volume settings.
- Add pooled audio sources.

Acceptance Criteria:

- Audio plays reliably.
- Settings affect audio.
- No overlapping audio chaos.

---

## 7.2 Required SFX

Status: Functional Pass Implemented with Procedural Runtime SFX; Final Clips/Music Pending
Priority: High

Add SFX for:

- Button press
- Gate pass
- Positive gate
- Negative gate
- Shooting
- Hit impact
- Enemy defeat
- Obstacle damage
- Obstacle destruction
- Coin collect
- Upgrade purchase
- Victory
- Defeat
- Boss intro
- Boss attack
- Boss defeat

Acceptance Criteria:

- Every major action has audio feedback.
- Audio improves satisfaction.

---

## 7.3 VFX System

Status: Functional Pass Implemented with Pooled Particle Prefabs; Library Expansion Pending
Priority: High

Tasks:

- Implement VFXManager.
- Implement pooled effects.
- Add reusable effect prefabs.
- Add effect spawn helpers.

Acceptance Criteria:

- VFX are pooled.
- VFX do not create performance spikes.
- Effects match visual style.

---

## 7.4 Required VFX

Status: Functional Pass Implemented; Final Authored Styling Pending
Priority: High

Add VFX for:

- Muzzle flash
- Bullet trails
- Hit sparks
- Gate activation
- Crowd add burst
- Crowd loss burst
- Coin burst
- [x] Upgrade glow
- Obstacle explosion
- Boss explosion
- Victory fireworks
- Defeat fade

Acceptance Criteria:

- Gameplay feels juicy.
- Effects remain readable.
- Effects are optimized.

---

## 7.5 Haptics

Status: Functional Pass Implemented with Settings Toggle; Device QA Pending
Priority: Medium

Tasks:

- [x] Add haptic wrapper.
- [x] Add haptics for gate pass.
- [x] Add haptics for upgrade purchase.
- [x] Add haptics for boss defeat.
- [x] Add haptics for major explosions.
- [x] Respect haptics setting.

Acceptance Criteria:

- Haptics work on supported iOS devices.
- Haptics can be disabled.

---

# Phase 8: Art Production and Polish

## 8.1 Character Asset Pass

Status: Not Started
Priority: Critical

Tasks:

- Replace all placeholder player units.
- Replace all placeholder enemy units.
- Add final materials.
- Add final animations.
- Add team variants.

Acceptance Criteria:

- No placeholder character assets remain.
- Characters match `VISUAL_STYLE.md`.

---

## 8.2 Environment Asset Pass

Status: Not Started
Priority: Critical

Tasks:

- Replace placeholder roads.
- Replace placeholder bridges.
- Replace placeholder ocean.
- Add background details.
- Add finish platform.
- Add bonus platform.

Acceptance Criteria:

- Gameplay scene looks production-ready.
- No graybox environment remains.

---

## 8.3 UI Art Pass

Status: Not Started
Priority: Critical

Tasks:

- Replace default UI.
- Add custom button visuals.
- Add custom panels.
- Add icons.
- Add coin icon.
- Add upgrade icons.
- Add settings icons.

Acceptance Criteria:

- UI looks like a commercial mobile title.
- No default Unity UI remains.

---

## 8.4 Animation Polish Pass

Status: Functional Animation Polish Pass Implemented; Device QA/Authored Polish Pending
Priority: High

Tasks:

- [x] Add button bounce animations.
- [x] Add reward count-up animation.
- [x] Add gate punch animation.
- [x] Add unit spawn animation.
- [x] Add unit death animation.
- [x] Add boss defeat animation.
- [x] Add victory celebration animation.

Acceptance Criteria:

- Game feels alive.
- Animations are consistent.
- No static lifeless screens.

---

# Phase 9: iOS/iPad Build Readiness

## 9.1 Mobile Performance Pass

Status: In Progress; Runtime Frame Pacing, Pool Prewarm, Formation Update, Instanced Material, and Pool Validation Baseline Implemented
Priority: Critical

Tasks:

- Profile crowd counts.
- Profile projectile pooling.
- Profile VFX pooling.
- [x] Set runtime 60 FPS target.
- [x] Disable runtime vSync dependency.
- [x] Prevent device sleep during play sessions.
- [x] Prewarm starting crowd and enemy visual pools before level spawn.
- [x] Top up pools instead of duplicating prewarm counts.
- [x] Reduce draw calls where possible with GPU-instanced generated materials.
- [x] Use shared generated materials.
- [x] Verify object pooling through production validation checks.
- [x] Avoid excessive player-crowd formation Update loops.

Acceptance Criteria:

- Game targets 60 FPS.
- No major frame spikes.
- Large crowds remain playable.

---

## 9.2 Safe Area Pass

Status: Functional Pass Implemented; Device Matrix QA Pending
Priority: Critical

Tasks:

- Test iPhone notch layout.
- Test Dynamic Island layout.
- Test Home Indicator layout.
- Test iPad layout.
- [x] Ensure all UI remains anchored inside the runtime safe-area fitter.

Acceptance Criteria:

- No critical UI is clipped.
- Buttons remain reachable.
- Gameplay remains centered and readable.

---

## 9.3 Xcode Export Validation

Status: Unity Xcode Export and Signing-Independent Xcode Build Passed; Simulator/Device Launch Pending
Priority: Critical

Tasks:

- [x] Export Unity project to Xcode.
- [x] Confirm Xcode project builds without signing requirements.
- Confirm app launches in simulator if available.
- Confirm app launches on device if available.
- Fix iOS build errors.

Acceptance Criteria:

- Xcode project builds without signing-independent errors.
- App launches successfully.
- No missing signing-independent build issues.

---

# Phase 10: QA and Release Polish

## 10.1 Gameplay QA

Status: Not Started
Priority: Critical

Tasks:

- Test every authored level.
- Confirm all levels are beatable.
- Confirm losses trigger correctly.
- Confirm victory triggers correctly.
- Confirm upgrades affect gameplay.
- Confirm rewards calculate correctly.

Acceptance Criteria:

- No progression blockers.
- No impossible levels.
- No broken reward flow.

---

## 10.2 Bug Fix Pass

Status: Not Started
Priority: Critical

Tasks:

- Fix console errors.
- Fix missing references.
- Fix animation errors.
- Fix broken prefabs.
- Fix UI overlap.
- Fix save/load bugs.
- Fix level loading bugs.

Acceptance Criteria:

- Console is clean during normal gameplay.
- No missing references.
- No broken scenes.

---

## 10.3 Final Polish Checklist

Status: Not Started
Priority: Critical

Tasks:

- Add final game feel pass.
- Add camera shake tuning.
- Add reward polish.
- Add UI polish.
- Add VFX polish.
- Add audio polish.
- Add animation polish.
- Remove debug tools.
- Remove temporary assets.

Acceptance Criteria:

- Game feels polished.
- No visible prototype artifacts.
- Core loop is satisfying.

---

# Future Phase: Content Pipeline

Status: Deferred
Priority: High

Create `CONTENT_PIPELINE.md` covering:

- Asset naming rules
- Prefab creation rules
- Animation import rules
- VFX creation rules
- Audio import rules
- ScriptableObject creation rules
- Level chunk creation rules
- Performance budgets
- Final asset replacement rules

---

# Future Phase: Monetization Implementation

Status: Deferred
Priority: Medium

Tasks:

- Add rewarded ad SDK architecture.
- Add ad availability wrapper.
- Add 2x reward flow.
- Add revive flow.
- Add store screen placeholder architecture.
- Add analytics hooks.

Do not implement forced ads in Phase 1.

---

# Future Phase: Cosmetics

Status: Deferred
Priority: Medium

Tasks:

- Add soldier skins.
- Add weapon skins.
- Add bullet trails.
- Add victory effects.
- Add cosmetic inventory.
- Add cosmetic unlocks.

---

# Future Phase: Live Ops

Status: Deferred
Priority: Low

Tasks:

- Add daily rewards.
- Add achievements.
- Add limited events.
- Add battle pass support.
- Add seasonal content support.

---

# Current Critical Path

Codex should implement in this order:

1. Validate project settings.
2. Create project folder structure.
3. Create scenes and managers.
4. Implement player movement and camera.
5. Implement crowd system.
6. Implement production soldier visual prefab.
7. Implement gates.
8. Implement enemies.
9. Implement crowd combat.
10. Implement shooting.
11. Implement obstacles.
12. Implement level data and level builder.
13. Build first 20 levels.
14. Implement economy and upgrades.
15. Implement UI flow.
16. Implement audio and VFX.
17. Implement bosses.
18. Complete art replacement pass.
19. Complete polish pass.
20. Validate iOS/Xcode build.

---

# Final Reminder

This project is not aiming for a rough prototype.

The target is a polished mobile game vertical slice.

Codex must continue working until systems are complete, connected, polished, documented, and testable.
