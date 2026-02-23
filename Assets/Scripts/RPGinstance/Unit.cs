using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public string unitName;
    public string elementalType;
    public int health;
    public int attackP;
    public int defence;
    public int speed;
    public float CriticalDMG;    // This is a percentage increase in damage when a critical hit occurs
    public float Criticalrate;   // This is the percentage chance that an attack will be a critical hit
    public float effectRes;      // This is the percentage chance to resist negative status effects

    public Move[] moveList;                      // Fixed array of moves this unit can use
    public List<StatusEffect> currentEffects;    // Dynamic list of active status effects on this unit

    void Awake()
    {
        currentEffects = new List<StatusEffect>();
    }
}

[System.Serializable]
public class Move
{
    public string moveName;
    public string moveType;       // e.g. "Attack", "Heal", "Buff", "Debuff"
    public string elementalType;
    public int baseDamage;
    public float accuracy;        // Percentage chance the move will land
    public string effectToApply;  // Name of a status effect this move may inflict
    public float effectChance;    // Percentage chance of applying the effect
}

[System.Serializable]
public class StatusEffect
{
    public string effectName;     // e.g. "Burn", "Poison", "Stun"
    public int duration;          // Turns remaining
    public int damagePerTurn;     // Damage dealt at the start of each turn (0 if none)
    public float statModifier;    // Multiplier applied to a stat (1.0 = no change)
    public string affectedStat;   // Which stat the modifier applies to e.g. "attackP", "speed"
}