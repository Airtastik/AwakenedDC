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
    public int attackP;
    public int defence;
    public int speed;
    public float criticalDMG;       // Damage multiplier on a crit e.g. 1.5 = 150%
    public float criticalRate;      // 0.0 - 1.0 chance to land a crit
    public float effectRes;         // 0.0 - 1.0 chance to resist a status effect

    [Header("Moves & Effects")]
    public Move[] moveList;
    public List<StatusEffect> currentEffects = new List<StatusEffect>();

    public bool IsAlive => currentHealth > 0;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        currentEffects = new List<StatusEffect>();
    }

    // ── Status Effect Helpers ────────────────────────────────────────────────

    public void AddEffect(StatusEffect effect)
    {
        float roll = Random.value;
        if (roll < effectRes)
        {
            Debug.Log($"{unitName} resisted {effect.effectName}!");
            return;
        }
        currentEffects.Add(effect);
        Debug.Log($"{unitName} is now afflicted with {effect.effectName}!");
    }

    public void TickEffects()
    {
        for (int i = currentEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = currentEffects[i];

            if (e.damagePerTurn > 0)
            {
                TakeDamage(e.damagePerTurn);
                Debug.Log($"{unitName} took {e.damagePerTurn} damage from {e.effectName}.");
            }

            e.duration--;
            if (e.duration <= 0)
            {
                Debug.Log($"{e.effectName} wore off on {unitName}.");
                currentEffects.RemoveAt(i);
            }
        }
    }

    // ── Combat Helpers ───────────────────────────────────────────────────────

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
        bool isCrit = Random.value < criticalRate;
        float critMult = isCrit ? criticalDMG : 1.0f;
        float def = Mathf.Max(target.defence, 1);

        int damage = Mathf.RoundToInt((attackP + move.baseDamage) * elementalMult * critMult / def);

        if (isCrit) Debug.Log("Critical hit!");
        Debug.Log($"Elemental multiplier: x{elementalMult}");

        return Mathf.Max(damage, 1);
    }
}
