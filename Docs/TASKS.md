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
- Added a generated branded ArmyRush iOS app icon, assigned all Unity iOS icon slots, and confirmed the simulator Xcode build no longer reports the missing 1024x1024 App Store icon warning.
- Polished the main-menu upgrade tile layout for simulator-sized portrait screens, corrected the play icon direction, and added validation coverage for upgrade icon/text overlap.
- Added an animated generated 3D main-menu character showcase with hero squad, runway, gate, coin medal, and validation coverage for showcase wiring/content.
- Added data-driven tank boss definitions, boss-level references, contact-triggered combat pause, telegraphed cannon attacks, boss defeat resume flow, boss reward bonus, and active boss/obstacle/critical upgrade scaling.
- Added prefab-backed helicopter and mech boss definitions, generated visual prefabs, boss-level variant rotation, pattern-driven boss motion/impact behavior, pooled boss attack telegraphs, helicopter rotor/crash defeat polish, and validation coverage for all boss prefabs.
- Added data-driven obstacle definitions, generated obstacle variant prefabs, authored-level obstacle variant coverage, endless obstacle variant reuse, boss-approach layout guardrails, and validation for obstacle prefabs/definitions.
- Corrected gameplay camera rig double-offset, added speed-based camera look-ahead, and added validation for local-zero camera child framing under the follow rig.
- Converted the in-run HUD settings control to a compact icon-led button and added validation for top-HUD overlap/readability.
- Added runtime dependency and pooling optimization passes, removed normal-play scene-wide object searches, cached pooled component lookups, optimized world-label camera work, and reconfirmed Unity production validation plus iOS simulator export.
- Added pooled particle VFX prefabs and cue-based spawning for hit sparks, gate bursts, coin bursts, and boss explosions.
- Added finish-line bonus run flow with data-driven bonus crate counts, crate health, crate coin rewards, generated crate/claim prefabs, pooled reward VFX/audio, and validation coverage for spawned bonus objects.
- Added pooled muzzle flash, obstacle debris, and victory burst VFX cues with generated particle prefabs and validation coverage.
- Added pooled crowd gain/loss VFX bursts for army count changes after the initial spawn count.
- Ran a post-mech iOS simulator validation pass: Unity simulator export, signing-independent Xcode simulator build, simulator install, simulator launch, portrait screenshot capture, and targeted runtime exception scan all passed.
- Manual Unity play-mode QA, physical device signing/deployment, final audio assets, final VFX assets, boss balance polish, and full art polish remain pending.

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

Status: Completed for Editor Foundation; Current iOS Export and Signing-Independent Xcode Build Refreshed After Art/UI Passes; Device QA Pending
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

Status: Functional Pass Implemented with Corrected Rig Framing, Dynamic Look-Ahead, and Impact Shake; Device Framing QA Pending
Priority: Critical

Tasks:

- Implement fixed follow camera.
- Camera follows crowd center.
- Camera uses portrait-friendly angle.
- Camera shows upcoming gates and enemies.
- Camera does not rotate freely.
- Camera avoids jitter.
- Add camera shake for obstacle, boss, and victory moments.
- [x] Correct generated camera rig double-offset.
- [x] Validate camera child local-zero framing under the follow rig.
- [x] Add speed-based forward look-ahead during movement.

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

Status: Functional Character Asset Pass Implemented with Equipment Polish and Full Gameplay Animation Pass; Device QA Pending
Priority: Critical

Tasks:

- [x] Create stylized blue soldier prefab.
- [x] Soldier must include body, head, helmet, vest, weapon, boots, and team color material.
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

Status: Functional Enemy Visual Pass Implemented with Team Color, Animation, and Distinct Silhouette Polish; Device QA Pending
Priority: Critical

Tasks:

- [x] Create stylized red enemy soldier prefab.
- [x] Add enemy run and death animations.
- [x] Add enemy shoot animation.
- [x] Add enemy hit reaction animation.
- [x] Add team color material.
- [x] Ensure enemy silhouette differs from player.

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

Status: Functional Pass Implemented with Data-Driven Variants, Reward Payouts, and Destruction Feedback; Device Balance QA Pending
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

Status: Functional Variant Asset Pass Implemented; Device Readability and Balance QA Pending
Priority: High

Tasks:

- [x] Add damage-state readability feedback.

Create production visuals for:

- [x] Crates
- [x] Barrels
- [x] Barricades
- [x] Concrete blocks
- [x] Military trucks
- [x] Turrets
- [x] Fuel tanks

Acceptance Criteria:

- No cubes used as final obstacles.
- Obstacles visually communicate durability.
- Destruction VFX are present.
- Obstacles fit the stylized military bridge theme.

---

## 2.3 Destruction VFX

Status: Functional Pass Implemented with Sparks, Smoke, Explosion, Debris, and Shake; Device Tuning Pending
Priority: High

Tasks:

- [x] Add impact sparks.
- [x] Add smoke puffs.
- [x] Add explosion effects for explosive obstacles.
- [x] Add debris bursts.
- [x] Add camera shake for large destruction.

Acceptance Criteria:

- Destroying obstacles feels satisfying.
- Effects are optimized for mobile.
- VFX match art style.

---

# Phase 3: Level System

## 3.1 Level Data Architecture

Status: Completed with Reusable Chunk A-F Data, Chunk Placements, Boss Data, Reward Data, and Validator Coverage
Priority: Critical

Tasks:

- [x] Create LevelData ScriptableObject.
- [x] Create LevelChunkData ScriptableObject.
- [x] Create gate placement data.
- [x] Create enemy placement data.
- [x] Create obstacle placement data.
- [x] Create boss placement data.
- [x] Create reward data.
- [x] Add reusable Chunk A-F assets matching `LEVEL_DESIGN.md`.
- [x] Add validator coverage for chunk assets and chunk placement rules.

Acceptance Criteria:

- Levels are data-driven.
- Codex can add levels without hardcoding scenes.
- Level data references reusable prefabs.
- Level balancing can be edited from assets.
- Authored and endless levels can resolve reusable chunks into gates, enemies, and obstacles.

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

Status: Functional Environment Asset Pass Implemented with Road, Ocean, Harbor Background Details, Finish, and Bonus Platforms; Device Readability QA Pending
Priority: Critical

Tasks:

- [x] Create production road chunks.
- [x] Create bridge segments.
- [x] Create side rails.
- [x] Create ocean environment.
- [x] Create military coastline background.
- [x] Create finish platform.
- [x] Create bonus run platform.

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

Status: Functional Deterministic Runtime Generation Implemented with Chunk Assembly and Boss Variant Rotation; Manual Endless QA Pending
Priority: Medium

Tasks:

- [x] Add procedural level generation after authored levels.
- [x] Use chunk system.
- [x] Scale difficulty.
- [x] Rotate boss types.
- [x] Scale rewards.

Acceptance Criteria:

- Game continues beyond authored levels.
- Procedural levels remain fair.
- Repetition is minimized.

---

# Phase 4: Boss System

## 4.1 Boss Architecture

Status: Functional Combat, Defeat Animation, and Telegraph VFX Pass Implemented with Prefab-Backed Variants; Balance QA Pending
Priority: High

Tasks:

- [x] Create BossController.
- [x] Create BossData ScriptableObject.
- [x] Add boss health system.
- [x] Add boss attack system.
- [x] Add boss damage receiving.
- [x] Add boss defeat state.
- [x] Add boss reward flow.
- [x] Add prefab-backed boss variant definitions.
- [x] Add readable pooled attack telegraph VFX.
- [x] Validate boss prefab controller, health, trigger, label, and mesh content.

Acceptance Criteria:

- Bosses are data-driven.
- Boss fights are readable.
- Boss defeat resumes the run after the combat pause.

---

## 4.2 Tank Boss

Status: Functional Pass Implemented with Damage States and Defeat Animation; Balance QA Pending
Priority: High

Tasks:

- [x] Create production tank boss model/prefab.
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

Status: Functional Visual, Attack, Rotor Motion, and Crash Defeat Pass Implemented; Balance QA Pending
Priority: Medium

Tasks:

- [x] Create helicopter boss prefab.
- [x] Add lateral movement.
- [x] Add missile attack.
- [x] Add hit effects.
- [x] Add authored crash/destruction animation.

Acceptance Criteria:

- Helicopter boss is visually distinct.
- Fight differs from tank boss.
- Helicopter defeat uses crash-specific motion and staged smoke/explosion feedback.

---

## 4.4 Mech Boss

Status: Functional Visual, Shockwave, Multi-Phase, and Collapse Defeat Pass Implemented; Device Balance QA Pending
Priority: Medium

Tasks:

- [x] Create mech boss prefab.
- [x] Add multi-phase attack behavior.
- [x] Add heavy impact VFX.
- [x] Add unique defeat animation.

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

Status: Functional Pass Implemented with Unlock Levels and Milestone Value Curves; Economy Balance QA Pending
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
- [x] Add documented unlock levels for boss, obstacle, critical chance, and critical damage upgrades.
- [x] Add milestone value curves for documented Level 10/25/50/100 upgrade targets.
- [x] Add validator coverage for upgrade unlocks, caps, costs, and non-regressing values.

Acceptance Criteria:

- Upgrades use formulas from `PROGRESSION.md`.
- Upgrades persist.
- Upgrades affect gameplay.
- Upgrade UI reflects cost and level.
- Locked upgrades communicate their unlock level and cannot be purchased early.

---

## 5.3 Reward Calculation

Status: Functional Pass Implemented with Enemy, Obstacle, Boss, Bonus, Multiplier Rewards, and Automated Early-Economy Validation; Manual Balance QA Pending
Priority: High

Tasks:

- [x] Calculate level completion reward.
- [x] Calculate enemy rewards.
- [x] Calculate obstacle rewards.
- [x] Calculate boss rewards.
- [x] Calculate bonus run crate rewards.
- [x] Apply coin multipliers.
- [x] Add validator simulation for early one-upgrade-per-run affordability.

Acceptance Criteria:

- Rewards feel generous early.
- Rewards scale correctly.
- Reward screen matches actual payout.
- Automated validation catches early economy regressions.

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

Status: Functional Pass Implemented with Compact Icon Settings Control; Device Layout QA Pending
Priority: Critical

Tasks:

- Add level label.
- Add progress bar.
- Add coin counter.
- Add soldier count indicator.
- Add settings button.
- Respect safe areas.
- [x] Use compact icon-led settings control during gameplay.
- [x] Validate top-HUD settings/progress/coin/level overlap.

Acceptance Criteria:

- HUD does not block gameplay.
- HUD is readable on iPhone and iPad.
- HUD updates in real time.
- HUD is not default Unity UI.

---

## 6.2 Main Menu

Status: Functional Pass Implemented with Animated Character Showcase; Final Brand/Device Polish Pending
Priority: Critical

Tasks:

- [x] Create polished main menu.
- [x] Add play button.
- [x] Add upgrade button.
- [x] Add settings button.
- [x] Add currency display.
- [x] Add animated background or character showcase.

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

Status: Functional Pass Implemented with Procedural Music and Pooled SFX Sources; Final Audio Assets Pending
Priority: High

Tasks:

- Implement AudioManager.
- Add SFX categories.
- [x] Add music support.
- [x] Add SFX volume settings.
- [x] Add pooled audio sources.

Acceptance Criteria:

- Audio plays reliably.
- Settings affect audio.
- No overlapping audio chaos.

---

## 7.2 Required SFX

Status: Functional Pass Complete with Procedural Runtime SFX and Boss Variant Cues; Final Authored Clips and Music Pending
Priority: High

Add SFX for:

- [x] Start run
- [x] Soldier gain/loss
- [x] Button press
- [x] Gate pass
- [x] Positive gate
- [x] Negative gate
- [x] Shooting
- [x] Hit impact
- [x] Enemy defeat
- [x] Obstacle damage
- [x] Obstacle destruction
- [x] Coin collect
- [x] Upgrade purchase
- [x] Victory
- [x] Defeat
- [x] Boss intro
- [x] Boss attack
- [x] Boss pattern-specific attack cues
- [x] Helicopter crash cue
- [x] Boss defeat

Acceptance Criteria:

- Every major action has audio feedback.
- Audio improves satisfaction.

---

## 7.3 VFX System

Status: Functional Pass Implemented with Pooled Particle Prefabs and Boss Telegraph Support; Library Expansion Pending
Priority: High

Tasks:

- Implement VFXManager.
- Implement pooled effects.
- Add reusable effect prefabs.
- Add effect spawn helpers.
- [x] Add radius/color/duration-controlled pooled boss telegraph helper.

Acceptance Criteria:

- VFX are pooled.
- VFX do not create performance spikes.
- Effects match visual style.

---

## 7.4 Required VFX

Status: Functional Pass Implemented with Boss Telegraph Readability; Final Authored Styling Pending
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
- [x] Boss attack telegraph
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

Status: Functional Pass Implemented with Player/Enemy Equipment, Materials, Animations, and Team Variants; Final Authored Polish and Device QA Pending
Priority: Critical

Tasks:

- [x] Replace all placeholder player units.
- [x] Replace all placeholder enemy units.
- [x] Add final materials.
- [x] Add final animations.
- [x] Add team variants.

Acceptance Criteria:

- No placeholder character assets remain.
- Characters match `VISUAL_STYLE.md`.

---

## 8.2 Environment Asset Pass

Status: Functional Pass Implemented with Road, Bridge, Ocean, Harbor Background, Finish, and Bonus Platform Assets; Device QA Pending
Priority: Critical

Tasks:

- [x] Replace placeholder roads.
- [x] Replace placeholder bridges.
- [x] Replace placeholder ocean.
- [x] Add background details.
- [x] Add finish platform.
- [x] Add bonus platform.

Acceptance Criteria:

- Gameplay scene looks production-ready.
- No graybox environment remains.

---

## 8.3 UI Art Pass

Status: In Progress; Generated Sprite UI Art Pass, Branded iOS App Icon, HUD Icon Polish, Main Menu Layout Polish, and Animated Character Showcase Implemented; Final Font/Brand Polish Pending
Priority: Critical

Tasks:

- Replace default UI.
- [x] Add custom button visuals.
- [x] Add custom panels.
- [x] Add icons.
- [x] Add coin icon.
- [x] Add upgrade icons.
- [x] Add settings icons.
- [x] Add branded iOS app icon.
- [x] Fix upgrade tile text/icon crowding on simulator-sized portrait screens.
- [x] Correct play button icon direction.
- [x] Add animated main-menu character showcase/background.
- [x] Add compact icon-led gameplay HUD settings button.

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

Status: In Progress; Runtime Frame Pacing, Pool Prewarm, Formation Update, Runtime Dependency Wiring, Instanced Material, Level Fixture Pooling, and Pool Validation Baseline Implemented
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
- [x] Wire level-spawned combat, finish, and bonus objects with cached `RunManager` references instead of repeated runtime scene searches.
- [x] Remove runtime scene-search fallbacks from static VFX and camera shake dispatch.
- [x] Cache pooled-object component lookups for repeated projectile, soldier, floating text, and particle checkouts.
- [x] Remove redundant crowd-label rotation updates and share main-camera lookup across billboard labels.
- [x] Share main-camera lookup across pooled floating text feedback.
- [x] Pool level fixtures for track segments, gates, enemy groups, obstacles, bosses, finish triggers, bonus crates, and bonus-end triggers across level rebuilds.

Acceptance Criteria:

- Game targets 60 FPS.
- No major frame spikes.
- Large crowds remain playable.
- Level object spawning avoids avoidable scene-wide dependency lookups during level construction.
- Runtime gameplay scripts avoid scene-wide object searches in normal play.
- Pool checkouts avoid repeated `GetComponent<T>()` lookups for cached pooled components.
- World-space labels avoid duplicate per-frame camera/rotation work.
- Pooled floating text avoids per-frame `Camera.main` lookups.
- Level rebuilds reuse pooled fixture objects instead of destroying and instantiating the full level layout.

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

Status: Latest Post-Boss Unity Xcode Export, Signing-Independent Xcode Simulator Build, App Icon Packaging, Simulator Install/Launch, Screenshot, and Runtime Error Scan Passed; Physical Device Launch Pending
Priority: Critical

Tasks:

- [x] Export Unity project to Xcode.
- [x] Confirm Xcode project builds without signing requirements.
- [x] Confirm app launches in simulator if available.
- [ ] Confirm app launches on device if available.
- [x] Fix signing-independent iOS build errors.
- [x] Assign branded iOS app icon across Unity iOS icon slots.
- [x] Confirm simulator Xcode build no longer reports missing 1024x1024 App Store icon.
- [x] Run refreshed post-mech Xcode simulator build, install, launch, screenshot, and targeted runtime log scan.
- [x] Run refreshed post-boss Xcode simulator build, install, launch, screenshot, and targeted runtime log scan after boss telegraph, boss audio, and floating-text optimization work.

Acceptance Criteria:

- [x] Xcode project builds without signing-independent errors.
- [x] App launches successfully in simulator.
- [ ] App launches successfully on a physical device.
- [x] No missing signing-independent build issues.
- [x] Generated Xcode asset catalog contains the required App Store icon.
- [x] Latest simulator launch log scan finds no Unity exception, missing-reference, null-reference, argument-exception, or crash signatures.

---

# Phase 10: QA and Release Polish

## 10.1 Gameplay QA

Status: Automated Level Data QA Baseline Implemented; Manual Playthrough Pending
Priority: Critical

Tasks:

- [x] Add automated authored level data validation.
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

Status: In Progress; Simulator Input System Console Exception Fixed; Missing App Store Icon Warning Fixed; Main Menu UI Crowding Fixed; Post-Boss Simulator Smoke Test Passed
Priority: Critical

Tasks:

- [x] Fix simulator launch `StandaloneInputModule` exception under Input System-only player settings.
- [x] Fix missing 1024x1024 iOS App Store icon warning in simulator Xcode build.
- [x] Fix main-menu upgrade tile text/icon crowding on simulator-sized portrait screens.
- [x] Correct reversed play icon sprite.
- [x] Confirm post-mech simulator launch has no Unity runtime exception signatures.
- [x] Confirm post-boss simulator launch has no Unity runtime exception signatures after latest VFX/audio/optimization changes.
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
