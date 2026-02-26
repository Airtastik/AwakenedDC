using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class SylvaVane : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level                 = 1;
        p.experience            = 0;
        p.experienceToNextLevel = 100;
        p.statPointsAvailable   = 0;

        p.unitName      = "Sylva Vane";
        p.elementalType = ElementalType.Nature;
        p.maxHealth     = 90;
        p.attackP       = 10;
        p.defence       = 12;
        p.speed         = 11;
        p.criticalDMG   = 1.40f;
        p.criticalRate  = 0.08f;
        p.effectRes     = 0.35f;
        
        p.moveList = new Move[]
        {
            // A panicked, accidental attack when an enemy gets too close
            new Move { moveName = "Startled Flinch",   moveType = MoveType.Attack, elementalType = ElementalType.Nature, baseDamage  = 14, accuracy = 0.95f, effectToApply = "Poison", effectChance = 0.40f },
            
            // A major heal—she panics and forces someone to breathe into a paper bag
            new Move { moveName = "Brown Paper Bag",   moveType = MoveType.Heal,   elementalType = ElementalType.Nature, baseHealing = 50, accuracy = 1.0f },
            
            // A minor heal—timidly offering a comfort item
            new Move { moveName = "Offer Peppermint",  moveType = MoveType.Heal,   elementalType = ElementalType.Nature, baseHealing = 25, accuracy = 1.0f },
            
            // Her sheer radiating panic makes the enemy second-guess themselves, lowering their attack
            new Move { moveName = "Projected Anxiety", moveType = MoveType.Debuff, elementalType = ElementalType.Nature, baseDamage  = 5,  accuracy = 0.85f, effectToApply = "Poison", effectChance = 0.95f, buffStat = StatType.AttackP, statModifier = 0.70f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. Max HP: {p.maxHealth}");
    }
}