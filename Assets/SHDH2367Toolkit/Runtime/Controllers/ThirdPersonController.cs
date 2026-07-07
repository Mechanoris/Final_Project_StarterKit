using UnityEngine;
using UnityEngine.InputSystem;

namespace SHDH2367.Toolkit.Runtime.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5.5f;
        public float rotationSpeed = 12f;
        public float jumpHeight = 1.2f;
        public float gravity = -9.81f;

        [Header("Camera")]
        public Transform cameraPivot;
        public Camera playerCamera;
        public Vector3 pivotOffset = new Vector3(0f, 1.8f, 0f);
        public Vector3 cameraOffset = new Vector3(0f, 1f, -4f);
        public float lookSensitivity = 0.8f;
        public float minPitch = -35f;
        public float maxPitch = 70f;

        CharacterController controller;
        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction sprintAction;

        float verticalVelocity;
        float yaw;
        float pitch;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
        }

        void OnEnable()
        {
            if (moveAction == null)
                BindPlayerActions();
        }

        void Start()
        {
            BindPlayerActions();

            if (cameraPivot == null)
            {
                GameObject pivot = new GameObject("CameraPivot");
                pivot.transform.SetParent(transform, false);
                cameraPivot = pivot.transform;
            }

            yaw = transform.eulerAngles.y;
            pitch = 12f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void BindPlayerActions()
        {
            if (playerInput == null || playerInput.actions == null)
                return;

            if (string.IsNullOrEmpty(playerInput.defaultActionMap))
                playerInput.defaultActionMap = "Player";

            var map = playerInput.actions.FindActionMap("Player");
            if (map != null)
            {
                playerInput.SwitchCurrentActionMap("Player");
                map.Enable();

                moveAction   = map.FindAction("Move");
                lookAction   = map.FindAction("Look");
                jumpAction   = map.FindAction("Jump");
                sprintAction = map.FindAction("Sprint");
            }
            else
            {
                var actions = playerInput.actions;
                moveAction   = actions.FindAction("Move");
                lookAction   = actions.FindAction("Look");
                jumpAction   = actions.FindAction("Jump");
                sprintAction = actions.FindAction("Sprint");

                moveAction?.Enable();
                lookAction?.Enable();
                jumpAction?.Enable();
                sprintAction?.Enable();
            }
        }

        void Update()
        {
            if (RuntimeGameStateBridge.IsGameOver())
                return;

            HandleLook();
            HandleMovement();
        }

        void LateUpdate()
        {
            UpdateCameraRig();
        }

        void HandleLook()
        {
            Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
            yaw += lookInput.x * lookSensitivity;
            pitch -= lookInput.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        void HandleMovement()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (controller.isGrounded && jumpAction != null && jumpAction.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity += gravity * Time.deltaTime;

            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            float speed = sprintAction != null && sprintAction.IsPressed() ? moveSpeed * 1.35f : moveSpeed;

            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 desired = yawRotation * new Vector3(moveInput.x, 0f, moveInput.y);
            desired = Vector3.ClampMagnitude(desired, 1f);

            Vector3 velocity = desired * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            Vector3 flat = new Vector3(desired.x, 0f, desired.z);
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(flat, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
            }
        }

        void UpdateCameraRig()
        {
            if (cameraPivot == null || playerCamera == null)
                return;

            cameraPivot.position = transform.position + pivotOffset;
            cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

            // When camera is parented to the pivot, its world position is managed by Unity's
            // transform hierarchy — setting localPosition in Start is enough. Only set position
            // explicitly for unparented cameras (manual/legacy setups).
            if (playerCamera.transform.parent != cameraPivot)
                playerCamera.transform.position = cameraPivot.TransformPoint(cameraOffset);

            playerCamera.transform.LookAt(cameraPivot.position);
        }
    }
}
