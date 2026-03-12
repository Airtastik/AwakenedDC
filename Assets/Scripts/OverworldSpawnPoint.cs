using UnityEngine;

/// <summary>
/// Marks a position in the overworld where the player should stand.
/// Used as a starting spawn point — place one in the scene and the player
/// will be moved here on Start if it is tagged as the default spawn.
///
/// No longer depends on SceneTransitionManager since the battle now happens
/// in the same scene via BattleStageManager. The player's overworld position
/// is saved and restored automatically by BattleStageManager.
/// </summary>
public class OverworldSpawnPoint : MonoBehaviour
{
    [Header("Identity")]
    public string spawnID = "default";

    [Header("Player")]
    public Transform player;

    [Tooltip("If true, the player is placed here as soon as the scene starts.")]
    public bool spawnOnStart = false;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (spawnOnStart) PlacePlayer();
    }

    public void PlacePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning($"[SpawnPoint:{spawnID}] No player found.");
            return;
        }

        // CharacterController must be disabled briefly to teleport without fighting physics
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = transform.position;
        player.rotation = transform.rotation;
        if (cc != null) cc.enabled = true;

        Debug.Log($"[SpawnPoint:{spawnID}] Player placed.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
    }
}
