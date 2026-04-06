using UnityEngine;

/// <summary>
/// Triggers a scripted multi-speaker scene dialogue when the player enters the collider.
/// 
/// Setup in Inspector:
///   1. Add each speaker to Participants (name, portrait, side).
///   2. Add each line to Scene Lines (pick the speaker by name, set side, write text).
///      The speakerName in each line must exactly match a Participant's speakerName.
///   3. Optionally set advanceStageOnFinish to automatically call GameStageManager.NextStage().
/// 
/// Example — a two-person conversation:
///   Participants: [ { "Dimitri", dimitriSprite, Left }, { "Silvia", silviaSprite, Right } ]
///   Lines:
///     { speakerName="Silvia",  side=Right, text="Are you okay?" }
///     { speakerName="Dimitri", side=Left,  text="..." }
///     { speakerName="Silvia",  side=Right, text="That's not an answer." }
/// </summary>
public class SceneDialogueTrigger : MonoBehaviour
{
    [Header("Participants — one entry per speaker in this scene")]
    public SceneParticipant[] participants;

    [Header("Scene Lines — in order")]
    public DialogueLine[] sceneLines;

    [Header("Options")]
    public bool triggerOnce          = true;
    public bool advanceStageOnFinish = false;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        if (Dialogue.Instance == null)
        {
            Debug.LogWarning("[SceneDialogueTrigger] No Dialogue instance in scene.");
            return;
        }

        hasTriggered = true;

        Dialogue.Instance.StartSceneDialogue(
            sceneLines,
            participants,
            advanceStageOnFinish ? (System.Action)OnFinished : null
        );
    }

    private void OnFinished()
    {
        GameStageManager.NextStage();
    }
}
