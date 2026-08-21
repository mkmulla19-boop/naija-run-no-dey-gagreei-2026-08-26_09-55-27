using UnityEngine;

namespace Olomu.Systems
{
    public class Interactor : MonoBehaviour
    {
        public float range = 2.4f;
        public float attackRange = 2.3f;
        public float attackDamage = 34f;

        public Gatherable CurrentTarget { get; private set; }
        public DrinkSpot CurrentDrinkSpot { get; private set; }
        public EnemyAI CurrentEnemy { get; private set; }

        public System.Action AttackPerformed;

        private ThirdPersonController player;
        private float scanTimer;
        private float attackTimer;

        private void Awake()
        {
            player = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer > 0f) return;
            scanTimer = 0.15f;

            CurrentEnemy = EnemyAI.FindNearest(transform.position, attackRange);
            CurrentTarget = Gatherable.FindNearest(transform.position, range);
            CurrentDrinkSpot = DrinkSpot.FindNearest(transform.position, range);
        }

        public bool TryGather(Inventory inventory)
        {
            if (CurrentTarget == null) return false;
            return CurrentTarget.TryGather(inventory, player);
        }

        public bool TryAttack()
        {
            if (Time.time < attackTimer) return false;
            attackTimer = Time.time + 0.55f;
            player?.PlayAttack();
            AttackPerformed?.Invoke();
            if (CurrentEnemy != null)
            {
                bool killed = CurrentEnemy.TakeDamage(attackDamage);
                return true;
            }
            return true;
        }
    }
}
