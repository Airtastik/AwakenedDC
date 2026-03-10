using UnityEngine;

/// <summary>
/// Attach to a world GameObject with Tag "Item" and a Collider (set Is Trigger).
/// Assign the Item ScriptableObject in the Inspector.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public Item item;

    public void PickUp(PlayerInventory inventory)
    {
        if (item == null) return;
        inventory.AddItem(item);
        gameObject.SetActive(false);
    }
}
