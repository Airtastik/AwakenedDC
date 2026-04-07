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

    [Header("Teleport After Dialogue (optional)")]
    [Tooltip("If set, teleports the player here when dialogue ends.")]
    public Transform teleportDestination;

    private bool hasTriggered = false;
    private bool playerInRange = false;

    void Start()
    {
        // Warn if this object lacks a trigger collider
        var col3 = GetComponent<Collider>();
        var col2 = GetComponent<Collider2D>();
        if (col3 == null && col2 == null)
            Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] No Collider on NPC; triggers won't fire.");

        if (col3 != null && !col3.isTrigger)
            Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] Collider is not set to Is Trigger.");
        if (col2 != null && !col2.isTrigger)
            Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] Collider2D is not set to Is Trigger.");

        // Check for a player object with expected components
        var player = GameObject.FindWithTag("Player");
        if (player == null)
            Debug.LogWarning("[NPCDialogueTrigger] No GameObject with tag 'Player' found in scene.");
        else
        {
            if (player.GetComponent<Collider>() == null && player.GetComponent<Collider2D>() == null)
                Debug.LogWarning("[NPCDialogueTrigger] Player has no Collider; triggers won't detect.");
            if (player.GetComponent<Rigidbody>() == null && player.GetComponent<Rigidbody2D>() == null)
                Debug.LogWarning("[NPCDialogueTrigger] Player has no Rigidbody/Rigidbody2D; triggers may not work.");
        }

        if (Dialogue.Instance == null)
            Debug.LogWarning("[NPCDialogueTrigger] Dialogue.Instance is null — ensure Dialogue UI object exists in scene.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] OnTriggerEnter by '{other.gameObject.name}' tag='{other.gameObject.tag}' layer='{LayerMask.LayerToName(other.gameObject.layer)}' hasRigidbody={(other.attachedRigidbody != null)} otherIsTrigger={other.isTrigger}");
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.LogWarning("Player entered NPC trigger for " + npcName);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] OnTriggerExit by '{other.gameObject.name}' tag='{other.gameObject.tag}'");
        if (other.CompareTag("Player")) playerInRange = false;

    }

    // 2D physics support (in case project uses 2D colliders)
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] OnTriggerEnter2D by '{other.gameObject.name}' tag='{other.gameObject.tag}' layer='{LayerMask.LayerToName(other.gameObject.layer)}' hasRigidbody2D={(other.attachedRigidbody != null)} otherIsTrigger={other.isTrigger}");
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.LogWarning("Player entered NPC trigger (2D) for " + npcName);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] OnTriggerExit2D by '{other.gameObject.name}' tag='{other.gameObject.tag}'");
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void Update()
    {
        if (!playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
                Debug.LogWarning($"[NPCDialogueTrigger:{npcName}] E pressed but player not in range.");
            return;
        }
        if (triggerOnce && hasTriggered) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (Dialogue.Instance == null)
        {
            Debug.LogWarning("[NPCDialogueTrigger] No Dialogue instance in scene.");
            return;
        }

        hasTriggered = true;
        Dialogue.Instance.StartStagedDialogue(
            dialogueStages, npcPortrait, npcName,
            finished: teleportDestination != null ? (System.Action)TeleportPlayer : null
        );
    }

    private void TeleportPlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogWarning("[NPCDialogueTrigger] No Player found to teleport."); return; }

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = teleportDestination.position;
        player.transform.rotation = teleportDestination.rotation;
        if (cc != null) cc.enabled = true;

        Debug.Log($"[NPCDialogueTrigger] Player teleported to {teleportDestination.name}.");
    }
}
