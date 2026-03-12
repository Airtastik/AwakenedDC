using UnityEngine;

/// <summary>
/// Attach to any NPC prefab. When the player walks into the trigger collider,
/// dialogue starts and player movement is locked until it finishes.
/// </summary>
public class NPCDialogueTrigger : MonoBehaviour
{
    public string[] npcLines;
    public Sprite   npcPortrait;

    private Dialogue dialogue;

    void Start()
    {
        dialogue = FindFirstObjectByType<Dialogue>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (dialogue == null)
        {
            Debug.LogWarning("[NPCDialogueTrigger] No Dialogue component found in scene.");
            return;
        }

        // Lock player movement for the duration of dialogue
        PlayerMovement.Instance?.LockForDialogue();

        dialogue.StartDialogue(npcLines, npcPortrait, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        PlayerMovement.Instance?.UnlockFromDialogue();
    }
}
