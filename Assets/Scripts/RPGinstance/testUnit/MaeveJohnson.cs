using UnityEngine;

[RequireComponent(typeof(PlayerUnit))]
public class MaeveJohnson : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit p = GetComponent<PlayerUnit>();
        p.level                 = 1;
        p.experience            = 0;
        p.experienceToNextLevel = 100;
        p.statPointsAvailable   = 0;

        p.unitName      = "Maeve Johnson";
        p.elementalType = ElementalType.Fire;
        p.maxHealth     = 80;
        p.attackP       = 28;
        p.defence       = 6;
        p.speed         = 14;
        p.criticalDMG   = 2.20f;
        p.criticalRate  = 0.30f;
        p.effectRes     = 0.10f;
        
        p.moveList = new Move[]
        {
            // A sharp, aggressive response to any perceived pity
            new Move { moveName = "Scathing Rebuttal", moveType = MoveType.Attack, elementalType = ElementalType.Fire,   baseDamage = 22, accuracy = 0.95f, effectToApply = "Burn",    effectChance = 0.35f },
            
            // She pushes herself past her limits to prove a point, causing massive damage
            new Move { moveName = "Burn Bridges",      moveType = MoveType.Attack, elementalType = ElementalType.Fire,   baseDamage = 40, accuracy = 0.70f },
            
            // A look so intensely judgmental it makes the target sweat and lowers their defenses
            new Move { moveName = "Condescending Glare",moveType = MoveType.Debuff, elementalType = ElementalType.Fire,  baseDamage = 8,  accuracy = 0.90f, effectToApply = "Burn",    effectChance = 0.90f, buffStat = StatType.Defence,  statModifier = 0.75f },
            
            // She absolutely refuses to fail, hyping herself up
            new Move { moveName = "Stubborn Pride",    moveType = MoveType.Buff,   elementalType = ElementalType.Normal, accuracy  = 1.0f,                                                                    buffStat = StatType.CriticalRate, statModifier = 1.5f },
        };

        Debug.Log($"[PlayerSetup] {p.unitName} configured. Max HP: {p.maxHealth}");
    }
}