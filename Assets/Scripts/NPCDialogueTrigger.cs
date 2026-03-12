using UnityEngine;

//Used ChatGPT 5.3
//Prompt: Right now, the dialogue starts right at the beginning of the game and never any time else. However, I want it to be when the player runs into the prefab of a character, it loads a preset dialogue with them. sprite1 will always be our character, but I want sprite2 to change to depend on the prefab the player runs into. How would I do this?

public class NPCDialogueTrigger : MonoBehaviour
{
    public string[] npcLines;
    public Sprite npcPortrait;

    private Dialogue dialogue;

    void Start()
    {
        dialogue = FindObjectOfType<Dialogue>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogue.StartDialogue(npcLines, npcPortrait);
        }
    }
}