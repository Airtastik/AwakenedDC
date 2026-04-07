using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit dialogue system supporting:
///   A) Staged NPC dialogue  — different lines per game stage
///   B) Scene dialogue       — scripted multi-speaker sequences
/// 
/// Setup:
///   1. Create empty GameObject "DialogueUI" in the scene.
///   2. Add UIDocument (Source: Dialogue.uxml, PanelSettings sort 5).
///   3. Add this script. Assign playerSprite in Inspector.
/// 
/// API:
///   // Staged NPC (picks block matching GameStageManager.CurrentStage):
///   Dialogue.Instance.StartStagedDialogue(stages, leftSprite, rightSprite, onFinished);
///
///   // Scene dialogue (multiple speakers, explicit portrait per line):
///   Dialogue.Instance.StartSceneDialogue(lines, participants, onFinished);
///
///   // Simple single-block (backward compatible):
///   Dialogue.Instance.StartDialogue(lines, npcName, playerSpr, npcSpr, onFinished, speakers);
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class Dialogue : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static Dialogue Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Typing")]
    public float textSpeed = 0.03f;

    [Header("Player Portrait")]
    public Sprite playerSprite;

    // ── UI references ─────────────────────────────────────────────────────────
    private VisualElement root;
    private VisualElement dialogueRoot;
    private VisualElement spriteLeftImg;
    private VisualElement spriteRightImg;
    private VisualElement spriteLeftFrame;
    private VisualElement spriteRightFrame;
    private Label         speakerNameLabel;
    private Label         dialogueTextLabel;
    private Label         continuePrompt;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private DialogueLine[] activeLines;
    private SceneParticipant[] participants; // for scene dialogue portrait lookup
    private int       index;
    private bool      isTyping;
    private bool      dialogueActive;
    private Action    onFinished;
    private Coroutine typeCoroutine;

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Update()
    {
        if (!dialogueActive) return;
        if (Input.GetKeyDown(KeyCode.E)) OnClick();
        if (Input.GetKeyDown(KeyCode.Space)) OnClick();
         if (Input.GetKeyDown(KeyCode.Mouse0)) OnClick();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        root = GetComponent<UIDocument>().rootVisualElement;
        dialogueRoot     = root.Q("dialogue-root");
        spriteLeftImg    = root.Q("sprite-left-img");
        spriteRightImg   = root.Q("sprite-right-img");
        spriteLeftFrame  = root.Q("sprite-left");
        spriteRightFrame = root.Q("sprite-right");
        speakerNameLabel  = root.Q<Label>("speaker-name");
        dialogueTextLabel = root.Q<Label>("dialogue-text");
        continuePrompt    = root.Q<Label>("continue-prompt");

        // Advance dialogue with E key, handled in Update
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A) STAGED NPC DIALOGUE
    /// Picks the DialogueStage whose stage number matches GameStageManager.CurrentStage.
    /// Falls back to the highest stage number that is <= current stage.
    /// Left sprite = player, right sprite = NPC.
    /// </summary>
    public void StartStagedDialogue(
        DialogueStage[] stages,
        Sprite          npcPortrait,
        string          npcName   = "???",
        Sprite          playerSpr = null,
        Action          finished  = null)
    {
        DialogueStage block = PickStage(stages);
        if (block == null || block.lines == null || block.lines.Length == 0)
        {
            Debug.LogWarning("[Dialogue] No matching stage found.");
            return;
        }

        participants = null;

        // Both sides fixed: left = player, right = NPC
        SetSprite(spriteLeftImg,  playerSpr != null ? playerSpr : playerSprite);
        SetSprite(spriteRightImg, npcPortrait);

        BeginSequence(block.lines, npcName, finished);
    }

    /// <summary>
    /// B) SCENE DIALOGUE
    /// A scripted sequence with multiple speakers. Each line specifies its speaker
    /// by name; the matching SceneParticipant supplies the portrait and side.
    /// Participants on the left appear on the left; right on the right.
    /// The inactive side dims automatically.
    /// </summary>
    public void StartSceneDialogue(
        DialogueLine[]     lines,
        SceneParticipant[] sceneParticipants,
        Action             finished = null)
    {
        if (lines == null || lines.Length == 0) return;

        participants = sceneParticipants;

        // Prime both sprite slots from the first line's participant
        RefreshPortraits(lines[0]);

        BeginSequence(lines, lines[0].speakerName, finished);
    }

    /// <summary>
    /// SIMPLE / BACKWARD-COMPATIBLE overload.
    /// Left = player, right = NPC. Optional per-line speaker array ("player" = left).
    /// </summary>
    public void StartDialogue(
        string[] newLines,
        string   npcName,
        Sprite   playerSpr,
        Sprite   npcSpr,
        Action   finished     = null,
        string[] lineSpeakers = null)
    {
        participants = null;

        SetSprite(spriteLeftImg,  playerSpr != null ? playerSpr : playerSprite);
        SetSprite(spriteRightImg, npcSpr);

        // Convert string[] to DialogueLine[] so we share one code path
        var dl = new DialogueLine[newLines.Length];
        for (int i = 0; i < newLines.Length; i++)
        {
            string spk  = (lineSpeakers != null && i < lineSpeakers.Length)
                          ? lineSpeakers[i] : npcName;
            bool isLeft = spk.ToLower() == "player";
            dl[i] = new DialogueLine
            {
                speakerName = isLeft ? "Player" : npcName,
                side        = isLeft ? SpeakerSide.Left : SpeakerSide.Right,
                text        = newLines[i]
            };
        }

        BeginSequence(dl, npcName, finished);
    }

    /// <summary>Convenience overload — no per-line speaker array.</summary>
    public void StartDialogue(string[] newLines, Sprite npcSprite, Action finished = null)
        => StartDialogue(newLines, "", playerSprite, npcSprite, finished);

    // ═════════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ═════════════════════════════════════════════════════════════════════════

    private void BeginSequence(DialogueLine[] lines, string defaultName, Action finished)
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);

        activeLines    = lines;
        index          = 0;
        onFinished     = finished;
        dialogueActive = true;

        speakerNameLabel.text = defaultName;
        dialogueRoot.RemoveFromClassList("hidden");
        PlayerMovement.Instance?.LockForDialogue();

        ShowLine();
    }

    private void ShowLine()
    {
        continuePrompt.RemoveFromClassList("visible");
        dialogueTextLabel.text = string.Empty;

        DialogueLine line = activeLines[index];
        speakerNameLabel.text = line.speakerName;

        // Refresh portrait on every line (per-line portrait or participant lookup)
        RefreshPortraits(line);

        // Dim inactive side
        bool leftSpeaks = line.side == SpeakerSide.Left;
        SetActive(spriteLeftFrame,  leftSpeaks);
        SetActive(spriteRightFrame, !leftSpeaks);

        typeCoroutine = StartCoroutine(TypeLine(line.text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        foreach (char c in text)
        {
            dialogueTextLabel.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
        continuePrompt.AddToClassList("visible");
    }

    private void OnClick()
    {
        if (!dialogueActive) return;

        if (isTyping)
        {
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            isTyping = false;
            dialogueTextLabel.text = activeLines[index].text;
            continuePrompt.AddToClassList("visible");
            return;
        }

        index++;
        if (index < activeLines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        dialogueRoot.AddToClassList("hidden");
        dialogueTextLabel.text = string.Empty;
        speakerNameLabel.text  = string.Empty;
        continuePrompt.RemoveFromClassList("visible");

        PlayerMovement.Instance?.UnlockFromDialogue();
        onFinished?.Invoke();
    }

    // ── Portrait management ───────────────────────────────────────────────────

    private void RefreshPortraits(DialogueLine line)
    {
        // Per-line portrait takes priority
        if (line.portrait != null)
        {
            if (line.side == SpeakerSide.Left)
                SetSprite(spriteLeftImg, line.portrait);
            else
                SetSprite(spriteRightImg, line.portrait);
            return;
        }

        // Fall back to participant lookup for backward compatibility
        if (participants == null) return;
        SceneParticipant p = FindParticipant(line.speakerName);
        if (p == null) return;
        if (p.side == SpeakerSide.Left)
            SetSprite(spriteLeftImg, p.portrait);
        else
            SetSprite(spriteRightImg, p.portrait);
    }

    private SceneParticipant FindParticipant(string name)
    {
        if (participants == null) return null;
        foreach (var p in participants)
            if (p.speakerName == name) return p;
        return null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DialogueStage PickStage(DialogueStage[] stages)
    {
        if (stages == null || stages.Length == 0) return null;
        int current = GameStageManager.CurrentStage;

        DialogueStage best = null;
        foreach (var s in stages)
        {
            if (s.stage <= current)
            {
                if (best == null || s.stage > best.stage)
                    best = s;
            }
        }
        return best;
    }

    private static void SetActive(VisualElement frame, bool active)
    {
        if (active) frame.RemoveFromClassList("inactive");
        else        frame.AddToClassList("inactive");
    }

    private static void SetSprite(VisualElement el, Sprite sprite)
    {
        if (sprite == null) return;
        el.style.backgroundImage = new StyleBackground(sprite);
    }
}
