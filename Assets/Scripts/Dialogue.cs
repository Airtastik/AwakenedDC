using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Used ChatGPT 5.3
//Prompt: Right now, the dialogue starts right at the beginning of the game and never any time else. However, I want it to be when the player runs into the prefab of a character, it loads a preset dialogue with them. sprite1 will always be our character, but I want sprite2 to change to depend on the prefab the player runs into. How would I do this?

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    public Image sprite1;
    public Image sprite2;

    public Sprite playerSprite;

    public GameObject dialoguePanel;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (lines == null || lines.Length == 0) return; 

        if (index >= lines.Length) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                Debug.Log("Test");
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void StartDialogue(string[] newLines, Sprite npcSprite)
    {
        
        StopAllCoroutines();
        PlayerMovement.Instance?.LockForDialogue();
        lines = newLines;
        index = 0;

        textComponent.text = string.Empty;

        sprite1.sprite = playerSprite;
        sprite2.sprite = npcSprite;

        dialoguePanel.SetActive(true);

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
            OnDialogueFinished();
            dialoguePanel.SetActive(false);
        }
    }

    private void OnDialogueFinished()
    {
        PlayerMovement.Instance?.UnlockFromDialogue();
    }
}