using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class DimitriGlass : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level = 1; p.experience = 0; p.experienceToNextLevel = 100; p.statPointsAvailable = 0;

        p.unitName      = "Dimitri Glass";
        p.elementalType = ElementalType.Normal;
        p.maxHealth     = 110;
        p.maxSP         = 5;
        p.attackP       = 18;
        p.defence       = 14;
        p.speed         = 10;
        p.criticalDMG   = 1.60f;
        p.criticalRate  = 0.15f;
        p.effectRes     = 0.20f;

        p.moveList = new Move[]
        {
            // ── Regular Moves (0-2) ──────────────────────────────────────────
            new Move { moveName = "Box Cutter",   moveType = MoveType.Attack, elementalType = ElementalType.Normal,
                       baseDamage = 20, accuracy = 1.00f, spCost = 0 },

            new Move { moveName = "Lash Out",     moveType = MoveType.Attack, elementalType = ElementalType.Normal,
                       baseDamage = 32, accuracy = 0.80f, spCost = 1 },

            new Move { moveName = "Dissociate",   moveType = MoveType.Heal,   elementalType = ElementalType.Normal,
                       baseHealing = 20, accuracy = 1.00f, spCost = 1 },

            // ── Special Moves (3-5) ──────────────────────────────────────────
            // Drains enemy defence with nihilistic indifference
            new Move { moveName = "Nihilism",     moveType = MoveType.Debuff, elementalType = ElementalType.Normal,
                       baseDamage = 10, accuracy = 0.90f, spCost = 2,
                       buffStat = StatType.Defence, statModifier = 0.60f },

            // Explosive burst — he stops caring about consequences
            new Move { moveName = "Glass Shard",  moveType = MoveType.Attack, elementalType = ElementalType.Normal,
                       baseDamage = 45, accuracy = 0.75f, spCost = 3 },

            // Absolute emotional shutdown — buffs own defence massively
            new Move { moveName = "Void Out",     moveType = MoveType.Buff,   elementalType = ElementalType.Absurd,
                       accuracy = 1.00f, spCost = 2,
                       buffStat = StatType.Defence, statModifier = 1.70f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP:{p.maxHealth} SP:{p.maxSP}");
    }
}
