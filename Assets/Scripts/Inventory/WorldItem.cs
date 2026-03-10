using UnityEngine;

/// <summary>
/// A pickable item in the world. Supports both the overworld InventoryManager
/// and the RPG battle PlayerInventory — assign whichever you use.
/// If both are assigned, the item is added to both.
/// </summary>
public class WorldItem : MonoBehaviour
{
    [Header("Overworld Inventory (UI-based)")]
    public ItemData itemData;      // Your existing Gemini inventory system

    [Header("Battle Inventory (RPG system)")]
    public Item battleItem;        // The RPG Item ScriptableObject for battle use

    [Header("Pickup Settings")]
    public float   pickupRange     = 2.5f;
    public bool    autoPickup      = false; // true = pick up on entering range
    public KeyCode pickupKey       = KeyCode.E;

    private bool   playerInRange   = false;
    private Transform playerTransform;

    void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        playerInRange = dist <= pickupRange;

        if (playerInRange)
        {
            if (autoPickup || Input.GetKeyDown(pickupKey))
                PickUp();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerTransform = other.transform;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
            playerInRange   = false;
        }
    }

    public void PickUp()
    {
        bool pickedUp = false;

        // Add to overworld InventoryManager if itemData is assigned
        if (itemData != null)
        {
            InventoryManager inv = FindFirstObjectByType<InventoryManager>();
            if (inv != null) { inv.AddItem(itemData); pickedUp = true; }
        }

        // Add to RPG PlayerInventory if battleItem is assigned
        if (battleItem != null && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(battleItem);
            pickedUp = true;
        }

        if (pickedUp)
        {
            Debug.Log($"[WorldItem] Picked up {gameObject.name}.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[WorldItem] No inventory found to receive {gameObject.name}.");
        }
    }

    // Draw pickup range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
