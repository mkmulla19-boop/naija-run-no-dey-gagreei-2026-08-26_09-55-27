using UnityEngine;
using UnityEngine.InputSystem;
using NaijaRun.Core;
using NaijaRun.Input;

namespace NaijaRun.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float forwardSpeed = 10f;
        [SerializeField] private float laneTransitionSpeed = 25f;
        [SerializeField] private float jumpHeight = 2.2f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float slideHeight = 0.9f;
        [SerializeField] private float standingCenterY = 0.9f;
        [SerializeField] private float slideCenterY = 0.45f;
        [SerializeField] private float slideDuration = 0.8f;

        private CharacterController characterController;
        private int currentLane = 1;
        private float verticalVelocity;
        private float slideTimer;
        private bool slideActive;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterController.height = standingHeight;
            characterController.center = new Vector3(0f, standingCenterY, 0f);
        }

        private void Update()
        {
            ReadInput();

            if (slideActive)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f)
                    SetStanding();
            }

            float targetX = (currentLane - 1) * 3f;
            float nextX = Mathf.MoveTowards(transform.position.x, targetX, laneTransitionSpeed * Time.deltaTime);
            float horizontalDelta = nextX - transform.position.x;

            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            verticalVelocity -= gravity * Time.deltaTime;

            Vector3 movement = new Vector3(horizontalDelta, verticalVelocity * Time.deltaTime, forwardSpeed * Time.deltaTime);
            characterController.Move(movement);
        }

        private void ReadInput()
        {
            Keyboard keyboard = Keyboard.current;
            bool leftPressed = keyboard != null && (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame);
            bool rightPressed = keyboard != null && (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame);
            bool jumpPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            bool slidePressed = keyboard != null && (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame);

            if (leftPressed || SwipeInput.ConsumeLeft())
                ChangeLane(-1);

            if (rightPressed || SwipeInput.ConsumeRight())
                ChangeLane(1);

            if (jumpPressed || SwipeInput.ConsumeUp())
                Jump();

            if (slidePressed || SwipeInput.ConsumeDown())
                Slide();
        }

        private void ChangeLane(int direction)
        {
            int nextLane = Mathf.Clamp(currentLane + direction, 0, 2);
            if (nextLane != currentLane)
            {
                currentLane = nextLane;
                AudioManager.PlayLaneSwitch();
            }
        }

        private void Jump()
        {
            if (!characterController.isGrounded || slideActive)
                return;

            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * gravity);
            AudioManager.PlayJump();
        }

        private void Slide()
        {
            if (!characterController.isGrounded || slideActive)
                return;

            slideActive = true;
            slideTimer = slideDuration;
            characterController.height = slideHeight;
            characterController.center = new Vector3(0f, slideCenterY, 0f);
            AudioManager.PlaySlide();
        }

        private void SetStanding()
        {
            slideActive = false;
            characterController.height = standingHeight;
            characterController.center = new Vector3(0f, standingCenterY, 0f);
        }
    }
}