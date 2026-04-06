using UnityEngine;
using TMPro;

/// <summary>
/// Base class for all interactable objects in the escape room.
/// Attach to any GameObject with a Collider (set to IsTrigger).
/// Requires a child or referenced UI element for the prompt.
///
/// SETUP:
/// 1. Add a Collider to this GameObject and check "Is Trigger".
///    Size it to the radius you want the player to be able to interact from.
/// 2. Assign the shared "Press E" prompt TextMeshProUGUI in the inspector
///    (a screen-space UI text works well — create one once and reference it
///    from every interactable via a singleton or direct drag).
/// 3. The player GameObject must have the tag "Player" and a Collider/Rigidbody.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The screen-space prompt text shared by all interactables.")]
    public TextMeshProUGUI interactPromptText;

    [Tooltip("Custom prompt message. Leave empty for default.")]
    public string promptMessage = "Press E to inspect";

    private bool playerInRange = false;

    protected virtual void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptText != null)
            {
                interactPromptText.text = promptMessage;
                interactPromptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptText != null)
            {
                interactPromptText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Called when the player presses E while in range.
    /// Override in subclasses to define behavior.
    /// </summary>
    protected abstract void OnInteract();

    /// <summary>
    /// Hides the prompt. Call from subclasses when interaction
    /// should dismiss the prompt (e.g. after pickup).
    /// </summary>
    protected void HidePrompt()
    {
        playerInRange = false;
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);
    }
}
