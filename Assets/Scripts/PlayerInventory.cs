// Generated using Gemini 3 Fast on 2/22/2026
// Prompt: I want to create an inventory and Item pickup system. I want to start
// with a simple health potion. I want to have the option to pick up the health potion when I get close enough.

using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Data")]
    public List<Item> items = new List<Item>();

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    private Item nearbyItem; // Stores the item we are currently standing near

    void Update()
    {
        // Check if we are near an item AND the player presses the key
        if (nearbyItem != null && Input.GetKeyDown(interactionKey))
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
}