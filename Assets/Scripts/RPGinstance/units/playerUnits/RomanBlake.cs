using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class RomanBlake : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level = 1; p.experience = 0; p.experienceToNextLevel = 100; p.statPointsAvailable = 0;

        p.unitName      = "Roman Blake";
        p.elementalType = ElementalType.Water;
        p.maxHealth     = 160;
        p.maxSP         = 5;
        p.attackP       = 12;
        p.defence       = 22;
        p.speed         = 7;
        p.criticalDMG   = 1.30f;
        p.criticalRate  = 0.05f;
        p.effectRes     = 0.40f;
        p.traitCannotBeHealed  = true; // Cannot be healed by party members

        p.moveList = new Move[]
        {
            // ── Regular Moves (0-2) ──────────────────────────────────────────
            new Move { moveName = "Cold Shoulder", moveType = MoveType.Attack, elementalType = ElementalType.Water,
                       baseDamage = 18, accuracy = 0.95f, spCost = 0,
                       effectToApply = "Soggy", effectChance = 0.40f },

            new Move { moveName = "Blank Stare",   moveType = MoveType.Attack, elementalType = ElementalType.Water,
                       baseDamage = 28, accuracy = 0.80f, spCost = 1 },

            new Move { moveName = "Repress",       moveType = MoveType.Heal,   elementalType = ElementalType.Water,
                       baseHealing = 40, accuracy = 1.0f, spCost = 2 },

            // ── Special Moves (3-5) ──────────────────────────────────────────
            // Monumental defence buff — he simply refuses to engage
            new Move { moveName = "Thick Skin",    moveType = MoveType.Buff,   elementalType = ElementalType.Water,
                       accuracy = 1.0f, spCost = 2,
                       buffStat = StatType.Defence, statModifier = 1.6f },

            // Channels suppressed emotion into a crushing wave
            new Move { moveName = "Undertow",      moveType = MoveType.Special, elementalType = ElementalType.Water,
                       baseDamage = 38, accuracy = 0.80f, spCost = 3,
                       effectToApply = "Soggy", effectChance = 0.75f,
                       buffStat = StatType.Speed, statModifier = 0.70f },

            // Total detachment — heals and removes all his status effects
            new Move { moveName = "Dissociation",  moveType = MoveType.Heal,   elementalType = ElementalType.Absurd,
                       baseHealing = 60, accuracy = 1.0f, spCost = 4 },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP:{p.maxHealth} SP:{p.maxSP}");
    }
}
