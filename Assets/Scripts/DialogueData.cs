using UnityEngine;

public enum SpeakerSide { Left, Right }

/// <summary>
/// A single line of dialogue.
/// The participant (portrait + side) is assigned directly on each line
/// so you don't need a separate Participants list.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Display name shown in the speaker tab.")]
    public string speakerName;

    [Tooltip("Portrait sprite shown for this speaker. Overrides any previous portrait on this side.")]
    public Sprite portrait;

    [Tooltip("Which side of the screen this speaker appears on.")]
    public SpeakerSide side = SpeakerSide.Right;

    [TextArea(2, 5)]
    public string text;
}

/// <summary>
/// One stage of dialogue for an NPC — shown when the game stage matches.
/// </summary>
[System.Serializable]
public class DialogueStage
{
    [Tooltip("This block plays when GameStageManager.CurrentStage == stage.")]
    public int stage = 0;

    [Tooltip("Lines spoken in this stage.")]
    public DialogueLine[] lines;
}

/// <summary>
/// A participant in a scene dialogue — kept for backward compatibility
/// but no longer required when using per-line portraits.
/// </summary>
[System.Serializable]
public class SceneParticipant
{
    public string     speakerName;
    public Sprite     portrait;
    public SpeakerSide side = SpeakerSide.Right;
}
