using UnityEngine;
using System.Collections.Generic;

public class StatsUIManager : MonoBehaviour
{
    [Header("Menu Settings")]
    [Tooltip("The parent GameObject that holds the entire Stats UI visual (e.g., the Background/Panel)")]
    public GameObject menuContainer;
    public KeyCode toggleKey = KeyCode.O;

    [Header("Member Slots")]
    [Tooltip("Assign your 4 UI panels here in order (Slot 1 to 4)")]
    public UIStatEntry[] uiSlots = new UIStatEntry[4];

    private bool _isMenuOpen = false;

    void Start()
    {
        // Ensure the menu starts closed
        _isMenuOpen = false;
        if (menuContainer != null) menuContainer.SetActive(false);
    }

    void Update()
    {
        // Listen for the "O" key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        _isMenuOpen = !_isMenuOpen;

        if (menuContainer != null)
        {
            menuContainer.SetActive(_isMenuOpen);
        }

        // If we just opened the menu, refresh the data to make sure it's current
        if (_isMenuOpen)
        {
            RefreshUI(PartyManager.Instance.GetRecords());
        }
    }

    void OnEnable()
    {
        PartyManager.OnPartyUpdated += RefreshUI;
    }

    void OnDisable()
    {
        PartyManager.OnPartyUpdated -= RefreshUI;
    }

    public void RefreshUI(List<MemberData> party)
    {
        // Safety check: if the menu is closed, we don't need to waste resources updating text
        if (menuContainer != null && !menuContainer.activeInHierarchy) return;

        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < party.Count)
                uiSlots[i].DisplayMember(party[i]);
            else
                uiSlots[i].DisplayMember(null);
        }
    }
}