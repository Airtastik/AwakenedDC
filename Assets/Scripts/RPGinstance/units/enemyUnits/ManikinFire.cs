using UnityEngine;

[RequireComponent(typeof(EnemyUnit))]
public class ManikinFire : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "Charred Manikin";
        e.elementalType    = ElementalType.Fire;
        e.maxHealth        = 55;
        e.attackP          = 14;
        e.defence          = 6;
        e.speed            = 11;
        e.criticalDMG      = 1.50f;
        e.criticalRate     = 0.12f;
        e.effectRes        = 0.10f;
        e.tier             = 1;
        e.behaviour        = EnemyBehaviour.Aggressive;
        e.experienceReward = 30;
        e.goldReward       = 10;

        e.moveList = new Move[]
        {
            // Embers leak from the cracks when it strikes.
            new Move { moveName = "Ember Joint",     moveType = MoveType.Attack, elementalType = ElementalType.Fire,
                       baseDamage = 14, accuracy = 0.95f, effectToApply = "Burn", effectChance = 0.30f },

            // It opens its chest. Something burning is inside.
            new Move { moveName = "Open Cavity",     moveType = MoveType.Attack, elementalType = ElementalType.Fire,
                       baseDamage = 22, accuracy = 0.75f, effectToApply = "Burn", effectChance = 0.50f },

            // The cracks glow brighter. It moves faster.
            new Move { moveName = "Stoke",           moveType = MoveType.Buff,   elementalType = ElementalType.Fire,
                       accuracy = 1.00f, buffStat = StatType.Speed, statModifier = 1.40f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}
