using System.Collections;
using UnityEngine;

namespace Olomu.Systems
{
    public class VirtualJoystick : MonoBehaviour
    {
        [Header("Visuals")]
        public RectTransform baseCircle;
        public RectTransform knob;

        [Header("Tuning")]
        public float radius = 130f;
        [Range(0f, 0.5f)] public float deadzone = 0.18f;
        public bool followFinger = true;

        public Vector2 Direction { get; private set; }
        public bool IsActive { get; private set; }

        private int touchId = -1;
        private Vector2 center;
        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            SetVisible(false);
        }

        private void Update()
        {
            if (Input.touchCount == 0)
            {
                if (IsActive) Reset();
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began && !IsActive && t.position.x < Screen.width * 0.45f)
                {
                    IsActive = true;
                    touchId = t.fingerId;
                    center = t.position;
                    SetVisible(true);
                    MoveBase(t.position);
                    MoveKnob(t.position);
                }
                else if (t.fingerId == touchId)
                {
                    if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                        Drag(t.position);
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                        Reset();
                }
            }
        }

        private void Drag(Vector2 pos)
        {
            Vector2 offset = pos - center;
            float dist = offset.magnitude;
            if (dist > radius)
            {
                offset = offset / dist * radius;
                if (followFinger)
                {
                    center = pos - offset;
                    MoveBase(center);
                }
            }

            MoveKnob(center + offset);

            Vector2 dir = offset / radius;
            float mag = dir.magnitude;
            if (mag < deadzone) Direction = Vector2.zero;
            else Direction = dir.normalized * ((mag - deadzone) / (1f - deadzone));
        }

        private void Reset()
        {
            IsActive = false;
            touchId = -1;
            Direction = Vector2.zero;
            SetVisible(false);
        }

        private void SetVisible(bool on)
        {
            if (baseCircle != null) baseCircle.gameObject.SetActive(on);
            if (knob != null) knob.gameObject.SetActive(on);
        }

        private void MoveBase(Vector2 screenPos)
        {
            if (baseCircle != null) baseCircle.position = screenPos;
        }

        private void MoveKnob(Vector2 screenPos)
        {
            if (knob != null) knob.position = screenPos;
        }
    }
}
