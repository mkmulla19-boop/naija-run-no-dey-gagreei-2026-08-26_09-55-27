using UnityEngine;

namespace Olomu.Systems
{
    public class Interactor : MonoBehaviour
    {
        public float range = 2.4f;

        public Gatherable CurrentTarget { get; private set; }
        public DrinkSpot CurrentDrinkSpot { get; private set; }

        private ThirdPersonController player;
        private float scanTimer;

        private void Awake()
        {
            player = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer > 0f) return;
            scanTimer = 0.15f;

            CurrentTarget = Gatherable.FindNearest(transform.position, range);
            CurrentDrinkSpot = DrinkSpot.FindNearest(transform.position, range);
        }

        public bool TryGather(Inventory inventory)
        {
            if (CurrentTarget == null) return false;
            return CurrentTarget.TryGather(inventory, player);
        }
    }
}
