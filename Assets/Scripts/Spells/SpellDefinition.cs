using UnityEngine;

namespace SpellSystem
{
    // Data-only definition of a spell. Designers create assets of this type.
    [CreateAssetMenu(menuName = "Spell System/Spell", fileName = "NewSpell")]
    public class SpellDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "New Spell";
        [TextArea]
        public string description;

        [Header("Classification")]
        public SpellTargeting targeting = SpellTargeting.Projectile;
        public DamageType type = DamageType.Fire;
        [Tooltip("True = healing, False = damage.")]
        public bool isHealing = false;
        [Min(0)] public int manaCost = 10;

        [Header("Effect")]
        public EffectDelivery delivery = EffectDelivery.Instant;
        [Tooltip("Instant: exact amount. OverTime: total amount across duration.")]
        public float amount = 25f;
        [Tooltip("Duration for OverTime effects.")]
        public float duration = 5f;
        [Tooltip("Tick interval for OverTime effects.")]
        public float tickInterval = 1f;

        [Header("Visuals & Audio")]
        [Tooltip("Main projectile object (e.g., a Sphere). Required for projectile spells.")]
        public GameObject projectilePrefab;
        [Tooltip("Particles to spawn at cast.")]
        public GameObject castVFXPrefab;
        [Tooltip("Particles to spawn on impact.")]
        public GameObject impactVFXPrefab;
        public AudioClip castSfx;
        public AudioClip impactSfx;

        [Header("Projectile Settings")]
        [Tooltip("Units per second.")]
        public float projectileSpeed = 15f;
        [Tooltip("Seconds before despawn if no impact.")]
        public float projectileLifetime = 5f;
        [Tooltip("Forward offset from caster when spawning.")]
        public float spawnOffset = 1f;
    }
}

