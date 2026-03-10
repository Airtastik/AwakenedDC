using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class SilviaVane : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level = 1; p.experience = 0; p.experienceToNextLevel = 100; p.statPointsAvailable = 0;

        p.unitName      = "Sylva Vane";
        p.elementalType = ElementalType.Nature;
        p.maxHealth     = 90;
        p.maxSP         = 5;
        p.attackP       = 10;
        p.defence       = 12;
        p.speed         = 11;
        p.criticalDMG   = 1.40f;
        p.criticalRate  = 0.08f;
        p.effectRes     = 0.35f;
        p.traitCannotAttack  = true;   // Cannot directly attack enemies

        p.moveList = new Move[]
        {
            // ── Regular Moves (0-2) ──────────────────────────────────────────
            new Move { moveName = "Startled Flinch",   moveType = MoveType.Attack, elementalType = ElementalType.Nature,
                       baseDamage = 14, accuracy = 0.95f, spCost = 0,
                       effectToApply = "Poison", effectChance = 0.40f },

            new Move { moveName = "Offer Peppermint",  moveType = MoveType.Heal,   elementalType = ElementalType.Nature,
                       baseHealing = 25, accuracy = 1.0f, spCost = 1 },

            new Move { moveName = "Projected Anxiety", moveType = MoveType.Debuff, elementalType = ElementalType.Nature,
                       baseDamage = 5, accuracy = 0.85f, spCost = 1,
                       effectToApply = "Poison", effectChance = 0.95f,
                       buffStat = StatType.AttackP, statModifier = 0.70f },

            // ── Special Moves (3-5) ──────────────────────────────────────────
            // Big emergency heal — panic breathing into a paper bag
            new Move { moveName = "Brown Paper Bag",   moveType = MoveType.Heal,   elementalType = ElementalType.Nature,
                       baseHealing = 50, accuracy = 1.0f, spCost = 3 },

            // Her anxiety becomes contagious — poisons and slows the whole enemy party
            new Move { moveName = "Mass Hysteria",     moveType = MoveType.Special, elementalType = ElementalType.Nature,
                       baseDamage = 8, accuracy = 0.80f, spCost = 3,
                       effectToApply = "Poison", effectChance = 0.85f,
                       buffStat = StatType.Speed, statModifier = 0.65f },

            // A burst of accidental healing energy — restores HP to the whole party
            new Move { moveName = "Nervous Energy",    moveType = MoveType.Special, elementalType = ElementalType.Absurd,
                       baseHealing = 20, accuracy = 1.0f, spCost = 4 },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP:{p.maxHealth} SP:{p.maxSP}");
    }
}
