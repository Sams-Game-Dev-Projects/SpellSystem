Spell System - Programmer Guide

Overview
- Data driven spells using ScriptableObject (SpellDefinition)
- Delivery options: Self or Projectile (SpellTargeting)
- Effect options: Instant or OverTime (EffectDelivery)
- Types via DamageType enum (Fire, Ice, Holy, Force, etc)
- Runtime orchestrator: SpellCaster (spends mana, spawns VFX or projectiles, applies effects)
- CharacterHealth handles HP and typed damage or healing plus status effects. ManaPool tracks mana
- Projectiles via SpellProjectile (kinematic forward motion, trigger collision)
- Input via the Input System (Unity Events) routed to PlayerController

Key Scripts
- Scripts/Common/SpellEnums.cs
  - DamageType, SpellTargeting, EffectDelivery enums

- Scripts/Spells/SpellDefinition.cs
  - Data asset with: classification, mana cost, delivery (instant or over time), amount, duration and tick, visuals (projectile, cast, impact), projectile movement params
  - Designers create assets: Create -> Spell System -> Spell

- Scripts/Spells/SpellCaster.cs
  - Depends on ManaPool (RequireComponent). Optionally reads CharacterHealth for self targeting
  - spellSlots[4] for keys 1-4. Use CastSlot(i) or Cast(def)
  - Spawns cast VFX, spends mana, and either applies to self or instantiates a projectile
  - Shared helper ApplySpellToTarget(CharacterHealth, SpellDefinition, GameObject) branches instant vs over time

- Scripts/Spells/SpellProjectile.cs
  - Initialize(SpellDefinition, owner, direction) sets lifetime, ensures collider and rigidbody, and stores the owner to avoid self hit
  - Kinematic forward motion in Update. OnTriggerEnter: apply effect if CharacterHealth is found, spawn impact VFX, destroy

- Scripts/Characters/CharacterHealth.cs
  - Public API: ApplyDamage, ApplyHealing, ApplyOverTime(total, duration, tick, isHealing, type, source)
  - Over time tracked with a small ActiveEffect class: remaining time, tick interval, per tick amount, type, source
  - Destroys the GameObject on health reaching 0. Events: OnHealthChanged(current,max), OnDeath

- Scripts/Characters/ManaPool.cs
  - Tracks max, current, optional regenPerSecond. API: TryConsume(cost) and AddMana
  - Event: OnManaChanged(current,max)

- Scripts/Player/PlayerController.cs
  - WASD movement relative to camera forward; optional CharacterController support
  - Input System (Unity Events) handlers: OnMove, OnSpell1..OnSpell4 -> calls SpellCaster.CastSlot(0..3)
  - Faces movement direction (y only) and moves each Update

Input System
- Use a PlayerInput component set to Behavior = "Invoke Unity Events"
- Actions expected by PlayerController: Player/Move (Vector2) and Player/Spell1..Player/Spell4 (Button)

Visuals
- SpellDefinition exposes: projectilePrefab, castVFXPrefab, impactVFXPrefab
- If projectilePrefab is null, SpellCaster creates a sphere at runtime (adds collider and projectile script)

Status Effects
- CharacterHealth.ApplyOverTime(total, duration, tick, isHealing, type, source) spreads total evenly across ticks (duration divided by tick)
- Each tick triggers ApplyDamage or ApplyHealing

Extensibility
- DamageType can be extended. Add resistances by introducing a resistance table in CharacterHealth (for example Dictionary<DamageType,float> multiplier) and applying it in ApplyDamage
- New delivery mechanisms (for example area or beam): add a new SpellTargeting value and a corresponding runtime handler in SpellCaster
- Cooldowns: add fields to SpellDefinition and a per spell state table in SpellCaster
