using UnityEngine;
using TMPro;

public class UIStatEntry : MonoBehaviour
{
    public TMP_Text statsText; // One text block to hold everything, or several

    public void DisplayMember(MemberData data)
    {
        if (data == null)
        {
            statsText.text = "--- EMPTY ---";
            return;
        }

        // Quickest way: string interpolation
        statsText.text = $"{data.unitName}\n\n" +
                         $"LV: {data.level}\n\n" +
                         $"HP: {data.currentHealth}/{data.maxHealth}\n\n" +
                         $"SP: {data.currentSP}/{data.maxSP}\n\n" +
                         $"PTS: {data.statPointsAvailable}";
    }
}