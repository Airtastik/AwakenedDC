using UnityEngine;

/// <summary>
/// SETUP: Add BOTH this script AND an EnemyUnit component to the same GameObject.
/// This script populates EnemyUnit's stats in Awake so they're ready before
/// BattleSystem reads them.
///
/// Alternatively, delete this script and fill in all the EnemyUnit fields
/// directly in the Inspector — either approach works fine.
/// </summary>
[RequireComponent(typeof(EnemyUnit))]
public class TestEnemyUnit : MonoBehaviour
{
    void Awake()
    {
        EnemyUnit enemy = GetComponent<EnemyUnit>();

        // ── Identity ──────────────────────────────────────────────────────────
        enemy.unitName      = "Johnny";
        enemy.elementalType = ElementalType.Fire;

        // ── Stats ─────────────────────────────────────────────────────────────
        enemy.maxHealth    = 80;
        enemy.attackP      = 15;
        enemy.defence      = 7;
        enemy.speed        = 8;
        enemy.criticalDMG  = 1.5f;
        enemy.criticalRate = 0.10f;
        enemy.effectRes    = 0.10f;

        // ── Enemy Settings ────────────────────────────────────────────────────
        enemy.tier             = 1;
        enemy.behaviour        = EnemyBehaviour.Aggressive;
        enemy.experienceReward = 40;
        enemy.goldReward       = 15;

        // ── Move List ─────────────────────────────────────────────────────────
        enemy.moveList = new Move[]
        {
            new Move
            {
                moveName      = "Ember Bite",
                moveType      = MoveType.Attack,
                elementalType = ElementalType.Fire,
                baseDamage    = 18,
                accuracy      = 0.95f,
                effectToApply = "Burn",
                effectChance  = 0.30f,
            },
            new Move
            {
                moveName      = "Scorch",
                moveType      = MoveType.Attack,
                elementalType = ElementalType.Fire,
                baseDamage    = 28,
                accuracy      = 0.80f,
            },
            new Move
            {
                moveName      = "Flame Skin",
                moveType      = MoveType.Buff,
                elementalType = ElementalType.Fire,
                accuracy      = 1.0f,
                buffStat      = StatType.Defence,
                statModifier  = 1.5f,
            },
            new Move
            {
                moveName      = "Absurd Belch",
                moveType      = MoveType.Special,
                elementalType = ElementalType.Absurd,
                baseDamage    = 22,
                accuracy      = 0.70f,
                effectToApply = "Confusion",
                effectChance  = 0.50f,
            },
        };

        Debug.Log($"[TestEnemyUnit] {enemy.unitName} configured. Max HP: {enemy.maxHealth}");
    }
}
