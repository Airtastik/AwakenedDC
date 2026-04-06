using UnityEngine;

/// <summary>
/// Attach to any NPC prefab.
/// Supports staged dialogue — different lines depending on GameStageManager.CurrentStage.
/// 
/// In the Inspector, add entries to Dialogue Stages:
///   Stage 0 — lines shown at the start of the game
///   Stage 1 — lines shown after the first floor / event
///   etc.
/// The system picks the highest stage block whose number <= CurrentStage.
/// </summary>
public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("NPC Info")]
    public string  npcName    = "???";
    public Sprite  npcPortrait;

    [Header("Staged Dialogue — one block per game stage")]
    public DialogueStage[] dialogueStages;

    [Header("Options")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        if (Dialogue.Instance == null)
        {
            Debug.LogWarning("[NPCDialogueTrigger] No Dialogue instance in scene.");
            return;
        }

        hasTriggered = true;
        Dialogue.Instance.StartStagedDialogue(dialogueStages, npcPortrait, npcName);
    }
}
