using UnityEngine;

/// <summary>
/// Triggers a scripted cutscene dialogue when the player gets close enough.
/// No separate Participants list needed — assign the portrait and side on each line.
///
/// Setup:
///   1. Add this script to an empty GameObject at the cutscene location.
///   2. Fill in Scene Lines in order. For each line:
///        - Speaker Name  : displayed in the name tab
///        - Portrait      : sprite shown on that side for this line
///        - Side          : Left or Right
///        - Text          : what they say
///   3. Set Trigger Radius to control how close the player must be.
///   4. Choose trigger mode: Auto (plays immediately) or Press E.
/// </summary>
public class SceneDialogueTrigger : MonoBehaviour
{
    [Header("Scene Lines — assign portrait and side per line")]
    public DialogueLine[] sceneLines;

    [Header("Trigger")]
    [Tooltip("How close the player must be (world units) to trigger the cutscene.")]
    public float triggerRadius = 4f;

    public enum TriggerMode { Auto, PressE }
    [Tooltip("Auto: plays as soon as player is in range.\nPressE: player must press E while in range.")]
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
        if (playerTransform == null) return;
        if (Dialogue.Instance == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > triggerRadius) return;

        bool shouldFire = triggerMode == TriggerMode.Auto
                       || (triggerMode == TriggerMode.PressE && Input.GetKeyDown(KeyCode.E));

        if (!shouldFire) return;

        hasTriggered = true;
        Dialogue.Instance.StartSceneDialogue(sceneLines, null, OnFinished);
    }

    private void OnFinished()
    {
        if (advanceStageOnFinish)
            GameStageManager.NextStage();

        if (teleportDestination != null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = teleportDestination.position;
            player.transform.rotation = teleportDestination.rotation;
            if (cc != null) cc.enabled = true;
        }
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
