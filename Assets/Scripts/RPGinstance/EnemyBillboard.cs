using UnityEngine;

/// <summary>
/// 2.5D billboard renderer for RPG battle enemies.
/// Attach to the enemy prefab alongside EnemyUnit.
/// Faces the camera at all times, swaps front/back sprites on movement,
/// and handles hurt flash, faint, and an idle menace pulse.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBillboard : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Hurt Flash")]
    public Color hurtColour    = new Color(1f, 0.3f, 0.3f, 1f);
    public float flashDuration = 0.12f;

    [Header("Faint")]
    public Color faintColour   = new Color(0.3f, 0.3f, 0.3f, 0.4f);

    [Header("Idle Pulse")]
    [Tooltip("Enemies gently pulse in scale while alive to feel threatening.")]
    public bool  idlePulse     = true;
    public float pulseScale    = 0.03f;   // How much it grows
    public float pulseSpeed    = 1.00f;   // How fast it pulses

    [Header("Boss Tint")]
    [Tooltip("Optional tint for bosses to make them stand out on the field.")]
    public bool  useBossTint   = false;
    public Color bossTint      = new Color(1f, 0.85f, 0.85f, 1f);

    // ── Internals ─────────────────────────────────────────────────────────────
    private SpriteRenderer sr;
    private Camera          cam;
    private EnemyUnit       unit;
    private Vector3         baseScale;
    private Vector3         lastPosition;
    private float           flashTimer;
    private bool            isFainted;

    void Awake()
    {
        sr   = GetComponent<SpriteRenderer>();
        unit = GetComponentInParent<EnemyUnit>() ?? GetComponent<EnemyUnit>();
    }

    void Start()
    {
        cam          = Camera.main;
        baseScale    = transform.localScale;
        lastPosition = transform.position;

        if (frontSprite != null) sr.sprite = frontSprite;
        if (useBossTint)         sr.color  = bossTint;
    }

    void Update()
    {
        if (cam == null) return;

        BillboardToCamera();
        SwapSprite();
        TickFlash();
        if (idlePulse && !isFainted) HandlePulse();

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
        if (isFainted || frontSprite == null || backSprite == null) return;

        Vector3 movement = transform.position - lastPosition;
        movement.y = 0;

        if (movement.magnitude > 0.001f)
        {
            Vector3 toCamera = (cam.transform.position - transform.position).normalized;
            float   dot      = Vector3.Dot(movement.normalized, toCamera);
            sr.sprite = (dot > 0) ? frontSprite : backSprite;
        }
    }

    // ── Hurt flash ────────────────────────────────────────────────────────────

    void TickFlash()
    {
        if (flashTimer <= 0) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0 && !isFainted)
            sr.color = useBossTint ? bossTint : Color.white;
    }

    /// <summary>Call from BattleSystem when this enemy takes damage.</summary>
    public void TriggerHurtFlash()
    {
        if (isFainted) return;
        sr.color   = hurtColour;
        flashTimer = flashDuration;
    }

    // ── Faint ─────────────────────────────────────────────────────────────────

    /// <summary>Call when the enemy is defeated.</summary>
    public void TriggerFaint()
    {
        isFainted            = true;
        sr.color             = faintColour;
        transform.localScale = baseScale;
        // Slump — tip 90 degrees
        transform.rotation   = Quaternion.Euler(0f, 0f, -90f);
    }

    // ── Idle pulse ────────────────────────────────────────────────────────────

    void HandlePulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
        transform.localScale = baseScale * pulse;
    }
}
