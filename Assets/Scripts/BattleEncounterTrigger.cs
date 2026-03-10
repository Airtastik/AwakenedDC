using UnityEngine;

/// <summary>
/// Place this on any GameObject in the overworld to start a battle
/// when the player walks into it.
///
/// SETUP:
/// - Add a Collider (Box or Sphere) to this GameObject, set Is Trigger = true
/// - Assign an EnemyRoster ScriptableObject in the Inspector
/// - Set the Encounter Level
/// - Tag your player GameObject as "Player"
/// - Optionally set a Spawn Point ID to return to after the battle
/// </summary>
public class BattleEncounterTrigger : MonoBehaviour
{
    [Header("Encounter")]
    public EnemyRoster enemyRoster;
    public int encounterLevel = 1;

    [Header("Return Point")]
    [Tooltip("The name of a SpawnPoint GameObject in the overworld to return to after battle.")]
    public string returnSpawnID = "";

    [Header("Options")]
    [Tooltip("Destroy this trigger after use so it cannot be re-entered.")]
    public bool destroyAfterTrigger = false;

    [Tooltip("Only trigger once even if not destroyed (re-enters are ignored until scene reload).")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        if (enemyRoster == null) { Debug.LogWarning("[EncounterTrigger] No EnemyRoster assigned!"); return; }

        hasTriggered = true;
        Debug.Log($"[EncounterTrigger] Battle triggered! Roster: {enemyRoster.name}  Level: {encounterLevel}");

        if (destroyAfterTrigger) Destroy(gameObject);

        SceneTransitionManager.StartBattle(enemyRoster, encounterLevel, returnSpawnID);
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
