[System.Serializable]
public class Move
{
    public string      moveName;
    public MoveType    moveType;
    public ElementalType elementalType;
    public int         baseDamage;
    public int         baseHealing;
    public float       accuracy;
    public int         spCost;         // SP consumed when this move is used (0 = free)
    public string      effectToApply;
    public float       effectChance;
    public StatType    buffStat;
    public float       statModifier;
}
