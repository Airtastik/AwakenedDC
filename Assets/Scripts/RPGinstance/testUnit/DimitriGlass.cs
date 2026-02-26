using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class DimitriGlass : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level                 = 1;
        p.experience            = 0;
        p.experienceToNextLevel = 100;
        p.statPointsAvailable   = 0;

        p.unitName      = "Dimitri Glass";
        p.elementalType = ElementalType.Normal;
        p.maxHealth     = 110;
        p.attackP       = 18;
        p.defence       = 14;
        p.speed         = 10;
        p.criticalDMG   = 1.60f;
        p.criticalRate  = 0.15f;
        p.effectRes     = 0.20f;
        
        p.moveList = new Move[]
        {
            new Move { moveName = "Box Cutter", moveType = MoveType.Attack, elementalType = ElementalType.Normal, baseDamage  = 20, accuracy = 1.00f },
            new Move { moveName = "Lash Out",   moveType = MoveType.Attack, elementalType = ElementalType.Normal, baseDamage  = 32, accuracy = 0.80f },
            new Move { moveName = "Dissociate", moveType = MoveType.Heal,   elementalType = ElementalType.Normal, baseHealing = 20, accuracy = 1.00f },
            new Move { moveName = "Nihilism",   moveType = MoveType.Debuff, elementalType = ElementalType.Normal, baseDamage  = 10, accuracy = 0.90f, buffStat = StatType.Defence, statModifier = 0.60f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. Max HP: {p.maxHealth}");
    }
}