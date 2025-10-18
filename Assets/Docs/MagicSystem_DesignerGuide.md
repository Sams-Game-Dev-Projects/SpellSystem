Spell System - Designer Guide

What this gives you
- 4 spell slots bound to keys 1-4
- Self target spells (instant or over time)
- Projectile spells (e.g. Fireball DoT, Eldritch Blast instant)
- Easy visuals: assign any 3D object and particle prefabs
- Mana cost per spell

Setup (Example already setup, but you can also setup your own with the following)

1) If Input System is not already setup, then Window > Package Manager > install Input System and enable it

Add the player
1) Create or select your Player object
2) Add components: CharacterHealth, ManaPool, SpellCaster, PlayerController, and (optional) CharacterController
3) Add a PlayerInput component (from the Input System)
   - Behavior: set to "Invoke Unity Events"
   - Actions: assign your existing Input Actions asset (for example InputSystem_Actions.inputactions)
   - Make sure the Player action map has these actions and bindings:
     - Move (Value Vector2), bind WASD or stick
     - Look (Also a Vector2), bind to Delta for mouse, stick for controller
     - Spell1 bound to Keyboard 1
     - Spell2 bound to Keyboard 2
     - Spell3 bound to Keyboard 3
     - Spell4 bound to Keyboard 4
   - In the events foldout, connect:
     - Player/Move -> PlayerController.OnMove
     - Player/Look -> PlayerController.OnLook
     - Player/Spell1 -> PlayerController.OnSpell1
     - Player/Spell2 -> PlayerController.OnSpell2
     - Player/Spell3 -> PlayerController.OnSpell3
     - Player/Spell4 -> PlayerController.OnSpell4

Create and assign spells
1) In the Project window, create a new spell: Create -> Spell System -> Spell
2) In the Inspector set:
   - Targeting: Self or Projectile
   - Type: Fire, Ice, Holy, Force, etc
   - Is Healing: on for heals, off for damage
   - Mana Cost: integer cost
   - Delivery: Instant or Over Time
   - Amount: Instant uses this exact value. Over Time is the total across the duration
   - Duration and Tick Interval: used only for Over Time
   - Visuals:
     - Projectile Prefab: any 3D object (e.g. a Sphere). The system will add needed components at runtime
     - Cast VFX Prefab: particle prefab spawned at the caster
     - Impact VFX Prefab: particle prefab spawned on impact
   - Projectile Settings: speed, lifetime, spawn offset
3) Select the Player -> SpellCaster and place your spells in slots 1-4
4) Press Play. Use WASD to move, 1-4 to cast in your facing direction

Tips
- Hook UI for health and mana using CharacterHealth.OnHealthChanged and ManaPool.OnManaChanged
- If a Camera is tagged MainCamera, movement aligns to camera forward
