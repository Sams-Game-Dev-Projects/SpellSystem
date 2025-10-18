using UnityEngine;
using UnityEngine.InputSystem;

namespace SpellSystem
{
    // Simple WASD controller that faces move direction and casts spells via Input System Unity Events.
    [RequireComponent(typeof(SpellCaster))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f; // deg/sec

        private Vector2 _moveInput;
        private SpellCaster _caster;
        private CharacterController _cc; // optional

        private void Awake()
        {
            _caster = GetComponent<SpellCaster>();
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 move = GetWorldMoveVector(_moveInput);
            if (move.sqrMagnitude > 0.0001f)
            {
                // Face movement direction (y-only)
                var targetRot = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Move
            Vector3 displacement = move * moveSpeed * Time.deltaTime;
            if (_cc != null)
            {
                _cc.Move(displacement);
            }
            else
            {
                transform.position += displacement;
            }
        }

        // Input System (Unity Events): bind Player/Move to this
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        // Bind actions Player/Spell1..4 to these. Only cast on performed.
        public void OnSpell1(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(0); }
        public void OnSpell2(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(1); }
        public void OnSpell3(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(2); }
        public void OnSpell4(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(3); }

        private static Vector3 GetWorldMoveVector(Vector2 input)
        {
            var v = new Vector3(input.x, 0f, input.y);
            if (v.sqrMagnitude > 1f) v.Normalize();

            // If a camera exists, move relative to its facing
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 fwd = cam.transform.forward; fwd.y = 0f; fwd.Normalize();
                Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();
                return (fwd * v.z + right * v.x).normalized;
            }

            return v;
        }
    }
}

