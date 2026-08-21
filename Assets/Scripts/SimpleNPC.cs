using System.Collections;
using UnityEngine;

namespace Olomu.Systems
{
    public class SimpleNPC : MonoBehaviour
    {
        public float wanderRadius = 8f;
        public float walkSpeed = 1.6f;
        public Vector2 idleTimeRange = new Vector2(2f, 4.5f);

        private Vector3 home;
        private Vector3 target;
        private bool walking;
        private CharacterController cc;
        private float idleTimer;

        private void Awake()
        {
            home = transform.position;
            target = home;
            cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (walking)
            {
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0f;
                if (toTarget.magnitude < 0.25f)
                {
                    walking = false;
                    idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
                    return;
                }

                Vector3 dir = toTarget.normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 4f * Time.deltaTime);
                cc.Move(dir * walkSpeed * Time.deltaTime + Vector3.down * 2f * Time.deltaTime);
            }
            else
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f) PickNewTarget();
            }
        }

        private void PickNewTarget()
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            target = home + new Vector3(r.x, 0f, r.y);
            walking = true;
        }
    }
}
