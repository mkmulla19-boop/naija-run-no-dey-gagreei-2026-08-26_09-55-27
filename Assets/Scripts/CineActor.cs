using UnityEngine;

namespace Olomu.Systems
{
    public class CineActor : MonoBehaviour
    {
        public Vector3[] waypoints;
        public float speed = 3.2f;
        public bool autoStart = false;

        private int index;
        private bool walking;
        private CharacterController cc;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (autoStart) Begin();
        }

        public void Begin()
        {
            index = 0;
            walking = waypoints != null && waypoints.Length > 0;
            if (walking) transform.rotation = Quaternion.LookRotation(FlatDir());
        }

        public bool IsWalking => walking;

        private Vector3 FlatDir()
        {
            Vector3 d = waypoints[index] - transform.position;
            d.y = 0f;
            return d.normalized;
        }

        private void Update()
        {
            if (!walking || cc == null) return;

            Vector3 dir = FlatDir();
            float step = speed * Time.deltaTime;
            if ((waypoints[index] - transform.position).magnitude <= step + 0.15f)
            {
                transform.position = new Vector3(waypoints[index].x, transform.position.y, waypoints[index].z);
                index++;
                if (index >= waypoints.Length) { walking = false; return; }
                dir = FlatDir();
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
            cc.Move(dir * step + Vector3.down * 2f * Time.deltaTime);
        }
    }
}
