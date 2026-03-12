using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DungeonEnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("State Management")]
    public EnemyState currentState = EnemyState.Patrol;
    public Transform  player;

    [Header("Detection Settings")]
    public float     detectRange     = 10f;
    public float     attackRange     = 2f;
    public LayerMask detectionLayers;

    [Header("Graphics (2.5D)")]
    public SpriteRenderer spriteRenderer;
    public Sprite         frontSprite;
    public Sprite         backSprite;

    [Header("Battle Encounter")]
    [Tooltip("The enemy roster to use when this enemy touches the player.")]
    public EnemyRoster enemyRoster;

    [Tooltip("Level to scale enemies to in the battle.")]
    public int encounterLevel = 1;

    [Tooltip("Spawn point ID to return the player to after the battle.")]
    public string returnSpawnID = "";

    [Tooltip("Destroy this enemy after triggering a battle (so it's gone when returning).")]
    public bool destroyAfterBattle = true;

    // ── Internals ─────────────────────────────────────────────────────────────
    private NavMeshAgent agent;
    private bool         battleTriggered = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || battleTriggered) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool  canSeePlayer     = HasLineOfSight(distanceToPlayer);

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                if (canSeePlayer) currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                agent.SetDestination(player.position);
                if (distanceToPlayer <= attackRange)     currentState = EnemyState.Attack;
                if (!canSeePlayer && distanceToPlayer > detectRange) currentState = EnemyState.Patrol;
                break;

            case EnemyState.Attack:
                agent.SetDestination(transform.position); // stop moving
                TriggerBattle();
                break;
        }

        UpdateVisuals();
    }

    // ── Collision fallback — catches fast-moving players ─────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            TriggerBattle();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TriggerBattle();
    }

    // ── Battle trigger ────────────────────────────────────────────────────────

    private void TriggerBattle()
    {
        if (battleTriggered) return;
        battleTriggered = true;

        if (enemyRoster == null)
        {
            Debug.LogWarning($"[DungeonEnemyAI] {gameObject.name} has no EnemyRoster assigned — cannot start battle.");
            battleTriggered = false;
            return;
        }

        Debug.Log($"[DungeonEnemyAI] {gameObject.name} triggered a battle!");

        if (destroyAfterBattle)
            Destroy(gameObject);

        SceneTransitionManager.StartBattle(enemyRoster, encounterLevel, returnSpawnID);
    }

    // ── Patrol ────────────────────────────────────────────────────────────────

    private float patrolTimer = 0f;

    void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer > 0) return;

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 5f;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    // ── Line of sight ─────────────────────────────────────────────────────────

    bool HasLineOfSight(float distance)
    {
        if (distance > detectRange) return false;
        Vector3 rayStart  = transform.position + Vector3.up;
        Vector3 direction = (player.position - rayStart).normalized;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, direction, out hit, detectRange, detectionLayers))
            return hit.transform.CompareTag("Player");
        return false;
    }

    // ── Visuals ───────────────────────────────────────────────────────────────

    void UpdateVisuals()
    {
        if (player == null) return;
        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(-dirToPlayer);

        if (agent.velocity.magnitude > 0.1f)
        {
            Vector3 moveDir  = agent.velocity.normalized;
            Vector3 toPlayer = dirToPlayer.normalized;
            float   dot      = Vector3.Dot(moveDir, toPlayer);
            if (spriteRenderer != null)
                spriteRenderer.sprite = (dot < 0) ? backSprite : frontSprite;
        }
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position);
        }
    }
}
