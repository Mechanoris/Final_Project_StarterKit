using UnityEngine;
using UnityEngine.InputSystem;

namespace SHDH2367.Toolkit.Runtime.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class SideViewController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpHeight = 1.3f;
        public float gravity = -16f;

        [Header("Side Axis")]
        public Vector3 moveAxis = Vector3.right;
        public bool faceMoveDirection = true;

        [Header("Camera")]
        public Camera playerCamera;
        public Vector3 cameraOffset = new Vector3(0f, 3f, -10f);
        public Vector3 cameraEuler = new Vector3(10f, 0f, 0f);

        CharacterController controller;
        PlayerInput playerInput;
        InputAction moveAction;
        InputAction jumpAction;
        float verticalVelocity;

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

                moveAction = map.FindAction("Move");
                jumpAction = map.FindAction("Jump");
            }
            else
            {
                var actions = playerInput.actions;
                moveAction = actions.FindAction("Move");
                jumpAction = actions.FindAction("Jump");

                moveAction?.Enable();
                jumpAction?.Enable();
            }
        }

        void LateUpdate()
        {
            if (playerCamera == null)
                return;

            playerCamera.transform.position = transform.position + cameraOffset;
            playerCamera.transform.rotation = Quaternion.Euler(cameraEuler);
        }

        void Update()
        {
            if (RuntimeGameStateBridge.IsGameOver())
                return;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (controller.isGrounded && jumpAction != null && jumpAction.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity += gravity * Time.deltaTime;

            float axisInput = 0f;
            if (moveAction != null)
            {
                Vector2 move = moveAction.ReadValue<Vector2>();
                axisInput = Mathf.Abs(move.x) > Mathf.Abs(move.y) ? move.x : move.y;
            }

            Vector3 lateral = moveAxis.normalized * axisInput * moveSpeed;
            Vector3 velocity = lateral + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            if (faceMoveDirection && Mathf.Abs(axisInput) > 0.01f)
            {
                float sign = Mathf.Sign(axisInput);
                transform.forward = moveAxis.normalized * sign;
            }
        }
    }
}
