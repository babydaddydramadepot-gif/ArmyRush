# MONETIZATION.md

# ArmyRush Monetization Architecture

Version: 1.0
Status: Source of Truth

---

# Purpose

This document defines:

* Monetization Philosophy
* Premium Currency
* Rewarded Ads
* Cosmetic Systems
* Future Offers
* Future Battle Pass Systems
* Store Architecture

This document exists so monetization systems can be planned early without negatively impacting gameplay.

---

# Core Philosophy

ArmyRush must remain fun without spending money.

The game should never feel:

Pay To Win

Pay To Progress

Paywalled

Exploitative

Frustrating

---

# Monetization Priorities

Priority 1

Retention

Priority 2

Engagement

Priority 3

Revenue

Never reverse this order.

---

# Player First Rule

If monetization reduces fun:

Remove it.

If monetization increases frustration:

Remove it.

If monetization damages retention:

Remove it.

---

# Revenue Streams

Supported Revenue Types

Rewarded Video

Interstitial Ads

Cosmetic Purchases

Currency Purchases

Bundles

Battle Pass

Limited Time Offers

---

# MVP Monetization

For launch:

Rewarded Video Only

No forced ads.

No paywalls.

No energy system.

---

# Rewarded Ads

Primary launch monetization.

---

# Rewarded Ad Locations

Post Level

Bonus Coins

Revive Opportunity

Extra Upgrade Reward

Daily Reward Multiplier

---

# Post Level Offer

Player Wins

↓

Reward Screen

↓

Watch Ad

↓

2x Coins

---

# Bonus Coin Offer

Player receives:

Normal Reward

OR

Watch Ad

↓

Double Reward

---

# Revive Offer

Player Loses

↓

Watch Ad

↓

Revive Once

---

# Revive Rules

Maximum:

1 revive per run

Never infinite revives.

---

# Daily Reward Multiplier

Daily reward claimed

↓

Watch Ad

↓

Double reward

---

# Interstitial Ads

Disabled at launch.

Architecture must support them.

---

# Interstitial Philosophy

Only between sessions.

Never during gameplay.

Never before first session.

---

# Premium Currency

Name:

Gems

Color:

Blue

Purpose:

Premium purchases

Cosmetics

Bundles

Future systems

---

# Gem Acquisition

Future:

Purchases

Events

Achievements

Battle Pass

---

# Coin Currency

Primary soft currency.

Used for:

Upgrades

Progression

Gameplay advancement

---

# Currency Separation

Coins:

Progression

Gems:

Premium

Never merge currencies.

---

# Cosmetic System

Future Feature

Must be supported architecturally.

---

# Cosmetic Categories

Soldier Skins

Weapon Skins

Trail Effects

Victory Effects

Bullet Effects

UI Themes

---

# Cosmetic Philosophy

Cosmetics never affect gameplay.

Never provide stat bonuses.

---

# Skin Rarities

Common

Rare

Epic

Legendary

Mythic

---

# Bundle System

Future Feature

Architecture Required

---

# Bundle Examples

Starter Pack

Boss Pack

Gem Bundle

Upgrade Bundle

Season Bundle

---

# Limited Time Offers

Future Feature

Must support:

Timer

Discount

Exclusive Item

Expiration

---

# Battle Pass

Future Feature

Not MVP

Architecture Required

---

# Battle Pass Structure

Free Track

Premium Track

Season Progression

Reward Unlocks

---

# Daily Reward System

Supported by progression architecture.

May include:

Coins

Gems

Cosmetics

Boosters

---

# Offer Trigger System

Future Architecture

Trigger Examples:

First Boss Defeat

Level 10

Level 25

Level 50

Special Events

---

# Store Architecture

Store must support:

Coins

Gems

Bundles

Cosmetics

Battle Pass

Offers

Future Products

---

# Pricing Philosophy

Low Friction

High Value

Fair Pricing

No aggressive monetization

---

# Whale Protection Philosophy

Avoid predatory systems.

Do not create:

Infinite spending loops

Mandatory purchases

Power purchases

---

# Retention Before Revenue

Player retention is more valuable than short-term revenue.

Always prioritize:

Fun

Progression

Engagement

Before monetization.

---

# Live Ops Support

Future Architecture

Supports:

Events

Limited Offers

Seasonal Content

Special Rewards

---

# Analytics Events

Architecture should support tracking:

Ad Viewed

Reward Claimed

Store Opened

Bundle Purchased

Battle Pass Purchased

Session Length

Retention Metrics

---

# Regional Support

Store architecture should support:

Multiple currencies

Localization

Regional pricing

Future expansion

---

# No Energy System

ArmyRush will not use:

Lives

Energy

Stamina

Timers that block gameplay

Players may play as long as they want.

---

# No Paywalls

Progression must remain available without purchases.

Purchases accelerate.

They do not unlock mandatory progression.

---

# Production Requirements

All monetization systems must:

Be optional

Be non-intrusive

Respect player time

Support future growth

Not impact gameplay quality

---

# Codex Instructions

Build monetization architecture.

Do not aggressively surface monetization.

Do not create pay-to-win systems.

Do not create energy systems.

Prioritize retention.

Prioritize player satisfaction.

Support future monetization expansion.

Update TASKS.md after monetization changes.

Update DEVLOG.md after monetization changes.

This document is the monetization source of truth.

