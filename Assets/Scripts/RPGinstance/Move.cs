[System.Serializable]

public class Move
{
    public string moveName;
    public MoveType moveType;
    public ElementalType elementalType;
    public int baseDamage;
    public int baseHealing;
    public float accuracy;
    public int spCost;
    public string effectToApply;
    public float effectChance;
    public StatType buffStat;
    public float statModifier;
    public bool clearPartyTraits; // If true, removes permanent debuffs from all allies
}