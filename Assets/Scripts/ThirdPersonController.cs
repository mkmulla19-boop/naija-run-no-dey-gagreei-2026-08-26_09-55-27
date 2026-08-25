using UnityEngine;

namespace Olomu.Systems
{
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4.0f;
        public float runSpeed = 6.5f;
        public float jumpVelocity = 4.2f;
        public float gravity = 9.81f;
        public float acceleration = 12.0f;
        public float friction = 14.0f;
        public float runThreshold = 0.85f;

        [Header("Camera / Look")]
        public float touchSensitivity = 0.0055f;
        public float cameraSmoothness = 6.0f;
        public float minPitch = -55f;
        public float maxPitch = 25f;
        public float cameraDistance = 7.5f;
        public float minCameraDistance = 2.2f;
        public float maxCameraDistance = 12.0f;
        public float zoomSpeed = 2.5f;
        public float headHeight = 1.45f;
        public float shoulderOffsetX = 0.45f;

        [Header("Refs")]
        public VirtualJoystick joystick;

        private CharacterController cc;
        private Transform camPivot;
        private Camera cam;
        private Animator animator;

        private Vector3 horizontalVel;
        private float verticalVel;
        private float targetYaw;
        private float targetPitch;
        private float currentPitch;
        private int lookTouchId = -1;
        private bool jumpQueued;
        private float currentCamDist;
        private float defaultFov = 55f;

        public bool IsRunning { get; private set; }
        public bool ControlsEnabled = true;
        public bool CinematicControl;

        private Vector2 cineInput;

        public void SetCinematicInput(Vector2 dir) => cineInput = dir;

        public void SetYaw(float yaw)
        {
            targetYaw = yaw;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public float CurrentYaw => transform.eulerAngles.y;

        private static readonly int AnimState = Animator.StringToHash("AnimState");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.skinWidth = Mathf.Max(cc.skinWidth, cc.radius * 0.1f);
                cc.minMoveDistance = 0f;
            }
            animator = GetComponentInChildren<Animator>();
            if (joystick == null) joystick = FindFirstObjectByType<VirtualJoystick>();

            camPivot = transform.Find("CamPivot");
            if (camPivot == null)
            {
                camPivot = new GameObject("CamPivot").transform;
                camPivot.SetParent(transform);
                camPivot.localPosition = new Vector3(0f, headHeight, 0f);
                camPivot.localRotation = Quaternion.identity;
            }
            else
            {
                camPivot.localPosition = new Vector3(0f, headHeight, 0f);
                camPivot.localRotation = Quaternion.identity;
            }

            cam = GetComponentInChildren<Camera>(true);
            GameObject camGo;
            if (cam == null)
            {
                camGo = new GameObject("MainCamera");
                camGo.transform.SetParent(camPivot);
                cam = camGo.AddComponent<Camera>();
            }
            else
            {
                camGo = cam.gameObject;
            }
            camGo.tag = "MainCamera";
            if (camGo.GetComponent<AudioListener>() == null)
                camGo.AddComponent<AudioListener>();
            currentCamDist = cameraDistance;
            camGo.transform.localPosition = new Vector3(shoulderOffsetX, 0f, -currentCamDist);
            camGo.transform.localRotation = Quaternion.identity;
            cam.farClipPlane = 500f;
            cam.fieldOfView = defaultFov;

            targetYaw = transform.eulerAngles.y;
            targetPitch = 15f;
            currentPitch = targetPitch;
            ApplyLookRotation();

            // Quality / Target Frame Rate setting
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            PlaceOnGround();
        }

        public void PlaceOnGround()
        {
            if (cc == null) cc = GetComponent<CharacterController>();
            Vector3 origin = transform.position + Vector3.up * 2f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 15f))
            {
                float targetY = hit.point.y + 0.05f;
                Teleport(new Vector3(transform.position.x, targetY, transform.position.z));
                verticalVel = -1.5f;
                Debug.Log($"[OLU] grounded at {transform.position} on {hit.collider.name}");
            }
            else
            {
                Debug.LogWarning($"[OLU] no ground below {transform.position}");
            }
            cc.Move(Vector3.zero);
        }

        private void Update()
        {
            HandleLookTouches();
            HandleZoom();
            SmoothLook();
            UpdateCameraOcclusion();
            Move();
            UpdateAnimation();

            if (Time.frameCount % 180 == 0)
                Debug.Log($"[OLU] pos={transform.position} grounded={cc.isGrounded} " +
                          $"speed={horizontalVel.magnitude:F1} controls={ControlsEnabled} cine={CinematicControl}");
        }

        private void HandleZoom()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (!ControlsEnabled || CinematicControl) return;
            cameraDistance = Mathf.Clamp(cameraDistance - Input.mouseScrollDelta.y * zoomSpeed * 0.2f,
                minCameraDistance, maxCameraDistance);
#endif
            if (Input.touchCount == 2 && ControlsEnabled && !CinematicControl)
            {
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                Vector2 previousFirst = first.position - first.deltaPosition;
                Vector2 previousSecond = second.position - second.deltaPosition;
                float previousGap = Vector2.Distance(previousFirst, previousSecond);
                float currentGap = Vector2.Distance(first.position, second.position);
                cameraDistance = Mathf.Clamp(cameraDistance - (currentGap - previousGap) * 0.01f,
                    minCameraDistance, maxCameraDistance);
            }
        }

        private void UpdateCameraOcclusion()
        {
            if (cam == null || camPivot == null) return;

            // Camera Obstacle Collision Avoidance
            Vector3 desiredLocalPos = new Vector3(shoulderOffsetX, 0f, -cameraDistance);
            Vector3 pivotWorld = camPivot.position;
            Vector3 desiredWorld = camPivot.TransformPoint(desiredLocalPos);
            Vector3 dir = (desiredWorld - pivotWorld).normalized;
            float maxDist = cameraDistance;

            float targetDist = maxDist;
            if (Physics.SphereCast(pivotWorld, 0.2f, dir, out RaycastHit hit, maxDist, ~LayerMask.GetMask("Ignore Raycast")))
            {
                if (hit.collider != null && !hit.collider.CompareTag("Player") && !hit.collider.isTrigger)
                {
                    targetDist = Mathf.Clamp(hit.distance - 0.1f, 1.2f, maxDist);
                }
            }

            currentCamDist = Mathf.Lerp(currentCamDist, targetDist, 12f * Time.deltaTime);
            cam.transform.localPosition = new Vector3(shoulderOffsetX, 0f, -currentCamDist);

            // Dynamic Sprint FOV Zoom
            float targetFov = IsRunning ? (defaultFov + 6f) : defaultFov;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 5f * Time.deltaTime);
        }

        private void HandleLookTouches()
        {
            if (!ControlsEnabled || CinematicControl) return;

            // 1. Mobile Touch Look
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                bool overUi = t.position.x < Screen.width * 0.45f;

                if (t.phase == TouchPhase.Began && !overUi)
                {
                    lookTouchId = t.fingerId;
                }
                else if (t.fingerId == lookTouchId)
                {
                    if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                    {
                        targetYaw += t.deltaPosition.x * touchSensitivity * Mathf.Rad2Deg;
                        targetPitch -= t.deltaPosition.y * touchSensitivity * Mathf.Rad2Deg;
                        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        lookTouchId = -1;
                    }
                }
            }

            // 2. PC Desktop Mouse Look (Right Click OR Left Click on Right Half)
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            bool mouseLookActive = Input.GetMouseButton(1) || (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width * 0.45f && UnityEngine.EventSystems.EventSystem.current != null && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());
            if (mouseLookActive)
            {
                targetYaw += Input.GetAxis("Mouse X") * 3.2f;
                targetPitch -= Input.GetAxis("Mouse Y") * 2.8f;
                targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            }
#endif
        }

        private void SmoothLook()
        {
            float y = Mathf.LerpAngle(transform.eulerAngles.y, targetYaw, cameraSmoothness * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, y, 0f);

            currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, cameraSmoothness * Time.deltaTime);
            ApplyLookRotation();
        }

        private void ApplyLookRotation()
        {
            if (camPivot != null)
                camPivot.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }

        private void Move()
        {
            if (transform.position.y < -5f)
            {
                Debug.LogWarning("[OLU] fell out of world, rescuing");
                Teleport(new Vector3(0f, 1.05f, 8f));
                PlaceOnGround();
                return;
            }

            bool grounded = cc.isGrounded;
            if (grounded && verticalVel < 0f) verticalVel = -1.5f;

            if (jumpQueued && grounded && ControlsEnabled && !CinematicControl)
            {
                verticalVel = jumpVelocity;
            }
            jumpQueued = false;

            verticalVel -= gravity * Time.deltaTime;

            Vector2 input;
            if (CinematicControl) input = cineInput;
            else input = ControlsEnabled && joystick != null ? joystick.Direction : Vector2.zero;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (input == Vector2.zero && ControlsEnabled)
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");
                if (input.sqrMagnitude > 1f) input.Normalize();
            }
#endif

            IsRunning = input.magnitude > runThreshold;
            float targetSpeed = IsRunning ? runSpeed : walkSpeed;

            Vector3 moveDir = transform.rotation * new Vector3(input.x, 0f, input.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            Vector3 targetVel = moveDir * targetSpeed;
            float rate = (moveDir.sqrMagnitude > 0.001f ? acceleration : friction) * Time.deltaTime;
            horizontalVel.x = Mathf.Lerp(horizontalVel.x, targetVel.x, rate);
            horizontalVel.z = Mathf.Lerp(horizontalVel.z, targetVel.z, rate);

            Vector3 final = horizontalVel + Vector3.up * verticalVel;
            cc.Move(final * Time.deltaTime);
        }

        private void UpdateAnimation()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            int state;
            if (!cc.isGrounded) state = 3;
            else if (horizontalVel.sqrMagnitude > 0.2f) state = IsRunning ? 2 : 1;
            else state = 0;

            animator.SetInteger(AnimState, state);
        }

        public void RequestJump() => jumpQueued = true;

        public void PlayAttack()
        {
            if (animator != null) animator.SetTrigger(AttackTrigger);
        }

        public void Teleport(Vector3 pos)
        {
            cc.enabled = false;
            transform.position = pos;
            cc.enabled = true;
            cc.Move(Vector3.zero);
        }
    }
}
