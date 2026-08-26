using UnityEngine;
using UnityEngine.InputSystem;

namespace NaijaRun.Input
{
    public static class SwipeInput
    {
        private const float MinimumSwipeDistance = 60f;
        private static Vector2 startPosition;
        private static bool tracking;
        private static int direction;

        public static bool ConsumeLeft() => ConsumeDirection(-1);
        public static bool ConsumeRight() => ConsumeDirection(1);
        public static bool ConsumeUp() => ConsumeDirection(2);
        public static bool ConsumeDown() => ConsumeDirection(-2);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var inputObject = new GameObject("SwipeInput");
            inputObject.AddComponent<SwipeInputRunner>();
            Object.DontDestroyOnLoad(inputObject);
        }

        private static bool ConsumeDirection(int requestedDirection)
        {
            if (direction != requestedDirection)
                return false;

            direction = 0;
            return true;
        }

        private sealed class SwipeInputRunner : MonoBehaviour
        {
            private void Update()
            {
                Touchscreen touchscreen = Touchscreen.current;
                if (touchscreen == null)
                    return;

                var touch = touchscreen.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    startPosition = touch.position.ReadValue();
                    tracking = true;
                }
                else if (tracking && touch.press.wasReleasedThisFrame)
                {
                    Vector2 delta = touch.position.ReadValue() - startPosition;
                    tracking = false;
                    if (delta.magnitude < MinimumSwipeDistance)
                        return;

                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        direction = delta.x < 0f ? -1 : 1;
                    else
                        direction = delta.y < 0f ? -2 : 2;
                }
            }
        }
    }
}