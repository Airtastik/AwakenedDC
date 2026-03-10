using System.Collections.Generic;

/// <summary>
/// Plain serialisable snapshot of one party member's state.
/// Lives inside PartyManager — no MonoBehaviour, no Unity dependency.
/// </summary>
[System.Serializable]
public class MemberData
{
    // ── Identity ──────────────────────────────────────────────────────────────
    public string        unitName;
    public ElementalType elementalType;

    // ── Stats ─────────────────────────────────────────────────────────────────
    public int   maxHealth;
    public int   currentHealth;
    public int   maxSP;
    public int   currentSP;
    public int   attackP;
    public int   defence;
    public int   speed;
    public float criticalDMG;
    public float criticalRate;
    public float effectRes;

    // ── Progression ───────────────────────────────────────────────────────────
    public int level;
    public int experience;
    public int experienceToNextLevel;
    public int statPointsAvailable;

    // ── Moves ─────────────────────────────────────────────────────────────────
    public Move[] moveList;

    // ── Status effects are cleared between battles ────────────────────────────
    // (intentional — battles are discrete encounters)

    /// <summary>Write a live PlayerUnit's current state into this record.</summary>
    public void CaptureFrom(PlayerUnit pu)
    {
        unitName      = pu.unitName;
        elementalType = pu.elementalType;

        maxHealth     = pu.maxHealth;
        currentHealth = pu.currentHealth;
        maxSP         = pu.maxSP;
        currentSP     = pu.currentSP;
        attackP       = pu.attackP;
        defence       = pu.defence;
        speed         = pu.speed;
        criticalDMG   = pu.criticalDMG;
        criticalRate  = pu.criticalRate;
        effectRes     = pu.effectRes;

        level                 = pu.level;
        experience            = pu.experience;
        experienceToNextLevel = pu.experienceToNextLevel;
        statPointsAvailable   = pu.statPointsAvailable;

        moveList = pu.moveList;
    }

    /// <summary>Apply this record's values onto a live PlayerUnit.</summary>
    public void ApplyTo(PlayerUnit pu)
    {
        pu.unitName      = unitName;
        pu.elementalType = elementalType;

        pu.maxHealth     = maxHealth;
        pu.currentHealth = currentHealth;
        pu.maxSP         = maxSP;
        pu.currentSP     = currentSP;
        pu.attackP       = attackP;
        pu.defence       = defence;
        pu.speed         = speed;
        pu.criticalDMG   = criticalDMG;
        pu.criticalRate  = criticalRate;
        pu.effectRes     = effectRes;

        pu.level                 = level;
        pu.experience            = experience;
        pu.experienceToNextLevel = experienceToNextLevel;
        pu.statPointsAvailable   = statPointsAvailable;

        pu.moveList        = moveList;
        pu.currentEffects  = new System.Collections.Generic.List<StatusEffect>();
    }
}
