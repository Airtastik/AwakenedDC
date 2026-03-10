using UnityEngine;

// Mid-boss. Nature type. Found in the hospital greenhouse ward — a manikin
// that has been left so long the building grew into it.
[RequireComponent(typeof(EnemyUnit))]
public class RootedManikin : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "The Rooted One";
        e.elementalType    = ElementalType.Nature;
        e.maxHealth        = 240;
        e.attackP          = 22;
        e.defence          = 28;
        e.speed            = 5;
        e.criticalDMG      = 1.70f;
        e.criticalRate     = 0.15f;
        e.effectRes        = 0.35f;
        e.tier             = 3;
        e.behaviour        = EnemyBehaviour.Disruptor;
        e.experienceReward = 200;
        e.goldReward       = 90;

        e.moveList = new Move[]
        {
            // Vines rip through its joints and lash outward.
            new Move { moveName = "Overgrowth",      moveType = MoveType.Attack,  elementalType = ElementalType.Nature,
                       baseDamage = 24, accuracy = 0.90f, effectToApply = "Poison", effectChance = 0.45f },

            // Roots erupt from the floor and hold an ally in place.
            new Move { moveName = "Root Hold",       moveType = MoveType.Debuff,  elementalType = ElementalType.Nature,
                       baseDamage = 10, accuracy = 0.85f,
                       buffStat = StatType.Speed, statModifier = 0.40f },

            // The vines pull nutrients from a poisoned target back into the manikin.
            new Move { moveName = "Siphon Green",    moveType = MoveType.Special, elementalType = ElementalType.Nature,
                       baseDamage = 18, accuracy = 0.80f, effectToApply = "Poison", effectChance = 0.70f,
                       buffStat = StatType.Defence, statModifier = 1.20f },

            // It stops moving entirely. Moss spreads. Its defence becomes absurd.
            new Move { moveName = "Calcify",         moveType = MoveType.Buff,    elementalType = ElementalType.Nature,
                       accuracy = 1.00f, buffStat = StatType.Defence, statModifier = 2.00f },

            // Every crack in its wood weeps black sap simultaneously.
            new Move { moveName = "Black Sap Burst", moveType = MoveType.Special, elementalType = ElementalType.Nature,
                       baseDamage = 30, accuracy = 0.75f, effectToApply = "Poison", effectChance = 0.90f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}
