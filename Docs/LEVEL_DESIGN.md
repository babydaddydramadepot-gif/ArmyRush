# LEVEL_DESIGN.md

# ArmyRush Level Design Specification

Version: 1.0
Status: Source of Truth

---

# Purpose

This document defines:

* Level flow
* Encounter design
* Difficulty progression
* Gate placement
* Crowd growth pacing
* Enemy pacing
* Obstacle pacing
* Boss encounters
* Reward pacing
* Endless scaling

This document exists to prevent random level generation.

ArmyRush levels must feel intentionally designed.

Players should feel:

"That was fair."

Never:

"That was random."

---

# Design Pillars

Every level must satisfy:

1. Readability
2. Choice
3. Risk
4. Reward
5. Power Growth
6. Escalation
7. Satisfaction

---

# Golden Rule

Every 5 seconds:

The player must make a meaningful decision.

No dead zones.

No empty stretches.

No waiting.

No autopilot gameplay.

---

# Level Duration

Target:

45 seconds

Minimum:

30 seconds

Maximum:

90 seconds

Never exceed:

120 seconds

---

# Level Structure

Every level follows:

INTRO

↓

POWER BUILDING

↓

FIRST ENCOUNTER

↓

POWER SPIKE

↓

MIDGAME COMBAT

↓

RISK / REWARD

↓

FINAL BUILDUP

↓

FINAL ENCOUNTER

↓

FINISH PLATFORM

↓

BONUS RUN

---

# Intro Section

Length:

3–5 seconds

Purpose:

Orient player

Show first gate

Teach level theme

Never punish player here.

---

# Power Building Section

Purpose:

Grow crowd

Teach choices

Create momentum

Recommended:

2–4 gate decisions

---

# Combat Section

Purpose:

Spend crowd power

Create tension

Reduce crowd size

Test previous decisions

---

# Risk Reward Section

Purpose:

Offer high reward

With visible risk

Example:

Safe Route:

+10

Danger Route:

x3

plus enemy encounter

---

# Final Encounter

Purpose:

Payoff

Challenge

Spectacle

Large enemy formation

Large obstacle

Mini boss

Boss

---

# Finish Platform

Purpose:

Victory

Reward

Celebration

Never place danger after finish trigger.

---

# Bonus Run

Purpose:

Additional rewards

Power fantasy

Coin generation

No failure possible.

---

# Core Design Philosophy

A level is:

Building Power

Then Spending Power

Building Power

Then Spending Power

Repeat

Never:

Constant growth

Never:

Constant punishment

---

# Lane Rules

Default:

3 lanes

Standard width:

Consistent

Player must always clearly identify:

Left path

Middle path

Right path

---

# Camera Visibility Rule

Player must see:

At least 8 seconds ahead

All upcoming gates

All upcoming enemies

All upcoming obstacles

---

# Gate Placement Philosophy

Every gate must create a decision.

Forbidden:

+10 vs +15

+20 vs +25

These are fake choices.

---

# Good Gate Examples

+15 vs x2

+25 vs safer route

x3 vs enemy crowd

+50 vs obstacle gauntlet

---

# Positive Gates

Allowed Values

+5

+10

+15

+20

+25

+50

+100

---

# Multiplier Gates

Allowed Values

x2

x3

x4

Rare:

x5

---

# Negative Gates

Allowed Values

-5

-10

-20

-30

÷2

Rare only.

---

# Gate Frequency

Early Game

80% positive

20% negative

Mid Game

70% positive

30% negative

Late Game

60% positive

40% negative

---

# Enemy Design Philosophy

Enemies exist to:

Test growth

Create excitement

Reduce crowd size

Never feel unfair.

---

# Enemy Categories

Light

Medium

Heavy

Elite

Boss

---

# Light Enemy Groups

5–20 units

Low damage

Weak health

Used in tutorials.

---

# Medium Enemy Groups

20–75 units

Standard challenge

Most common encounter.

---

# Heavy Enemy Groups

75–150 units

Require proper gate choices.

---

# Elite Groups

150+

High threat

Rare

Used as major level moments.

---

# Obstacle Philosophy

Obstacles convert:

Damage

Into

Progress

Player should feel stronger by destroying them.

---

# Obstacle Types

Wood Crates

Barrels

Barricades

Concrete Walls

Military Trucks

Watch Towers

Turrets

Fuel Tanks

---

# Obstacle Health Scaling

Level 1

100 HP

Level 5

500 HP

Level 10

1500 HP

Level 20

5000 HP

Level 50+

Scale dynamically.

---

# Obstacle Placement Rules

Never block every path.

Always provide:

Decision

Risk

Reward

---

# Crowd Growth Targets

Level 1 Finish:

20–40 units

Level 5 Finish:

50–80 units

Level 10 Finish:

80–120 units

Level 20 Finish:

150–250 units

---

# Difficulty Curve

Stage 1

Levels 1–5

Teach

---

Stage 2

Levels 6–15

Challenge

---

Stage 3

Levels 16–30

Mastery

---

Stage 4

Levels 31+

Scaling

---

# Boss System

Boss every 5 levels.

---

# Boss Level Structure

Intro

↓

Power Build

↓

Mini Encounter

↓

Boss Arena

↓

Reward

---

# Boss Types

Tank

Helicopter

Mech

Rocket Platform

Heavy Artillery

---

# Tank Boss

Large HP

Slow movement

High durability

Weak to large crowds

---

# Helicopter Boss

Moves laterally

Fires missiles

Requires sustained fire

---

# Mech Boss

Multiple attacks

Heavy health

Visual spectacle

---

# Boss Arena Rules

Wide arena

No clutter

Clear visibility

Spectacle first

---

# Reward Philosophy

Rewards should feel generous.

Players should leave every level stronger.

---

# Coin Rewards

Level completion

Enemy defeat

Obstacle destruction

Bonus run

Boss defeat

---

# Bonus Run Design

No enemies

No failure

Pure reward

Player destroys objects

Collects coins

Triggers celebration effects

---

# Tutorial Level

Level 1 Specification

Starting Units:

10

First Gate:

+10

Second Gate:

x2

Enemy:

10 units

Obstacle:

100 HP crate

Finish:

Simple bonus run

Purpose:

Teach all systems.

---

# Level 2

Introduce:

Negative gate

Small risk reward decision

---

# Level 3

Introduce:

Multiple enemy groups

---

# Level 4

Introduce:

Obstacle chain

---

# Level 5

First Boss

Tank

---

# Levels 6-10

Introduce:

Advanced gate combinations

Large enemy groups

Multi-wave encounters

---

# Levels 11-20

Introduce:

Elite groups

Complex pathing

High risk routes

Heavy obstacles

---

# Endless System

After designed levels:

Generate procedurally.

Use:

Chunk system

Difficulty scaling

Reward scaling

Boss rotation

---

# Chunk System

Codex must build levels from reusable chunks.

---

# Chunk A

Intro Gates

2 positive gates

No enemies

---

# Chunk B

Gate + Enemy

2 gates

1 enemy group

---

# Chunk C

Obstacle Corridor

2 obstacles

1 reward gate

---

# Chunk D

Risk Reward Split

Safe path

Danger path

---

# Chunk E

Elite Encounter

Large enemy crowd

High reward

---

# Chunk F

Boss Lead-In

Preparation area

Final upgrade gate

Boss arena

---

# Chunk Assembly Rules

Never repeat same chunk 3 times in a row.

Alternate:

Growth

Combat

Growth

Combat

Reward

---

# Player Psychology Targets

Player should constantly feel:

Bigger

Stronger

Smarter

More powerful

Never weaker over multiple levels.

---

# Frustration Rules

Avoid:

Cheap losses

Invisible damage

Impossible choices

Unavoidable punishment

Fake decisions

---

# Production Requirements

Every level must contain:

Meaningful choices

Visible rewards

Combat

Growth

Feedback

Clear pacing

No dead space

No filler

---

# Codex Instructions

Levels must be intentionally designed.

Do not randomly place gates.

Do not randomly place enemies.

Do not generate unfair situations.

Use chunk system.

Maintain pacing.

Maintain readability.

Update TASKS.md after level additions.

Update DEVLOG.md after major level system changes.

This document is the authoritative source for all level creation.

