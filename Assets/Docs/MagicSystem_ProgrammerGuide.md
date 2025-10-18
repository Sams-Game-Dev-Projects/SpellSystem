Spell System – Programmer Guide

Overview
- Data‑driven spells using `ScriptableObject` (`SpellDefinition`).
- Delivery options: `Self` or `Projectile` (SpellTargeting).
- Effect options: `Instant` or `OverTime` (EffectDelivery).
- Types via `DamageType` enum (Fire, Ice, Holy, Force, etc.).
- Runtime orchestrator: `SpellCaster` (spends mana, spawns VFX/projectiles, applies effects).
- `CharacterHealth` handles HP and typed damage + status effects. `ManaPool` tracks mana.
- Projectiles via `SpellProjectile` (kinematic forward motion, trigger collision).
- Input via new Input System (Unity Events) routed to `PlayerController`.

Key Scripts
- `Scripts/Common/SpellEnums.cs`
  • `DamageType`, `SpellTargeting`, `EffectDelivery` enums.

- `Scripts/Spells/SpellDefinition.cs`
  • SO containing: classification, mana cost, delivery (instant/over time), amount, duration/tick, visuals (projectile/cast/impact), and projectile movement params.
  • Designers create assets: Create → Spell System → Spell.

- `Scripts/Spells/SpellCaster.cs`
  • Depends on `ManaPool` (RequireComponent). Optionally reads `CharacterHealth` for self-target.
  • `spellSlots[4]` for 1–4 casting. `CastSlot(i)` or `Cast(def)`.
  • Spawns cast VFX, spends mana, and either applies to self or instantiates a projectile.
  • Shared helper `ApplySpellToTarget(CharacterHealth, SpellDefinition, GameObject)` branches instant vs. over‑time.

- `Scripts/Spells/SpellProjectile.cs`
  • `Initialize(SpellDefinition, owner, direction)` sets lifetime, ensures collider/rigidbody, and stores the owner to avoid self‑hit.
  • Kinematic forward motion in `Update()`. OnTriggerEnter: apply effect if `CharacterHealth` is found; spawn impact VFX; destroy.

- `Scripts/Characters/CharacterHealth.cs`
  • Public API: `ApplyDamage`, `ApplyHealing`, `ApplyOverTime(total, duration, tick, isHealing, type, source)`.
  • Over‑time tracked via a small `ActiveEffect` class: remembers remaining time, tick interval, per‑tick amount, type, and source.
  • Destroys the GameObject on health reaching 0. Events: `OnHealthChanged(current,max)`, `OnDeath`.

- `Scripts/Characters/ManaPool.cs`
  • Tracks `max`, `current`, optional `regenPerSecond`. API: `TryConsume(cost)` and `AddMana`.
  • Event: `OnManaChanged(current,max)`.

- `Scripts/Player/PlayerController.cs`
  • WASD movement relative to camera forward; optional `CharacterController` support.
  • Input System (Unity Events) handlers: `OnMove`, `OnSpell1`…`OnSpell4` → calls `SpellCaster.CastSlot(0..3)`.
  • Faces movement direction (y‑only) and moves each `Update()`.

Input System
- Use a `PlayerInput` component set to Behavior = “Invoke Unity Events”.
- Actions expected by `PlayerController`: `Player/Move` (Vector2) and `Player/Spell1..Spell4` (Button).
- You can use your project’s existing `.inputactions` asset. Ensure the `Player` map contains those actions and binds 1–4.

Visuals
- `SpellDefinition` exposes: `projectilePrefab`, `castVFXPrefab`, `impactVFXPrefab`.
- If `projectilePrefab` is null, `SpellCaster` creates a sphere at runtime (adds collider + projectile script).
- Audio is optional via `castSfx`/`impactSfx`.

Status Effects
- `CharacterHealth.ApplyOverTime(total, duration, tick, isHealing, type, source)` spreads `total` evenly across ticks (`duration/tick`).
- Each tick triggers `ApplyDamage` or `ApplyHealing`.
- Effects are lightweight; consider pooling/combining for heavy use.

Extensibility
- DamageType can be extended. Add resistances by introducing a resistance table in `CharacterHealth` (e.g., `Dictionary<DamageType,float>` multiplier) and applying it in `ApplyDamage`.
- New delivery mechanisms (e.g., area, beam): add a new `SpellTargeting` and a corresponding runtime handler in `SpellCaster`.
- Cooldowns: add fields to `SpellDefinition` and a per‑spell state table in `SpellCaster`.
- Object pooling: swap `Instantiate/Destroy` with a pool for projectiles/VFX.

Editor Tools
- `Editor/SpellSystemSetup.cs` provides:
  • Create Sample Content: materials, VFX prefabs, projectile prefabs, and example spells (Instant Heal, Heal Regen, Fireball DoT, Eldritch Blast).
  • Setup Sample Scene: builds a quick playground with a Player and enemies and assigns spells.

Conventions/Notes
- Everything uses meters/seconds; projectile movement is kinematic and frame‑rate independent.
- Projectiles ignore their owner (simple same‑GO check). For teams, replace with team/faction filters.
- `SpellDefinition.amount` is the total for over‑time and exact for instant.
- All methods are small and documented; designer‑facing choices sit in ScriptableObjects.
