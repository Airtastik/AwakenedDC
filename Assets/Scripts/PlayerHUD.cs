// Gemini 3 Fast on 3/7/26
// Prompt: Create a PlayerHUD script that listens to the PartyManager's OnPartyUpdated event and updates the player's HP, XP, and level display accordingly.
// The HUD should specifically track the Main Character's stats, which are stored in the party list.
// Ensure that the HUD updates in real-time as the Main Character's stats change during gameplay.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    [Header("Settings")]
    public string mainCharacterName = "Dimitri";

    [Header("UI References")]
    public Slider hpSlider;
    public Slider xpSlider;
    public TMP_Text levelText;
    public TMP_Text hpText; 

    void OnEnable()
    {
        // Subscribe to the event so the HUD updates whenever stats change
        PartyManager.OnPartyUpdated += UpdateHUD;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        PartyManager.OnPartyUpdated -= UpdateHUD;
    }

    private void UpdateHUD(List<MemberData> party)
    {
        // Find the specific record for the Main Character
        MemberData mcData = party.Find(m => m.unitName == mainCharacterName);

        if (mcData == null) return;

        // Update HP Bar
        if (hpSlider != null)
        {
            hpSlider.maxValue = mcData.maxHealth;
            hpSlider.value = mcData.currentHealth;
        }

        // Update XP Bar
        if (xpSlider != null)
        {
            // Assuming experience resets to 0 or tracks progress toward the next level
            xpSlider.maxValue = mcData.experienceToNextLevel;
            xpSlider.value = mcData.experience;
        }

        // Update Text
        if (levelText != null)
            levelText.text = $"Lv. {mcData.level}";

        if (hpText != null)
            hpText.text = $"{mcData.currentHealth} / {mcData.maxHealth}";
    }
}