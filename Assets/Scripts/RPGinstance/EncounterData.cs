using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Describes one enemy encounter — which enemy prefabs appear and at what level.
/// Set this up before loading the battle scene, e.g.:
///
///     EncounterManager.Prepare(new EncounterData {
///         enemies = new List<EnemySlot> {
///             new EnemySlot { prefab = incineratorPrefab, level = 3 },
///             new EnemySlot { prefab = incineratorPrefab, level = 3 },
///         }
///     });
///
/// Or use an EnemyRoster ScriptableObject and call Prepare(roster, level).
/// </summary>
[System.Serializable]
public class EncounterData
{
    public List<EnemySlot> enemies = new List<EnemySlot>();
}

[System.Serializable]
public class EnemySlot
{
    public GameObject prefab;   // Must have EnemyUnit + setup script (e.g. IncineratorShade)
    public int        level;    // Level to scale this enemy to
}
