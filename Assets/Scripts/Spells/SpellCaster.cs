using UnityEngine;
using UnityEngine.Events;

namespace SpellSystem
{
    // Responsible for spending mana and delivering spells to targets/projectiles.
    [RequireComponent(typeof(ManaPool))]
    [DisallowMultipleComponent]
    public class SpellCaster : MonoBehaviour
    {
        [Tooltip("Spells available in slots 1-4.")]
        public SpellDefinition[] spellSlots = new SpellDefinition[4];

        [Tooltip("Optional spawn transform for projectiles. If not set, uses caster forward + offset.")]
        public Transform projectileSpawnPoint;

        public UnityEvent<SpellDefinition> OnSpellCast; // for UI hooks/logging

        private ManaPool _mana;
        private CharacterHealth _selfHealth;

        private void Awake()
        {
            _mana = GetComponent<ManaPool>();
            _selfHealth = GetComponent<CharacterHealth>();
        }

        public bool CastSlot(int slotIndex)
        {
            var spell = GetSpell(slotIndex);
            if (spell == null) return false;
            return Cast(spell);
        }

        public bool Cast(SpellDefinition spell)
        {
            if (spell == null) return false;

            if (_mana != null && !_mana.TryConsume(spell.manaCost))
            {
                // Not enough mana.
                return false;
            }

            // Cast VFX/SFX at origin (one-shot, auto-destroy)
            if (spell.castVFXPrefab != null)
                SpawnOneShotVFX(spell.castVFXPrefab, transform.position, transform.rotation);
            if (spell.castSfx != null)
                AudioSource.PlayClipAtPoint(spell.castSfx, transform.position);

            switch (spell.targeting)
            {
                case SpellTargeting.Self:
                    ApplySpellToTarget(_selfHealth, spell, gameObject);
                    break;
                case SpellTargeting.Projectile:
                    SpawnProjectile(spell);
                    break;
            }

            OnSpellCast?.Invoke(spell);
            return true;
        }

        private void SpawnProjectile(SpellDefinition spell)
        {
            Vector3 forward = transform.forward;
            Vector3 spawnPos = projectileSpawnPoint
                ? projectileSpawnPoint.position
                : (transform.position + forward * Mathf.Max(0f, spell.spawnOffset) + Vector3.up * 0.5f);
            Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

            GameObject projectile;
            if (spell.projectilePrefab != null)
            {
                projectile = Instantiate(spell.projectilePrefab, spawnPos, rot);
            }
            else
            {
                // Fallback: simple sphere projectile if none assigned.
                projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projectile.transform.SetPositionAndRotation(spawnPos, rot);
                projectile.transform.localScale = Vector3.one * 0.3f;
            }

            var proj = projectile.GetComponent<SpellProjectile>();
            if (proj == null) proj = projectile.AddComponent<SpellProjectile>();
            proj.Initialize(spell, gameObject, forward);
        }

        // Shared helper: apply spell either instant or over-time.
        internal static void ApplySpellToTarget(CharacterHealth target, SpellDefinition spell, GameObject source)
        {
            if (target == null || spell == null) return;

            if (spell.delivery == EffectDelivery.Instant)
            {
                if (spell.isHealing)
                    target.ApplyHealing(spell.amount, spell.type, source);
                else
                    target.ApplyDamage(spell.amount, spell.type, source);
            }
            else
            {
                target.ApplyOverTime(spell.amount, spell.duration, spell.tickInterval, spell.isHealing, spell.type, source);
            }
        }

        private SpellDefinition GetSpell(int index)
        {
            if (spellSlots == null || index < 0 || index >= spellSlots.Length) return null;
            return spellSlots[index];
        }

        private static void SpawnOneShotVFX(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var go = Instantiate(prefab, position, rotation);
            // Try to make any particles auto-destroy and non-looping, even if the prefab loops.
            var systems = go.GetComponentsInChildren<ParticleSystem>(true);
            float maxDur = 0f;
            foreach (var ps in systems)
            {
                var main = ps.main;
                main.loop = false;
                main.stopAction = ParticleSystemStopAction.Destroy;

                // Approximate lifetime for safety-destroy
                float dur = main.duration;
                var sl = main.startLifetime;
                float life = sl.mode == ParticleSystemCurveMode.TwoConstants ? sl.constantMax : sl.constant;
                maxDur = Mathf.Max(maxDur, dur + life);
            }
            if (systems.Length > 0)
            {
                foreach (var ps in systems) ps.Play();
                Object.Destroy(go, Mathf.Clamp(maxDur + 0.5f, 0.5f, 10f));
            }
            else
            {
                // No particles? Destroy after a short delay to be safe.
                Object.Destroy(go, 3f);
            }
        }
    }
}
