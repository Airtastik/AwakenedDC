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
    public EnemyRoster enemyRoster;
    public int         encounterLevel    = 1;
    public bool        destroyAfterBattle = true;

    private NavMeshAgent agent;
    private bool         battleTriggered = false;
    private float        patrolTimer     = 0f;

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
                if (distanceToPlayer <= attackRange)                     currentState = EnemyState.Attack;
                if (!canSeePlayer && distanceToPlayer > detectRange)     currentState = EnemyState.Patrol;
                break;

            case EnemyState.Attack:
                agent.SetDestination(transform.position);
                TriggerBattle();
                break;
        }

        UpdateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TriggerBattle();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) TriggerBattle();
    }

    private void TriggerBattle()
    {
        if (battleTriggered) return;
        battleTriggered = true;

        if (enemyRoster == null)
        {
            Debug.LogWarning($"[DungeonEnemyAI] {gameObject.name} has no EnemyRoster assigned.");
            battleTriggered = false;
            return;
        }

        if (BattleStageManager.Instance == null)
        {
            Debug.LogError("[DungeonEnemyAI] No BattleStageManager found in scene!");
            return;
        }

        Debug.Log($"[DungeonEnemyAI] {gameObject.name} triggered a battle.");

        if (destroyAfterBattle) Destroy(gameObject);

        BattleStageManager.Instance.EnterBattle(enemyRoster, encounterLevel);
    }

    void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer > 0) return;
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * 5f + transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, 5f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    bool HasLineOfSight(float distance)
    {
        if (distance > detectRange) return false;
        Vector3    rayStart  = transform.position + Vector3.up;
        Vector3    direction = (player.position - rayStart).normalized;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, direction, out hit, detectRange, detectionLayers))
            return hit.transform.CompareTag("Player");
        return false;
    }

    void UpdateVisuals()
    {
        if (player == null) return;
        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(-dirToPlayer);

        if (spriteRenderer != null && agent.velocity.magnitude > 0.1f)
        {
            float dot = Vector3.Dot(agent.velocity.normalized, dirToPlayer.normalized);
            spriteRenderer.sprite = (dot < 0) ? backSprite : frontSprite;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (player != null)
            Gizmos.DrawLine(transform.position + Vector3.up, player.position);
    }
}
