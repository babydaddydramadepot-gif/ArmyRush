# GAMEPLAY_LOOP.md

# ArmyRush Gameplay Loop Specification

Version: 2.0  
Project: ArmyRush  
Engine: Unity 6 LTS  
Target Platform: iOS, iPhone and iPad  
Orientation: Portrait-first  
Document Role: Primary gameplay source of truth  
Goal Mode Priority: Critical  
Production Quality: Required  
Prototype Quality: Not Acceptable

---

## 0. Codex Read Order

Codex must read and obey this file after reading:

1. `Docs/TECH_ARCHITECTURE.md`
2. `Docs/GAMEPLAY_LOOP.md`

This file defines what the game does moment to moment.

If any other document conflicts with this document regarding gameplay behavior, this file wins.

If this file conflicts with `TECH_ARCHITECTURE.md` on architecture, performance, folder structure, or code organization, `TECH_ARCHITECTURE.md` wins.

---

## 1. Core Product Goal

ArmyRush is a polished mobile portrait runner inspired by the early playable onboarding experience seen in Last War-style mobile ads.

The game must feel like a high-quality, App Store-ready, military crowd runner where the player leads a blue army forward through mathematical gates, grows the army, shoots enemies, destroys barricades, survives red army encounters, earns coins, and upgrades between runs.

The intended first version is not a test scene.

The intended first version is not a Unity demo.

The intended first version must feel like a real mobile game vertical slice.

The game must be immediately understandable within 3 seconds of gameplay.

The player should know:

- Blue army is mine.
- Red army is dangerous.
- Green or blue positive gates are good.
- Red negative gates are bad.
- Bigger army means stronger army.
- Reaching the end means victory.
- Coins upgrade power.

---

## 2. Non-Negotiable Production Rules

### 2.1 No Prototype Completion

Codex must not mark any gameplay feature complete if it only works with temporary visual placeholders.

The following are not acceptable as final gameplay visuals:

- Capsule soldiers
- Cube enemies
- Sphere bullets as final bullets
- Plain Unity default materials
- Default UI buttons without styling
- Debug text as final UI
- Plain gray floors as final tracks
- Default particle systems without tuning
- Randomly placed objects without level design intent
- Editor-only test controls as final controls

Temporary assets may be used only during development, but before a feature is marked complete, Codex must replace them with polished stylized assets made from simple optimized meshes, custom materials, clean prefabs, and mobile-ready visual presentation.

### 2.2 No Hidden TODO Gameplay

Codex must not leave gameplay systems half-finished with comments such as:

- TODO add later
- placeholder for now
- temporary balancing
- fake upgrade
- mock reward
- not implemented

If a system is referenced as part of MVP, it must function in the build.

### 2.3 No Unplayable One-Go Build

Goal Mode can run for a long time, so Codex must complete systems thoroughly instead of rushing a fake MVP.

The game should be playable, winnable, losable, upgradeable, and replayable.

---

## 3. Game Identity

### 3.1 Working Title

ArmyRush

### 3.2 Genre

Mobile hypercasual/hybrid-casual runner with crowd combat, gate math, shooting, and upgrade progression.

### 3.3 Camera Format

Portrait mobile view.

### 3.4 Main Theme

Stylized modern military.

### 3.5 Player Fantasy

The player starts with a small squad and turns it into a massive army by making smart gate choices, surviving firefights, and overpowering enemy forces.

### 3.6 Game Feel Keywords

- Fast
- Punchy
- Clean
- Readable
- Rewarding
- Bright
- Military arcade
- Satisfying
- One more run
- Low friction

---

## 4. Core Gameplay Loop

### 4.1 Macro Loop

1. Player arrives at main menu or upgrade screen.
2. Player taps to start level.
3. Army spawns at the start of the track.
4. Army automatically runs forward.
5. Player drags left and right to steer army.
6. Player chooses positive gates and avoids negative gates.
7. Army size changes instantly after gate contact.
8. Army automatically shoots enemies and obstacles.
9. Army collides with enemy groups when needed.
10. Army count decreases during combat.
11. Player reaches finish line if at least one soldier survives.
12. Remaining army enters bonus zone.
13. Player earns coins based on performance.
14. Victory screen displays earnings.
15. Player upgrades permanent stats.
16. Player starts next level.

### 4.2 Moment-to-Moment Loop

See gate.

Move toward best gate.

Watch army size grow.

Shoot obstacle.

Fight red army.

Lose some troops.

Recover through another good gate.

Reach end.

Earn coins.

Upgrade.

Repeat.

### 4.3 Emotional Loop

The player should feel:

- Small at the start.
- Smart when choosing the best gate.
- Powerful when army size multiplies.
- Tense when approaching enemies.
- Satisfied when enemies pop away.
- Rewarded when coins burst out.
- Motivated to upgrade and replay.

---

## 5. MVP Scope

The first complete Goal Mode build must include:

- Portrait iOS gameplay scene
- Touch drag left-right controls
- Auto-forward movement
- Blue player army crowd
- Red enemy army crowds
- Addition gates
- Multiplication gates
- Subtraction gates
- Gate visual states
- Gate labels
- Combat between armies
- Auto-shooting at enemies and obstacles
- Projectile pooling
- Enemy elimination
- Obstacle destruction
- Coin rewards
- End-level bonus area
- Victory screen
- Defeat screen
- Upgrade screen
- Persistent player progression
- 20 handcrafted or data-driven levels minimum
- Polished mobile UI
- Audio hooks
- VFX hooks
- Object pooling
- iPhone and iPad safe area support
- No visible debug controls in production

The MVP must not include:

- Real PvP
- Online alliances
- Base building
- Hero gacha
- Live events
- Real ads or IAP integration

Those belong to later documents or future phases.

---

## 6. Level Start Flow

### 6.1 Pre-Level State

Before gameplay begins, the player sees:

- Current level number
- Coin count
- Upgrade buttons
- Tap to start prompt

The army should be visible at the start line.

The first gate or obstacle should be visible ahead.

The player must never begin in a blank scene.

### 6.2 Start Input

On tap or drag:

- Tap prompt disappears.
- Army starts moving forward.
- Camera begins follow.
- UI switches to in-run HUD.

### 6.3 Initial Army Spawn

Starting soldier count comes from progression data.

Default Level 1 starting count:

`10`

The count must display above or near the army.

---

## 7. Player Movement

### 7.1 Movement Model

The army moves automatically along the forward axis.

The player controls horizontal offset only.

Forward movement is constant except during scripted finish, victory, or defeat states.

### 7.2 Touch Input

Input type:

- One finger drag
- Relative horizontal movement

Dragging left moves army left.

Dragging right moves army right.

Player does not directly control forward speed.

### 7.3 Keyboard Support

Editor-only keyboard support is allowed for testing:

- A or Left Arrow = left
- D or Right Arrow = right

Keyboard support must not be visible in UI.

### 7.4 Movement Feel

Movement must feel responsive.

No floaty delay.

No sliding after release.

No rigidbody wobble.

The crowd center should smoothly follow the input target.

Recommended values:

- Forward speed: 6.0 to 8.5 units/sec
- Horizontal speed: 12.0 to 18.0 units/sec
- Horizontal smoothing: 0.05 to 0.12 sec
- Track half width: 3.0 to 4.5 units

### 7.5 Bounds

The player cannot move off the track.

Horizontal movement clamps to the current track section width.

If track narrows, the formation compresses.

---

## 8. Camera System

### 8.1 Camera Type

Third-person follow camera.

Portrait-optimized.

Fixed angle.

No player camera rotation.

### 8.2 Camera Target

Camera follows the army center.

Camera must not follow individual soldiers.

### 8.3 Camera Placement

Approximate starting values:

- Offset X: 0
- Offset Y: 8 to 11
- Offset Z: -9 to -13
- Pitch: 35 to 45 degrees downward
- FOV: 45 to 58 depending on device aspect

### 8.4 Camera Priorities

Camera must show:

- Player army
- Immediate path
- Next gate decision
- Upcoming enemy or obstacle
- Enough vertical space for UI

### 8.5 Camera Polish

The camera should include:

- Smooth follow
- Tiny impact shake on explosions
- Bigger shake on boss hits
- No motion sickness
- No extreme bobbing

---

## 9. Crowd System

### 9.1 Crowd as Health

The player's army count is both:

- Health
- Damage source
- Score multiplier

If count reaches zero, the level fails.

### 9.2 Soldier Unit Requirements

Player soldiers must appear as stylized low-poly blue military troops.

Each soldier should visually include:

- Head
- Helmet or cap
- Torso
- Arms
- Legs
- Simple rifle or weapon
- Blue team material
- Tiny run animation or bobbing motion

Final soldiers must not be capsules.

### 9.3 Enemy Unit Requirements

Enemy soldiers must appear as stylized low-poly red military troops.

Enemy soldiers should use similar proportions to player soldiers but red team color.

Final enemies must not be capsules.

### 9.4 Formation Shape

The army should arrange into a compact group.

Preferred formation:

- Centered grid/circle hybrid
- Front rows slightly arced
- Avoid perfect robotic square look
- Keep count readability

### 9.5 Formation Spacing

Recommended spacing:

- Horizontal: 0.35 to 0.55 units
- Forward: 0.35 to 0.55 units

Spacing may expand with small counts and compress with large counts.

### 9.6 Visible Count Cap

For performance, all units do not need unique high-cost behavior at huge counts.

Recommended:

- 1 to 120: render individual soldiers
- 121 to 300: render optimized soldiers with reduced animation complexity
- 301+: use visual batching, impostor clusters, or capped visuals while preserving logical count

The displayed number must always reflect the actual logical count.

### 9.7 Logical Count vs Visual Units

The game may separate logical army count from visible unit count.

Example:

- Logical count = 450
- Visible soldiers = 220
- Displayed count = 450

This keeps performance stable.

### 9.8 Crowd Count Display

The player's current army count must be visible during gameplay.

Display options:

- Floating number above crowd
- HUD number near top
- Both if clean

The count must animate when changed.

---

## 10. Gate System

### 10.1 Gate Purpose

Gates are the main decision mechanic.

Every gate choice must be readable before the player reaches it.

### 10.2 Gate Types

Allowed MVP gates:

- Addition: `+5`, `+10`, `+20`, `+30`, `+50`
- Multiplication: `x2`, `x3`, `x4`
- Subtraction: `-5`, `-10`, `-20`, `-30`, `-50`

Division gates are not MVP unless explicitly added later.

### 10.3 Gate Visual Rules

Positive gates:

- Blue, cyan, green, or glowing friendly color
- Upward particles
- Clear plus/multiply value

Negative gates:

- Red or orange danger color
- Warning glow
- Clear minus value

Multiplication gates should feel more exciting than addition gates.

### 10.4 Gate Layout

Most gate decisions should appear as paired choices:

- Left gate vs right gate
- Occasionally three gates

The player should have enough time to react.

Minimum reaction distance:

- 10 to 16 units before gate

### 10.5 Gate Math

When the player army center crosses a gate trigger, apply exactly one gate operation.

The gate should not trigger multiple times.

#### Addition Example

Current count: 20  
Gate: +10  
New count: 30

#### Multiplication Example

Current count: 20  
Gate: x3  
New count: 60

#### Subtraction Example

Current count: 20  
Gate: -10  
New count: 10

If subtraction would reduce below zero, clamp to zero and trigger loss.

### 10.6 Gate Feedback

On gate pass:

- Gate flashes
- Number flies toward army count
- Crowd count animates
- New soldiers spawn from gate glow
- Positive sound or negative sound plays
- Particle burst occurs

### 10.7 Gate Balancing Principles

Early levels:

- Mostly positive gates
- Obvious best choices
- Teach player fast

Mid levels:

- Introduce tradeoffs
- Sometimes `+30` vs `x2`
- Sometimes best choice depends on current count

Later levels:

- Bigger enemies
- More punishing negative gates
- More crowded lanes

### 10.8 Gate Decision Examples

Level 1:

- `+5` vs `+10`
- `+10` vs `x2`

Level 3:

- `+20` vs `x2`
- `-10` vs `+15`

Level 8:

- `x2` vs `+50`
- `x3` vs `-30` near obstacle layout

---

## 11. Shooting System

### 11.1 Purpose

The shooting system adds action and spectacle before physical crowd collisions.

The player's soldiers should automatically fire at enemies and destructible objects ahead.

### 11.2 Auto Fire Rules

Player does not aim.

Player does not tap to shoot.

Soldiers automatically shoot when targets are in range.

### 11.3 Target Priority

Priority order:

1. Immediate blocking obstacle
2. Closest enemy crowd unit ahead
3. Turret or ranged enemy
4. Boss weak point

Never target objects behind the player.

### 11.4 Firing Distribution

Not every soldier needs to fire every frame.

For visual clarity and performance:

- Small army: many visible shooters
- Large army: sampled shooters
- Huge army: pooled muzzle flashes and aggregate damage

### 11.5 Damage Model

The army's damage output should scale with logical count and upgrades.

Recommended formula:

`DamagePerSecond = ArmyCount * UnitDamage * FireRateMultiplier`

Apply damage in ticks.

### 11.6 Projectile Visuals

Projectiles should look like arcade bullet tracers.

They should be:

- Bright
- Fast
- Readable
- Pooled
- Mobile optimized

Final projectile visuals must not look like unstyled Unity spheres.

### 11.7 Muzzle Flash

Every firing burst should include:

- Tiny muzzle flash
- Optional shell effect
- Light hit spark on impact

Do not use expensive realtime lights for every muzzle flash on mobile.

Use particles or emissive billboards.

---

## 12. Enemy Crowd Combat

### 12.1 Enemy Purpose

Enemy crowds are red groups that reduce the player's army count.

The player can defeat them by having enough soldiers and damage.

### 12.2 Enemy Movement

Enemy crowds may:

- Stand blocking the road
- Run toward the player
- March slowly forward

Early levels should use mostly stationary enemies.

Later levels may use moving enemies.

### 12.3 Enemy Count Display

Every enemy crowd must display its enemy count above it.

The number should decrease as enemies are killed.

### 12.4 Contact Combat

If player army collides with enemy army, resolve combat using count subtraction unless a more advanced combat simulation is implemented.

Simple model:

- Player count loses enemy remaining count
- Enemy count reaches zero if player count was higher
- If player count is less than or equal to enemy count, player loses

Example:

Player: 75  
Enemy: 25  
Result: Player survives with 50

Player: 25  
Enemy: 75  
Result: Player loses

### 12.5 Shooting Before Contact

Shooting should reduce enemy count before collision.

If enemy count reaches zero before collision:

- Enemy units pop away
- Player passes through
- Reward feedback plays

### 12.6 Enemy Death Feedback

Enemy unit deaths require:

- Pop or ragdoll-like stylized disappearance
- Red particle puff
- Small hit flash
- Count decrease animation

Do not use realistic gore.

No blood.

---

## 13. Obstacle System

### 13.1 Obstacle Purpose

Obstacles create pressure and reward large armies.

They should be destructible barriers that block or threaten the player.

### 13.2 MVP Obstacle Types

- Wooden barricade
- Metal barricade
- Explosive barrel cluster
- Crate wall
- Sandbag wall
- Military gate barrier
- Parked armored jeep

### 13.3 Obstacle Visual Requirements

Final obstacles must use stylized low-poly meshes.

They must not be plain cubes.

### 13.4 Obstacle Health

Obstacles have HP.

HP must be visible when relevant.

Example values:

- Wooden barricade: 50
- Crate wall: 120
- Sandbag wall: 200
- Metal barrier: 350
- Armored jeep: 600

### 13.5 Obstacle Damage

Player shooting damages obstacles.

If obstacle is not destroyed before contact, the army may lose soldiers.

### 13.6 Obstacle Collision Penalty

If contact occurs:

- Wooden barricade: small soldier loss
- Metal barrier: medium soldier loss
- Vehicle: large soldier loss

Never instantly kill the army in early levels.

### 13.7 Destruction Feedback

Destroyed obstacles should:

- Break apart or pop
- Spawn debris particles
- Play impact sound
- Trigger small camera shake
- Award small coin burst if appropriate

---

## 14. Boss System

### 14.1 MVP Boss Requirement

At least one boss encounter should exist by Level 10 or Level 20.

Bosses add spectacle and improve retention.

### 14.2 Boss Types

Recommended first bosses:

- Armored tank
- Giant turret
- Helicopter hover boss
- Missile truck

### 14.3 Boss Behavior

Bosses should be simple but polished.

Possible attacks:

- Fires slow projectiles down lanes
- Spawns red troops
- Blocks center lane
- Has weak points

### 14.4 Boss HP

Boss HP scales by level.

HP must display as a boss health bar at top of screen.

### 14.5 Boss Defeat

Boss defeat must include:

- Large explosion
- Camera shake
- Coin burst
- Slow motion or pause hit optional
- Victory celebration

---

## 15. Finish and Bonus Zone

### 15.1 Finish Line

Each level must have a clear finish line.

The finish line should use:

- Banner
- Checkered or military marker
- Highlighted platform
- Victory trigger

### 15.2 Bonus Zone

After crossing the finish, remaining soldiers may enter a bonus sequence.

Bonus zone options:

- Soldiers shoot coin crates
- Soldiers attack final multiplier wall
- Soldiers climb reward stairs
- Soldiers charge bonus enemy line

### 15.3 Bonus Reward Rule

Remaining soldier count should matter.

More survivors = more coins.

Recommended formula:

`BonusCoins = RemainingSoldiers * LevelBonusMultiplier`

### 15.4 End State Transition

After bonus sequence:

- Camera eases forward or zooms slightly
- Victory UI appears
- Coins count up
- Upgrade buttons appear

---

## 16. Victory Screen

### 16.1 Victory Requirements

Victory screen must show:

- Level complete text
- Coins earned
- Total coin count
- Continue button
- Upgrade prompt or upgrade panel

### 16.2 Victory Feel

Victory should feel rewarding.

Use:

- Coin burst animation
- Confetti or fireworks
- Button bounce
- Positive sound

No plain text-only victory screen.

---

## 17. Defeat Screen

### 17.1 Defeat Conditions

Defeat occurs when:

- Player count reaches zero
- Player fails boss encounter
- Player cannot destroy mandatory blocker and loses all troops

### 17.2 Defeat Screen Requirements

Defeat screen must show:

- Failed text
- Restart button
- Optional upgrade suggestion
- Current coins if earned

### 17.3 Defeat Tone

Defeat should be quick and replay-friendly.

Do not make the user wait through long animations.

---

## 18. Upgrade System

### 18.1 Upgrade Purpose

Upgrades create meta progression and make each run feel more powerful.

### 18.2 MVP Upgrades

Required upgrades:

1. Starting Soldiers
2. Unit Damage
3. Fire Rate
4. Coin Value

Optional upgrades:

5. Critical Chance
6. Critical Damage
7. Bonus Multiplier

### 18.3 Upgrade UI

Upgrade panel should appear before or after each level.

Buttons must show:

- Upgrade name
- Current level
- Cost
- Effect preview if possible

Example:

`Damage Lv. 4`  
`Upgrade: 250 coins`

### 18.4 Upgrade Costs

Costs should scale upward.

Recommended formula:

`Cost = BaseCost * Pow(1.18, UpgradeLevel)`

Use ScriptableObject data, not hardcoded values in multiple scripts.

### 18.5 Upgrade Effects

Starting Soldiers:

- Adds 1 to 5 starting units per level depending balance

Unit Damage:

- Increases damage per soldier

Fire Rate:

- Reduces time between shots or increases DPS multiplier

Coin Value:

- Multiplies earned coins

### 18.6 Persistence

Upgrade levels and coins must persist locally.

Use a save system defined by architecture docs.

Do not reset progress on app restart unless debug reset is intentionally accessed in editor only.

---

## 19. Currency and Rewards

### 19.1 Currency

Primary soft currency:

Coins

### 19.2 Coin Sources

Coins earned from:

- Completing levels
- Defeating enemies
- Destroying obstacles
- Remaining soldiers
- Bonus zone

### 19.3 Reward Calculation

Recommended base formula:

`TotalCoins = BaseLevelCoins + EnemyCoins + ObstacleCoins + SurvivorBonus + BonusZoneCoins`

Then apply:

`FinalCoins = TotalCoins * CoinMultiplierUpgrade`

### 19.4 Level Coin Targets

Approximate early economy:

- Level 1: 80 to 150 coins
- Level 2: 120 to 220 coins
- Level 3: 180 to 300 coins
- Level 5: 300 to 500 coins
- Level 10: 800 to 1500 coins

Balance should allow frequent early upgrades.

---

## 20. Level Design Rules

### 20.1 Level Length

Target level duration:

- Early: 30 to 45 seconds
- Mid: 45 to 60 seconds
- Boss: 60 to 90 seconds

### 20.2 Level Width

Default track width:

- 7 units total

Lane count:

- 3 implied lanes

### 20.3 Level Section Types

Every level can be built from sections:

1. Start Section
2. Gate Choice Section
3. Obstacle Section
4. Enemy Crowd Section
5. Recovery Gate Section
6. Bonus Gate Section
7. Boss or Finish Section

### 20.4 Level 1 Tutorial

Level 1 should teach without text overload.

Sequence:

1. Start with 10 soldiers
2. Present `+5` vs `+10`
3. Present small red enemy count 8
4. Present `x2` gate
5. Present crate wall HP 40
6. Finish line
7. Victory and upgrade

### 20.5 Level 2

Introduce slightly harder choices.

Sequence:

1. Start
2. `+10` vs `+15`
3. enemy 20
4. `x2` vs `+20`
5. obstacle HP 100
6. finish

### 20.6 Level 3

Introduce negative gate.

Sequence:

1. `+20` vs `-10`
2. enemy 25
3. `x2` vs `+30`
4. barricade HP 150
5. finish

### 20.7 Level 5 Mini-Boss

Introduce larger obstacle or turret.

Must feel like a milestone.

### 20.8 Level 10 Boss

First true boss.

Must include boss health bar.

---

## 21. Difficulty Scaling

### 21.1 Scaling Inputs

Difficulty scales through:

- Enemy counts
- Obstacle HP
- Gate risk
- Level length
- Boss HP
- Enemy density

### 21.2 Avoid Cheap Difficulty

Do not make levels hard by:

- Hiding gate labels
- Making choices unreadable
- Forcing unavoidable death
- Using narrow lanes without warning
- Spawning enemies directly on top of player

### 21.3 Fail Rate Target

Early levels:

- Almost no failures

Mid levels:

- Occasional failure if poor choices

Later levels:

- Failure possible without upgrades

---

## 22. UI During Gameplay

### 22.1 Required HUD Elements

During run show:

- Level number
- Coin count or run coins
- Army count
- Optional progress bar

### 22.2 HUD Placement

Portrait safe area.

Top:

- Level number
- Progress bar
- Coin count

Center gameplay area must remain clear.

Do not cover gates.

### 22.3 Floating Text

Use floating text for:

- Gate changes
- Damage numbers optional
- Coins
- Upgrade feedback

Floating text must be readable but not spammy.

---

## 23. VFX Requirements

### 23.1 Required VFX

- Gate pass burst
- New soldier spawn sparkle
- Enemy death pop
- Bullet hit spark
- Obstacle destruction debris
- Coin burst
- Victory confetti
- Boss explosion

### 23.2 VFX Style

Arcade stylized.

Bright but not cluttered.

Mobile optimized.

Use object pooling for repeated effects.

### 23.3 VFX Performance

Avoid excessive overdraw.

Use short particle lifetimes.

Limit particle counts on older devices.

---

## 24. Audio Requirements

### 24.1 Required Sounds

- Start run
- Gate positive
- Gate negative
- Soldier spawn
- Shooting loop or bursts
- Enemy hit
- Enemy defeated
- Obstacle hit
- Obstacle destroyed
- Coin collect
- Upgrade purchase
- Victory
- Defeat

### 24.2 Audio Style

Punchy arcade military.

Not realistic violent combat.

No overly harsh gunfire.

Sounds should be satisfying and mobile friendly.

---

## 25. Animation Requirements

### 25.1 Soldier Animations

Required:

- Run
- Shoot
- Hit reaction
- Death/pop
- Victory cheer

Simple procedural animations are acceptable if polished.

Final movement must not look like sliding capsules.

### 25.2 Enemy Animations

Required:

- Idle or run
- Shoot or attack
- Hit reaction
- Defeat

### 25.3 UI Animations

Required:

- Button scale punch
- Coin count up
- Upgrade purchase bounce
- Victory panel slide/fade
- Defeat panel slide/fade

---

## 26. Performance Requirements

### 26.1 FPS Target

Target:

60 FPS on modern iPhones.

Minimum acceptable:

30 FPS on older supported devices.

### 26.2 Object Pooling Required For

- Soldiers
- Enemies
- Projectiles
- Hit effects
- Coin bursts
- Floating text
- Debris pieces if repeated

### 26.3 Avoid Runtime Instantiation Spikes

Do not instantiate large crowds during gameplay without pooling.

Gate soldier increases should pull from pool.

Enemy groups should prewarm before level start.

### 26.4 Physics

Avoid heavy physics per soldier.

Use simple colliders and logical calculations.

Do not give every soldier expensive rigidbody simulation.

---

## 27. Data-Driven Design

### 27.1 ScriptableObjects

Use ScriptableObjects for:

- Level data
- Gate data
- Enemy group data
- Obstacle data
- Upgrade data
- Boss data
- Economy data

### 27.2 Avoid Hardcoded Balance

Do not scatter numbers across scripts.

Centralize values in data assets.

### 27.3 Level Data

Each level should define:

- Starting section
- Gate placements
- Enemy placements
- Obstacle placements
- Boss optional
- Finish position
- Reward tuning

---

## 28. iPhone and iPad Requirements

### 28.1 Orientation

Portrait primary.

The game must run on iPhone and iPad.

### 28.2 Safe Area

UI must respect safe areas.

No important buttons under notch, home indicator, or rounded corners.

### 28.3 Aspect Ratio

Gameplay must work on:

- Tall iPhones
- Standard iPhones
- iPads

Camera may adjust FOV/offset based on aspect ratio.

### 28.4 Touch Area

Buttons must be large enough for mobile.

Minimum touch target:

44x44 points equivalent.

---

## 29. Polish Checklist

A level is not complete until:

- It has readable gates.
- It has enemy/obstacle pacing.
- It can be won.
- It can be lost.
- It has visual feedback.
- It has sound hooks.
- It rewards coins.
- It transitions properly to victory/defeat.
- It supports upgrades.
- It runs smoothly.
- It has no visible debug placeholders.

A gameplay system is not complete until:

- It works.
- It is data-driven where appropriate.
- It is visually polished.
- It has VFX/audio hooks.
- It is pooled if repeated.
- It updates TASKS.md and DEVLOG.md.

---

## 30. Required First Build Outcome

After Codex completes the first major Goal Mode build, the Unity project should open to a playable game where:

1. The player can press play in Unity.
2. The game loads into a portrait mobile scene.
3. The player taps to start.
4. A polished blue army runs forward.
5. The player drags left and right.
6. Gates alter army size.
7. Enemies appear and fight/shoot.
8. Obstacles are destroyed.
9. Coins are earned.
10. Victory and defeat screens work.
11. Upgrades persist.
12. Multiple levels can be played.
13. The game feels like a real mobile runner, not a test prototype.

---

## 31. Codex Implementation Instructions

Codex must:

1. Read this entire file before implementing gameplay.
2. Follow `TECH_ARCHITECTURE.md` for code structure.
3. Implement production-quality systems, not placeholders.
4. Use temporary assets only while constructing systems.
5. Replace temporary assets before marking features complete.
6. Create prefabs for reusable gameplay objects.
7. Use object pooling for repeated runtime objects.
8. Use ScriptableObjects for balancing.
9. Keep the game iPhone/iPad compatible.
10. Maintain clean scene hierarchy.
11. Keep gameplay readable in portrait.
12. Update `Docs/TASKS.md` after major milestones.
13. Update `Docs/DEVLOG.md` after completed work.
14. Document any deviations clearly.
15. Do not introduce large systems outside MVP unless core MVP is complete.

---

## 32. Future Expansion Notes

These are not MVP unless the core runner is complete:

- Hero collection
- Base building
- Idle production
- PvP arena
- Alliance systems
- Event calendar
- Cosmetic skins
- Weapon rarity
- Daily rewards
- Ad monetization
- In-app purchases

The first goal is to make the runner feel polished, satisfying, and complete.

---

## 33. Final Quality Bar

The final result should not feel like:

- A Unity tutorial
- A student prototype
- A graybox demo
- A test scene
- A capsule runner
- A fake mobile ad mockup

The final result should feel like:

- A real iOS mobile game
- A polished Last War-inspired runner
- A game that could be recorded for TikTok ads
- A game with readable progression
- A game that is satisfying to replay

