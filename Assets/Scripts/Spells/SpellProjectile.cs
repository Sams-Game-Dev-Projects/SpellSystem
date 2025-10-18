using UnityEngine;

namespace SpellSystem
{
    // Moves forward and applies its SpellDefinition on contact.
    [DisallowMultipleComponent]
    public class SpellProjectile : MonoBehaviour
    {
        [Tooltip("Runtime-assigned spell data for this projectile instance.")]
        public SpellDefinition spell;

        [Tooltip("The GameObject that fired this projectile. Used to avoid self-hit.")]
        public GameObject owner;

        [Tooltip("Normalized forward direction of travel.")]
        public Vector3 direction = Vector3.forward;

        private float _lifetime;

        public void Initialize(SpellDefinition def, GameObject owner, Vector3 dir)
        {
            this.spell = def;
            this.owner = owner;
            direction = dir.normalized;
            _lifetime = (def != null && def.projectileLifetime > 0f) ? def.projectileLifetime : 5f;

            // Ensure it can move and detect hits.
            var rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true; // we move kinematically

            var col = GetComponent<Collider>();
            if (col == null) col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            float speed = (spell != null) ? Mathf.Max(0f, spell.projectileSpeed) : 10f;
            transform.position += direction * speed * Time.deltaTime;

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            var hitGo = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            if (hitGo == owner) return; // don't hit the caster

            var health = other.GetComponentInParent<CharacterHealth>();
            if (health != null)
            {
                SpellCaster.ApplySpellToTarget(health, spell, owner);
                if (spell != null)
                {
                    if (spell.impactVFXPrefab != null)
                        SpawnOneShotVFX(spell.impactVFXPrefab, transform.position);
                    if (spell.impactSfx != null)
                        AudioSource.PlayClipAtPoint(spell.impactSfx, transform.position);
                }
                Destroy(gameObject);
                return;
            }

            // Hit environment: still show VFX and despawn.
            if (spell != null && spell.impactVFXPrefab != null)
                SpawnOneShotVFX(spell.impactVFXPrefab, transform.position);
            Destroy(gameObject);
        }

        private static void SpawnOneShotVFX(GameObject prefab, Vector3 position)
        {
            var go = Instantiate(prefab, position, Quaternion.identity);
            var systems = go.GetComponentsInChildren<ParticleSystem>(true);
            float maxDur = 0f;
            foreach (var ps in systems)
            {
                var main = ps.main;
                main.loop = false;
                main.stopAction = ParticleSystemStopAction.Destroy;

                float dur = main.duration;
                var sl = main.startLifetime;
                float life = sl.mode == ParticleSystemCurveMode.TwoConstants ? sl.constantMax : sl.constant;
                maxDur = Mathf.Max(maxDur, dur + life);
            }
            if (systems.Length > 0)
            {
                foreach (var ps in systems) ps.Play();
                Destroy(go, Mathf.Clamp(maxDur + 0.5f, 0.5f, 10f));
            }
            else
            {
                Destroy(go, 3f);
            }
        }
    }
}
