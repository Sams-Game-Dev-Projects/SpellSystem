using UnityEngine;
using UnityEngine.Events;

namespace SpellSystem
{
    // Tracks mana and exposes a simple consumption API.
    public class ManaPool : MonoBehaviour
    {
        [Header("Mana")]
        public float maxMana = 100f;
        public float startMana = 100f;
        public float regenPerSecond = 0f; // Set >0 for passive regen.

        [SerializeField] private float currentMana;
        public float CurrentMana => currentMana;

        public UnityEvent<float, float> OnManaChanged; // (current, max)

        private void Awake()
        {
            currentMana = Mathf.Clamp(startMana, 0f, maxMana);
            RaiseManaChanged();
        }

        private void Update()
        {
            if (regenPerSecond > 0f && currentMana < maxMana)
            {
                currentMana = Mathf.Clamp(currentMana + regenPerSecond * Time.deltaTime, 0f, maxMana);
                RaiseManaChanged();
            }
        }

        public bool TryConsume(float cost)
        {
            if (cost <= 0f) return true;
            if (currentMana < cost) return false;
            currentMana -= cost;
            RaiseManaChanged();
            return true;
        }

        public void AddMana(float amount)
        {
            currentMana = Mathf.Clamp(currentMana + Mathf.Max(0f, amount), 0f, maxMana);
            RaiseManaChanged();
        }

        private void RaiseManaChanged()
        {
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }
}

