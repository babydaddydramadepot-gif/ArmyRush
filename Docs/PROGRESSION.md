# PROGRESSION.md

# ArmyRush Progression & Economy Specification

Version: 1.0
Status: Source of Truth

---

# Purpose

This document defines:

* Player progression
* Upgrade systems
* Economy systems
* Reward systems
* Difficulty scaling
* Long-term retention
* Power growth
* Unlock systems
* Future expansion systems

ArmyRush is not a level-based game.

ArmyRush is a power fantasy.

Players must always feel stronger than before.

The primary objective of progression is:

Increase Player Power

Increase Player Satisfaction

Increase Session Count

Increase Long-Term Retention

---

# Progression Pillars

Every progression system must satisfy:

1. Understandable
2. Rewarding
3. Consistent
4. Fair
5. Long-Term
6. Scalable
7. Exciting

---

# Core Philosophy

The player should never ask:

"Why am I playing another run?"

The answer should always be obvious.

Every run should provide:

Coins

Power

Unlocks

Progress

---

# Meta Progression Loop

Play Level

↓

Earn Coins

↓

Purchase Upgrades

↓

Become Stronger

↓

Reach Further

↓

Earn More Coins

↓

Repeat

---

# Primary Currency

Coins

Color:

Gold

Purpose:

Permanent upgrades

Unlocks

Future systems

---

# Premium Currency

Gems

Future feature.

Not required for MVP.

Must be architected now.

Color:

Blue

Purpose:

Premium purchases

Cosmetics

Special offers

Future content

---

# Player Power Categories

Damage

Fire Rate

Starting Soldiers

Critical Chance

Critical Damage

Coin Bonus

Boss Damage

Obstacle Damage

---

# Upgrade Philosophy

Upgrades must always feel meaningful.

Never allow:

+0.5%

+1%

Invisible gains

Every upgrade should be felt.

---

# Damage Upgrade

Purpose:

Increase all weapon damage.

---

# Damage Formula

Base Damage:

10

Damage Level 1:

12

Damage Level 10:

35

Damage Level 25:

100

Damage Level 50:

300

Damage Level 100:

1000+

---

# Fire Rate Upgrade

Purpose:

Increase attack frequency.

---

# Fire Rate Progression

Base:

1 shot/sec

Level 10:

2 shots/sec

Level 25:

4 shots/sec

Level 50:

7 shots/sec

Level 100:

10 shots/sec

---

# Starting Soldier Upgrade

Purpose:

Increase starting crowd size.

---

# Starting Soldiers

Default:

10

Level 10:

15

Level 25:

25

Level 50:

40

Level 100:

75

---

# Critical Chance

Unlock:

After Level 10

---

# Critical Chance Scaling

Base:

0%

Max:

50%

---

# Critical Damage

Unlock:

After Level 15

---

# Critical Damage Scaling

Base:

150%

Maximum:

500%

---

# Coin Bonus

Purpose:

Increase rewards.

---

# Coin Bonus Scaling

Base:

100%

Level 10:

120%

Level 25:

150%

Level 50:

250%

Level 100:

500%

---

# Boss Damage Upgrade

Unlock:

After First Boss

Purpose:

Increase boss kill speed.

---

# Obstacle Damage Upgrade

Unlock:

After Level 5

Purpose:

Destroy barriers faster.

---

# Upgrade Cost Philosophy

Upgrades must:

Start cheap

Scale gradually

Remain attainable

Never become impossible

---

# Upgrade Cost Formula

Base Cost × Growth Rate ^ Level

---

# Damage Upgrade Costs

Level 1:

100

Level 2:

115

Level 3:

132

Level 4:

152

Level 5:

175

Continue scaling.

---

# Fire Rate Costs

Level 1:

150

Level 2:

173

Level 3:

198

Level 4:

228

---

# Starting Soldiers Costs

Level 1:

250

Level 2:

288

Level 3:

331

Level 4:

381

---

# Economy Philosophy

Players should earn:

At least one upgrade every run.

Especially early game.

---

# Early Game Economy

Levels 1–10

Reward generously.

Player should constantly upgrade.

---

# Mid Game Economy

Levels 11–50

Reward steadily.

Require meaningful choices.

---

# Late Game Economy

Levels 51+

Require optimization.

Still avoid excessive grind.

---

# Coin Reward Formula

Base Level Reward

*

Enemy Reward

*

Boss Reward

*

Bonus Reward

×

Multiplier

---

# Base Reward Scaling

Level 1:

100 Coins

Level 5:

250 Coins

Level 10:

500 Coins

Level 20:

1200 Coins

Level 50:

5000 Coins

Level 100:

15000 Coins

---

# Enemy Rewards

Per Enemy Unit:

Small bonus

Scaled by level.

---

# Boss Rewards

Bosses must feel important.

Minimum:

3x standard reward

---

# Bonus Run Rewards

Purpose:

Provide excitement.

Not mandatory for progression.

---

# Daily Reward System

Future Feature

Day 1

100 Coins

Day 2

250 Coins

Day 3

500 Coins

Day 4

750 Coins

Day 5

1000 Coins

Day 6

1500 Coins

Day 7

3000 Coins

---

# Achievement System

Future Feature

Examples:

Reach 50 Soldiers

Reach 100 Soldiers

Destroy 100 Obstacles

Defeat 10 Bosses

Reach Level 50

Reach Level 100

---

# Weapon Progression

Future Expansion

---

# Tier 1

Recruit Rifle

---

# Tier 2

Military Rifle

---

# Tier 3

Combat Rifle

---

# Tier 4

Heavy Rifle

---

# Tier 5

Elite Rifle

---

# Weapon Unlock Philosophy

Weapons should feel:

Rare

Exciting

Powerful

Not mandatory

---

# Cosmetic System

Future Feature

---

# Soldier Skins

Urban

Desert

Arctic

Special Forces

Elite

Legendary

---

# Crowd Trail Effects

Future Cosmetic

---

# Bullet Trail Effects

Future Cosmetic

---

# Victory Effects

Future Cosmetic

---

# Boss Progression

Bosses evolve over time.

---

# Tank I

Level 5

---

# Tank II

Level 15

---

# Tank III

Level 30

---

# Elite Tank

Level 50

---

# Legendary Tank

Level 100

---

# Helicopter Progression

Level 20

Level 40

Level 60

Level 100

---

# Mech Progression

Level 50+

---

# Difficulty Progression

Difficulty should rise slower than player power.

Player should feel stronger.

---

# Power Curve Targets

Level 1

Weak

---

# Level 10

Strong

---

# Level 25

Powerful

---

# Level 50

Elite Commander

---

# Level 100

Army Destroyer

---

# First Session Goals

Within 10 minutes:

Player should:

Beat multiple levels

Purchase upgrades

Defeat first boss

Understand progression

Feel powerful

---

# First Hour Goals

Player should:

Unlock new systems

See significant growth

Understand long-term goals

Remain motivated

---

# Long-Term Goals

Reach Level 50

Reach Level 100

Reach 500 Soldiers

Max Damage

Max Fire Rate

Defeat Elite Bosses

Unlock All Skins

Master Endless Mode

---

# Endless Progression

Future System

Difficulty increases forever.

Rewards scale forever.

No hard cap.

---

# Prestige System

Future Feature

Player resets.

Receives permanent bonuses.

Unlocks exclusive rewards.

---

# Retention Design

Player should always have:

Next Upgrade

Next Goal

Next Unlock

Next Boss

Next Milestone

---

# Frustration Rules

Never allow:

Paywalls

Impossible grind

Mandatory purchases

Progression dead ends

Excessive waiting

---

# Production Requirements

Progression must:

Feel rewarding

Scale indefinitely

Support future content

Remain understandable

Support both casual and dedicated players

---

# Codex Instructions

All progression systems must reference this document.

Do not invent random upgrade costs.

Do not invent random scaling formulas.

Do not create excessive grind.

Always prioritize player satisfaction.

Always maintain visible growth.

Update TASKS.md after progression changes.

Update DEVLOG.md after progression changes.

This document is the progression source of truth.

