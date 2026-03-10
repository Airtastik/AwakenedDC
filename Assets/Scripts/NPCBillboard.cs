using UnityEngine;

/// <summary>
/// 2.5D billboard renderer for NPCs (Luna and others).
/// Simpler than PlayerBillboard — no hurt flash, no faint tipping.
/// Faces the camera at all times and optionally bobs gently when idle.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class NPCBillboard : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Idle Bob")]
    public bool  idleBob       = true;
    public float bobHeight     = 0.04f;
    public float bobSpeed      = 1.20f;

    [Header("Interaction Highlight")]
    public Color highlightColour = new Color(1f, 1f, 0.6f, 1f);

    // ── Internals ─────────────────────────────────────────────────────────────
    private SpriteRenderer sr;
    private Camera          cam;
    private Vector3         startLocalPos;
    private Vector3         lastPosition;
    private bool            highlighted;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        cam          = Camera.main;
        startLocalPos = transform.localPosition;
        lastPosition  = transform.position;

        if (frontSprite != null) sr.sprite = frontSprite;
    }

    void Update()
    {
        if (cam == null) return;

        BillboardToCamera();
        SwapSprite();

        if (idleBob) HandleBob();

        lastPosition = transform.position;
    }

    // ── Billboard ─────────────────────────────────────────────────────────────

    void BillboardToCamera()
    {
        Vector3 dir = cam.transform.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(-dir);
    }

    // ── Sprite swapping ───────────────────────────────────────────────────────

    void SwapSprite()
    {
        if (frontSprite == null || backSprite == null) return;

        Vector3 movement = transform.position - lastPosition;
        movement.y = 0;

        if (movement.magnitude > 0.001f)
        {
            Vector3 toCamera = (cam.transform.position - transform.position).normalized;
            float   dot      = Vector3.Dot(movement.normalized, toCamera);
            sr.sprite = (dot > 0) ? frontSprite : backSprite;
        }
    }

    // ── Idle bob ──────────────────────────────────────────────────────────────

    void HandleBob()
    {
        Vector3 movement = transform.position - lastPosition;
        // Only bob when not moving
        if (movement.magnitude < 0.001f)
        {
            float newY = startLocalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);
        }
        else
        {
            // Snap back to base position while moving
            transform.localPosition = Vector3.Lerp(transform.localPosition, startLocalPos, Time.deltaTime * 10f);
        }
    }

    // ── Interaction highlight ─────────────────────────────────────────────────

    /// <summary>Call when the player is in range to talk to this NPC.</summary>
    public void SetHighlight(bool on)
    {
        highlighted = on;
        sr.color    = on ? highlightColour : Color.white;
    }
}
