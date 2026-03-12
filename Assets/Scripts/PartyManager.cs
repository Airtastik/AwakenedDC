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
    public MemberData       GetRecord(int index)      => (index >= 0 && index < records.Count) ? records[index] : null;
    public int              PartySize                 => records.Count;

    // ── Party composition ─────────────────────────────────────────────────────

    /// <summary>
    /// Add a new member to the party mid-game (e.g. story event, NPC joins).
    /// Pass the prefab that has PlayerUnit + character script on it.
    /// Does nothing if the party is already at max size or member is already in.
    /// </summary>
    public void AddMember(GameObject prefab, int maxPartySize = 4)
    {
        if (prefab == null) { Debug.LogWarning("[PartyManager] AddMember: null prefab."); return; }
        if (records.Count >= maxPartySize) { Debug.LogWarning($"[PartyManager] Party is full ({maxPartySize} members)."); return; }

        // Check not already in party
        GameObject temp = Instantiate(prefab);
        temp.SetActive(true);
        PlayerUnit pu = temp.GetComponent<PlayerUnit>();

        if (pu == null)
        {
            Debug.LogWarning($"[PartyManager] {prefab.name} has no PlayerUnit.");
            Destroy(temp);
            return;
        }

        // Check by name — don't add duplicates
        if (records.Exists(r => r.unitName == pu.unitName))
        {
            Debug.LogWarning($"[PartyManager] {pu.unitName} is already in the party.");
            Destroy(temp);
            return;
        }

        pu.InitHealth();
        var data = new MemberData();
        data.CaptureFrom(pu);
        records.Add(data);
        memberPrefabs.Add(prefab);
        Destroy(temp);

        OnPartyUpdated?.Invoke(records);
        Debug.Log($"[PartyManager] {data.unitName} joined the party! Party size: {records.Count}");
    }

    /// <summary>
    /// Remove a member from the party by name (e.g. they leave after a story beat).
    /// </summary>
    public void RemoveMember(string memberName)
    {
        int index = records.FindIndex(r => r.unitName == memberName);
        if (index < 0) { Debug.LogWarning($"[PartyManager] {memberName} not found in party."); return; }

        records.RemoveAt(index);
        if (index < memberPrefabs.Count) memberPrefabs.RemoveAt(index);

        OnPartyUpdated?.Invoke(records);
        Debug.Log($"[PartyManager] {memberName} left the party. Party size: {records.Count}");
    }

    /// <summary>
    /// Replace one member with another (e.g. a character transforms or is substituted).
    /// </summary>
    public void ReplaceMember(string memberName, GameObject newPrefab)
    {
        int index = records.FindIndex(r => r.unitName == memberName);
        if (index < 0) { Debug.LogWarning($"[PartyManager] {memberName} not found."); return; }

        GameObject temp = Instantiate(newPrefab);
        temp.SetActive(true);
        PlayerUnit pu = temp.GetComponent<PlayerUnit>();
        if (pu == null) { Destroy(temp); return; }

        pu.InitHealth();
        var data = new MemberData();
        data.CaptureFrom(pu);
        records[index] = data;
        if (index < memberPrefabs.Count) memberPrefabs[index] = newPrefab;
        Destroy(temp);

        OnPartyUpdated?.Invoke(records);
        Debug.Log($"[PartyManager] {memberName} replaced with {data.unitName}.");
    }

    /// <summary>Swap two party members by index. Used by the party menu to reorder.</summary>
    public void SwapMembers(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0 || indexA >= records.Count || indexB >= records.Count) return;
        var temp       = records[indexA];
        records[indexA] = records[indexB];
        records[indexB] = temp;

        // Also swap prefabs so SpawnParty uses correct prefab per slot
        if (memberPrefabs.Count > Mathf.Max(indexA, indexB))
        {
            var tempPrefab          = memberPrefabs[indexA];
            memberPrefabs[indexA]   = memberPrefabs[indexB];
            memberPrefabs[indexB]   = tempPrefab;
        }

        OnPartyUpdated?.Invoke(records);
        Debug.Log($"[PartyManager] Swapped slots {indexA} and {indexB}.");
    }

    // ── Overworld HP/SP helpers ───────────────────────────────────────────────

    /// <summary>Heal one member by name. Clamps to maxHealth.</summary>
    public void HealMember(string memberName, int amount)
    {
        var r = GetRecord(memberName);
        if (r == null) return;
        r.currentHealth = Mathf.Min(r.currentHealth + amount, r.maxHealth);
        OnPartyUpdated?.Invoke(records);
    }

    /// <summary>Restore SP to one member by name. Clamps to maxSP.</summary>
    public void RestoreSPMember(string memberName, int amount)
    {
        var r = GetRecord(memberName);
        if (r == null) return;
        r.currentSP = Mathf.Min(r.currentSP + amount, r.maxSP);
        OnPartyUpdated?.Invoke(records);
    }

    /// <summary>Restore all SP to the whole party.</summary>
    public void RestoreAllSP()
    {
        foreach (var r in records) r.currentSP = r.maxSP;
        OnPartyUpdated?.Invoke(records);
    }

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

    // ── Floor change (unlocks Dimitri's XP) ──────────────────────────────────

    /// <summary>
    /// Call this when the player moves to a new floor.
    /// Unlocks Dimitri's XP and processes any pending level-ups.
    /// </summary>
    public void NotifyFloorChanged()
    {
        // We can't call OnFloorChanged() directly since records are MemberData not live units.
        // Instead we process any pending level-ups in the record directly.
        foreach (var r in records)
        {
            if (!r.traitLockedXP) continue;
            int safetyLimit = 20;
            while (r.experience >= r.experienceToNextLevel && safetyLimit-- > 0)
            {
                r.level++;
                r.experience -= r.experienceToNextLevel;
                r.experienceToNextLevel = Mathf.RoundToInt(r.experienceToNextLevel * 1.2f);
                r.statPointsAvailable++;
                Debug.Log($"[PartyManager] {r.unitName} levelled up to {r.level} after floor change!");
            }
        }
        OnPartyUpdated?.Invoke(records);
    }

    // ── Stat point spending (overworld menu) ──────────────────────────────────

    /// <summary>Spend a stat point for a member from the overworld menu.</summary>
    public bool SpendStatPoint(string memberName, StatType stat)
    {
        var r = GetRecord(memberName);
        if (r == null || r.statPointsAvailable <= 0) return false;

        switch (stat)
        {
            case StatType.Health:  r.maxHealth += 10; r.currentHealth += 10; break;
            case StatType.AttackP: r.attackP   += 2;  break;
            case StatType.Defence: r.defence   += 2;  break;
            case StatType.Speed:   r.speed     += 1;  break;
            default: return false;
        }

        r.statPointsAvailable--;
        OnPartyUpdated?.Invoke(records);
        Debug.Log($"[PartyManager] {memberName} spent a stat point on {stat}.");
        return true;
    }
}
