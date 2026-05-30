# AI_AGENT_RULES.md

# ArmyRush AI Agent Rules

Version: 1.0
Status: Mandatory

---

# Purpose

This document defines how AI coding agents, including Codex Goal Mode, must behave while working on ArmyRush.

These rules override convenience.

These rules prioritize production quality.

---

# Golden Rule

The objective is NOT:

"Finish quickly."

The objective IS:

"Build a production-quality mobile game."

---

# Documentation Hierarchy

When conflicts exist:

1. GAME_VISION.md
2. TECH_ARCHITECTURE.md
3. GAMEPLAY_LOOP.md
4. VISUAL_STYLE.md
5. LEVEL_DESIGN.md
6. PROGRESSION.md
7. UI_UX.md
8. MONETIZATION.md
9. CONTENT_PIPELINE.md
10. TASKS.md
11. DEVLOG.md

Follow highest priority source.

---

# Required Startup Process

Before making changes:

Read all files inside Docs/.

Do not assume.

Do not skip documents.

---

# Production First

Always prefer:

Maintainable code

Scalable systems

Reusable architecture

Long-term solutions

Never prefer:

Quick hacks

Temporary fixes

Magic numbers

Hardcoded content

---

# Placeholder Rules

Temporary placeholders are allowed.

Permanent placeholders are forbidden.

---

# Forbidden Final Content

Unity Capsules

Unity Cubes

Unity Spheres

Default Materials

Default UI

Placeholder Icons

Placeholder Audio

Placeholder Animations

Debug Art

---

# Definition Of Complete

A feature is not complete until:

Implementation exists

UI exists

Audio exists

VFX exists

Testing exists

Documentation updated

No errors exist

---

# Task Rules

Never mark a task complete prematurely.

Verify implementation first.

---

# Documentation Rules

After major work:

Update TASKS.md

Update DEVLOG.md

No exceptions.

---

# Performance Rules

Target:

60 FPS

iPhone

iPad

Performance matters.

---

# Error Rules

Zero console errors.

Zero missing references.

Zero broken prefabs.

Zero compilation errors.

---

# Architecture Rules

Respect existing architecture.

Do not create duplicate systems.

Do not create redundant managers.

Do not create parallel implementations.

---

# Refactor Rules

Refactor when:

Code duplication exists

Maintainability suffers

Architecture degrades

---

# Asset Rules

Every asset must:

Follow CONTENT_PIPELINE.md

Be named correctly

Be organized correctly

Be optimized

---

# Scene Rules

Never create unnecessary scenes.

Keep scene count manageable.

---

# Gameplay Rules

Gameplay feel is more important than implementation speed.

---

# UI Rules

UI must be polished.

UI must be responsive.

UI must feel commercial quality.

---

# Mobile Rules

ArmyRush is mobile-first.

Every feature must support:

Portrait

Touch input

iPhone

iPad

---

# Completion Workflow

Implement

↓

Test

↓

Polish

↓

Optimize

↓

Update TASKS.md

↓

Update DEVLOG.md

↓

Mark Complete

---

# Commit Rules

Commit frequently.

Small commits preferred.

Large risky commits discouraged.

---

# Quality Over Speed

If forced to choose:

Choose quality.

---

# Final Rule

Build ArmyRush like a commercial mobile game.

Never build ArmyRush like a prototype.

