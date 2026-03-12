using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Overworld party stats and management menu.
/// Reads from PartyManager and displays live party data.
/// Supports viewing stats, spending stat points, and reordering the party.
///
/// SETUP:
/// - Create a Canvas UI panel for the party menu
/// - Add this component to a persistent GameObject or the UI root
/// - Assign the panel root and four MemberUISlot references in the Inspector
/// - Press Tab (or your chosen key) to open/close
/// </summary>
public class PartyMenuController : MonoBehaviour
{
    [Header("Menu Root")]
    public GameObject menuPanel;
    public KeyCode    toggleKey = KeyCode.Tab;

    [Header("Member Slots (up to 4)")]
    public List<MemberUISlot> memberSlots = new List<MemberUISlot>();

    [Header("Detail Panel (shown when a member is selected)")]
    public GameObject     detailPanel;
    public TextMeshProUGUI detailName;
    public TextMeshProUGUI detailType;
    public TextMeshProUGUI detailLevel;
    public TextMeshProUGUI detailXP;
    public TextMeshProUGUI detailHP;
    public TextMeshProUGUI detailSP;
    public TextMeshProUGUI detailATK;
    public TextMeshProUGUI detailDEF;
    public TextMeshProUGUI detailSPD;
    public TextMeshProUGUI detailTrait;
    public TextMeshProUGUI detailStatPoints;

    [Header("Stat Point Buttons (in detail panel)")]
    public Button btnSpendHP;
    public Button btnSpendATK;
    public Button btnSpendDEF;
    public Button btnSpendSPD;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool   isOpen       = false;
    private int    selectedIndex = -1;
    private int    swapFromIndex = -1; // -1 = not in swap mode

    void OnEnable()
    {
        PartyManager.OnPartyUpdated += OnPartyUpdated;
    }

    void OnDisable()
    {
        PartyManager.OnPartyUpdated -= OnPartyUpdated;
    }

    void Start()
    {
        menuPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
        WireStatButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    // ── Open / Close ──────────────────────────────────────────────────────────

    public void Toggle()
    {
        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen)
        {
            RefreshAllSlots();
            selectedIndex = -1;
            swapFromIndex = -1;
            if (detailPanel != null) detailPanel.SetActive(false);
        }
    }

    // ── Slot population ───────────────────────────────────────────────────────

    public void RefreshAllSlots()
    {
        if (PartyManager.Instance == null) return;
        var records = PartyManager.Instance.GetRecords();

        for (int i = 0; i < memberSlots.Count; i++)
        {
            if (memberSlots[i] == null) continue;

            if (i < records.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Populate(records[i], i, this);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // ── Member selection ──────────────────────────────────────────────────────

    public void OnSlotClicked(int index)
    {
        if (swapFromIndex >= 0)
        {
            // Second click — complete the swap
            if (swapFromIndex != index)
            {
                PartyManager.Instance.SwapMembers(swapFromIndex, index);
                Debug.Log($"[PartyMenu] Swapped slots {swapFromIndex} and {index}.");
            }
            swapFromIndex = -1;
            RefreshAllSlots();

            // Highlight swap mode off
            foreach (var slot in memberSlots)
                slot?.SetSwapHighlight(false);
            return;
        }

        selectedIndex = index;
        ShowDetail(index);
    }

    public void OnSlotSwapClicked(int index)
    {
        swapFromIndex = index;
        // Highlight all other slots to show they can be swapped to
        for (int i = 0; i < memberSlots.Count; i++)
            memberSlots[i]?.SetSwapHighlight(i != index);
    }

    // ── Detail panel ──────────────────────────────────────────────────────────

    private void ShowDetail(int index)
    {
        if (detailPanel == null) return;

        MemberData r = PartyManager.Instance?.GetRecord(index);
        if (r == null) { detailPanel.SetActive(false); return; }

        detailPanel.SetActive(true);

        if (detailName      != null) detailName.text      = r.unitName;
        if (detailType      != null) detailType.text      = r.elementalType.ToString();
        if (detailLevel     != null) detailLevel.text     = $"Level  {r.level}";
        if (detailXP        != null) detailXP.text        = $"XP  {r.experience} / {r.experienceToNextLevel}";
        if (detailHP        != null) detailHP.text        = $"HP  {r.currentHealth} / {r.maxHealth}";
        if (detailSP        != null) detailSP.text        = $"SP  {r.currentSP} / {r.maxSP}";
        if (detailATK       != null) detailATK.text       = $"Attack   {r.attackP}";
        if (detailDEF       != null) detailDEF.text       = $"Defence  {r.defence}";
        if (detailSPD       != null) detailSPD.text       = $"Speed    {r.speed}";
        if (detailTrait     != null) detailTrait.text     = $"Trait:  {r.traitDescription}";
        if (detailStatPoints!= null) detailStatPoints.text =
            r.statPointsAvailable > 0
            ? $"{r.statPointsAvailable} stat point{(r.statPointsAvailable > 1 ? "s" : "")} available"
            : "No stat points";

        // Enable stat buttons only if points are available
        bool canSpend = r.statPointsAvailable > 0;
        btnSpendHP?.gameObject.SetActive(canSpend);
        btnSpendATK?.gameObject.SetActive(canSpend);
        btnSpendDEF?.gameObject.SetActive(canSpend);
        btnSpendSPD?.gameObject.SetActive(canSpend);
    }

    // ── Stat point buttons ────────────────────────────────────────────────────

    private void WireStatButtons()
    {
        btnSpendHP? .onClick.AddListener(() => SpendStat(StatType.Health));
        btnSpendATK?.onClick.AddListener(() => SpendStat(StatType.AttackP));
        btnSpendDEF?.onClick.AddListener(() => SpendStat(StatType.Defence));
        btnSpendSPD?.onClick.AddListener(() => SpendStat(StatType.Speed));
    }

    private void SpendStat(StatType stat)
    {
        if (selectedIndex < 0 || PartyManager.Instance == null) return;
        MemberData r = PartyManager.Instance.GetRecord(selectedIndex);
        if (r == null) return;

        bool success = PartyManager.Instance.SpendStatPoint(r.unitName, stat);
        if (success)
        {
            ShowDetail(selectedIndex);
            RefreshAllSlots();
        }
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnPartyUpdated(System.Collections.Generic.List<MemberData> party)
    {
        if (!isOpen) return;
        RefreshAllSlots();
        if (selectedIndex >= 0) ShowDetail(selectedIndex);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// One card in the party menu — shows portrait, name, HP bar, SP pips.
/// Assign UI elements per slot in the Inspector.
/// </summary>
[System.Serializable]
public class MemberUISlot
{
    public GameObject      gameObject;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI typeLabel;
    public TextMeshProUGUI hpLabel;
    public Slider          hpSlider;
    public TextMeshProUGUI spLabel;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI traitLabel;
    public Button          selectButton;
    public Button          swapButton;
    public Image           portrait;           // Optional — assign a sprite per character

    // Visual highlight when in swap mode
    public Image           slotBackground;
    public Color           normalColour   = new Color(0.15f, 0.15f, 0.2f, 1f);
    public Color           swapColour     = new Color(0.3f, 0.6f, 0.3f, 1f);

    private PartyMenuController controller;
    private int slotIndex;

    public void Populate(MemberData r, int index, PartyMenuController ctrl)
    {
        controller = ctrl;
        slotIndex  = index;

        if (nameLabel  != null) nameLabel.text  = r.unitName;
        if (typeLabel  != null) typeLabel.text  = r.elementalType.ToString();
        if (levelLabel != null) levelLabel.text = $"Lv.{r.level}";
        if (traitLabel != null) traitLabel.text = r.traitDescription;

        // HP
        float hpRatio = (float)r.currentHealth / Mathf.Max(r.maxHealth, 1);
        if (hpSlider != null) { hpSlider.minValue = 0; hpSlider.maxValue = 1; hpSlider.value = hpRatio; }
        if (hpLabel  != null) hpLabel.text = $"{r.currentHealth} / {r.maxHealth}";

        // SP
        if (spLabel != null) spLabel.text = $"SP  {r.currentSP} / {r.maxSP}";

        // Wire buttons
        selectButton?.onClick.RemoveAllListeners();
        selectButton?.onClick.AddListener(() => controller.OnSlotClicked(slotIndex));

        swapButton?.onClick.RemoveAllListeners();
        swapButton?.onClick.AddListener(() => controller.OnSlotSwapClicked(slotIndex));

        SetSwapHighlight(false);
    }

    public void SetSwapHighlight(bool on)
    {
        if (slotBackground != null)
            slotBackground.color = on ? swapColour : normalColour;
    }
}
