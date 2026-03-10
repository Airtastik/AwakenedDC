using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class CathereneBlanc : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level = 1; p.experience = 0; p.experienceToNextLevel = 100; p.statPointsAvailable = 0;

        p.unitName      = "Catherene Blanc";
        p.elementalType = ElementalType.Fire;
        p.maxHealth     = 80;
        p.maxSP         = 5;
        p.attackP       = 28;
        p.defence       = 6;
        p.speed         = 14;
        p.criticalDMG   = 2.20f;
        p.criticalRate  = 0.30f;
        p.effectRes     = 0.10f;
        p.traitCannotBeBiuffed = true; // Cannot be buffed by party members

        p.moveList = new Move[]
        {
            // ── Regular Moves (0-2) ──────────────────────────────────────────
            new Move { moveName = "Scathing Rebuttal",   moveType = MoveType.Attack, elementalType = ElementalType.Fire,
                       baseDamage = 22, accuracy = 0.95f, spCost = 0,
                       effectToApply = "Burn", effectChance = 0.35f },

            new Move { moveName = "Condescending Glare", moveType = MoveType.Debuff, elementalType = ElementalType.Fire,
                       baseDamage = 8, accuracy = 0.90f, spCost = 1,
                       effectToApply = "Burn", effectChance = 0.90f,
                       buffStat = StatType.Defence, statModifier = 0.75f },

            new Move { moveName = "Stubborn Pride",      moveType = MoveType.Buff,   elementalType = ElementalType.Normal,
                       accuracy = 1.0f, spCost = 1,
                       buffStat = StatType.CriticalRate, statModifier = 1.5f },

            // ── Special Moves (3-5) ──────────────────────────────────────────
            // She pushes past her limits — nuclear damage
            new Move { moveName = "Burn Bridges",        moveType = MoveType.Attack, elementalType = ElementalType.Fire,
                       baseDamage = 40, accuracy = 0.70f, spCost = 3 },

            // Scorched earth — hits all enemies simultaneously
            new Move { moveName = "Ignition Point",      moveType = MoveType.Special, elementalType = ElementalType.Fire,
                       baseDamage = 28, accuracy = 0.85f, spCost = 3,
                       effectToApply = "Burn", effectChance = 0.60f },

            // Rage-fuelled self destruct — massive damage, costs most of her SP
            new Move { moveName = "Last Resort",         moveType = MoveType.Attack, elementalType = ElementalType.Absurd,
                       baseDamage = 60, accuracy = 0.60f, spCost = 4 },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP:{p.maxHealth} SP:{p.maxSP}");
    }
}
