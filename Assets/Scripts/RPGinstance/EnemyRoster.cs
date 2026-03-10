using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines a named encounter for use in the Inspector.
/// Create via Assets → Create → RPG → Enemy Roster.
///
/// Drag enemy prefabs into the slots and set their base levels.
/// The overworld can then call EncounterManager.Prepare(roster, scaledLevel)
/// to override all enemy levels at once for difficulty scaling.
/// </summary>
[CreateAssetMenu(fileName = "NewRoster", menuName = "RPG/Enemy Roster")]
public class EnemyRoster : ScriptableObject
{
    [Tooltip("Name shown in logs and UI, e.g. 'Crematorium Wing 1'")]
    public string encounterName = "Unnamed Encounter";

    [Tooltip("Each slot = one enemy in the battle")]
    public List<EnemySlot> slots = new List<EnemySlot>();

    /// <summary>Convert this roster into an EncounterData, optionally overriding all levels.</summary>
    public EncounterData ToEncounterData(int overrideLevel = -1)
    {
        var data = new EncounterData();
        foreach (var slot in slots)
        {
            data.enemies.Add(new EnemySlot
            {
                prefab = slot.prefab,
                level  = overrideLevel > 0 ? overrideLevel : slot.level
            });
        }
        return data;
    }
}
