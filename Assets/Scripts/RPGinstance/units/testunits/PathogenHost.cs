using UnityEngine;

[RequireComponent(typeof(EnemyUnit))]
public class PathogenHost : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "Pathogen Host";
        e.elementalType    = ElementalType.Nature;
        e.maxHealth        = 160;
        e.attackP          = 18;
        e.defence          = 16;
        e.speed            = 9;
        e.criticalDMG      = 1.60f;
        e.criticalRate     = 0.12f;
        e.effectRes        = 0.25f;
        e.tier             = 2;
        e.behaviour        = EnemyBehaviour.Disruptor;
        e.experienceReward = 100;
        e.goldReward       = 45;
        
        e.moveList = new Move[]
        {
            new Move { moveName = "Infectious Touch", moveType = MoveType.Attack, elementalType = ElementalType.Nature, baseDamage = 20, accuracy = 0.90f, effectToApply = "Poison",    effectChance = 0.60f },
            new Move { moveName = "Viral Load",       moveType = MoveType.Debuff, elementalType = ElementalType.Nature, baseDamage = 8,  accuracy = 0.85f, effectToApply = "Poison",    effectChance = 0.90f, buffStat = StatType.AttackP,  statModifier = 0.70f },
            new Move { moveName = "Quarantine",       moveType = MoveType.Debuff, elementalType = ElementalType.Normal, baseDamage = 6,  accuracy = 0.90f,                                                      buffStat = StatType.Speed,    statModifier = 0.60f },
            new Move { moveName = "Cell Division",    moveType = MoveType.Heal,   elementalType = ElementalType.Nature, baseHealing = 30, accuracy = 1.0f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}