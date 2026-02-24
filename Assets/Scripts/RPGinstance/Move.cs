[System.Serializable]
public class Move
{
    public string moveName;
    public MoveType moveType;
    public ElementalType elementalType;
    public int baseDamage;
    public int baseHealing;
    public float accuracy;          // 0.0 - 1.0 chance the move lands
    public string effectToApply;    // Name of a StatusEffect this move may inflict
    public float effectChance;      // 0.0 - 1.0 chance of applying the effect
    public StatType buffStat;       // Stat targeted by Buff/Debuff moves
    public float statModifier;      // Multiplier to apply to the targeted stat
}
