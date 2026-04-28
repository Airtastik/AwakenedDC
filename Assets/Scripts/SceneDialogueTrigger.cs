using UnityEngine;
using System; // Required for Actions

/// <summary>
/// Triggers a scripted cutscene dialogue when the player gets close enough, 
/// or programmatically via PlaySequence().
/// </summary>
public class SceneDialogueTrigger : MonoBehaviour
{
    [Header("Scene Lines — assign portrait and side per line")]
    public DialogueLine[] sceneLines;

    [Header("Trigger")]
    [Tooltip("How close the player must be (world units) to trigger the cutscene.")]
    public float triggerRadius = 4f;

    // ADDED: Scripted mode for programmatic triggering (like WaveSpawner)
    public enum TriggerMode { Auto, PressE, Scripted } 
    [Tooltip("Auto: plays as soon as player is in range.\nPressE: player must press E while in range.\nScripted: Only triggers via code.")]
    public TriggerMode triggerMode = TriggerMode.Auto;

    [Header("Options")]
    public bool triggerOnce          = true;
    public bool advanceStageOnFinish = false;

    [Header("Teleport After Dialogue (optional)")]
    [Tooltip("If set, teleports the player here when dialogue ends.")]
    public Transform teleportDestination;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private bool      hasTriggered   = false;
    private Transform playerTransform;
    private Action    onDialogueComplete; // Stores the callback

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("[SceneDialogueTrigger] No GameObject tagged 'Player' found.");
    }

    void Update()
    {
        if (triggerOnce && hasTriggered) return;
        if (triggerMode == TriggerMode.Scripted) return; // Skip proximity checks for Scripted events
        if (playerTransform == null) return;
        if (Dialogue.Instance == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > triggerRadius) return;

        bool shouldFire = triggerMode == TriggerMode.Auto
                       || (triggerMode == TriggerMode.PressE && Input.GetKeyDown(KeyCode.E));

        if (!shouldFire) return;

        PlaySequence();
    }

    /// <summary>
    /// Programmatically starts the sequence. 
    /// Pass an optional callback to execute logic when dialogue finishes.
    /// </summary>
    public void PlaySequence(Action onComplete = null)
    {
        if (triggerOnce && hasTriggered) 
        {
            onComplete?.Invoke();
            return;
        }

        hasTriggered = true;
        onDialogueComplete = onComplete;
        Dialogue.Instance.StartSceneDialogue(sceneLines, null, OnFinished);
    }

    private void OnFinished()
    {
        if (advanceStageOnFinish)
            GameStageManager.NextStage();

        if (teleportDestination != null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = teleportDestination.position;
                player.transform.rotation = teleportDestination.rotation;
                if (cc != null) cc.enabled = true;
            }
        }

        // Fire the callback to let the WaveSpawner know we're done
        onDialogueComplete?.Invoke(); 
    }

    // Draw the trigger radius in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}