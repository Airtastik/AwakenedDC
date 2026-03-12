using UnityEngine;
using TMPro; // Use 'using UnityEngine.UI;' if using standard Text

public class UIStatEntry : MonoBehaviour
{
    [Header("UI References")]
    public GameObject contentParent; // The object holding the actual text/images
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text spText;
    public TMP_Text lvText;
    public TMP_Text statPointsText;

    // Call this to update the visuals
    public void DisplayMember(MemberData data)
    {
        if (data == null)
        {
            contentParent.SetActive(false); // Hide stats if slot is empty
            return;
        }

        contentParent.SetActive(true);
        nameText.text = data.unitName;
        hpText.text = $"HP: {data.currentHealth}/{data.maxHealth}";
        spText.text = $"SP: {data.currentSP}/{data.maxSP}";
        lvText.text = $"Lv: {data.level}";

        if (statPointsText != null)
            statPointsText.text = $"Points: {data.statPointsAvailable}";
    }
}