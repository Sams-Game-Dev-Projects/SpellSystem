Spell System – Designer Guide

What this gives you
- 4 spell slots bound to keys 1–4.
- Self-target spells (instant or over time) like heal and regen.
- Outgoing spells (projectiles) like Fireball (DoT) and Eldritch Blast (instant).
- Easy visuals: assign any 3D object for the projectile and any particle prefabs for cast/impact.
- Mana cost on each spell.

Setup (one-time)
1) Window > Package Manager > ensure “Input System” is installed and active (Unity will prompt to restart).
2) Top menu Tools > Spell System > Create Sample Content. This creates ready-to-use spells and VFX under `Assets/SpellSystem`.
3) Optional: Tools > Spell System > Setup Sample Scene to see everything wired.

Add the player
1) Create or select your Player object.
2) Add components: `CharacterHealth`, `ManaPool`, `SpellCaster`, `PlayerController`, and (recommended) `CharacterController`.
3) Add a `PlayerInput` component (from the Input System):
   - Behavior: set to “Invoke Unity Events”.
   - Actions: use your existing Input Actions asset (e.g., `InputSystem_Actions.inputactions`) or any you prefer.
   - Ensure the Player action map contains these actions and bindings:
     • `Move` (Value/Vector2) with WASD or stick.
     • `Spell1` bound to `Keyboard 1`
     • `Spell2` bound to `Keyboard 2`
     • `Spell3` bound to `Keyboard 3`
     • `Spell4` bound to `Keyboard 4`
   - In the events foldout, hook:
     • Player/Move → PlayerController.OnMove
     • Player/Spell1 → PlayerController.OnSpell1
     • Player/Spell2 → PlayerController.OnSpell2
     • Player/Spell3 → PlayerController.OnSpell3
     • Player/Spell4 → PlayerController.OnSpell4

Assign the example spells
1) Select the Player → SpellCaster.
2) Drag these into slots 1–4 in order:
   - Self_Heal_Instant
   - Self_Heal_Regen
   - Fireball_DoT
   - EldritchBlast_Instant
3) Press Play. Use WASD to move, 1–4 to cast in your facing direction.

Make your own spell (no coding)
1) Right‑click in Project window → Create → Spell System → Spell.
2) In the Inspector:
   - Name/Description: for your reference.
   - Targeting: Self or Projectile.
   - Type: Fire, Ice, Holy, Force, etc.
   - Is Healing: on for heals, off for damage.
   - Mana Cost: integer cost.
   - Delivery: Instant or Over Time.
   - Amount: Instant = exact amount. Over Time = total amount over the duration.
   - Duration/Tick Interval: only for Over Time.
   - Visuals:
     • Projectile Prefab: any 3D object (e.g., a Sphere). The system will add needed components at runtime.
     • Cast VFX Prefab: particle prefab spawned at the caster.
     • Impact VFX Prefab: particle prefab spawned where the projectile hits.
   - Projectile Settings: speed, lifetime, and spawn offset from the player.
3) Assign your new Spell to a `SpellCaster` slot on the Player.

Tips
- You can tweak movement speed on the PlayerController.
- Hook UI for health/mana: `CharacterHealth.OnHealthChanged` and `ManaPool.OnManaChanged` (both provide current,max).
- To aim with camera, keep a Camera tagged MainCamera; player movement aligns to camera forward.
