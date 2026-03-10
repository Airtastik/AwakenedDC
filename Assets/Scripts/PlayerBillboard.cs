using UnityEngine;

/// <summary>
/// 2.5D billboard renderer for player party members.
/// Attach to the player prefab alongside PlayerUnit.
/// Faces the camera at all times and swaps sprites based on movement direction.
/// Also handles idle, hurt flash, and faint visuals.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerBillboard : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite frontSprite;   // Facing camera / toward enemy
    public Sprite backSprite;    // Moving away

    [Header("Hurt Flash")]
    public Color hurtColour  = new Color(1f, 0.3f, 0.3f, 1f);
    public float flashDuration = 0.12f;

    [Header("Faint")]
    public Color faintColour = new Color(0.4f, 0.4f, 0.4f, 0.5f);


    [Header("Idle Pulse")]
    [Tooltip("Enemies gently pulse in scale while alive to feel threatening.")]
    public bool idlePulse = true;
    public float pulseScale = 0.03f;   // How much it grows
    public float pulseSpeed = 1.00f;   // How fast it pulses

    // ── Internals ─────────────────────────────────────────────────────────────
    private SpriteRenderer sr;
    private Camera          cam;
    private PlayerUnit      unit;
    private Vector3         lastPosition;
    private float           flashTimer;
    private bool            isFainted;

    void Awake()
    {
        sr   = GetComponent<SpriteRenderer>();
        unit = GetComponentInParent<PlayerUnit>() ?? GetComponent<PlayerUnit>();
    }

    void Start()
    {
        cam          = Camera.main;
        lastPosition = transform.position;

        if (frontSprite != null) sr.sprite = frontSprite;
    }

    void Update()
    {
        if (cam == null) return;

        BillboardToCamera();
        SwapSprite();
        TickFlash();

        lastPosition = transform.position;
    }


    // ── Billboard ─────────────────────────────────────────────────────────────

    void BillboardToCamera()
    {
        Vector3 dirToCamera = cam.transform.position - transform.position;
        dirToCamera.y = 0;
        if (dirToCamera != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(-dirToCamera);
    }

    // ── Sprite swapping ───────────────────────────────────────────────────────

    void SwapSprite()
    {
        if (isFainted || frontSprite == null || backSprite == null) return;

        Vector3 movement = transform.position - lastPosition;
        movement.y = 0;

        if (movement.magnitude > 0.001f)
        {
            Vector3 toCamera  = (cam.transform.position - transform.position).normalized;
            float   dot       = Vector3.Dot(movement.normalized, toCamera);
            sr.sprite = (dot > 0) ? frontSprite : backSprite;
        }
    }

    // ── Hurt flash ────────────────────────────────────────────────────────────

    void TickFlash()
    {
        if (flashTimer <= 0) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0 && !isFainted)
            sr.color = Color.white;
    }

    /// <summary>Call from BattleSystem or UI when this unit takes damage.</summary>
    public void TriggerHurtFlash()
    {
        if (isFainted) return;
        sr.color   = hurtColour;
        flashTimer = flashDuration;
    }

    /// <summary>Call when the unit faints.</summary>
    public void TriggerFaint()
    {
        isFainted  = true;
        sr.color   = faintColour;
        // Tip over — rotate 90 degrees on Z axis
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
    }

    /// <summary>Call when the unit is revived.</summary>
    public void TriggerRevive()
    {
        isFainted  = false;
        sr.color   = Color.white;
        BillboardToCamera();
    }
}
