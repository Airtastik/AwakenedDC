using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton that persists between scenes and owns the canonical party state.
///
/// SETUP:
///   1. Create an empty GameObject in your first scene, add PartyManager to it.
///   2. Drag your four character setup scripts (DimitriGlass, MaeveJohnson, etc.)
///      into the Inspector list "Member Prefabs". These are the GameObjects that
///      have PlayerUnit + character script on them.
///   3. That's it. BattleSystem reads from PartyManager automatically.
///
/// FLOW:
///   - On first load, PartyManager snapshots stats from the member prefabs.
///   - Before a battle, BattleTransfer.EnemyRoster is set by the overworld.
///   - BattleSystem.LateStart() calls PartyManager.SpawnParty() to get live
///     PlayerUnit GameObjects, then calls SaveParty() after battle ends.
///   - Status effects are cleared between battles; HP/SP/XP carry over.
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [Header("Starting roster — assign GameObjects with PlayerUnit + character scripts")]
    public List<GameObject> memberPrefabs = new List<GameObject>();

    // Persistent data records — one per party member
    private List<MemberData> records = new List<MemberData>();

    // Live GameObjects spawned for the current battle (kept under this transform)
    private List<GameObject> spawnedMembers = new List<GameObject>();

    // ── Events ────────────────────────────────────────────────────────────────
    public delegate void PartyUpdated(List<MemberData> party);
    public static event PartyUpdated OnPartyUpdated;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitialiseFromPrefabs();
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>
    /// First-time setup: instantiate each prefab briefly, capture its stats
    /// into a MemberData record, then destroy the temporary instance.
    /// </summary>
    private void InitialiseFromPrefabs()
    {
        records.Clear();
        foreach (GameObject prefab in memberPrefabs)
        {
            if (prefab == null) continue;

            // Instantiate off-screen so Awake() runs and stats get set
            GameObject temp = Instantiate(prefab);
            temp.SetActive(false);

            PlayerUnit pu = temp.GetComponent<PlayerUnit>();
            if (pu == null)
            {
                Debug.LogWarning($"[PartyManager] {prefab.name} has no PlayerUnit — skipped.");
                Destroy(temp);
                continue;
            }

            // InitHealth so currentHealth/SP are set correctly
            pu.InitHealth();

            var data = new MemberData();
            data.CaptureFrom(pu);
            records.Add(data);

            Destroy(temp);
            Debug.Log($"[PartyManager] Registered {data.unitName}  HP:{data.maxHealth}  SP:{data.maxSP}");
        }

        OnPartyUpdated?.Invoke(records);
    }

    // ── Battle interface ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates live PlayerUnit GameObjects from current records.
    /// Called by BattleSystem before battle starts.
    /// Returns the list of spawned GameObjects to hand to BattleSystem.
    /// </summary>
    public List<GameObject> SpawnParty()
    {
        // Clean up any previous spawn
        foreach (var obj in spawnedMembers)
            if (obj != null) Destroy(obj);
        spawnedMembers.Clear();

        foreach (MemberData data in records)
        {
            var obj = new GameObject($"Party_{data.unitName}");
            obj.transform.SetParent(transform);

            // Add PlayerUnit and apply saved data
            var pu = obj.AddComponent<PlayerUnit>();
            data.ApplyTo(pu);

            spawnedMembers.Add(obj);
        }

        Debug.Log($"[PartyManager] Spawned {spawnedMembers.Count} party members for battle.");
        return spawnedMembers;
    }

    /// <summary>
    /// Reads current state back from live PlayerUnits into records.
    /// Called by BattleSystem after battle ends (win or loss).
    /// </summary>
    public void SaveParty(List<PlayerUnit> liveParty)
    {
        for (int i = 0; i < records.Count && i < liveParty.Count; i++)
        {
            records[i].CaptureFrom(liveParty[i]);
            // Clamp HP to 1 on loss so they aren't dead outside battle
            if (records[i].currentHealth <= 0)
                records[i].currentHealth = 1;
        }

        OnPartyUpdated?.Invoke(records);
        Debug.Log("[PartyManager] Party state saved.");
    }

    // ── Accessors ─────────────────────────────────────────────────────────────

    public List<MemberData> GetRecords() => records;

    public MemberData GetRecord(string unitName) =>
        records.FirstOrDefault(r => r.unitName == unitName);

    public int PartySize => records.Count;

    /// <summary>Restore full HP and SP for all members (e.g. rest point).</summary>
    public void FullRestore()
    {
        foreach (var r in records)
        {
            r.currentHealth = r.maxHealth;
            r.currentSP     = r.maxSP;
        }
        OnPartyUpdated?.Invoke(records);
        Debug.Log("[PartyManager] Party fully restored.");
    }

    /// <summary>Restore a partial amount of HP to all living members.</summary>
    public void RestoreHP(int amount)
    {
        foreach (var r in records)
            if (r.currentHealth > 0)
                r.currentHealth = Mathf.Min(r.currentHealth + amount, r.maxHealth);
        OnPartyUpdated?.Invoke(records);
    }
}
