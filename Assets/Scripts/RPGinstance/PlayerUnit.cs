using UnityEngine;

public class PlayerUnit : Unit
{
    [Header("Player Info")]
    public int level;
    public int experience;
    public int experienceToNextLevel;

    [Header("Progression")]
    public int statPointsAvailable;

    // ── Permanent Character Traits ────────────────────────────────────────────
    [Header("Permanent Traits")]
    [Tooltip("Cannot level up until the player changes floors.")]
    public bool traitLockedXP        = false;

    [Tooltip("Cannot directly attack enemies — attack and special moves with a target are blocked.")]
    public bool traitCannotAttack    = false;

    [Tooltip("Cannot be directly buffed by party members (items or self-buffs still work).")]
    public bool traitCannotBeBiuffed = false;

    [Tooltip("Cannot be directly healed by party members or items (self-heals still work).")]
    public bool traitCannotBeHealed  = false;

    // Displayed in the UI as a short line under the unit name
    public string TraitDescription()
    {
        if (traitLockedXP)        return "XP locked until next floor";
        if (traitCannotAttack)    return "Cannot directly attack";
        if (traitCannotBeBiuffed) return "Cannot be buffed by allies";
        if (traitCannotBeHealed)  return "Cannot be healed by allies";
        return "";
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void GainExperience(int amount)
    {
        if (traitLockedXP)
        {
            // XP still accumulates but level up is deferred until floor changes
            experience += amount;
            Debug.Log($"{unitName} gained {amount} XP but cannot level up yet. ({experience}/{experienceToNextLevel})");
            return;
        }

        experience += amount;
        Debug.Log($"{unitName} gained {amount} XP. ({experience}/{experienceToNextLevel})");
        if (experience >= experienceToNextLevel) LevelUp();
    }

    /// <summary>Called by the floor/world system when the player moves to a new floor.
    /// Unlocks XP and immediately processes any pending level-ups.</summary>
    public void OnFloorChanged()
    {
        if (!traitLockedXP) return;
        Debug.Log($"{unitName}'s XP lock lifted — processing pending level-ups.");
        while (experience >= experienceToNextLevel) LevelUp();
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
