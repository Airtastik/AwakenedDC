using UnityEngine;

/// <summary>
/// Attach to a world object that the player can pick up.
/// When the player presses E, the linked ItemData is added to
/// their inventory and the world object is optionally destroyed.
///
/// SETUP:
/// 1. Attach this script to the pickup GameObject in the scene.
/// 2. Drag your ItemData ScriptableObject into the "itemToGive" field.
/// 3. Set "quantityToGive" (defaults to 1).
/// 4. Drag the scene's InventoryManager into the "inventoryManager" field.
/// 5. Assign the shared interact-prompt TextMeshProUGUI (inherited from Interactable).
/// 6. Make sure the GameObject has a Collider with "Is Trigger" checked.
/// </summary>
public class PickupInteractable : Interactable
{
    [Header("Pickup Settings")]
    [Tooltip("The ItemData asset this object gives the player.")]
    public ItemData itemToGive;

    [Tooltip("How many of the item to give.")]
    public int quantityToGive = 1;

    [Tooltip("Destroy the world object after pickup?")]
    public bool destroyOnPickup = true;

    [Tooltip("True if item is needed to pickup the item")]
    public bool itemNeededBool = false;

    [Tooltip("Item needed to pickup the item")]
    public ItemData itemNeeded;

    [Tooltip("Reference to the scene's InventoryManager.")]
    public InventoryManager inventoryManager;

    protected override void OnInteract()
    {
        if (itemToGive == null)
        {
            Debug.LogWarning($"PickupInteractable on {gameObject.name}: No ItemData assigned.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning($"PickupInteractable on {gameObject.name}: No InventoryManager assigned.");
            return;
        }

        if (itemNeededBool && !inventoryManager.InInventory(itemToGive))
        {
            Debug.LogWarning("Cannot open the box...");
            return;
        }



        // Create a temporary copy with the desired quantity so AddItem
        // can stack correctly without mutating the ScriptableObject asset.
        ItemData giveData = itemToGive.GetCopy();
        giveData.quantity = quantityToGive;

        inventoryManager.AddItem(giveData);
        Debug.Log($"Picked up {quantityToGive}x {itemToGive.itemName}");

        HidePrompt();

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            // Disable the script so the player can't pick up again
            enabled = false;
        }
    }
}
