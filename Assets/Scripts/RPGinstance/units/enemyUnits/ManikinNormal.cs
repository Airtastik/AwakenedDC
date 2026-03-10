using UnityEngine;

[RequireComponent(typeof(EnemyUnit))]
public class ManikinNormal : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit e = GetComponent<EnemyUnit>();

        e.unitName         = "Ward Manikin";
        e.elementalType    = ElementalType.Normal;
        e.maxHealth        = 60;
        e.attackP          = 10;
        e.defence          = 8;
        e.speed            = 8;
        e.criticalDMG      = 1.30f;
        e.criticalRate     = 0.08f;
        e.effectRes        = 0.10f;
        e.tier             = 1;
        e.behaviour        = EnemyBehaviour.Aggressive;
        e.experienceReward = 25;
        e.goldReward       = 8;

        e.moveList = new Move[]
        {
            // Knocks into the target with a stiff jointed arm. Basic.
            new Move { moveName = "Hollow Strike",   moveType = MoveType.Attack, elementalType = ElementalType.Normal,
                       baseDamage = 12, accuracy = 1.00f },

            // The head rotates. Wrong direction. Too far.
            new Move { moveName = "Wrong Turn",      moveType = MoveType.Debuff, elementalType = ElementalType.Normal,
                       baseDamage = 6, accuracy = 0.90f,
                       buffStat = StatType.Speed, statModifier = 0.75f },

            // It stands completely still. You can't tell if it's waiting or broken.
            new Move { moveName = "Hold Position",   moveType = MoveType.Buff,   elementalType = ElementalType.Normal,
                       accuracy = 1.00f, buffStat = StatType.Defence, statModifier = 1.30f },
        };

        Debug.Log($"[EnemySetup] {e.unitName} configured. Max HP: {e.maxHealth}");
    }
}
