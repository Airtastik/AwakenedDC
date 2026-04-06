using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A draggable inventory icon spawned in the puzzle panel's inventory sidebar.
/// Uses Unity's EventSystem drag interfaces to let the player drag it onto a PuzzleSlot.
///
/// PREFAB SETUP:
/// 1. Create a UI GameObject (e.g. 80x80).
/// 2. Add an Image component for the item icon.
/// 3. Optionally add a child TextMeshProUGUI for the item name or quantity.
/// 4. Attach this script.
/// 5. Drag the Image into "iconImage" and the text into "nameText".
/// 6. Save as a prefab and assign it to PuzzlePanel.draggableItemPrefab.
///
/// IMPORTANT: The Canvas must have a GraphicRaycaster, and the scene needs an EventSystem.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI nameText;

    // Exposed for PuzzleSlot to read
    public ItemData ItemData { get; private set; }
    public PuzzlePanel OwnerPanel { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Canvas rootCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Called by PuzzlePanel.PopulateInventorySidebar() after instantiation.
    /// </summary>
    public void Setup(ItemData item, PuzzlePanel panel)
    {
        ItemData = item;
        OwnerPanel = panel;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = $"{item.itemName} x{item.quantity}";

        // Cache the root canvas for coordinate conversion during drag
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;

        // Re-parent to the root canvas so it renders on top of everything
        transform.SetParent(rootCanvas.transform, true);

        // Let raycasts pass through so the PuzzleSlot underneath receives OnDrop
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the icon to follow the pointer (works at any canvas scale)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        rectTransform.position = rootCanvas.transform.TransformPoint(localPoint);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // If not dropped on a valid slot, snap back
        if (transform.parent == rootCanvas.transform)
        {
            transform.SetParent(originalParent, false);
            transform.position = originalPosition;
        }
    }
}
