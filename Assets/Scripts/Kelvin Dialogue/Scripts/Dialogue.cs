using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    public float clickDialogueRate = 100f;

    [Header("References")]
    [SerializeField] private DialogueInputHandler dialogueInputHandler;

    private int index;
    private float clickDialogueInterval;


    // Start is called before the first frame update
    void Start()
    {
        clickDialogueInterval = clickDialogueRate;
        textComponent.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (clickDialogueInterval >= clickDialogueRate && dialogueInputHandler.DialogueNextLineTriggered)
        {
            clickDialogueInterval = 0;
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }

        HandleClickRate();
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void HandleClickRate()
    {
        if (clickDialogueInterval <= clickDialogueRate)
        {
            clickDialogueInterval += 1f;
        }
    }
}