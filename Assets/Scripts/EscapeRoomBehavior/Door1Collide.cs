//Made with Gemini 3
//Prompt: I am trying to create a unity game where if the user has the correct item in their inventory, the door should be destroyed. The door currently has a mesh collider to stop the player from walking through, and a box collider with the isTrigger set to true. How would you do this?

using UnityEngine;
using System.Collections.Generic;

public class Door1Collide : MonoBehaviour
{
    [Header("Key Required to Open")]
    [SerializeField] private ItemData requiredItem; // Assign in Inspector
    [SerializeField] private InventoryManager inventory;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered
        if (!other.CompareTag("Player"))
            return;

        if (inventory == null)
        {
            Debug.LogWarning("Player has no InventoryManager!");
            return;
        }

        // Check if player has the key
        if (inventory.InInventory(requiredItem))
        {
            Debug.Log("Player has the key! Opening door...");
            Destroy(transform.parent.gameObject); // destroy parent door
        }
        else
        {
            Debug.Log("Door is locked! Player does not have the key.");
        }
    }
}
