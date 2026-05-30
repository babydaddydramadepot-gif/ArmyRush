# CONTENT_PIPELINE.md

# ArmyRush Content Pipeline Specification

Version: 1.0
Status: Source of Truth

---

# Purpose

This document defines:

* Asset organization
* Asset creation
* Asset import standards
* Prefab standards
* Animation standards
* Audio standards
* VFX standards
* Naming conventions
* Scene organization
* ScriptableObject standards
* Optimization standards
* Production readiness requirements

This document exists to ensure the project remains scalable and maintainable as Codex continuously develops the game.

This is one of the most important documents in the repository.

---

# Golden Rule

Every asset in the project must:

Be organized

Be reusable

Be scalable

Be production ready

Be optimized

Nothing should exist without purpose.

---

# Folder Structure

Assets/

Art/

Characters/

Enemies/

Bosses/

Weapons/

Environment/

Props/

UI/

Materials/

Textures/

Animations/

Audio/

Music/

SFX/

Prefabs/

Characters/

Enemies/

Bosses/

Weapons/

Environment/

UI/

Systems/

ScriptableObjects/

Levels/

Scenes/

Scripts/

Managers/

Gameplay/

UI/

Combat/

Progression/

Systems/

VFX/

Resources/

Addressables/

---

# Forbidden Practices

Do not place assets randomly.

Do not place assets in root folders.

Do not create duplicate content.

Do not create unnamed assets.

Do not create folders named:

New Folder

Temp

Test

Stuff

Misc

Random

---

# Asset Naming Rules

All assets must use:

PascalCase

Examples:

PlayerSoldier

EnemySoldier

BossTank

BridgeSegment

UpgradeButton

CoinRewardEffect

---

# Prefix Rules

Recommended:

SO_

ScriptableObjects

Example:

SO_WeaponData

SO_LevelData

SO_UpgradeData

---

# Prefab Prefix

PF_

Examples:

PF_PlayerSoldier

PF_EnemySoldier

PF_BossTank

PF_BridgeSection

---

# Material Prefix

MAT_

Examples:

MAT_PlayerBlue

MAT_EnemyRed

MAT_BridgeConcrete

---

# Animation Prefix

AN_

Examples:

AN_PlayerRun

AN_PlayerShoot

AN_BossDeath

---

# Audio Prefix

SFX_

MUS_

Examples:

SFX_Gunshot

SFX_Explosion

MUS_MainTheme

---

# VFX Prefix

VFX_

Examples:

VFX_Explosion

VFX_CoinBurst

VFX_LevelComplete

---

# Script Naming Rules

Every script must have:

Single responsibility

Clear naming

Purpose-driven structure

---

# Good Examples

PlayerMovementController

EnemyCrowdManager

WeaponSystem

BossHealthController

CoinRewardSystem

---

# Bad Examples

Manager1

Controller2

TempScript

TestSystem

RandomManager

---

# Prefab Philosophy

Prefabs must be reusable.

No one-off prefabs unless necessary.

---

# Prefab Requirements

Every prefab must contain:

Proper naming

Organized hierarchy

Comments where needed

No missing references

No warnings

No errors

---

# Character Pipeline

Player Character

↓

Model

↓

Material

↓

Animator

↓

Prefab

↓

Optimization Pass

↓

Production Ready

---

# Enemy Pipeline

Enemy Model

↓

Animator

↓

Combat Components

↓

Prefab

↓

Optimization

↓

Production Ready

---

# Boss Pipeline

Bosses require:

Unique model

Unique animations

Unique VFX

Unique audio

Unique behavior profile

---

# Environment Pipeline

Road Sections

Bridge Pieces

Props

Obstacles

Decorations

All must be modular.

---

# Modular Environment Rules

Every environment piece must support:

Reuse

Rotation

Scaling

Level generation

---

# Material Philosophy

Minimal material count.

Shared materials preferred.

Avoid excessive material instances.

---

# Texture Rules

Target:

Mobile optimized

Readable

Compressed

Efficient

---

# Texture Sizes

Small Props:

512

Characters:

1024

Bosses:

2048 maximum

UI:

Appropriate resolution

---

# Animation Pipeline

Every animation requires:

Preview

Testing

Performance review

Integration review

---

# Animation Categories

Locomotion

Combat

Death

Victory

Boss

UI

VFX

---

# Animation Standards

Smooth

Readable

Responsive

Stylized

---

# Audio Pipeline

Every audio file must:

Be normalized

Be optimized

Be named correctly

Be categorized

---

# Audio Categories

Weapons

Explosions

UI

Bosses

Rewards

Movement

Environment

Music

---

# Music Standards

Positive

Energetic

Mobile friendly

Loop cleanly

---

# VFX Pipeline

Every VFX must:

Support pooling

Support performance scaling

Remain readable

---

# Required VFX Categories

Combat

Weapons

Coins

Bosses

Destruction

Rewards

Level Complete

Upgrades

---

# VFX Performance Rules

Avoid excessive particles.

Avoid excessive overdraw.

Prioritize readability.

---

# ScriptableObject Philosophy

All balancing data belongs in ScriptableObjects.

Never hardcode balance values.

---

# Required ScriptableObjects

SO_WeaponData

SO_EnemyData

SO_BossData

SO_LevelData

SO_UpgradeData

SO_RewardData

SO_AudioData

---

# Level Data Pipeline

Levels should be data-driven.

Never hardcode levels.

Use SO_LevelData.

---

# Chunk System

Every level chunk must be reusable.

Examples:

IntroChunk

CombatChunk

BossChunk

RewardChunk

RiskRewardChunk

---

# Scene Organization

Required Scenes

Bootstrap

MainMenu

Gameplay

Loading

PersistentSystems

---

# Bootstrap Scene

Responsible for:

Initialization

Dependency setup

System startup

---

# Gameplay Scene

Contains:

Level content

Player

Enemies

UI

Camera

---

# Persistent Systems

Should survive scene loads.

Examples:

Audio

Save Data

Progression

Analytics

---

# Addressables

Required for scalability.

Large content must support:

Addressables

Future downloadable content

Asset streaming

---

# Save Data Pipeline

Must support:

Upgrades

Progression

Settings

Achievements

Future systems

---

# Asset Review Checklist

Before asset approval:

Correct naming

Correct folder

Optimized

Readable

No errors

No warnings

Production quality

---

# Performance Targets

iPhone:

60 FPS

iPad:

60 FPS

---

# Memory Targets

Minimize runtime allocations.

Pool frequently spawned objects.

---

# Pooling Requirements

Required For:

Projectiles

Enemies

Crowd Units

Coins

Effects

Damage Numbers

---

# Production Readiness Rules

Content is NOT complete unless:

Art exists

Animation exists

Audio exists

VFX exists

Optimization exists

QA exists

Documentation exists

---

# Placeholder Content Rules

Placeholder assets may be used temporarily.

Placeholder assets are NEVER allowed in a release candidate.

Before release:

All placeholders must be replaced.

---

# Release Checklist

No missing references

No warnings

No console spam

No placeholder art

No placeholder UI

No debug tools

No temporary assets

No unused content

Stable 60 FPS

---

# Codex Instructions

Every new asset must follow this document.

Every new prefab must follow this document.

Every new scene must follow this document.

Every new ScriptableObject must follow this document.

Update TASKS.md after major content creation.

Update DEVLOG.md after major content creation.

This document is the content creation source of truth.

