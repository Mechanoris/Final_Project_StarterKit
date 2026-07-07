using GLTFast.Schema;
using System;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{

    [Header("Visual")]
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 1.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float lookSensitivity = 0.5f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Camera Reference")]
    public GameObject playerCamera;

    [Header("Mesh References")]
    public GameObject bodyMesh;
    //public GameObject bodyMesh;

    CharacterController controller;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;

    public float verticalVelocity;
    float cameraPitch;
    float angularVelocity;
    float friction = 1.1f;
    public Vector3 spawnPosition;
    public Airplane airplane;
    Vector3 airplaneSpeed;
    public handTrigger handTrigger;
    Vector3 handSpeed;

    Vector3 updatePos;

    bool OnPlane;
    bool OffPlane;

    public bool jumpscaring = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        spawnPosition = transform.position;
        var map = playerInput.actions.FindActionMap("Player");
        if (map != null)
        {
            if (string.IsNullOrEmpty(playerInput.defaultActionMap))
                playerInput.defaultActionMap = "Player";
            playerInput.SwitchCurrentActionMap("Player");

            moveAction   = map.FindAction("Move");
            lookAction   = map.FindAction("Look");
            jumpAction   = map.FindAction("Jump");
            sprintAction = map.FindAction("Sprint");
        }
        else
        {
            var actions = playerInput.actions;
            moveAction   = actions["Move"];
            lookAction   = actions["Look"];
            jumpAction   = actions["Jump"];
            sprintAction = actions["Sprint"];
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        HandleMovement();
        HandleLook();
        updatePos = bodyMesh.transform.localPosition;
        if (FallTooMuch())
        {
            ReturntoSpawn();
        }
    }

    void HandleMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded && jumpAction != null && jumpAction.WasPressedThisFrame())
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        float speed = (sprintAction != null && sprintAction.IsPressed()) ? sprintSpeed : moveSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y; 

        ////edited: lookInput to moveInput, moved up 7 lines
        //transform.Rotate(Vector3.up * moveInput.x * lookSensitivity);

        if ((sprintAction != null && sprintAction.IsPressed()))
        {
            sprintSpeed = 9f;
        }
        else
        {
            sprintSpeed = 0;
        }

        bodyMesh.transform.localRotation = Quaternion.Euler(moveInput.y * sprintSpeed * 6f, 0f, -moveInput.x * speed * 2f); //Forward tilt when sprint, side tilt when strafing.
        //bodyMesh.transform.localRotation = Quaternion.Euler(0f, 0f, moveInput.y * lookSensitivity * 30f);

        if ((sprintAction != null && sprintAction.IsPressed()) || (moveAction != null && moveAction.IsPressed())) //Walk / run bobbing effect.
        {
            Vector3 pos = updatePos;
            pos.y = Mathf.Sin(Time.time * speed * 2) * speed / 200;
            bodyMesh.transform.localPosition = pos;
        }
        else
        {
            Bob();
        }

        if (airplane.Triggered)
        {
            airplaneSpeed = airplane.currentVelocity;
        }
        else {
            airplaneSpeed = new Vector3(0,0,0);
        }

        if (handTrigger.Triggered)
        {
            handSpeed = handTrigger.currentVelocity;
        }
        else
        {
            handSpeed = new Vector3(0, 0, 0);
        }


        controller.Move(move * speed * Time.deltaTime + Vector3.up * verticalVelocity * Time.deltaTime + airplaneSpeed * Time.deltaTime + handSpeed * Time.deltaTime);
    }

    void HandleLook()
    {
        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        float speed = (sprintAction != null && sprintAction.IsPressed()) ? sprintSpeed : moveSpeed;

        //{
        //transform.Rotate(Vector3.up * lookInput.x * lookSensitivity); //edited: lookInput to moveInput, moved up 7 lines
        //}
        if ((sprintAction != null && sprintAction.IsPressed()) || (moveAction != null && moveAction.IsPressed()))
        {
            if (moveInput.y > 0f)
            {
                transform.right = Vector3.Lerp(transform.right, playerCamera.transform.right,speed * Time.deltaTime);
            }
        }

        cameraPitch -= lookInput.y * lookSensitivity; //edited: lookInput to moveInput, moved up 7 lines

        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (playerCamera != null)
            playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
    }

    void Bob()
    {
        Vector3 pos = updatePos;
        pos.y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        bodyMesh.transform.localPosition = pos;
    }

    public bool FallTooMuch()
    {
        if (jumpscaring) return false;
        if (verticalVelocity < -70f)
            while (transform.position != spawnPosition)
            {
                return true;
            }
        return false;
    }



    void ReturntoSpawn()
    {
        //gravity = 0f;
        //if (transform.position == spawnPosition)
        //{
        //    gravity = -9.81f;
        //}
        transform.position = Vector3.Lerp(transform.position, spawnPosition, Time.deltaTime * 1000f);
    }
}
