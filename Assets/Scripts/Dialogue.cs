using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textComponent;
    public Image           portraitImage;   // Assign the portrait Image in the Inspector

    [Header("Settings")]
    public float textSpeed = 0.05f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private string[]     lines;
    private int          index;
    private bool         isRunning = false;
    private System.Action onFinished;
    public GameObject dialoguePanel;

    private int index;

    void Start()
    {
        if (textComponent != null) textComponent.text = string.Empty;
        gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isRunning) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
                NextLine();
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Start a dialogue sequence. onComplete fires when the last line is dismissed.
    /// Called by NPCDialogueTrigger.
    /// </summary>
    public void StartDialogue(string[] dialogueLines, Sprite portrait = null, System.Action onComplete = null)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;
        
        StopAllCoroutines();

        lines      = dialogueLines;
        onFinished = onComplete;
        index      = 0;

        if (portraitImage != null)
        {
            portraitImage.sprite  = portrait;
            portraitImage.enabled = portrait != null;
        }

        gameObject.SetActive(true);
        textComponent.text = string.Empty;
        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
        isRunning = true;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;
        foreach (char c in lines[index])
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }
        else
        {
            // Last line dismissed — close and fire callback
            isRunning = false;
            gameObject.SetActive(false);
            onFinished?.Invoke();
            onFinished = null;
        }
    }
}
