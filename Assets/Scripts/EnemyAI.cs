using UnityEngine;

namespace Olomu.Systems
{
    public class EnemyAI : MonoBehaviour
    {
        public enum State { Patrol, Chase, Attack, Dead }

        public float patrolRadius = 9f;
        public float walkSpeed = 1.7f;
        public float chaseSpeed = 5.9f;
        public float detectRadius = 10f;
        public float attackRange = 1.8f;
        public float attackDamage = 12f;
        public float attackCooldown = 1.15f;
        public float health = 100f;

        public string lootItem = "hide";
        public int lootAmount = 1;

        public State Current { get; private set; } = State.Patrol;

        private Transform player;
        private CharacterController cc;
        private Health playerHealth;
        private Vector3 home;
        private Vector3 patrolTarget;
        private bool patrolling;
        private float idleTimer;
        private float cooldownTimer;
        private static readonly System.Collections.Generic.List<EnemyAI> all =
            new System.Collections.Generic.List<EnemyAI>();

        public static EnemyAI FindNearest(Vector3 pos, float maxDist)
        {
            EnemyAI best = null;
            float bestSqr = maxDist * maxDist;
            for (int i = 0; i < all.Count; i++)
            {
                var e = all[i];
                if (e.Current == State.Dead) continue;
                float d = (e.transform.position - pos).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = e; }
            }
            return best;
        }

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            home = transform.position;
        }

        private void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
            }
        }

        private void Update()
        {
            if (Current == State.Dead || player == null) return;
            cooldownTimer -= Time.deltaTime;

            float distToPlayer = Vector3.Distance(transform.position, player.position);

            switch (Current)
            {
                case State.Patrol:
                    if (distToPlayer < detectRadius && playerHealth != null && playerHealth.IsAlive)
                        Current = State.Chase;
                    else Patrol();
                    break;

                case State.Chase:
                    if (playerHealth == null || !playerHealth.IsAlive || distToPlayer > detectRadius * 2.2f)
                    {
                        Current = State.Patrol;
                        break;
                    }
                    if (distToPlayer <= attackRange)
                    {
                        Current = State.Attack;
                        break;
                    }
                    MoveTowards(player.position, chaseSpeed);
                    break;

                case State.Attack:
                    FaceTowards(player.position);
                    if (distToPlayer > attackRange * 1.35f) { Current = State.Chase; break; }
                    if (cooldownTimer <= 0f)
                    {
                        cooldownTimer = attackCooldown;
                        if (playerHealth != null) playerHealth.Damage(attackDamage);
                    }
                    break;
            }
        }

        private void Patrol()
        {
            if (!patrolling)
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    Vector2 r = Random.insideUnitCircle * patrolRadius;
                    patrolTarget = home + new Vector3(r.x, 0f, r.y);
                    patrolling = true;
                }
                return;
            }

            Vector3 flat = patrolTarget - transform.position;
            flat.y = 0f;
            if (flat.magnitude < 0.4f)
            {
                patrolling = false;
                idleTimer = Random.Range(1.5f, 4f);
                return;
            }
            MoveTowards(patrolTarget, walkSpeed);
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            Vector3 flat = target - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < 0.001f) return;
            Vector3 dir = flat.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), 5f * Time.deltaTime);
            cc.Move(dir * speed * Time.deltaTime + Vector3.down * 2.5f * Time.deltaTime);
        }

        private void FaceTowards(Vector3 target)
        {
            Vector3 flat = target - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(flat.normalized), 8f * Time.deltaTime);
        }

        public bool TakeDamage(float amount)
        {
            if (Current == State.Dead) return false;
            health -= amount;
            if (health <= 0f)
            {
                Current = State.Dead;
                StartCoroutine(DeathSequence());
                return true;
            }
            if (Current == State.Patrol) Current = State.Chase;
            return false;
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            var inv = player != null ? player.GetComponent<Inventory>() : null;
            if (inv != null && lootAmount > 0) inv.AddItem(lootItem, lootAmount);

            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            cc.enabled = false;

            yield return new WaitForSeconds(30f);
            Destroy(gameObject);
        }
    }
}
