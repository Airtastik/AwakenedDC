using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A single slot on a PuzzlePanel where the player can drop an inventory item.
/// Implements IDropHandler so Unity's EventSystem routes drops here.
///
/// SETUP:
/// 1. Create a UI Image (acts as the slot background). Attach this script.
/// 2. Set "requiredItemName" to the exact name of the item that solves this slot.
/// 3. Add a child Image for displaying the dropped item's icon (drag into "slotIcon").
/// 4. Optionally add a child TextMeshProUGUI for a label (drag into "slotLabel").
/// 5. The slot's RectTransform must have Raycast Target enabled on its Image.
/// 6. Make sure your Canvas has a GraphicRaycaster and EventSystem in the scene.
/// </summary>
public class PuzzleSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Puzzle Logic")]
    [Tooltip("The exact ItemData.itemName that this slot accepts as correct.")]
    public string requiredItemName;

    [Header("UI")]
    [Tooltip("Image component used to show the item placed in this slot.")]
    public Image slotIcon;

    [Tooltip("Optional label shown on the slot (e.g. hint text).")]
    public TextMeshProUGUI slotLabel;

    [Header("Feedback Colors")]
    public Color emptyColor = new Color(1f, 1f, 1f, 0.3f);
    public Color correctColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
    public Color incorrectColor = new Color(0.9f, 0.2f, 0.2f, 0.8f);

    // Internal state
    private ItemData placedItem = null;
    private PuzzlePanel parentPanel;
    private Image backgroundImage;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        parentPanel = GetComponentInParent<PuzzlePanel>();
        ClearSlot();
    }

    /// <summary>
    /// Called by EventSystem when a DraggableItem is dropped onto this slot.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        // If slot already has an item, return it first
        if (placedItem != null)
        {
            ClearSlot();
            parentPanel.ReturnItemToSidebar();
        }

        PlaceItem(dragged.ItemData, dragged.OwnerPanel);

        // Remove this item visually from the sidebar
        Destroy(dragged.gameObject);
    }

    /// <summary>
    /// Right-click (or tap) the slot to remove the placed item.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && placedItem != null)
        {
            ClearSlot();
            parentPanel.ReturnItemToSidebar();
            parentPanel.CheckSolution();
        }
    }

    private void PlaceItem(ItemData item, PuzzlePanel panel)
    {
        placedItem = item;
        parentPanel = panel;

        // Show the item icon
        if (slotIcon != null)
        {
            slotIcon.sprite = item.icon;
            slotIcon.color = Color.white;
            slotIcon.gameObject.SetActive(true);
        }

        // Color feedback
        if (backgroundImage != null)
        {
            backgroundImage.color = IsCorrect() ? correctColor : incorrectColor;
        }

        parentPanel.CheckSolution();
    }

    private void ClearSlot()
    {
        placedItem = null;

        if (slotIcon != null)
        {
            slotIcon.sprite = null;
            slotIcon.color = Color.clear;
            slotIcon.gameObject.SetActive(false);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = emptyColor;
        }
    }

    /// <summary>
    /// Returns true if the placed item matches the required item.
    /// </summary>
    public bool IsCorrect()
    {
        return placedItem != null &&
               placedItem.itemName == requiredItemName;
    }

    /// <summary>
    /// Removes one of the placed item from the player's inventory.
    /// Called by PuzzlePanel when the puzzle is fully solved.
    /// </summary>
    public void ConsumeItem(InventoryManager inventory)
    {
        if (placedItem == null) return;

        ItemData existing = inventory.items.Find(i => i.itemName == placedItem.itemName);
        if (existing != null)
        {
            existing.quantity--;
            if (existing.quantity <= 0)
                inventory.items.Remove(existing);
        }
    }
}
