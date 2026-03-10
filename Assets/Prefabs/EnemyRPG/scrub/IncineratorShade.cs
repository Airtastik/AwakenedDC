using UnityEngine;

[RequireComponent(typeof(EnemyUnit))]
public class IncineratorShade : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "Incinerator Shade";
        e.elementalType    = ElementalType.Fire;
        e.maxHealth        = 55;
        e.attackP          = 12;
        e.defence          = 5;
        e.speed            = 14;
        e.criticalDMG      = 1.40f;
        e.criticalRate     = 0.10f;
        e.effectRes        = 0.05f;
        e.tier             = 1;
        e.behaviour        = EnemyBehaviour.Random;
        e.experienceReward = 30;
        e.goldReward       = 10;
        
        e.moveList = new Move[]
        {
            new Move { moveName = "Biohazard Burn", moveType = MoveType.Attack, elementalType = ElementalType.Fire,   baseDamage = 14, accuracy = 0.95f, effectToApply = "Burn", effectChance = 0.25f },
            new Move { moveName = "Sterilize",      moveType = MoveType.Attack, elementalType = ElementalType.Fire,   baseDamage = 22, accuracy = 0.80f },
            new Move { moveName = "Fever Dream",    moveType = MoveType.Special,elementalType = ElementalType.Absurd, baseDamage = 16, accuracy = 0.75f, effectToApply = "Confusion", effectChance = 0.40f },
            new Move { moveName = "Scalpel Swipe",  moveType = MoveType.Attack, elementalType = ElementalType.Normal, baseDamage = 10, accuracy = 1.00f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}