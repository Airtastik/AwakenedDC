using UnityEngine;

[RequireComponent(typeof(EnemyUnit))]
public class ManikinWater : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "Waterlogged Manikin";
        e.elementalType    = ElementalType.Water;
        e.maxHealth        = 70;
        e.attackP          = 9;
        e.defence          = 12;
        e.speed            = 6;
        e.criticalDMG      = 1.30f;
        e.criticalRate     = 0.08f;
        e.effectRes        = 0.15f;
        e.tier             = 1;
        e.behaviour        = EnemyBehaviour.Defensive;
        e.experienceReward = 28;
        e.goldReward       = 9;

        e.moveList = new Move[]
        {
            // Black water weeps from its joints onto the target.
            new Move { moveName = "Seeping Wound",   moveType = MoveType.Attack, elementalType = ElementalType.Water,
                       baseDamage = 10, accuracy = 0.95f, effectToApply = "Poison", effectChance = 0.35f },

            // It exhales something damp. The air feels wrong.
            new Move { moveName = "Damp Exhale",     moveType = MoveType.Debuff, elementalType = ElementalType.Water,
                       baseDamage = 5, accuracy = 0.90f,
                       buffStat = StatType.AttackP, statModifier = 0.80f },

            // It absorbs the dark water back into itself.
            new Move { moveName = "Reabsorb",        moveType = MoveType.Heal,   elementalType = ElementalType.Water,
                       baseHealing = 18, accuracy = 1.00f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}
