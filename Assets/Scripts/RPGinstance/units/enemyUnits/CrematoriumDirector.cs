using UnityEngine;

[RequireComponent(typeof(EnemyUnit))]
public class CrematoriumDirector : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "Crematorium Director";
        e.elementalType    = ElementalType.Fire;
        e.maxHealth        = 320;
        e.attackP          = 30;
        e.defence          = 20;
        e.speed            = 11;
        e.criticalDMG      = 2.00f;
        e.criticalRate     = 0.20f;
        e.effectRes        = 0.40f;
        e.tier             = 5;
        e.behaviour        = EnemyBehaviour.Aggressive;
        e.experienceReward = 300;
        e.goldReward       = 150;
        
        e.moveList = new Move[]
        {
            new Move { moveName = "Fever Pitch",    moveType = MoveType.Attack, elementalType = ElementalType.Fire,   baseDamage = 28, accuracy = 0.95f, effectToApply = "Burn",      effectChance = 0.50f },
            new Move { moveName = "Cremate",        moveType = MoveType.Attack, elementalType = ElementalType.Fire,   baseDamage = 50, accuracy = 0.70f },
            new Move { moveName = "Lead Apron",     moveType = MoveType.Buff,   elementalType = ElementalType.Fire,   accuracy  = 1.0f,                                                buffStat = StatType.Defence,      statModifier = 1.80f },
            new Move { moveName = "Malpractice",    moveType = MoveType.Debuff, elementalType = ElementalType.Fire,   baseDamage = 18, accuracy = 0.85f, effectToApply = "Burn",      effectChance = 0.70f, buffStat = StatType.Defence, statModifier = 0.60f },
            new Move { moveName = "Code Black",     moveType = MoveType.Special,elementalType = ElementalType.Absurd, baseDamage = 38, accuracy = 0.75f, effectToApply = "Confusion", effectChance = 0.55f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}