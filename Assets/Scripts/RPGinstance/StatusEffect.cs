[System.Serializable]
public class StatusEffect
{
    public string effectName;       // e.g. "Burn", "Confusion", "Soggy"
    public ElementalType sourceElement;
    public int duration;            // Turns remaining
    public int damagePerTurn;       // Damage dealt at start of each turn (0 if none)
    public StatType affectedStat;
    public float statModifier;      // Multiplier applied to affected stat (1.0 = no change)
}
