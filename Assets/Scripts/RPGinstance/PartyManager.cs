using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton that persists between scenes and owns the canonical party state.
///
/// SETUP (two options):
///
///   A) Use the default test party (quickest):
///      - Create an empty GameObject, add PartyManager.
///      - Leave Member Prefabs empty — the four default characters are created
///        automatically from scratch using DefaultParty.
///
///   B) Use your own prefabs:
///      - Create GameObjects with PlayerUnit + character script (e.g. DimitriGlass).
///      - Drag them into Member Prefabs in the Inspector.
///      - PartyManager will snapshot their stats on startup.
///
/// BattleSystem reads from PartyManager automatically and assigns the spawned
/// GameObjects to its playerPartyObjects list before battle begins.
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [Header("Party prefabs — leave empty to use built-in test party")]
    public List<GameObject> memberPrefabs = new List<GameObject>();

    [Header("Spawn Points — assign Player1..Player4 transforms from battle scene")]
    public List<Transform> spawnPoints = new List<Transform>();

    // Persistent data records
    private List<MemberData> records = new List<MemberData>();

    // Live GameObjects currently spawned for battle
    private List<GameObject> spawnedMembers = new List<GameObject>();

    public delegate void PartyUpdated(List<MemberData> party);
    public static event PartyUpdated OnPartyUpdated;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (memberPrefabs.Count > 0)
            InitialiseFromPrefabs();
        else
            Debug.LogWarning("[PartyManager] No member prefabs assigned! Add prefabs to the Member Prefabs list in the Inspector.");
    }

    // ── Option B: initialise from Inspector prefabs ───────────────────────────

    private void InitialiseFromPrefabs()
    {
        records.Clear();
        foreach (GameObject prefab in memberPrefabs)
        {
            if (prefab == null) continue;

            // Instantiate active so Awake() fires and character scripts set stats
            GameObject temp = Instantiate(prefab);
            temp.SetActive(true);

            PlayerUnit pu = temp.GetComponent<PlayerUnit>();
            if (pu == null)
            {
                Debug.LogWarning($"[PartyManager] {prefab.name} has no PlayerUnit — skipped.");
                Destroy(temp);
                continue;
            }

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
    /// Instantiates the actual prefab (preserving sprite, billboard, etc.) and
    /// applies the saved MemberData stats on top. Returns the spawned GameObjects.
    /// BattleSystem calls this in LateStart().
    /// </summary>
    public List<GameObject> SpawnParty()
    {
        foreach (var obj in spawnedMembers)
            if (obj != null) Destroy(obj);
        spawnedMembers.Clear();

        for (int i = 0; i < records.Count; i++)
        {
            MemberData data = records[i];

            // Find the matching prefab by index — falls back to a blank GO if none found
            GameObject prefab = (i < memberPrefabs.Count) ? memberPrefabs[i] : null;

            GameObject obj;
            if (prefab != null)
            {
                // Instantiate the real prefab so sprite, billboard, etc. are preserved
                obj = Instantiate(prefab);
                obj.name = $"Party_{data.unitName}";
            }
            else
            {
                // Fallback: plain GameObject with just a PlayerUnit
                Debug.LogWarning($"[PartyManager] No prefab for slot {i} ({data.unitName}) — spawning blank.");
                obj = new GameObject($"Party_{data.unitName}");
                obj.AddComponent<PlayerUnit>();
            }

            // Apply saved stats (HP, SP, XP, level etc.) on top of prefab defaults
            var pu = obj.GetComponent<PlayerUnit>();
            if (pu != null)
                data.ApplyTo(pu);
            else
                Debug.LogWarning($"[PartyManager] Spawned {obj.name} has no PlayerUnit.");

            // Position at spawn point if assigned
            if (spawnPoints != null && i < spawnPoints.Count && spawnPoints[i] != null)
            {
                obj.transform.position = spawnPoints[i].position;
                obj.transform.rotation = spawnPoints[i].rotation;
            }

            spawnedMembers.Add(obj);
            Debug.Log($"[PartyManager] Spawned {data.unitName} from prefab. HP:{data.currentHealth}/{data.maxHealth}  SP:{data.currentSP}/{data.maxSP}");
        }

        return spawnedMembers;
    }

    /// <summary>
    /// Saves live PlayerUnit state back into records after a battle.
    /// HP is clamped to 1 minimum so nobody is dead in the overworld.
    /// </summary>
    public void SaveParty(List<PlayerUnit> liveParty)
    {
        for (int i = 0; i < records.Count && i < liveParty.Count; i++)
        {
            records[i].CaptureFrom(liveParty[i]);
            if (records[i].currentHealth <= 0)
                records[i].currentHealth = 1;
        }

        OnPartyUpdated?.Invoke(records);
        Debug.Log("[PartyManager] Party state saved.");
    }

    // ── Accessors ─────────────────────────────────────────────────────────────

    public List<MemberData> GetRecords()              => records;
    public MemberData       GetRecord(string name)    => records.FirstOrDefault(r => r.unitName == name);
    public int              PartySize                 => records.Count;

    public void FullRestore()
    {
        foreach (var r in records) { r.currentHealth = r.maxHealth; r.currentSP = r.maxSP; }
        OnPartyUpdated?.Invoke(records);
        Debug.Log("[PartyManager] Party fully restored.");
    }

    public void RestoreHP(int amount)
    {
        foreach (var r in records)
            if (r.currentHealth > 0)
                r.currentHealth = Mathf.Min(r.currentHealth + amount, r.maxHealth);
        OnPartyUpdated?.Invoke(records);
    }
}
