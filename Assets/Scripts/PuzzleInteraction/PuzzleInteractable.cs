using UnityEngine;

/// <summary>
/// Attach to a world object that opens a puzzle panel when the player
/// presses E (e.g. a locked door, a safe, a strange device).
///
/// SETUP:
/// 1. Attach this script to the puzzle GameObject in the scene.
/// 2. Drag the corresponding PuzzlePanel (a UI panel in your Canvas) into
///    the "puzzlePanel" field.
/// 3. Assign the shared interact-prompt TextMeshProUGUI (inherited).
/// 4. Make sure the GameObject has a Collider with "Is Trigger" checked.
/// </summary>
public class PuzzleInteractable : Interactable
{
    [Header("Puzzle Settings")]
    [Tooltip("The PuzzlePanel UI to open when the player interacts.")]
    public PuzzlePanel puzzlePanel;

    protected override void OnInteract()
    {
        if (puzzlePanel == null)
        {
            Debug.LogWarning($"PuzzleInteractable on {gameObject.name}: No PuzzlePanel assigned.");
            return;
        }

        HidePrompt();
        puzzlePanel.Open();
    }
}
