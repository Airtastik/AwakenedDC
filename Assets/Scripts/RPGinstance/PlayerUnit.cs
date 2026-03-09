// by Tristan Hall - 2026-03-09
using UnityEngine;

public class PlayerUnit : Unit
{
    [Header("Player Info")]
    public int level;
    public int experience;
    public int experienceToNextLevel;

    [Header("Progression")]
    public int statPointsAvailable;

    protected override void Awake()
    {
        base.Awake();
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        Debug.Log($"{unitName} gained {amount} XP. ({experience}/{experienceToNextLevel})");

        if (experience >= experienceToNextLevel)
            LevelUp();
    }

    private void LevelUp()
    {
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
        statPointsAvailable++;

        Debug.Log($"{unitName} leveled up to level {level}!");
    }

    // Call this from your UI when the player spends a stat point
    public void SpendStatPoint(StatType stat)
    {
        if (statPointsAvailable <= 0)
        {
            Debug.Log("No stat points available.");
            return;
        }

        switch (stat)
        {
            case StatType.Health:  maxHealth += 10; currentHealth += 10; break;
            case StatType.AttackP: attackP   += 2;  break;
            case StatType.Defence: defence   += 2;  break;
            case StatType.Speed:   speed     += 1;  break;
            default: Debug.Log("Stat cannot be increased this way."); return;
        }

        statPointsAvailable--;
        Debug.Log($"{unitName} increased {stat}!");
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        // Hook: trigger UI health bar update, death screen, etc.
    }
}
