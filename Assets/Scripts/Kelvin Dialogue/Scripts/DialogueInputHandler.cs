using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInputHandler : MonoBehaviour
{

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Dialogue";

    [Header("Action Name Reference")]
    [SerializeField] private string dialogueNextLine = "DialogueNextLine";

    private InputAction dialogueNextLineAction;

    public bool DialogueNextLineTriggered {get; private set;}

    private void Awake()
    {
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        dialogueNextLineAction = mapReference.FindAction(dialogueNextLine);

        SubscribeActionValuesToInputEvents();
    }

    private void SubscribeActionValuesToInputEvents()
    {
        dialogueNextLineAction.performed += inputInfo => DialogueNextLineTriggered = true;
        dialogueNextLineAction.canceled += inputInfo => DialogueNextLineTriggered = false;
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }
}
