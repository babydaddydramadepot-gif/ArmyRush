# UI_UX.md

# ArmyRush User Interface & User Experience Specification

Version: 1.0
Status: Source of Truth

---

# Purpose

This document defines:

* HUD
* Menus
* Buttons
* Animations
* Rewards
* Upgrade Screens
* Onboarding
* Navigation
* Screen Flow
* User Experience Standards

This document exists to ensure ArmyRush feels like a professional mobile game.

Not a prototype.

Not a student project.

Not a Unity demo.

---

# UX Pillars

Every screen must be:

1. Clear
2. Fast
3. Readable
4. Rewarding
5. Mobile Friendly
6. One-Hand Friendly
7. Easy To Learn

---

# Mobile First Design

ArmyRush is designed for:

iPhone

and

iPad

first.

Everything must feel natural on touch devices.

---

# Portrait Design Rules

All screens must support:

9:16

Portrait

Primary target:

iPhone

Secondary target:

iPad

---

# Safe Area Requirements

All UI must respect:

Notch

Dynamic Island

Home Indicator

iPad Safe Areas

No important content may be clipped.

---

# Main HUD

Visible During Gameplay

Contains:

Coin Counter

Current Soldier Count

Current Level

Progress Bar

Settings Button

---

# Coin Counter

Position:

Top Right

Behavior:

Always visible

Updates instantly

Uses counting animation

---

# Soldier Counter

Position:

Above Crowd

Behavior:

Updates in real time

Always readable

Large font

High contrast

---

# Level Indicator

Position:

Top Center

Example:

Level 12

---

# Progress Bar

Position:

Top Center

Below level text

Displays:

Start

Current Position

Finish

Boss

---

# Settings Button

Position:

Top Left

Small but visible

Never intrusive

---

# Gameplay UI Rules

Never block gameplay.

Never cover gates.

Never cover enemies.

Never obstruct decisions.

---

# Floating Feedback

Required:

+10

+20

x2

Coins

Damage

Critical Hits

Boss Damage

---

# Floating Number Rules

Fast

Readable

Color Coded

Animated

---

# Positive Feedback Colors

Green

Blue

Gold

---

# Negative Feedback Colors

Red

Orange

---

# Start Screen

Purpose:

Get player into gameplay immediately.

---

# Start Screen Layout

Logo

Play Button

Upgrade Button

Settings Button

Currency Display

---

# Play Button

Largest button on screen.

Most visually important.

---

# Upgrade Screen

One of the most important screens.

Must feel satisfying.

---

# Upgrade Categories

Damage

Fire Rate

Starting Soldiers

Coin Bonus

Boss Damage

Obstacle Damage

---

# Upgrade Button Behavior

On Press:

Scale Up

Coin Burst

Sound Effect

Glow Effect

Number Increase

---

# Upgrade Feedback

Player should immediately feel stronger.

Never allow upgrades to feel invisible.

---

# Victory Screen

Appears after level completion.

Contains:

Victory Header

Coins Earned

Bonus Coins

Upgrade Shortcut

Next Level Button

---

# Victory Screen Goals

Celebrate success.

Encourage next run.

Minimize friction.

---

# Defeat Screen

Appears after loss.

Contains:

Try Again

Upgrade Button

Coins Earned

---

# Defeat Screen Goals

Reduce frustration.

Encourage retry.

Encourage upgrading.

---

# Reward Screen

Used for:

Boss Rewards

Major Milestones

Achievements

Future Systems

---

# Reward Presentation

Rewards must feel exciting.

Use:

Particles

Sound

Glow

Scale Animations

---

# Boss Defeat Screen

Special version of victory screen.

Must feel important.

Contains:

Boss Defeated

Large Reward

Progression Milestone

---

# Level Complete Animation

Required:

Fireworks

Coin Burst

Victory Sound

Screen Flash

Confetti Optional

---

# Button Design

Style:

Rounded

Modern

Large

Readable

---

# Button States

Idle

Hover

Pressed

Disabled

---

# Press Animation

Scale Down

Bounce Back

Play Sound

---

# Navigation Rules

Maximum:

2 taps to gameplay

Never bury gameplay behind menus.

---

# Loading Screens

Keep short.

Display:

Tips

Progress

Level Preview

---

# Loading Philosophy

Fast.

Players should spend time playing.

Not waiting.

---

# Tutorial Philosophy

No long tutorials.

Teach through gameplay.

---

# Tutorial Rules

Level 1 teaches:

Movement

Gate Choice

Combat

Rewards

---

# Tutorial UI

Use:

Arrows

Highlights

Simple Text

Never overwhelm player.

---

# Accessibility

Text must remain readable.

Buttons must remain touchable.

Color blindness considerations encouraged.

---

# Font Rules

Bold

Readable

Mobile Friendly

No decorative fonts

---

# Notification System

Future Feature

Used for:

Daily Rewards

Achievements

Events

---

# Shop Screen

Future Feature

Must support:

Cosmetics

Currency Packs

Bundles

---

# Settings Screen

Contains:

Music Volume

SFX Volume

Vibration Toggle

Privacy Policy

Terms

Restore Purchases

---

# Audio UX

Every action gets feedback.

Required:

Button Press

Upgrade

Reward

Victory

Defeat

Boss Defeat

---

# Haptics

Required:

Gate Pass

Upgrade Purchase

Boss Defeat

Victory

Large Explosions

---

# UX Success Metrics

Player should always know:

What happened

Why it happened

What to do next

---

# Production Requirements

No placeholder UI.

No default Unity buttons.

No debug menus.

No developer tools visible.

No unfinished screens.

All screens animated.

All screens polished.

---

# Codex Instructions

UI must feel premium.

UI must feel responsive.

UI must feel rewarding.

Always prioritize clarity.

Always prioritize usability.

Update TASKS.md after UI changes.

Update DEVLOG.md after UI changes.

This document is the UI/UX source of truth.

