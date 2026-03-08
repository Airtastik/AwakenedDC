using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class MaeveJohnson : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level = 1; p.experience = 0; p.experienceToNextLevel = 100; p.statPointsAvailable = 0;

        p.unitName      = "Maeve Johnson";
        p.elementalType = ElementalType.Fire;
        p.maxHealth     = 80;
        p.maxSP         = 5;
        p.attackP       = 28;
        p.defence       = 6;
        p.speed         = 14;
        p.criticalDMG   = 2.20f;
        p.criticalRate  = 0.30f;
        p.effectRes     = 0.10f;

        p.moveList = new Move[]
        {
            // Free attack — always available
            new Move { moveName = "Scathing Rebuttal",  moveType = MoveType.Attack, elementalType = ElementalType.Fire,
                       baseDamage = 22, accuracy = 0.95f, spCost = 0,
                       effectToApply = "Burn", effectChance = 0.35f },

            // Nuclear option — costs 3 SP
            new Move { moveName = "Burn Bridges",       moveType = MoveType.Attack, elementalType = ElementalType.Fire,
                       baseDamage = 40, accuracy = 0.70f, spCost = 3 },

            // Debuff + Burn — costs 2 SP
            new Move { moveName = "Condescending Glare",moveType = MoveType.Debuff, elementalType = ElementalType.Fire,
                       baseDamage = 8, accuracy = 0.90f, spCost = 2,
                       effectToApply = "Burn", effectChance = 0.90f,
                       buffStat = StatType.Defence, statModifier = 0.75f },

            // Self crit-rate buff — costs 1 SP
            new Move { moveName = "Stubborn Pride",     moveType = MoveType.Buff,   elementalType = ElementalType.Normal,
                       accuracy = 1.0f, spCost = 1,
                       buffStat = StatType.CriticalRate, statModifier = 1.5f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. HP: {p.maxHealth}  SP: {p.maxSP}");
    }
}
