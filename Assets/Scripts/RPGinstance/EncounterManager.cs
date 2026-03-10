using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Non-persistent singleton that holds the current encounter definition and
/// spawns scaled enemy GameObjects for BattleSystem.
///
/// Does NOT use DontDestroyOnLoad — it lives only for the battle scene.
/// The overworld sets up the encounter before loading the scene.
///
/// LEVEL SCALING RULES (applied on top of base stats from setup scripts):
///   Per level above 1, each stat grows by its ScalingRate:
///     HP        +8% per level
///     Attack    +6% per level
///     Defence   +5% per level
///     Speed     +3% per level
///     XP reward +10% per level
///     Gold      +8% per level
///   Crit/effectRes are not scaled — those are fixed by design.
/// </summary>
public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    // ── Scaling knobs — tweak in Inspector or leave as defaults ──────────────
    [Header("Level Scaling Rates (multiplier added per level above 1)")]
    [Range(0f, 0.20f)] public float hpScale      = 0.08f;
    [Range(0f, 0.20f)] public float attackScale   = 0.06f;
    [Range(0f, 0.20f)] public float defenceScale  = 0.05f;
    [Range(0f, 0.10f)] public float speedScale    = 0.03f;
    [Range(0f, 0.20f)] public float xpScale       = 0.10f;
    [Range(0f, 0.20f)] public float goldScale     = 0.08f;

    [Header("Spawn Points — assign Enemy1..Enemy4 transforms from battle scene")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Test Encounter — drag enemy prefabs here to test without an overworld")]
    public List<GameObject> testEnemyPrefabs = new List<GameObject>();
    public int testLevel = 1;

    // ── Pending encounter set before scene load ───────────────────────────────
    private static EncounterData pendingEncounter;

    // ── Spawned GameObjects for this battle ───────────────────────────────────
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // Intentionally NOT DontDestroyOnLoad — resets each scene
    }

    // ── Static setup (called from overworld before LoadScene) ─────────────────

    /// <summary>Queue an encounter to be used when the battle scene loads.</summary>
    public static void Prepare(EncounterData data)
    {
        pendingEncounter = data;
        Debug.Log($"[EncounterManager] Encounter prepared: {data.enemies.Count} enemies.");
    }

    /// <summary>Queue an encounter from a ScriptableObject roster.</summary>
    public static void Prepare(EnemyRoster roster, int overrideLevel = -1)
    {
        Prepare(roster.ToEncounterData(overrideLevel));
    }

    /// <summary>True if an encounter has been queued and is ready to spawn.</summary>
    public static bool HasPendingEncounter => 
        (pendingEncounter != null && pendingEncounter.enemies.Count > 0) ||
        (Instance != null && Instance.testEnemyPrefabs != null && Instance.testEnemyPrefabs.Count > 0);

    // ── Spawning ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates, configures, and level-scales all enemies for this battle.
    /// Called by BattleSystem.LateStart().
    /// Returns list of GameObjects ready to be added to enemyParty.
    /// </summary>
    public List<GameObject> SpawnEnemies()
    {
        foreach (var obj in spawnedEnemies)
            if (obj != null) Destroy(obj);
        spawnedEnemies.Clear();

        if (pendingEncounter == null || pendingEncounter.enemies.Count == 0)
        {
            // No prepared encounter — try test prefabs assigned directly in Inspector
            if (testEnemyPrefabs == null || testEnemyPrefabs.Count == 0)
            {
                Debug.LogWarning("[EncounterManager] SpawnEnemies called but no encounter or test prefabs are set.");
                return spawnedEnemies;
            }

            Debug.Log("[EncounterManager] Using test prefabs for this encounter.");
            foreach (GameObject prefab in testEnemyPrefabs)
            {
                if (prefab == null) continue;

                GameObject obj = Instantiate(prefab);
                obj.transform.SetParent(transform);

                EnemyUnit eu = obj.GetComponent<EnemyUnit>();
                if (eu == null) { Debug.LogWarning($"[EncounterManager] {prefab.name} has no EnemyUnit — skipped."); Destroy(obj); continue; }

                ApplyLevelScaling(eu, testLevel);
                eu.InitHealth();

                int index = spawnedEnemies.Count;
                if (spawnPoints != null && index < spawnPoints.Count && spawnPoints[index] != null)
                {
                    obj.transform.position = spawnPoints[index].position;
                    obj.transform.rotation = spawnPoints[index].rotation;
                }

                spawnedEnemies.Add(obj);
                Debug.Log($"[EncounterManager] Test-spawned {eu.unitName} at level {testLevel}.");
            }

            return spawnedEnemies;
        }

        foreach (EnemySlot slot in pendingEncounter.enemies)
        {
            if (slot.prefab == null) { Debug.LogWarning("[EncounterManager] Null prefab in slot — skipped."); continue; }

            GameObject obj = Instantiate(slot.prefab);
            obj.transform.SetParent(transform);

            EnemyUnit eu = obj.GetComponent<EnemyUnit>();
            if (eu == null) { Debug.LogWarning($"[EncounterManager] {slot.prefab.name} has no EnemyUnit — skipped."); Destroy(obj); continue; }

            // Stats are set by the character Awake() script (e.g. CrematoriumDirector).
            // We apply level scaling on top of those base values.
            ApplyLevelScaling(eu, slot.level);
            eu.InitHealth();

            // Position at spawn point if one is assigned for this slot
            int index = spawnedEnemies.Count;
            if (spawnPoints != null && index < spawnPoints.Count && spawnPoints[index] != null)
            {
                obj.transform.position = spawnPoints[index].position;
                obj.transform.rotation = spawnPoints[index].rotation;
            }

            spawnedEnemies.Add(obj);
            Debug.Log($"[EncounterManager] Spawned {eu.unitName} at level {slot.level}. HP:{eu.maxHealth} ATK:{eu.attackP}");
        }

        // Clear so next battle must call Prepare() again
        pendingEncounter = null;
        return spawnedEnemies;
    }

    // ── Level scaling ─────────────────────────────────────────────────────────

    private void ApplyLevelScaling(EnemyUnit eu, int level)
    {
        if (level <= 1) return; // Level 1 = base stats, no scaling needed

        float mult = 1f + (level - 1);   // levels above 1

        eu.maxHealth        = Scale(eu.maxHealth,        hpScale,     mult);
        eu.attackP          = Scale(eu.attackP,          attackScale, mult);
        eu.defence          = Scale(eu.defence,          defenceScale,mult);
        eu.speed            = Scale(eu.speed,            speedScale,  mult);
        eu.experienceReward = Scale(eu.experienceReward, xpScale,     mult);
        eu.goldReward       = Scale(eu.goldReward,       goldScale,   mult);
    }

    private static int Scale(int baseValue, float rate, float levelsAboveOne)
        => Mathf.RoundToInt(baseValue * (1f + rate * levelsAboveOne));

    // ── Accessors ─────────────────────────────────────────────────────────────

    public int EnemyCount => spawnedEnemies.Count;
}
