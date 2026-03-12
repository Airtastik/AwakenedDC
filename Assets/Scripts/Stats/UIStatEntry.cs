// Generated using Gemini 3 Fast on 3/11/26
//Prompts: I have (party manager) script. Can I use it to retrieve stat data from all party members and display it in a stats ui menu that I created? 
// Assume there are only up to four members. And that if there are less than 4, the screen for their stats is there its just blank
// Can you update statsUImanager to toggle the stats menu off and on by clicking "O"
// Can you automatically populate the parents for each UIStatEntry from the objects in the memberPrefabs list?

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