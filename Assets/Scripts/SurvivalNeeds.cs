using System;
using UnityEngine;

namespace Olomu.Systems
{
    public class SurvivalNeeds : MonoBehaviour
    {
        public float maxHunger = 100f;
        public float maxThirst = 100f;
        public float hungerDrainRate = 0.8f;
        public float thirstDrainRate = 1.2f;

        public float Hunger { get; private set; } = 100f;
        public float Thirst { get; private set; } = 100f;
        public bool IsAlive { get; private set; } = true;

        public event Action<float> HungerChanged;
        public event Action<float> ThirstChanged;
        public event Action PlayerDied;

        private void Update()
        {
            if (!IsAlive) return;

            Hunger = Mathf.Max(Hunger - hungerDrainRate * Time.deltaTime, 0f);
            Thirst = Mathf.Max(Thirst - thirstDrainRate * Time.deltaTime, 0f);

            HungerChanged?.Invoke(Hunger);
            ThirstChanged?.Invoke(Thirst);

            if (Hunger <= 0f || Thirst <= 0f) Die();
        }

        public void Eat(float amount)
        {
            Hunger = Mathf.Min(Hunger + amount, maxHunger);
            HungerChanged?.Invoke(Hunger);
        }

        public void Drink(float amount)
        {
            Thirst = Mathf.Min(Thirst + amount, maxThirst);
            ThirstChanged?.Invoke(Thirst);
        }

        public void Restore(float hunger, float thirst)
        {
            Eat(hunger);
            Drink(thirst);
        }

        private void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            PlayerDied?.Invoke();
            Debug.Log("Player collapsed from hunger or thirst.");
        }

        public void Revive()
        {
            IsAlive = true;
            Hunger = maxHunger * 0.6f;
            Thirst = maxThirst * 0.6f;
        }
    }
}
