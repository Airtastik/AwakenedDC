using UnityEngine;

/// <summary>
/// Stationary trigger zone that starts a battle when the player walks in.
/// Uses BattleStageManager instead of SceneTransitionManager since the
/// battle stage now lives in the same scene.
/// </summary>
public class BattleEncounterTrigger : MonoBehaviour
{
    [Header("Encounter")]
    public EnemyRoster enemyRoster;
    public int         encounterLevel   = 1;

    [Header("Options")]
    public bool destroyAfterTrigger = false;
    public bool triggerOnce         = true;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        if (enemyRoster == null) { Debug.LogWarning("[EncounterTrigger] No EnemyRoster assigned!"); return; }

        if (BattleStageManager.Instance == null)
        {
            Debug.LogError("[EncounterTrigger] No BattleStageManager in scene!");
            return;
        }

        hasTriggered = true;
        if (destroyAfterTrigger) Destroy(gameObject);

        BattleStageManager.Instance.EnterBattle(enemyRoster, encounterLevel);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
            Gizmos.DrawCube(transform.position, box.size);
        else if (col is SphereCollider sphere)
            Gizmos.DrawSphere(transform.position, sphere.radius);
    }
}
