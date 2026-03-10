using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place empty GameObjects in the overworld at positions where the player
/// should be placed after returning from battle.
///
/// SETUP:
/// - Create empty GameObjects in the overworld, name them e.g. "SpawnAfterBattle1"
/// - Add this component to each one
/// - Set the Spawn ID to match what you put in BattleEncounterTrigger's Return Spawn ID
/// - Assign the Player reference (or it will find it by tag on Start)
/// </summary>
public class OverworldSpawnPoint : MonoBehaviour
{
    [Header("Identity")]
    public string spawnID = "default";

    [Header("Player")]
    public Transform player;

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Check if we should spawn here after returning from battle
        if (SceneTransitionManager.Instance == null) return;
        if (SceneTransitionManager.Instance.ReturnSpawnID == spawnID)
        {
            PlacePlayer();
        }
    }

    public void PlacePlayer()
    {
        if (player == null) { Debug.LogWarning($"[SpawnPoint:{spawnID}] No player found."); return; }
        player.position = transform.position;
        player.rotation = transform.rotation;
        Debug.Log($"[SpawnPoint:{spawnID}] Player placed at return point.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
    }
}
