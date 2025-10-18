using UnityEngine;
using UnityEngine.InputSystem;

namespace SpellSystem
{
    // WASD movement with A/D strafing and mouse look for turning. Uses Input System Unity Events.
    [RequireComponent(typeof(SpellCaster))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;

        [Header("Mouse Look")]
        public float mouseSensitivity = 2.0f; // degrees per pixel

        private Vector2 _moveInput;
        private Vector2 _lookDelta;
        private float _yaw;

        private SpellCaster _caster;
        private CharacterController _cc; // optional

        private void Awake()
        {
            _caster = GetComponent<SpellCaster>();
            _cc = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            // Apply mouse look yaw to turn player
            if (_lookDelta.sqrMagnitude > 0f)
            {
                _yaw += _lookDelta.x * mouseSensitivity;
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                // consume this frame's delta
                _lookDelta = Vector2.zero;
            }

            // Move relative to player facing (W/S forward/back, A/D strafe)
            Vector3 move = GetLocalMoveVector(_moveInput, transform);

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

        // Input System: bind Player/Look to this (uses mouse delta)
        public void OnLook(InputAction.CallbackContext context)
        {
            var delta = context.ReadValue<Vector2>();
            _lookDelta += delta; // accumulate for this frame
        }

        // Bind actions Player/Spell1..4 to these. Only cast on performed.
        public void OnSpell1(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(0); }
        public void OnSpell2(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(1); }
        public void OnSpell3(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(2); }
        public void OnSpell4(InputAction.CallbackContext context) { if (context.performed) _caster.CastSlot(3); }

        private static Vector3 GetLocalMoveVector(Vector2 input, Transform t)
        {
            var forward = t.forward; forward.y = 0f; forward.Normalize();
            var right = t.right; right.y = 0f; right.Normalize();
            var v = (right * input.x + forward * input.y);
            if (v.sqrMagnitude > 1f) v.Normalize();
            return v;
        }
    }
}
