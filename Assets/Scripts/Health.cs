using System;
using UnityEngine;

namespace Olomu.Systems
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float Current { get; private set; }
        public bool IsAlive { get; private set; } = true;

        public event Action<float> Changed;
        public event Action Died;

        private void Awake() => Current = maxHealth;

        public void Damage(float amount)
        {
            if (!IsAlive) return;
            Current = Mathf.Max(Current - amount, 0f);
            Changed?.Invoke(Current);
            if (Current <= 0f)
            {
                IsAlive = false;
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            Current = Mathf.Min(Current + amount, maxHealth);
            Changed?.Invoke(Current);
        }

        public void ResetFull()
        {
            Current = maxHealth;
            IsAlive = true;
            Changed?.Invoke(Current);
        }
    }
}
