// Generated using Gemini 3 Fast on 3/11/26
// Prompt: I want to use the partymanager data to fill out a hud.
// I only want to display data for the character named "Dimitri Glass". I want to show his level in one text field and his health bar on a slider.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DimitriHUD : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelText;
    public Slider healthSlider;

    private const string TargetName = "Dimitri Glass";

    void OnEnable()
    {
        // Update whenever the party changes (damage, level up, etc.)
        PartyManager.OnPartyUpdated += UpdateHUD;

        // Initial refresh
        if (PartyManager.Instance != null)
            UpdateHUD(PartyManager.Instance.GetRecords());
    }

    void OnDisable()
    {
        PartyManager.OnPartyUpdated -= UpdateHUD;
    }

    private void UpdateHUD(System.Collections.Generic.List<MemberData> party)
    {
        // Use the built-in helper to find Dimitri
        MemberData dimitri = PartyManager.Instance.GetRecord(TargetName);

        if (dimitri != null)
        {
            // 1. Update Level Text
            if (levelText != null)
                levelText.text = $"Lv {dimitri.level}";

            // 2. Update Health Slider
            if (healthSlider != null)
            {
                healthSlider.maxValue = dimitri.maxHealth;
                healthSlider.value = dimitri.currentHealth;
            }
        }
        else
        {
            // Optional: Handle if Dimitri isn't in the party
            Debug.LogWarning("Dimitri Glass not found in party records.");
        }
    }
}