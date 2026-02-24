// Generated using Gemini 3 Fast on 2/22/2026
// Prompt: I want to create an inventory and Item pickup system. I want to start
// with a simple health potion. I want to have the option to pick up the health potion when I get close enough.

using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Data")]
    public List<Item> items = new List<Item>();

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    private Item nearbyItem; // Stores the item we are currently standing near

    [Header("UI References")]
    public GameObject inventoryMenu; 
    private bool isInventoryOpen = false;

    [Header("UI Definition")]
    public TextMeshProUGUI itemListText; 

    void Update()
    {
        // Toggle Inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryMenu.SetActive(isInventoryOpen);

            // Handle Mouse and Time
            if (isInventoryOpen)
            {
                Time.timeScale = 0f; // Pause the game
                Cursor.lockState = CursorLockMode.None; // Free the mouse
                Cursor.visible = true;
                UpdateUI(); // Refresh the list
            }
            else
            {
                Time.timeScale = 1f; // Resume
                Cursor.lockState = CursorLockMode.Locked; // Re-lock mouse for FPS
                Cursor.visible = false;
            }
        }

        // Interaction logic
        if (!isInventoryOpen && nearbyItem != null && Input.GetKeyDown(interactionKey))
        {
            PickUp(nearbyItem);
        }
    }

    // --- DETECTION LOGIC ---

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has the "Item" tag
        if (other.CompareTag("Item"))
        {
            Item itemScript = other.GetComponent<Item>();
            if (itemScript != null)
            {
                nearbyItem = itemScript;
                Debug.Log("Near " + nearbyItem.itemName + ". Press " + interactionKey + " to pick up.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When walking away, clear the reference so we can't pick it up from afar
        if (other.CompareTag("Item"))
        {
            nearbyItem = null;
            Debug.Log("Left item range.");
        }
    }

    // --- INVENTORY LOGIC ---

    void PickUp(Item item)
    {
        Debug.Log("Successfully picked up: " + item.itemName);

        items.Add(item);

        // Option A: Destroy the object in the world
        // Destroy(item.gameObject); 

        // Option B: Just deactivate it (Better if you want to reference the specific object later)
        item.gameObject.SetActive(false);

        // Clear the nearby reference since the object is now "gone"
        nearbyItem = null;
    }

    void UpdateUI()
    {
        // Clear the current text
        itemListText.text = "--- INVENTORY ---\n";

        if (items.Count == 0)
        {
            itemListText.text += "Empty";
            return;
        }

        // Create a dictionary to count duplicates (e.g., "Health Potion x3")
        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (Item item in items)
        {
            if (counts.ContainsKey(item.itemName))
                counts[item.itemName]++;
            else
                counts[item.itemName] = 1;
        }

        // Display the items and their counts
        foreach (var entry in counts)
        {
            itemListText.text += entry.Key + " x" + entry.Value + "\n";
        }
    }
}