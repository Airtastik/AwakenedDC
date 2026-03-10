using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Tracks the player's items both in the overworld and in battle.
/// Attach to the player GameObject. Assign your item ScriptableObjects
/// in the Inspector under Starting Items for testing.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public List<Item> items = new List<Item>();

    [Header("Starting Items (for testing)")]
    public List<Item> startingItems = new List<Item>();

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    private ItemPickup nearbyPickup;

    [Header("Legacy UI (overworld)")]
    public GameObject  inventoryMenu;
    public TextMeshProUGUI itemListText;
    private bool isInventoryOpen = false;

    // ── Static instance so BattleSystem can find it across scenes ────────────
    public static PlayerInventory Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        foreach (var item in startingItems)
            if (item != null) items.Add(item);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryMenu != null) inventoryMenu.SetActive(isInventoryOpen);
            if (isInventoryOpen) { Time.timeScale = 0f; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; UpdateLegacyUI(); }
            else                 { Time.timeScale = 1f; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }

        if (!isInventoryOpen && nearbyPickup != null && Input.GetKeyDown(interactionKey))
            nearbyPickup.PickUp(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            var pickup = other.GetComponent<ItemPickup>();
            if (pickup != null) { nearbyPickup = pickup; Debug.Log($"Near {pickup.item?.itemName}. Press {interactionKey} to pick up."); }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item")) nearbyPickup = null;
    }

    // ── Inventory helpers ─────────────────────────────────────────────────────

    public void AddItem(Item item)
    {
        items.Add(item);
        Debug.Log($"[Inventory] Picked up {item.itemName}. Total: {CountOf(item.itemName)}");
    }

    /// <summary>
    /// Consume one copy of the item. Returns false if not in inventory.
    /// </summary>
    public bool ConsumeItem(Item item)
    {
        int idx = items.IndexOf(item);
        if (idx < 0) { Debug.LogWarning($"[Inventory] {item.itemName} not found."); return false; }
        items.RemoveAt(idx);
        return true;
    }

    public int CountOf(string itemName) => items.FindAll(i => i.itemName == itemName).Count;

    public List<Item> GetUniqueItems()
    {
        var seen  = new HashSet<string>();
        var unique = new List<Item>();
        foreach (var item in items)
            if (seen.Add(item.itemName)) unique.Add(item);
        return unique;
    }

    // ── Legacy overworld UI ───────────────────────────────────────────────────

    void UpdateLegacyUI()
    {
        if (itemListText == null) return;
        itemListText.text = "--- INVENTORY ---\n";
        if (items.Count == 0) { itemListText.text += "Empty"; return; }
        var counts = new Dictionary<string, int>();
        foreach (var item in items)
            counts[item.itemName] = counts.ContainsKey(item.itemName) ? counts[item.itemName] + 1 : 1;
        foreach (var entry in counts)
            itemListText.text += $"{entry.Key} x{entry.Value}\n";
    }
}
