using UnityEngine;

/// <summary>
/// A single line of dialogue — who says it, what they say.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Display name shown in the speaker tab.")]
    public string speakerName;

    [Tooltip("'left' or 'right' — which side of the screen this speaker is on.")]
    public SpeakerSide side = SpeakerSide.Right;

    [TextArea(2, 5)]
    public string text;
}

public enum SpeakerSide { Left, Right }

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
/// A participant in a scene dialogue — a speaker with a portrait and a side.
/// </summary>
[System.Serializable]
public class SceneParticipant
{
    public string   speakerName;
    public Sprite   portrait;
    public SpeakerSide side = SpeakerSide.Right;
}
