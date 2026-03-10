using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays live party stats from PartyManager in the overworld.
/// Press O to toggle. Auto-refreshes when the panel opens.
/// Assign the stat panel UI elements in the Inspector.
/// </summary>
public class StatsManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject statsUI;

    [Header("Party Stat Slots — one per party member (max 4)")]
    public List<PartyStatSlot> slots = new List<PartyStatSlot>();

    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            ToggleStats();
    }

    public void ToggleStats()
    {
        isOpen = !isOpen;
        statsUI.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen) RefreshStats();
    }

    public void RefreshStats()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("[StatsManager] No PartyManager found.");
            return;
        }

        List<MemberData> records = PartyManager.Instance.GetRecords();

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;

            if (i < records.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Populate(records[i]);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }
}

/// <summary>
/// One stat card for a single party member.
/// Assign TMP labels in the Inspector for each field.
/// </summary>
[System.Serializable]
public class PartyStatSlot
{
    public GameObject  gameObject;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI typeLabel;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI hpLabel;
    public TextMeshProUGUI spLabel;
    public TextMeshProUGUI attackLabel;
    public TextMeshProUGUI defenceLabel;
    public TextMeshProUGUI speedLabel;
    public TextMeshProUGUI traitLabel;

    public void Populate(MemberData d)
    {
        if (nameLabel    != null) nameLabel.text    = d.unitName;
        if (typeLabel    != null) typeLabel.text    = d.elementalType.ToString();
        if (levelLabel   != null) levelLabel.text   = $"Lv. {d.level}  ({d.experience}/{d.experienceToNextLevel} XP)";
        if (hpLabel      != null) hpLabel.text      = $"HP  {d.currentHealth} / {d.maxHealth}";
        if (spLabel      != null) spLabel.text      = $"SP  {d.currentSP} / {d.maxSP}";
        if (attackLabel  != null) attackLabel.text  = $"ATK  {d.attackP}";
        if (defenceLabel != null) defenceLabel.text = $"DEF  {d.defence}";
        if (speedLabel   != null) speedLabel.text   = $"SPD  {d.speed}";
        if (traitLabel   != null) traitLabel.text   = d.traitDescription;
    }
}
