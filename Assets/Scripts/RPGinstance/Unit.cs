using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [Header("Identity")]
    public string unitName;
    public ElementalType elementalType;

    [Header("Stats")]
    public int maxHealth;
    public int currentHealth;
    public int maxSP;
    public int currentSP;
    public int attackP;
    public int defence;
    public int speed;
    public float criticalDMG;
    public float criticalRate;
    public float effectRes;

    [Header("Moves & Effects")]
    public Move[] moveList;
    public List<StatusEffect> currentEffects = new List<StatusEffect>();

    public bool IsAlive => currentHealth > 0;

    protected virtual void Awake()
    {
        currentEffects = new List<StatusEffect>();
    }

    /// <summary>
    /// Called by BattleSystem once stats are confirmed set.
    /// Players start with 5 SP; enemies get 0 (they don't use SP).
    /// </summary>
    public void InitHealth()
    {
        if (maxHealth > 0)
            currentHealth = maxHealth;
        else
            Debug.LogWarning($"[Unit] {unitName} has maxHealth of 0!");

        currentSP = maxSP;
    }

    // ── SP ───────────────────────────────────────────────────────────────────

    public bool HasSP(int cost) => currentSP >= cost;

    public void SpendSP(int amount)
    {
        currentSP = Mathf.Max(currentSP - amount, 0);
        Debug.Log($"{unitName} spent {amount} SP. SP: {currentSP}/{maxSP}");
    }

    public void RestoreSP(int amount)
    {
        currentSP = Mathf.Min(currentSP + amount, maxSP);
        Debug.Log($"{unitName} restored {amount} SP. SP: {currentSP}/{maxSP}");
    }

    // ── Status Effects ───────────────────────────────────────────────────────

    public void AddEffect(StatusEffect effect)
    {
        if (Random.value < effectRes) { Debug.Log($"{unitName} resisted {effect.effectName}!"); return; }
        currentEffects.Add(effect);
        Debug.Log($"{unitName} is now afflicted with {effect.effectName}!");
    }

    public void TickEffects()
    {
        for (int i = currentEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = currentEffects[i];
            if (e.damagePerTurn > 0) { TakeDamage(e.damagePerTurn); Debug.Log($"{unitName} took {e.damagePerTurn} from {e.effectName}."); }
            e.duration--;
            if (e.duration <= 0) { Debug.Log($"{e.effectName} wore off on {unitName}."); currentEffects.RemoveAt(i); }
        }
    }

    // ── Combat ───────────────────────────────────────────────────────────────

    public virtual void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        Debug.Log($"{unitName} took {amount} damage. HP: {currentHealth}/{maxHealth}");
    }

    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{unitName} healed {amount} HP. HP: {currentHealth}/{maxHealth}");
    }

    public int CalculateDamage(Move move, Unit target)
    {
        float elementalMult = ElementalChart.GetMultiplier(move.elementalType, target.elementalType);
        bool  isCrit        = Random.value < criticalRate;
        float critMult      = isCrit ? criticalDMG : 1.0f;
        float def           = Mathf.Max(target.defence, 1);
        int   damage        = Mathf.RoundToInt((attackP + move.baseDamage) * elementalMult * critMult / def);
        if (isCrit) Debug.Log("Critical hit!");
        return Mathf.Max(damage, 1);
    }
}
