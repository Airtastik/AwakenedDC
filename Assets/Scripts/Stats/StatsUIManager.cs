// Generated using Gemini 3 Fast on 3/11/26
//Prompts: I have (party manager) script. Can I use it to retrieve stat data from all party members and display it in a stats ui menu that I created? 
// Assume there are only up to four members. And that if there are less than 4, the screen for their stats is there its just blank
// Can you update statsUImanager to toggle the stats menu off and on by clicking "O"
// Can you automatically populate the parents for each UIStatEntry from the objects in the memberPrefabs list?

using UnityEngine;
using System.Collections.Generic;

public class StatsUIManager : MonoBehaviour
{
    public GameObject menuContainer;
    public KeyCode toggleKey = KeyCode.O;

    private UIStatEntry[] uiSlots;

    void Awake()
    {
        // Automatically finds all UIStatEntry scripts in your children
        uiSlots = GetComponentsInChildren<UIStatEntry>(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            menuContainer.SetActive(!menuContainer.activeSelf);
            if (menuContainer.activeSelf) RefreshUI();
        }
    }

    void OnEnable() => PartyManager.OnPartyUpdated += UpdateData;
    void OnDisable() => PartyManager.OnPartyUpdated -= UpdateData;

    // This matches the event signature from PartyManager
    private void UpdateData(List<MemberData> data) => RefreshUI();

    public void RefreshUI()
    {
        if (!menuContainer.activeInHierarchy) return;

        var records = PartyManager.Instance.GetRecords();

        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < records.Count)
                uiSlots[i].DisplayMember(records[i]);
            else
                uiSlots[i].DisplayMember(null);
        }
    }
}