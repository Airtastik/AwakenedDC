using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class SylvaVane : MonoBehaviour
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

        p.moveList = new Move[]
        {
            // Free attack — instinctive flinch, no SP needed
            new Move { moveName = "Startled Flinch",   moveType = MoveType.Attack, elementalType = ElementalType.Nature,
                       baseDamage = 14, accuracy = 0.95f, spCost = 0,
                       effectToApply = "Poison", effectChance = 0.40f },

            // Major heal — costs 3 SP (her big move)
            new Move { moveName = "Brown Paper Bag",   moveType = MoveType.Heal,   elementalType = ElementalType.Nature,
                       baseHealing = 50, accuracy = 1.0f, spCost = 3 },

            // Minor heal — costs 1 SP
            new Move { moveName = "Offer Peppermint",  moveType = MoveType.Heal,   elementalType = ElementalType.Nature,
                       baseHealing = 25, accuracy = 1.0f, spCost = 1 },

            // Attack debuff — costs 2 SP
            new Move { moveName = "Projected Anxiety", moveType = MoveType.Debuff, elementalType = ElementalType.Nature,
                       baseDamage = 5, accuracy = 0.85f, spCost = 2,
                       effectToApply = "Poison", effectChance = 0.95f,
                       buffStat = StatType.AttackP, statModifier = 0.70f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP: {p.maxHealth}  SP: {p.maxSP}");
    }
}
