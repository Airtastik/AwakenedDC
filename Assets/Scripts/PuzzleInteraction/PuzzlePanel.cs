using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls one puzzle panel UI. Each puzzle in the game gets its own
/// PuzzlePanel component on a UI panel inside your Canvas.
///
/// SETUP:
/// 1. Create a UI Panel under your Canvas. This is the puzzle screen.
/// 2. Attach this script to that panel.
/// 3. Inside the panel, create child GameObjects for each "slot" where
///    the player can drop an inventory item. Attach PuzzleSlot to each one.
/// 4. Drag all PuzzleSlot children into the "slots" list.
/// 5. For each PuzzleSlot, set the "requiredItemName" to the item name
///    that solves that slot (must match ItemData.itemName exactly).
/// 6. Assign the InventoryManager reference.
/// 7. Create a separate UI panel as the "Inventory Sidebar" that shows
///    draggable copies of inventory items. Assign "inventorySidebarParent"
///    and "draggableItemPrefab". The panel will auto-populate when opened.
/// 8. Wire up "onPuzzleSolved" in the inspector to trigger whatever
///    should happen on success (open a door, play a cutscene, etc.).
/// 9. Add a Close button on the panel that calls PuzzlePanel.Close().
/// 10. The panel should start disabled in the scene.
/// </summary>
public class PuzzlePanel : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;

    [Tooltip("All slots in this puzzle.")]
    public List<PuzzleSlot> slots = new List<PuzzleSlot>();

    [Header("Inventory Sidebar")]
    [Tooltip("Parent transform in the puzzle UI where draggable inventory items are spawned.")]
    public Transform inventorySidebarParent;

    [Tooltip("Prefab for a draggable inventory icon. Needs a DraggableItem component.")]
    public GameObject draggableItemPrefab;

    [Header("Events")]
    [Tooltip("Fires when every slot is filled with the correct item.")]
    public UnityEvent onPuzzleSolved;

    [Header("State")]
    public bool isSolved = false;

    /// <summary>
    /// Called by PuzzleInteractable when the player presses E.
    /// </summary>
    public void Open()
    {
        if (isSolved) return; // Already solved — don't reopen

        gameObject.SetActive(true);
        Time.timeScale = 0f;

        // Show the cursor so the player can drag items
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PopulateInventorySidebar();
    }

    /// <summary>
    /// Wire this to a Close / X button on the panel.
    /// </summary>
    public void Close()
    {
        foreach (PuzzleSlot slot in slots)
        {
            slot.ClearSlot();
        }

        gameObject.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Builds the sidebar list of draggable icons from the player's
    /// current inventory. Called every time the panel opens and
    /// whenever a slot changes.
    /// </summary>
    public void PopulateInventorySidebar()
    {
        // Clear old entries
        foreach (Transform child in inventorySidebarParent)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in inventoryManager.items)
        {
            GameObject obj = Instantiate(draggableItemPrefab, inventorySidebarParent);
            DraggableItem drag = obj.GetComponent<DraggableItem>();
            drag.Setup(item, this);
        }
    }

    /// <summary>
    /// Called by PuzzleSlot whenever an item is placed or removed.
    /// Checks if the puzzle is now solved.
    /// </summary>
    public void CheckSolution()
    {
        foreach (PuzzleSlot slot in slots)
        {
            if (!slot.IsCorrect())
                return; // At least one slot is wrong or empty
        }

        // All slots correct!
        isSolved = true;
        Debug.Log("Puzzle solved!");
        onPuzzleSolved?.Invoke();

        // Optionally consume the items from inventory
        foreach (PuzzleSlot slot in slots)
        {
            slot.ConsumeItem(inventoryManager);
        }

        Close();
    }

    /// <summary>
    /// Returns an item back to the inventory sidebar (when removed from a slot).
    /// Just refreshes the sidebar so it reappears.
    /// </summary>
    public void ReturnItemToSidebar()
    {
        PopulateInventorySidebar();
    }
}
