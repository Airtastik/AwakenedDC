using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class RomanBlake : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level                 = 1;
        p.experience            = 0;
        p.experienceToNextLevel = 100;
        p.statPointsAvailable   = 0;

        p.unitName      = "Roman Blake";
        p.elementalType = ElementalType.Water; // "Water" works well for someone emotionally fluid or cold/ice-like
        p.maxHealth     = 160;
        p.attackP       = 12;
        p.defence       = 22;
        p.speed         = 7;
        p.criticalDMG   = 1.30f;
        p.criticalRate  = 0.05f;
        p.effectRes     = 0.40f;
        
        p.moveList = new Move[]
        {
            // A brutally dismissive gesture that dampens the enemy's spirit
            new Move { moveName = "Cold Shoulder",  moveType = MoveType.Attack, elementalType = ElementalType.Water,  baseDamage = 18, accuracy = 0.95f, effectToApply = "Soggy",   effectChance = 0.40f },
            
            // He just stares at them until they feel profoundly uncomfortable
            new Move { moveName = "Blank Stare",    moveType = MoveType.Attack, elementalType = ElementalType.Water,  baseDamage = 28, accuracy = 0.80f },
            
            // He simply refuses to engage with the reality of the situation, massively boosting his defense
            new Move { moveName = "Thick Skin",     moveType = MoveType.Buff,   elementalType = ElementalType.Water,  accuracy  = 1.0f,                                                                    buffStat = StatType.Defence, statModifier = 1.6f },
            
            // Pushing down his feelings to keep going, restoring a large chunk of HP
            new Move { moveName = "Repress",        moveType = MoveType.Heal,   elementalType = ElementalType.Water,  baseHealing = 40, accuracy = 1.0f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. Max HP: {p.maxHealth}");
    }
}