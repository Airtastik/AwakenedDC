using UnityEngine;

// Final boss. Absurd type. The first manikin. It has been in the hospital
// longer than the hospital has existed. It does not know what it is.
// Neither does the hospital. Neither do you.
[RequireComponent(typeof(EnemyUnit))]
public class TheOriginalManikin : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "The Original";
        e.elementalType    = ElementalType.Absurd;
        e.maxHealth        = 480;
        e.attackP          = 35;
        e.defence          = 25;
        e.speed            = 13;
        e.criticalDMG      = 2.20f;
        e.criticalRate     = 0.25f;
        e.effectRes        = 0.60f;
        e.tier             = 6;
        e.behaviour        = EnemyBehaviour.Random; // Its logic cannot be predicted
        e.experienceReward = 500;
        e.goldReward       = 250;

        e.moveList = new Move[]
        {
            // It reaches out. Something about the angle is completely wrong.
            new Move { moveName = "Incorrect Reach",    moveType = MoveType.Attack,  elementalType = ElementalType.Absurd,
                       baseDamage = 30, accuracy = 0.90f,
                       effectToApply = "Confusion", effectChance = 0.40f },

            // It disassembles and reassembles. Stats shift. Nobody is sure which direction.
            new Move { moveName = "Self Revision",      moveType = MoveType.Special, elementalType = ElementalType.Absurd,
                       baseDamage = 20, accuracy = 0.85f,
                       buffStat = StatType.AttackP, statModifier = 1.50f,
                       effectToApply = "Confusion", effectChance = 0.60f },

            // The room reorganises itself around it. The party feels displaced.
            new Move { moveName = "Ward Shift",         moveType = MoveType.Debuff,  elementalType = ElementalType.Absurd,
                       baseDamage = 15, accuracy = 0.90f,
                       buffStat = StatType.Defence, statModifier = 0.55f,
                       effectToApply = "Confusion", effectChance = 0.75f },

            // It stands absolutely still. It heals. This should not work.
            new Move { moveName = "Idle Protocol",      moveType = MoveType.Heal,    elementalType = ElementalType.Absurd,
                       baseHealing = 60, accuracy = 1.00f },

            // Every joint bends the wrong way simultaneously. Full party attack.
            new Move { moveName = "Full Articulation",  moveType = MoveType.Special, elementalType = ElementalType.Absurd,
                       baseDamage = 45, accuracy = 0.80f,
                       effectToApply = "Confusion", effectChance = 0.85f,
                       buffStat = StatType.Defence, statModifier = 1.40f },

            // It opens its featureless face. There is something inside. You wish there wasn't.
            new Move { moveName = "Open The Face",      moveType = MoveType.Special, elementalType = ElementalType.Absurd,
                       baseDamage = 55, accuracy = 0.65f,
                       effectToApply = "Confusion", effectChance = 1.00f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}
