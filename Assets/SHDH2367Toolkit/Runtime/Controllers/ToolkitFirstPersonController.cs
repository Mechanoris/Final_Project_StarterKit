using UnityEngine;
using UnityEngine.InputSystem;

namespace SHDH2367.Toolkit.Runtime.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ToolkitFirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float sprintSpeed = 8f;
        public float jumpHeight = 1.2f;
        public float gravity = -9.81f;

        [Header("Look Settings")]
        public float lookSensitivity = 0.5f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Camera")]
        public Camera playerCamera;

        CharacterController controller;
        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction sprintAction;

        float verticalVelocity;
        float cameraPitch;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnEnable()
        {
            if (moveAction == null)
                BindPlayerActions();
        }

        void Start()
        {
            BindPlayerActions();
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
                // Explicitly enable the map — SwitchCurrentActionMap alone is not
                // reliable when the asset is the project-wide singleton (all maps
                // are enabled by default and PlayerInput warns about this).
                playerInput.SwitchCurrentActionMap("Player");
                map.Enable();

                moveAction   = map.FindAction("Move");
                lookAction   = map.FindAction("Look");
                jumpAction   = map.FindAction("Jump");
                sprintAction = map.FindAction("Sprint");
            }
            else
            {
                // Fallback: search all maps via the asset-level indexer
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

            HandleMovement();
            HandleLook();
        }

        void HandleMovement()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (controller.isGrounded && jumpAction != null && jumpAction.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity += gravity * Time.deltaTime;

            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            float speed = sprintAction != null && sprintAction.IsPressed() ? sprintSpeed : moveSpeed;
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            controller.Move(move * speed * Time.deltaTime + Vector3.up * verticalVelocity * Time.deltaTime);
        }

        void HandleLook()
        {
            Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
            transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

            cameraPitch -= lookInput.y * lookSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
            if (playerCamera != null)
                playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
        }
    }
}
