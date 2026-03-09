using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class DimitriQuestion : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level = 1; p.experience = 0; p.experienceToNextLevel = 100; p.statPointsAvailable = 0;

        p.unitName      = "Dimitri?????";
        p.elementalType = ElementalType.Absurd;
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

            // He opens a box. Something comes out. It hits them.
            new Move { moveName = "Open The Box",      moveType = MoveType.Attack,  elementalType = ElementalType.Absurd,
                       baseDamage = 20, accuracy = 1.00f, spCost = 0 },

            // He says something. Nobody knows what it means. Including him.
            new Move { moveName = "Say The Thing",     moveType = MoveType.Debuff,  elementalType = ElementalType.Absurd,
                       baseDamage = 5, accuracy = 1.00f, spCost = 1,
                       effectToApply = "Confusion", effectChance = 0.80f,
                       buffStat = StatType.AttackP, statModifier = 0.75f },

            // Heals. Probably. The mechanism is unclear.
            new Move { moveName = "It's Fine",         moveType = MoveType.Heal,    elementalType = ElementalType.Absurd,
                       baseHealing = 20, accuracy = 1.00f, spCost = 1 },

            // ── Special Moves (3-5) ──────────────────────────────────────────

            // He stops existing briefly. The enemy is very confused.
            new Move { moveName = "Temporary Absence", moveType = MoveType.Special, elementalType = ElementalType.Absurd,
                       baseDamage = 10, accuracy = 0.90f, spCost = 2,
                       effectToApply = "Confusion", effectChance = 0.95f,
                       buffStat = StatType.Defence, statModifier = 1.50f },

            // A box cutter that cuts something metaphysical. Very damaging.
            new Move { moveName = "Cut The Concept",   moveType = MoveType.Attack,  elementalType = ElementalType.Absurd,
                       baseDamage = 50, accuracy = 0.70f, spCost = 3 },

            // He becomes entirely unreadable. Stats? What stats.
            new Move { moveName = "??????????",        moveType = MoveType.Special, elementalType = ElementalType.Absurd,
                       baseDamage = 30, accuracy = 0.85f, spCost = 4,
                       effectToApply = "Confusion", effectChance = 1.00f,
                       buffStat = StatType.Defence, statModifier = 1.80f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP:{p.maxHealth} SP:{p.maxSP}");
    }
}
