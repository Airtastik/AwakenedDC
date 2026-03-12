using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed    = 6f;
    public float runSpeed     = 12f;
    public float jumpPower    = 7f;
    public float gravity      = 10f;
    public float lookSpeed    = 2f;
    public float lookXLimit   = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed  = 3f;

    public float     interactRange  = 3f;
    public LayerMask interactLayer;

    private Vector3          moveDirection = Vector3.zero;
    private float            rotationX     = 0;
    private CharacterController characterController;

    private bool canMove = true;

    // Locked flags — each system sets its own lock independently
    private bool lockedByInventory = false;
    private bool lockedByDialogue  = false;
    private bool lockedByBattle    = false;

    private InventoryManager inventoryManager;

    // ── Static access so other scripts can lock movement cleanly ──────────────
    public static PlayerMovement Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterController  = GetComponent<CharacterController>();
        inventoryManager     = FindFirstObjectByType<InventoryManager>();
        SetCursorState(true);

        // Subscribe to battle events
        BattleStageManager.OnBattleEnter += LockForBattle;
        BattleStageManager.OnBattleExit  += UnlockFromBattle;
    }

    void OnDestroy()
    {
        BattleStageManager.OnBattleEnter -= LockForBattle;
        BattleStageManager.OnBattleExit  -= UnlockFromBattle;
    }

    // ── Lock API — called by other systems ────────────────────────────────────

    public void LockForDialogue()  { lockedByDialogue  = true;  RefreshCanMove(); }
    public void UnlockFromDialogue(){ lockedByDialogue  = false; RefreshCanMove(); }
    public void LockForBattle()    { lockedByBattle    = true;  RefreshCanMove(); }
    public void UnlockFromBattle() { lockedByBattle    = false; RefreshCanMove(); }

    private void RefreshCanMove()
    {
        canMove = !lockedByInventory && !lockedByDialogue && !lockedByBattle;
        SetCursorState(canMove);
    }

    // ── Main update ───────────────────────────────────────────────────────────

    void Update()
    {
        // Inventory lock check (InventoryManager drives its own UI)
        if (inventoryManager != null && inventoryManager.inventoryUI != null)
        {
            bool inventoryOpen = inventoryManager.inventoryUI.activeSelf;
            if (inventoryOpen != lockedByInventory)
            {
                lockedByInventory = inventoryOpen;
                RefreshCanMove();
            }
        }

        // Interaction
        if (canMove && Input.GetKeyDown(KeyCode.E))
            HandleInteraction();

        // Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right   = transform.TransformDirection(Vector3.right);

        bool  isRunning  = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX  = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical")   : 0;
        float curSpeedY  = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float savedY     = moveDirection.y;
        moveDirection    = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = savedY;

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Crouch
        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed  = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed  = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Look
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX  = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    // ── Item interaction ──────────────────────────────────────────────────────

    void HandleInteraction()
    {
        Collider[] closeItems = Physics.OverlapSphere(transform.position, interactRange, interactLayer);
        Debug.Log("Items found in range: " + closeItems.Length);

        float     closestDist = Mathf.Infinity;
        WorldItem closestItem = null;

        foreach (Collider col in closeItems)
        {
            WorldItem item = col.GetComponent<WorldItem>();
            if (item != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDist) { closestDist = dist; closestItem = item; }
            }
            else
            {
                Debug.Log("Found " + col.name + " but it has no WorldItem script!");
            }
        }

        if (closestItem != null)
        {
            Debug.Log("Picking up: " + closestItem.itemData.itemName);
            closestItem.PickUp();
        }
    }

    // ── Cursor ────────────────────────────────────────────────────────────────

    void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
