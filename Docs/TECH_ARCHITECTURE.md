# ArmyRush — TECH_ARCHITECTURE.md

## Purpose of This Document

This file is the primary technical architecture guide for Codex Goal Mode. Codex must read this file before making any code, scene, prefab, asset, or configuration changes.

The goal is to build a polished iOS-first Unity runner game inspired by the onboarding/action-runner portion of games like Last War: Survival: fast forward movement, lane dodging, troop crowd growth, gate math, auto combat, enemy waves, obstacles, coins, upgrades, and short satisfying levels.

This project must not copy proprietary code, assets, branding, character models, UI, names, logos, or exact protected art from any existing game. It should replicate the broad gameplay feel and mobile design language while using original naming, original assets, original code, and original data.

Official store descriptions for Last War emphasize quick reflexes, strategic thinking, dodging and combating waves of zombies, lane-based obstacles, and mobile survival action layered with progression and strategy systems. ArmyRush V1 focuses only on the runner/action portion, not the full strategy/base-building meta. Sources reviewed before drafting this architecture include the official App Store listing, Google Play listing, official Last War website, and third-party gameplay guides describing the game as a mix of shoot-em-up action, base management, heroes, troops, squads, and progression.

---

## Codex Operating Rules

### Mandatory Read Order

Before implementing anything, Codex must read these docs in this order:

1. `Docs/TECH_ARCHITECTURE.md`
2. `Docs/GAME_VISION.md`
3. `Docs/GAMEPLAY_LOOP.md`
4. `Docs/VISUAL_STYLE.md`
5. `Docs/LEVEL_DESIGN.md`
6. `Docs/UI_UX.md`
7. `Docs/PROGRESSION.md`
8. `Docs/MONETIZATION.md`
9. `Docs/TASKS.md`
10. `Docs/DEVLOG.md`

If a file does not exist yet, Codex should create it only when explicitly instructed or when the task requires it.

### Mandatory Documentation Updates

After every meaningful implementation pass, Codex must update:

- `Docs/TASKS.md`
- `Docs/DEVLOG.md`

`TASKS.md` should reflect remaining work, completed work, bugs, follow-up improvements, and test needs.

`DEVLOG.md` should summarize what changed, what files were touched, what systems were added, what is unverified, and what should be tested manually in Unity.

### No Silent Architecture Drift

Codex must not invent a new architecture if it conflicts with this document. If the current codebase has drifted from this architecture, Codex should either:

- refactor toward this architecture, or
- document why it could not safely do so in `DEVLOG.md`.

### One-Pass Goal Mode Bias

Because this project may be built through one large Codex Goal Mode prompt, the architecture must support a complete first playable build in a single implementation run. Codex should prioritize a stable playable vertical slice over experimental polish.

### Build Target

Primary platform:

- Unity 6
- Universal Render Pipeline
- iOS
- Portrait orientation
- iPhone + iPad compatible
- 60 FPS target on modern iPhones
- Playable in Unity Editor with mouse drag controls
- Playable on mobile with touch drag controls

Secondary platform:

- macOS editor play mode only for development and debugging

Do not prioritize Android, WebGL, PC, or console.

---

## Product Scope

### V1 Product Definition

ArmyRush V1 is a portrait mobile runner where the player controls a growing crowd of blue soldiers moving forward automatically down a straight track. The player drags left and right to steer the crowd through good gates, avoid bad gates, shoot red enemies, destroy light obstacles, collect coins, and reach an end-zone reward sequence.

### V1 Includes

- Portrait 3D runner camera
- Automatic forward movement
- Player drag left/right steering
- Player soldier crowd
- Gate math system
- Additive gates such as `+5`, `+10`, `+25`
- Multiplicative gates such as `x2`, `x3`
- Negative gates such as `-5`, `-10`, `/2`, or damage zones if level design uses them
- Enemy crowds
- Auto-shoot combat
- Simple bullet projectiles or hitscan-like projectile visuals
- Obstacles with health
- Level end platform
- Coins
- Upgrade menu
- Starting Troops upgrade
- Damage upgrade
- Fire Rate upgrade
- Coin Reward upgrade
- Level progression
- Simple save/load
- Basic UI
- Basic audio hooks
- Basic haptics hooks
- 20 sample levels or procedural level data sufficient to test progression

### V1 Does Not Include

- Base building
- Real-time multiplayer
- Alliances
- PvP
- Full hero collection
- Gacha
- LiveOps events
- Real ads implementation
- Real IAP implementation
- Backend server
- Cloud save
- User accounts
- Push notifications
- Exact Last War branding, names, logos, or assets

---

## High-Level System Architecture

The project should be organized as modular feature systems under `Assets/_Project/`.

The desired folder tree is:

```text
Assets/
  _Project/
    Art/
      Materials/
      Models/
      Particles/
      UI/
    Audio/
      Music/
      SFX/
    Prefabs/
      Core/
      Player/
      Enemies/
      Gates/
      Obstacles/
      Levels/
      UI/
      VFX/
    Scenes/
      Boot.unity
      MainMenu.unity
      Game.unity
    Scripts/
      Core/
      Input/
      Camera/
      Player/
      Crowd/
      Combat/
      Gates/
      Enemies/
      Obstacles/
      Level/
      Economy/
      Progression/
      UI/
      Save/
      Audio/
      Haptics/
      Utility/
    ScriptableObjects/
      Levels/
      Upgrades/
      Economy/
      Tuning/
    Settings/
    Shaders/
    Tests/
```

Keep Unity-generated packages and settings outside `_Project` unchanged unless necessary.

---

## Scene Architecture

### Scene 1: Boot

Path:

```text
Assets/_Project/Scenes/Boot.unity
```

Purpose:

- Initialize game services
- Load save data
- Initialize audio, haptics, economy, progression, level state
- Then load MainMenu or Game depending on implementation

V1 can skip a visible boot screen if the systems are lightweight, but the `Boot` scene should exist for clean architecture.

Required objects:

- `GameBootstrapper`
- `ServiceRoot`
- Optional loading canvas

### Scene 2: MainMenu

Path:

```text
Assets/_Project/Scenes/MainMenu.unity
```

Purpose:

- Display title
- Show coins
- Show current upgrade buttons
- Start run button
- Show current level number

V1 may combine MainMenu and Game scene if needed, but the architecture should still separate menus from gameplay logic.

### Scene 3: Game

Path:

```text
Assets/_Project/Scenes/Game.unity
```

Purpose:

- Actual runner gameplay
- Track generation or level loading
- Player crowd control
- Gates
- Enemies
- Obstacles
- End sequence
- Runtime HUD

Required objects:

- `GameRoot`
- `LevelRoot`
- `PlayerRoot`
- `CameraRig`
- `RuntimeCanvas`
- `EnvironmentRoot`
- `LightingRoot`

---

## Manager and Service Architecture

Use simple MonoBehaviour-based managers for V1. Avoid over-engineered dependency injection frameworks.

### Core Service List

Required services:

- `GameBootstrapper`
- `GameStateManager`
- `SceneLoader`
- `SaveService`
- `EconomyService`
- `ProgressionService`
- `UpgradeService`
- `AudioService`
- `HapticsService`
- `SettingsService`

Gameplay managers:

- `RunManager`
- `LevelManager`
- `TrackManager`
- `PlayerController`
- `CrowdManager`
- `CombatManager`
- `GateManager`
- `EnemyManager`
- `ObstacleManager`
- `RewardManager`
- `CameraFollowRig`
- `UIManager`

### Manager Lifetime Rules

Persistent across scenes:

- `GameBootstrapper`
- `SaveService`
- `EconomyService`
- `ProgressionService`
- `UpgradeService`
- `AudioService`
- `HapticsService`
- `SettingsService`

Scene-specific:

- `RunManager`
- `LevelManager`
- `TrackManager`
- `PlayerController`
- `CrowdManager`
- `CombatManager`
- `GateManager`
- `EnemyManager`
- `ObstacleManager`
- `RewardManager`
- `CameraFollowRig`
- `UIManager`

### Service Access Pattern

For V1, use a simple static service locator only if necessary:

```csharp
ServiceLocator.Get<T>()
```

But prefer serialized references in scene roots where practical.

Do not create hard-to-test global singletons everywhere. If a singleton is used, it must be minimal and documented.

---

## Code Style Rules

### General C# Style

- Use C# conventions.
- Public class names use PascalCase.
- Private fields use `_camelCase`.
- Serialized private fields use `[SerializeField] private`.
- Constants use PascalCase or ALL_CAPS only when truly static constants.
- Avoid magic numbers inside gameplay scripts.
- Put gameplay tunables in ScriptableObjects or serialized settings.
- Keep MonoBehaviours focused.
- Avoid one giant `GameManager` that does everything.
- Prefer events for cross-system notifications.

### Example Field Style

```csharp
[SerializeField] private float _forwardSpeed = 8f;
[SerializeField] private float _laneWidth = 2.4f;
private bool _isRunning;
```

### Namespaces

Use one root namespace:

```csharp
namespace ArmyRush
```

Sub-namespaces are allowed:

```csharp
namespace ArmyRush.Player
namespace ArmyRush.Combat
namespace ArmyRush.Levels
namespace ArmyRush.UI
```

### Comments

Comments should explain why, not obvious what.

Good:

```csharp
// We clamp lateral movement to keep the crowd inside the track even on high swipe sensitivity.
```

Bad:

```csharp
// Add speed to position.
```

### Error Handling

- Log missing references clearly.
- Avoid null reference crashes where simple validation is possible.
- Use `Debug.LogWarning` for recoverable missing optional content.
- Use `Debug.LogError` for required missing references.

---

## Assembly Definition Plan

For V1, assembly definitions are optional. If Codex can create them cleanly, use:

```text
Assets/_Project/Scripts/ArmyRush.Runtime.asmdef
Assets/_Project/Tests/ArmyRush.Tests.asmdef
```

If asmdefs create build complexity, skip them. Stable compilation matters more than assembly structure in the first Goal Mode pass.

---

## Input Architecture

The project currently includes Unity Input System package assets from the template. Use Unity's Input System if it is already installed and configured, but keep controls simple.

### Required Input Behavior

The player should:

- Move forward automatically.
- Drag left/right to steer the crowd horizontally.
- Release finger without stopping forward movement.
- Never need a virtual joystick.
- Be playable with mouse drag in editor.

### Control Model

Use normalized horizontal drag input:

```text
Input value range: -1 to +1
-1 = full left drag intent
+1 = full right drag intent
0 = neutral/no drag
```

### Mobile Touch

- Track first active touch.
- Calculate delta X from screen movement.
- Convert delta X to world-space lateral movement.
- Smooth movement to avoid jitter.

### Editor Mouse

- Mouse down begins drag.
- Mouse move controls lateral position.
- Mouse up stops lateral input.

### Input Script Responsibilities

`RunnerInputController`:

- Reads touch/mouse
- Produces lateral delta or target position
- Does not move the player directly

`PlayerController`:

- Consumes input value
- Moves the player root
- Clamps the player to track bounds

---

## Camera Architecture

### Camera Style

The camera should mimic a polished mobile runner:

- Portrait framing
- Slightly elevated third-person view
- Looking forward down the track
- Player crowd near bottom third of screen
- Gates/enemies visible ahead
- Smooth follow
- No player-controlled rotation

### Camera Rig

Required object:

```text
CameraRig
  Main Camera
```

Required script:

```text
CameraFollowRig.cs
```

### Camera Settings

Suggested starting values:

```text
Position Offset: (0, 10, -9)
Rotation: X = 55 to 65 degrees, Y = 0, Z = 0
Field of View: 50 to 60
Follow Smoothing: 8 to 12
Look Ahead Distance: 5 to 8
```

### Camera Rules

- Camera follows player forward movement.
- Camera smoothly follows lateral movement but less aggressively than player movement.
- Camera should not shake excessively.
- Camera can add small impact shake for big combat/boss/end events.
- Camera must frame gates early enough for the player to react.

---

## Player Architecture

### Player Root

Prefab path:

```text
Assets/_Project/Prefabs/Player/PlayerRoot.prefab
```

Hierarchy:

```text
PlayerRoot
  CrowdAnchor
  AimOrigin
  CollisionProxy
```

Scripts:

- `PlayerController`
- `CrowdManager`
- `PlayerCombatController`
- `PlayerHealthOrCountController`

### PlayerController Responsibilities

- Owns forward auto movement.
- Owns lateral movement.
- Owns bounds/clamping.
- Knows whether run is active, paused, won, lost.
- Does not handle gate math directly.
- Does not spawn soldiers directly except through `CrowdManager`.

### Movement Parameters

Initial values:

```text
Forward Speed: 7.5
Lateral Sensitivity: 0.018 world units per screen pixel
Lateral Smooth Time: 0.08
Track Half Width: 3.2
Acceleration: instant or lightly smoothed
```

### Run States

Use clear run states:

```csharp
public enum RunState
{
    None,
    PreRun,
    Running,
    CombatPaused,
    FinishSequence,
    Victory,
    Defeat,
    Results
}
```

V1 can keep movement continuous during combat unless level design requires stop points.

---

## Crowd Architecture

### Crowd Definition

The player crowd is the main visible unit group. It represents the player's soldier count.

### Crowd Goals

- Show many small blue units.
- Keep performance stable.
- Make count changes instantly satisfying.
- Visually arrange soldiers in a packed formation.
- Avoid physics-heavy individual characters.

### Soldier Unit Prefab

Path:

```text
Assets/_Project/Prefabs/Player/SoldierUnit_Blue.prefab
```

Suggested hierarchy:

```text
SoldierUnit_Blue
  Body
  Helmet
  Weapon
  MuzzlePoint
```

Scripts:

- `SoldierUnitVisual`
- optional `SoldierShooter`

### CrowdManager Responsibilities

- Track current soldier count.
- Add soldiers after positive gates.
- Remove soldiers after damage, bad gates, or enemy collisions.
- Spawn/recycle soldier visuals.
- Arrange formation positions.
- Notify UI when count changes.
- Notify RunManager on zero soldiers.

### Recommended Count Range

V1 should support:

```text
Minimum: 1
Typical: 10 to 150
Soft visual cap: 200
Hard cap: 300
```

If count exceeds visual cap, show limited visuals and preserve logical count in UI.

### Formation Algorithm

Use grid/rings/packed rows relative to `CrowdAnchor`.

Recommended simple formation:

- Arrange soldiers in rows.
- Each row has increasing width until max columns reached.
- Soldiers are centered around player root.
- Row depth extends backward from the front.

Example:

```text
Front row: 5 units
Rows behind: 6-8 units
Spacing: 0.42 to 0.55 units
```

### Formation Behavior

- Soldiers should not use individual NavMesh.
- Soldiers should interpolate to target slots.
- Soldiers should bob/run using simple animation.
- Soldiers can face forward by default.
- During shooting, they can face target direction lightly.

### Object Pooling

Use pooling for soldier visuals.

Required class:

```text
ObjectPool<T>
```

or simple `SoldierPool`.

Avoid `Instantiate`/`Destroy` every time count changes during gameplay if the count may change rapidly.

---

## Combat Architecture

### Combat Style

ArmyRush uses automatic firing while the crowd moves forward. Combat should feel like the screenshot-style runner:

- Blue soldiers shoot forward automatically.
- Red enemies or zombies are ahead on the track.
- Projectiles streak forward.
- Hits create small spark/impact effects.
- Enemy count decreases as shots land.
- If enemies reach the player crowd or player reaches them, close-range attrition may remove player soldiers.

### Combat Systems

Required scripts:

- `PlayerCombatController`
- `SoldierShooter`
- `Projectile`
- `ProjectilePool`
- `Damageable`
- `EnemyUnit`
- `EnemyGroup`
- `CombatTargetingSystem`

### Damage Model

Use simple integer health.

```text
Soldier Damage = base damage + upgrade damage
Projectile Damage = Soldier Damage
Enemy Health = level-defined value
Obstacle Health = level-defined value
```

### Fire Rate Model

Fire rate should be affected by upgrades.

```text
Base Fire Interval: 0.35 seconds
Upgrade reduces interval or increases shots per second
Minimum Fire Interval: 0.08 seconds
```

### Targeting Rules

Priority order:

1. Enemy directly ahead and closest
2. Obstacle directly ahead and closest
3. Boss/end target
4. No target: shoot forward only if visual polish needs it

### Projectile Rules

- Use simple forward-moving projectile visuals.
- Use trigger collision or raycast step.
- Must be pooled.
- Lifetime should be capped.
- Projectile speed should be fast enough to feel responsive.

Suggested values:

```text
Projectile Speed: 18 to 28
Projectile Lifetime: 1.5 seconds
Projectile Radius: 0.08
```

### Hit Feedback

On hit:

- Spawn small spark impact.
- Play light hit SFX with cooldown.
- Add tiny enemy flash or scale punch.
- Reduce health.
- If health <= 0, despawn enemy/obstacle with pop effect.

### Player Damage

Player loses soldiers when:

- Passing through negative gates.
- Colliding with enemy units.
- Enemy group survives long enough to reach player.
- Obstacle collision if not destroyed.

Use count-based damage rather than HP bar in V1.

---

## Enemy Architecture

### Enemy Types

V1 enemy types:

1. `BasicRedSoldier`
2. `RunnerZombie` if zombie theme is used later
3. `ShieldEnemy` optional
4. `MiniBoss` optional

Since V1 is near Last War-style military runner, use red soldiers for enemies.

### Enemy Unit Prefab

Path:

```text
Assets/_Project/Prefabs/Enemies/EnemyUnit_Red.prefab
```

Hierarchy:

```text
EnemyUnit_Red
  Body
  Helmet
  Weapon
  HitCollider
```

Scripts:

- `EnemyUnit`
- `Damageable`

### EnemyGroup Prefab

Path:

```text
Assets/_Project/Prefabs/Enemies/EnemyGroup.prefab
```

Responsibilities:

- Holds multiple enemy units.
- Displays group count if needed.
- Manages formation.
- Notifies LevelManager when cleared.

### Enemy Behavior

For V1:

- Most enemies stand or slowly advance.
- Enemies may shoot simple red projectiles later, but V1 can focus on player auto-shooting.
- Enemies die when health reaches zero.
- If player crowd physically reaches enemy group, resolve attrition quickly.

### Enemy Formation

Use simple grid formation similar to player but smaller.

Suggested group sizes:

```text
Early levels: 5 to 20 enemies
Mid levels: 20 to 60 enemies
Later V1: 60 to 120 enemies
```

---

## Gate Architecture

### Gate Definition

Gates are level objects that modify the player's soldier count or combat stats when passed through.

### Gate Types

```csharp
public enum GateOperation
{
    Add,
    Subtract,
    Multiply,
    Divide,
    SetMinimum,
    DamageBoost,
    FireRateBoost,
    CoinBoost
}
```

V1 required:

- Add
- Subtract
- Multiply
- Divide

Optional:

- DamageBoost
- FireRateBoost
- CoinBoost

### Gate Prefab

Path:

```text
Assets/_Project/Prefabs/Gates/Gate.prefab
```

Hierarchy:

```text
Gate
  LeftPost
  RightPost
  Panel
  TextLabel
  Trigger
  VFX_Good
  VFX_Bad
```

Scripts:

- `GateController`
- `GateVisual`

### Gate Visual Rules

Good gates:

- Blue/cyan panel
- Text like `+10`, `x2`, `+25`
- Positive glow

Bad gates:

- Red/orange panel
- Text like `-10`, `/2`
- Danger glow

### Gate Trigger Rules

- Gate triggers once per run.
- Gate applies only when the player/crowd root crosses it.
- Gate should not apply multiple times due to multiple soldier colliders.
- Gate should play feedback instantly.

### Gate Calculation Rules

Examples:

```text
Current 10 +5 = 15
Current 15 x2 = 30
Current 30 -8 = 22
Current 22 /2 = 11
```

Minimum count after gate:

```text
0 allowed only if it causes defeat intentionally.
Usually clamp to 1 unless gate is designed to kill.
```

### Gate Choice Layout

Most gate moments should be paired choices:

```text
Left lane: +5
Right lane: x2
```

or

```text
Left lane: -8
Right lane: +10
```

Use track width and lanes so the choice is readable.

---

## Obstacle Architecture

### Obstacle Purpose

Obstacles create short pressure moments where player crowd must shoot/destroy barriers or avoid them.

### Obstacle Types

V1:

- Barrel stack
- Barricade
- Crate wall
- Explosive barrel optional

### Obstacle Prefab

Path:

```text
Assets/_Project/Prefabs/Obstacles/Obstacle_Barrier.prefab
```

Scripts:

- `ObstacleController`
- `Damageable`

### Obstacle Rules

- Has health.
- Takes projectile damage.
- Can block track lane partially or fully.
- If destroyed before collision, player passes safely.
- If collision occurs, player loses soldiers or run ends depending on obstacle type.

### Obstacle Feedback

On damage:

- Flash material.
- Small chip particles.

On destroyed:

- Pop/explosion effect.
- Coin burst optional.
- Remove collider.
- Despawn chunks or play simple animation.

---

## Level Architecture

### Level Data Strategy

Use ScriptableObjects for level definitions.

Path:

```text
Assets/_Project/ScriptableObjects/Levels/Level_001.asset
```

Required data class:

```text
LevelData.cs
```

LevelData should define:

- Level index
- Track length
- Starting soldier count override optional
- List of gate placements
- List of enemy group placements
- List of obstacle placements
- Coin reward baseline
- Difficulty rating
- End sequence type

### Level Element Data

Use serializable data structs/classes:

- `GateSpawnData`
- `EnemyGroupSpawnData`
- `ObstacleSpawnData`
- `CoinLineSpawnData`
- `EndZoneData`

Each spawn data should include:

- Distance along track/Z position
- X lane/lateral position
- Prefab reference or type enum
- Values such as gate amount, enemy count, health

### Track Coordinate System

Use Unity coordinates:

```text
Z = forward direction
X = horizontal lane movement
Y = vertical/up
```

Track center:

```text
X = 0
```

Track width:

```text
Usable X range: -3.2 to +3.2
```

### Level Root

Runtime generated hierarchy:

```text
LevelRoot
  TrackSegments
  Gates
  Enemies
  Obstacles
  Coins
  EndZone
```

### Level Generation

`LevelManager` should load LevelData and spawn prefabs at runtime.

V1 can also use one handcrafted scene layout if ScriptableObjects take too long, but the preferred architecture is data-driven.

### Level Count

V1 target:

```text
20 playable levels
```

Codex should create at least enough LevelData assets or procedural fallback logic to prove progression works.

---

## Track Architecture

### Track Visual

Use modular track segments.

Prefab path:

```text
Assets/_Project/Prefabs/Levels/TrackSegment.prefab
```

Track style:

- Smooth gray road/concrete surface
- Bright clean edges
- Railings or side barriers
- Floating/bridge-like segments optional
- Clear lane readability

### TrackManager Responsibilities

- Build track length based on LevelData.
- Place side rails.
- Place background props.
- Ensure player bounds match track width.

### Track Dimensions

Suggested values:

```text
Track Width: 7.0
Usable Half Width: 3.1
Segment Length: 10.0
Level Length Early: 90 to 130
Level Length Later: 150 to 220
```

---

## End Zone Architecture

### End Sequence Goals

When the player reaches the end:

- Stop normal movement.
- Convert remaining soldier count into reward pressure.
- Show satisfying end action.
- Award coins.
- Move to results screen.

### V1 End Sequence Options

Preferred simple version:

- Player reaches finish platform.
- Remaining soldiers run/shoot at final target.
- Bonus multiplier increases based on remaining count.
- Coins awarded.

Alternative simple version:

- Player crosses finish line.
- Remaining count directly multiplies coin reward.

### EndZone Scripts

- `FinishLineTrigger`
- `EndSequenceController`
- `RewardManager`

### Reward Formula

Initial formula:

```text
CoinsEarned = BaseLevelReward + RemainingSoldiers * SoldierCoinValue + BonusCoins
```

Example:

```text
BaseLevelReward = 50
SoldierCoinValue = 2
RemainingSoldiers = 75
CoinsEarned = 200
```

---

## Economy Architecture

### Currency

V1 currency:

```text
Coins
```

Optional future:

```text
Gems
Tickets
Keys
```

### EconomyService Responsibilities

- Store coin balance.
- Add coins.
- Spend coins.
- Validate affordability.
- Notify UI.
- Save changes.

### Coin Events

Coins earned from:

- Level completion
- Destroyed obstacles optional
- Bonus end sequence

Coins spent on:

- Starting troops upgrade
- Damage upgrade
- Fire rate upgrade
- Coin reward upgrade

---

## Progression Architecture

### PlayerProgressData

Saved data should include:

```text
CurrentLevelIndex
CoinBalance
StartingTroopsLevel
DamageLevel
FireRateLevel
CoinRewardLevel
CompletedLevels
TutorialFlags
Settings
```

### Save File

Use Unity `PlayerPrefs` for V1 simplicity.

Create `SaveService` that serializes to JSON and stores in PlayerPrefs.

Key:

```text
ArmyRush_SaveData_v1
```

### SaveService Requirements

- Load on boot.
- Create default data if none exists.
- Save after spending coins.
- Save after completing a level.
- Save after changing settings.
- Provide reset function for debugging.

### Upgrade Data

Use ScriptableObjects for upgrade curves.

Path:

```text
Assets/_Project/ScriptableObjects/Upgrades/
```

Required upgrades:

1. Starting Troops
2. Damage
3. Fire Rate
4. Coin Reward

Each upgrade config should define:

- Upgrade ID
- Display name
- Description
- Base cost
- Cost scaling
- Base value
- Value per level or curve
- Max level optional

---

## UI Architecture

### UI Scenes

Runtime Canvas should exist in Game scene.

MainMenu Canvas should exist in MainMenu scene or be instantiated by UIManager.

### Required Screens

V1 screens:

- Main Menu
- HUD
- Pause optional
- Results/Victory
- Defeat
- Upgrade Panel

### HUD Elements

During run:

- Soldier count at top or above crowd
- Level progress bar
- Coin preview optional
- Current level number
- Optional pause button

### Main Menu Elements

- Game title
- Current coins
- Start button
- Upgrade buttons
- Current level

### Upgrade Button Anatomy

Each upgrade button should show:

- Upgrade name
- Current level
- Current effect
- Cost
- Button enabled/disabled based on coins

### UI Scaling

Must support:

- iPhone portrait
- iPad portrait
- Safe areas

Use Canvas Scaler:

```text
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1080 x 1920
Screen Match Mode: Match Width Or Height
Match: 0.5
```

### Safe Area

Use a SafeArea script to keep UI out of notches/status bars.

Even though status bar is hidden, still respect device cutouts.

---

## Audio Architecture

### AudioService Responsibilities

- Play UI SFX
- Play gameplay SFX
- Manage music volume
- Manage SFX volume
- Respect settings

### Required SFX Hooks

- Button tap
- Gate positive
- Gate negative
- Soldier added
- Soldier lost
- Shoot loop or randomized shots
- Projectile hit
- Enemy destroyed
- Obstacle destroyed
- Coin reward
- Victory
- Defeat

### Audio Implementation

Use simple AudioSource pooling.

Do not play one audio source per soldier; aggregate shooting sound with cooldown or loop.

---

## Haptics Architecture

### HapticsService Responsibilities

- Light tap for UI
- Light impact for positive gate
- Warning impact for negative gate
- Medium impact for obstacle destroyed
- Success haptic for victory
- Failure haptic for defeat

V1 can stub haptics in editor.

On iOS, use Unity Handheld vibration only if no native haptics plugin exists.

---

## VFX Architecture

### Required VFX

- Gate pass burst positive
- Gate pass burst negative
- Muzzle flash
- Projectile trail
- Hit sparks
- Enemy pop/death
- Obstacle destruction
- Coin burst
- Victory confetti optional

### Performance Rule

Use simple particle systems with low emission counts.

Avoid expensive transparent overdraw covering the whole screen.

Pool frequently used VFX.

---

## Object Pooling Architecture

### Pooling Required For

- Soldier visuals
- Enemy visuals if dynamically spawned
- Projectiles
- Hit VFX
- Floating text
- Coin pickups if used

### Pool Manager

Required script:

```text
PoolManager.cs
```

Can be generic or specific pools.

### Pool Rules

- Prewarm common pools at level start.
- Avoid Destroy during gameplay.
- Return objects on death/lifetime end.
- Parent pooled objects under `PoolRoot`.

---

## Physics Architecture

### Physics Use

Use physics sparingly.

Recommended:

- Trigger colliders for gates.
- Trigger colliders for finish line.
- Simple collider for obstacles.
- Projectile trigger or raycast.

Avoid:

- Rigidbody physics for every soldier.
- NavMesh.
- Ragdolls.
- Complex mesh colliders.

### Layers

Create these layers if possible:

```text
Player
PlayerUnit
Enemy
Obstacle
Gate
Projectile
Track
Collectible
```

### Collision Matrix

Optimize collisions so unnecessary layers do not interact.

Examples:

- Player trigger detects Gate, Enemy, Obstacle, Finish.
- Projectiles detect Enemy and Obstacle.
- Player units do not collide with each other.
- Enemy units do not collide with each other.

---

## Data Architecture

### ScriptableObject Usage

Use ScriptableObjects for:

- Global game tuning
- Level data
- Upgrade configs
- Enemy configs
- Obstacle configs
- Gate visual configs

### GlobalTuning Asset

Path:

```text
Assets/_Project/ScriptableObjects/Tuning/GlobalTuning.asset
```

Data:

- Player forward speed
- Lateral sensitivity
- Track bounds
- Soldier spacing
- Max visual soldiers
- Projectile speed
- Base damage
- Base fire interval
- Coin formula values
- Camera offsets

### Why ScriptableObjects

They allow balancing without code changes and help Codex separate tuning from logic.

---

## Prefab Architecture

### Required Prefabs

Core:

- `GameBootstrapper.prefab`
- `GameRoot.prefab`
- `ServiceRoot.prefab`

Player:

- `PlayerRoot.prefab`
- `SoldierUnit_Blue.prefab`

Enemies:

- `EnemyUnit_Red.prefab`
- `EnemyGroup.prefab`

Gates:

- `Gate_Add.prefab`
- `Gate_Multiply.prefab`
- `Gate_Subtract.prefab`
- `Gate_Divide.prefab`

Obstacles:

- `Obstacle_Barrier.prefab`
- `Obstacle_Barrel.prefab`

Level:

- `TrackSegment.prefab`
- `FinishLine.prefab`
- `EndPlatform.prefab`

UI:

- `MainMenuCanvas.prefab`
- `HUDCanvas.prefab`
- `ResultsPanel.prefab`
- `UpgradeButton.prefab`

VFX:

- `VFX_GatePositive.prefab`
- `VFX_GateNegative.prefab`
- `VFX_HitSpark.prefab`
- `VFX_MuzzleFlash.prefab`
- `VFX_EnemyPop.prefab`

---

## Materials and Visual Optimization

### Material Strategy

Use few shared materials:

- `MAT_PlayerBlue`
- `MAT_EnemyRed`
- `MAT_TrackGray`
- `MAT_GateBlue`
- `MAT_GateRed`
- `MAT_ProjectileYellow`
- `MAT_CoinGold`
- `MAT_ObstacleBrown`
- `MAT_UIWhite`

### Rendering Rules

- Prefer URP Lit or Simple Lit.
- Avoid expensive transparent materials except VFX.
- Avoid many unique materials per unit.
- Use shared materials to support batching.
- Keep models low poly.

### Lighting

Use one Directional Light.

Use ambient lighting.

Avoid real-time shadows on hundreds of soldiers if performance drops.

For V1, shadows can be disabled on small units if needed.

---

## Performance Targets

### Frame Rate

Target:

```text
60 FPS on modern iPhones
30 FPS minimum fallback on older devices
```

### Soldier Count Performance

The game should remain stable with:

```text
150 visible player soldiers
100 visible enemies
50 projectiles active
```

If performance suffers, reduce visual count and keep logical count.

### Avoid These

- Per-frame FindObjectOfType
- Per-frame GetComponent in loops
- Creating/destroying projectiles every shot
- Individual Rigidbodies on every soldier
- Complex skinned meshes for huge crowds
- Expensive realtime shadows on all units
- Heavy post-processing

### Use These

- Object pooling
- Cached references
- Simple colliders
- Shared materials
- ScriptableObject tuning
- Minimal Update loops
- Centralized managers

---

## Mobile/iOS Configuration Notes

### Orientation

Current desired setting:

```text
Default Orientation: Portrait
Allowed Orientation: Portrait only
```

### Target Devices

Desired:

```text
iPhone + iPad
```

The game is portrait-first but should scale to iPad safely.

### Safe Area

All menus and HUD must respect safe area.

### Build Type

Development builds are okay during testing.

Release builds should disable script debugging.

### Xcode Export

Unity exports an Xcode project for iOS.

Xcode handles:

- Signing
- Device deployment
- App Store archive
- Provisioning profiles

Codex should not attempt to manage Apple Developer signing unless explicitly instructed.

---

## Save/Load Implementation Details

### Save Data Class

Create serializable class:

```csharp
[Serializable]
public class PlayerSaveData
{
    public int currentLevelIndex;
    public int coins;
    public int startingTroopsLevel;
    public int damageLevel;
    public int fireRateLevel;
    public int coinRewardLevel;
    public bool tutorialCompleted;
    public float musicVolume;
    public float sfxVolume;
    public bool hapticsEnabled;
}
```

### Default Values

```text
currentLevelIndex = 1
coins = 0
startingTroopsLevel = 0
damageLevel = 0
fireRateLevel = 0
coinRewardLevel = 0
tutorialCompleted = false
musicVolume = 1
sfxVolume = 1
hapticsEnabled = true
```

### Debug Reset

Add editor-only reset option or keyboard shortcut:

```text
R key in editor resets save only if debug enabled
```

---

## Upgrade Architecture Details

### Upgrade IDs

Use stable string or enum IDs:

```csharp
public enum UpgradeType
{
    StartingTroops,
    Damage,
    FireRate,
    CoinReward
}
```

### Upgrade Calculations

Starting Troops:

```text
Base: 10
+5 per level early
```

Damage:

```text
Base: 1
+1 every level or curve-based
```

Fire Rate:

```text
Base interval: 0.35s
Each level reduces by 4% to 8%
Minimum: 0.08s
```

Coin Reward:

```text
Base multiplier: 1.0
+0.1 per level
```

### Upgrade Cost Scaling

Suggested formula:

```text
Cost = BaseCost * pow(CostMultiplier, Level)
```

Initial base costs:

```text
Starting Troops: 75
Damage: 100
Fire Rate: 125
Coin Reward: 150
```

Cost multiplier:

```text
1.35 to 1.55
```

---

## Event Architecture

Use C# events or UnityEvents carefully.

### Important Events

```csharp
OnRunStarted
OnRunPaused
OnRunResumed
OnRunWon
OnRunLost
OnSoldierCountChanged
OnGateApplied
OnEnemyGroupCleared
OnObstacleDestroyed
OnCoinsChanged
OnUpgradePurchased
OnLevelCompleted
```

### Event Rules

- Unsubscribe on destroy.
- Avoid memory leaks.
- UI should listen to services/managers instead of polling constantly.

---

## Testing Plan

### Manual Editor Tests

Codex should leave the project in a state where these tests are possible:

1. Open Game scene.
2. Press Play.
3. Player crowd starts with visible soldiers.
4. Player moves forward automatically.
5. Mouse drag moves player left/right.
6. Passing through `+` gate increases count.
7. Passing through `x` gate multiplies count.
8. Projectiles fire at enemies.
9. Enemies lose health and despawn.
10. Obstacles take damage.
11. Finish line triggers victory/results.
12. Coins are awarded.
13. Upgrade buttons spend coins and increase stats.
14. Save persists after stopping and playing again.

### iOS Tests

After export to Xcode:

1. Build to iPhone.
2. Touch drag works.
3. UI respects safe area.
4. Portrait orientation locks correctly.
5. Performance remains smooth.
6. No major layout issues on iPad simulator/device.

---

## Debug Tools

### Debug Overlay Optional

If quick to implement, add debug overlay showing:

- FPS
- Current level
- Soldier count
- Enemy count
- Projectile count
- Coins
- Run state

### Debug Keyboard Shortcuts

Editor only:

```text
R = reset run
N = skip/win level
C = add coins
U = buy all upgrades once or test upgrade
K = kill player crowd
```

Guard with `#if UNITY_EDITOR`.

---

## Asset Creation Expectations for Codex

Codex may create primitive placeholder assets using Unity primitives:

- Capsules for soldiers
- Capsules/cubes for enemies
- Cubes for gates/posts
- Planes/cubes for track
- Cylinders/cubes for obstacles
- TextMeshPro for labels

Placeholder visuals are acceptable for the first pass as long as:

- Gameplay is functional.
- Colors are clear.
- Scale is readable.
- Prefabs are organized.
- Systems are ready for later art replacement.

### TextMeshPro

Use TextMeshPro for:

- Gate labels
- UI text
- Floating count labels

If TMP essentials need import, Codex should note it in DEVLOG if not automated.

---

## Package Expectations

Allowed packages:

- Unity Input System
- TextMeshPro
- URP packages already included
- Cinemachine optional but not required

Avoid adding external paid or unknown packages.

Do not depend on Asset Store packages for V1.

---

## Build Reliability Rules

Codex must keep the project compiling.

Before finishing, Codex should:

- Check for C# compile errors if possible.
- Avoid duplicate class names.
- Avoid missing namespaces.
- Avoid scripts referencing non-existent assets without null checks.
- Ensure scenes are included in Build Profiles/Build Settings.
- Ensure created prefabs have required scripts assigned.

If compilation cannot be verified, Codex must state so in `DEVLOG.md`.

---

## Suggested Implementation Order for Codex Goal Mode

Codex should implement in this order:

1. Create `_Project` folder structure.
2. Create core script folders.
3. Create data classes and ScriptableObject types.
4. Create save/economy/progression services.
5. Create Game scene root objects.
6. Create player movement.
7. Create crowd manager and soldier visuals.
8. Create camera follow rig.
9. Create gate system.
10. Create combat/projectile system.
11. Create enemy system.
12. Create obstacle system.
13. Create level data/level spawning.
14. Create finish/results flow.
15. Create UI screens.
16. Create upgrade system.
17. Create audio/haptics stubs.
18. Create sample levels.
19. Create debug tools.
20. Update TASKS.md.
21. Update DEVLOG.md.

---

## Minimum Playable Build Acceptance Criteria

The build is acceptable when:

- Project compiles.
- Game scene runs in editor.
- Player moves forward automatically.
- Player can steer left/right by dragging mouse/touch.
- Blue soldier crowd is visible.
- Soldier count changes from gates.
- Enemy groups appear and can be defeated by shooting.
- Obstacles appear and can be destroyed or punish player.
- Finish line ends level.
- Coins are awarded.
- At least one upgrade can be purchased.
- Progress saves.
- UI is portrait mobile-friendly.

---

## Future Phase Architecture Notes

These are not V1 requirements, but the architecture should not block them.

### Phase 2 Possible Additions

- Skins
- Boss fights
- Special weapons
- Better animations
- Daily rewards
- Ad reward hooks
- More upgrade types
- Procedural level generator
- More enemy types

### Phase 3 Possible Additions

- Base-building screen
- Hero cards
- Squad formation
- Tech tree
- Idle resource generation
- PvE campaign map

### Phase 4 Possible Additions

- Ads SDK
- IAP
- Remote config
- Analytics
- Cloud save
- LiveOps events

The V1 architecture should use services and data definitions that can grow into those systems without rewriting the whole project.

---

## Anti-Patterns Codex Must Avoid

Do not:

- Put all code in one `GameManager.cs`.
- Hardcode all levels in one MonoBehaviour.
- Use `GameObject.Find` repeatedly in Update.
- Spawn/destroy projectiles every shot.
- Give every soldier expensive physics.
- Depend on exact scene object names without validation.
- Create systems only playable in landscape.
- Ignore safe area.
- Forget to update docs.
- Use copyrighted Last War assets, logos, or names.
- Build base-building before the runner is playable.
- Add real monetization SDKs in V1.
- Add backend complexity in V1.

---

## Final Architecture Summary

ArmyRush should be a clean, modular Unity 6 URP iOS project with data-driven levels, pooled crowd/projectile systems, simple mobile input, service-based progression, and mobile-safe UI. The first playable build should focus on the satisfying Last War-style runner loop: steer a growing blue soldier crowd through math gates, auto-shoot red enemies, destroy obstacles, reach the finish, earn coins, and upgrade for the next run.

This architecture exists to make Codex Goal Mode consistent, controlled, and buildable in one large implementation pass.
