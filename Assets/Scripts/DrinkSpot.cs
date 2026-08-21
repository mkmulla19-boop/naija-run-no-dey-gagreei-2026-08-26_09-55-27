using System.Collections;
using UnityEngine;

namespace Olomu.Systems
{
    public class DrinkSpot : MonoBehaviour
    {
        public float drinkAmount = 40f;
        public float cooldownSeconds = 20f;

        public bool IsReady => !onCooldown;
        private bool onCooldown;

        private static readonly System.Collections.Generic.List<DrinkSpot> all =
            new System.Collections.Generic.List<DrinkSpot>();

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);

        public static DrinkSpot FindNearest(Vector3 pos, float maxDist)
        {
            DrinkSpot best = null;
            float bestSqr = maxDist * maxDist;
            for (int i = 0; i < all.Count; i++)
            {
                DrinkSpot s = all[i];
                if (!s.IsReady) continue;
                float d = (s.transform.position - pos).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = s; }
            }
            return best;
        }

        public bool TryDrink(SurvivalNeeds needs)
        {
            if (onCooldown || needs == null || !needs.IsAlive) return false;
            needs.Drink(drinkAmount);
            StartCoroutine(CooldownRoutine());
            return true;
        }

        private IEnumerator CooldownRoutine()
        {
            onCooldown = true;
            yield return new WaitForSeconds(cooldownSeconds);
            onCooldown = false;
        }
    }
}
