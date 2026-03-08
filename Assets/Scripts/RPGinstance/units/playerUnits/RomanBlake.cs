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

        p.moveList = new Move[]
        {
            // Free attack — always available
            new Move { moveName = "Cold Shoulder", moveType = MoveType.Attack, elementalType = ElementalType.Water,
                       baseDamage = 18, accuracy = 0.95f, spCost = 0,
                       effectToApply = "Soggy", effectChance = 0.40f },

            // Slow heavy hit — costs 2 SP
            new Move { moveName = "Blank Stare",   moveType = MoveType.Attack, elementalType = ElementalType.Water,
                       baseDamage = 28, accuracy = 0.80f, spCost = 2 },

            // Big defence buff — costs 2 SP
            new Move { moveName = "Thick Skin",    moveType = MoveType.Buff,   elementalType = ElementalType.Water,
                       accuracy = 1.0f, spCost = 2,
                       buffStat = StatType.Defence, statModifier = 1.6f },

            // Large self-heal — costs 3 SP (powerful, so costly)
            new Move { moveName = "Repress",       moveType = MoveType.Heal,   elementalType = ElementalType.Water,
                       baseHealing = 40, accuracy = 1.0f, spCost = 3 },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP: {p.maxHealth}  SP: {p.maxSP}");
    }
}
