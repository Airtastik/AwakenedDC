using UnityEngine;

public class FinalBossTrigger : MonoBehaviour
{
    [Header("Dimitri Transformation")]
    public GameObject dimitriQuestionPrefab;

    [Header("Boss Battle")]
    public EnemyRoster bossRoster;
    public int         bossLevel = 10;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (bossRoster == null) { Debug.LogWarning("[FinalBoss] No EnemyRoster assigned!"); return; }

        hasTriggered = true;

        // Swap Dimitri Glass → Dimitri Question before the fight
        if (dimitriQuestionPrefab != null && PartyManager.Instance != null)
            PartyManager.Instance.ReplaceMember("Dimitri Glass", dimitriQuestionPrefab);

        // Start the boss battle
        if (BattleStageManager.Instance != null)
            BattleStageManager.Instance.EnterBattle(bossRoster, bossLevel);
        else
            Debug.LogError("[FinalBoss] No BattleStageManager in scene!");
    }
}
