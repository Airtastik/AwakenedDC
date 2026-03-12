using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the transition between the overworld and the RPG battle stage
/// which exists at a fixed offset in the same scene (default 400, 400, 400).
///
/// SETUP:
/// 1. Create an empty GameObject named "BattleStageManager" and attach this script
/// 2. Assign BattleCam (the Camera at the RPG stage) in the Inspector
/// 3. Assign OverworldCam (your main overworld camera) in the Inspector
/// 4. Assign the Player GameObject
/// 5. Assign the BattleSystem in the RPG stage
/// 6. Assign the BattleUI (the UIDocument/canvas at the RPG stage)
/// 7. Optionally assign a PlayerMovement component to disable during battle
/// </summary>
public class BattleStageManager : MonoBehaviour
{
    public static BattleStageManager Instance { get; private set; }

    [Header("Cameras")]
    public Camera overworldCam;
    public Camera battleCam;

    [Header("Player")]
    public GameObject    player;
    public PlayerMovement playerMovement; // Drag your PlayerMovement script here

    [Header("Battle Stage")]
    public Vector3 battleStageCenter   = new Vector3(400f, 400f, 400f);
    public Vector3 playerBattleOffset  = new Vector3(0f, 0f, -3f); // Where player stands relative to stage center

    [Header("Battle System")]
    public BattleSystem        battleSystem;
    public GameObject          battleUI;        // The UIDocument or canvas for the battle UI
    public MonoBehaviour       battleUIController; // Drag the NewMonoBehaviourScript here

    [Header("Transition")]
    public float transitionDelay = 0.3f;  // Brief pause before battle starts
    public float returnDelay     = 2.5f;  // Pause after battle ends before returning

    // ── State ─────────────────────────────────────────────────────────────────
    private Vector3    overworldPlayerPos;
    private Quaternion overworldPlayerRot;
    private bool       inBattle = false;

    // ── Events ────────────────────────────────────────────────────────────────
    public static event System.Action OnBattleEnter;
    public static event System.Action OnBattleExit;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        BattleSystem.OnStateChanged += OnBattleStateChanged;
    }

    void OnDisable()
    {
        BattleSystem.OnStateChanged -= OnBattleStateChanged;
    }

    void Start()
    {
        // Ensure battle stage is hidden at start
        if (battleCam != null) battleCam.gameObject.SetActive(false);
        if (battleUI  != null) battleUI.SetActive(false);
    }

    // ── Public entry point ────────────────────────────────────────────────────

    /// <summary>
    /// Call this from DungeonEnemyAI or any trigger to start a battle.
    /// Replaces SceneTransitionManager.StartBattle() when using a merged scene.
    /// </summary>
    public void EnterBattle(EnemyRoster roster, int level = 1)
    {
        if (inBattle) return;
        StartCoroutine(TransitionToBattle(roster, level));
    }

    // ── Transition to battle ──────────────────────────────────────────────────

    private IEnumerator TransitionToBattle(EnemyRoster roster, int level)
    {
        inBattle = true;
        OnBattleEnter?.Invoke();

        // 1. Save overworld position so we can return
        overworldPlayerPos = player.transform.position;
        overworldPlayerRot = player.transform.rotation;

        // 2. Lock player movement
        if (playerMovement != null) playerMovement.LockForBattle();

        // 3. Brief pause
        yield return new WaitForSeconds(transitionDelay);

        // 4. Teleport player to battle stage
        Vector3 battlePlayerPos = battleStageCenter + playerBattleOffset;
        player.transform.position = battlePlayerPos;
        player.transform.rotation = Quaternion.identity;

        // 5. Swap cameras
        if (overworldCam != null) overworldCam.gameObject.SetActive(false);
        if (battleCam    != null) battleCam.gameObject.SetActive(true);

        // 6. Show battle UI
        if (battleUI != null) battleUI.SetActive(true);

        // 7. Prepare the encounter
        EncounterManager.Prepare(roster, level);

        // Wait one frame for EncounterManager to register
        yield return null;

        // 8. Reset BattleSystem and UI, then kick off setup in the right order
        if (battleSystem != null)
        {
            // Disable then re-enable UI controller so its OnEnable/Start re-run
            // against a fresh BattleSystem state
            if (battleUIController != null)
            {
                battleUIController.enabled = false;
            }

            // Reset clears party lists and stops stale coroutines
            battleSystem.ResetForNewBattle();

            // BeginSetup starts LateStart() — it will idle on waitForSignal
            battleSystem.BeginSetup();

            // One frame for LateStart to reach the waitForSignal yield
            yield return null;

            // Re-enable UI controller — its Start() will call WaitForBattleReady
            if (battleUIController != null)
            {
                battleUIController.enabled = true;
            }

            // Signal BattleSystem to proceed past the idle
            battleSystem.StartBattle();
        }

        Debug.Log("[BattleStageManager] Battle started.");
    }

    // ── Transition back to overworld ──────────────────────────────────────────

    private IEnumerator TransitionToOverworld()
    {
        yield return new WaitForSeconds(returnDelay);

        // 1. Hide battle UI
        if (battleUI != null) battleUI.SetActive(false);

        // 2. Swap cameras back
        if (battleCam    != null) battleCam.gameObject.SetActive(false);
        if (overworldCam != null) overworldCam.gameObject.SetActive(true);

        // 3. Return player to overworld position
        player.transform.position = overworldPlayerPos;
        player.transform.rotation = overworldPlayerRot;

        // 4. Unlock player movement
        if (playerMovement != null) playerMovement.UnlockFromBattle();

        inBattle = false;
        OnBattleExit?.Invoke();

        Debug.Log("[BattleStageManager] Returned to overworld.");
    }

    // ── BattleSystem event ────────────────────────────────────────────────────

    private void OnBattleStateChanged(BattleState state)
    {
        if (!inBattle) return;

        if (state == BattleState.Won || state == BattleState.Lost)
            StartCoroutine(TransitionToOverworld());
    }

    // ── Gizmo ─────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.4f, 1f, 0.3f);
        Gizmos.DrawCube(battleStageCenter, new Vector3(20f, 10f, 20f));
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(battleStageCenter + playerBattleOffset, 0.4f);
    }
}
