using UnityEngine;

public class TestEnemyUnit : MonoBehaviour
{
    private EnemyUnit enemy;

    void Awake()
    {
        enemy = gameObject.AddComponent<EnemyUnit>();

        // ── Identity ─────────────────────────────────────────────────────────
        enemy.unitName      = "Cinder Toad";
        enemy.elementalType = ElementalType.Fire;

        // ── Stats ─────────────────────────────────────────────────────────────
        enemy.maxHealth    = 80;
        enemy.attackP      = 15;
        enemy.defence      = 7;
        enemy.speed        = 8;
        enemy.criticalDMG  = 1.5f;   // 150% damage on crit
        enemy.criticalRate = 0.10f;  // 10% crit chance
        enemy.effectRes    = 0.10f;  // 10% chance to resist status effects

        // ── Enemy Settings ────────────────────────────────────────────────────
        enemy.tier              = 1;
        enemy.behaviour         = EnemyBehaviour.Aggressive;
        enemy.experienceReward  = 40;
        enemy.goldReward        = 15;

        // ── Move List ─────────────────────────────────────────────────────────
        enemy.moveList = new Move[]
        {
            new Move
            {
                moveName      = "Ember Bite",
                moveType      = MoveType.Attack,
                elementalType = ElementalType.Fire,
                baseDamage    = 18,
                baseHealing   = 0,
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
                baseHealing   = 0,
                accuracy      = 0.80f,
                effectToApply = "",
                effectChance  = 0f,
            },
            new Move
            {
                moveName      = "Flame Skin",
                moveType      = MoveType.Buff,
                elementalType = ElementalType.Fire,
                baseDamage    = 0,
                baseHealing   = 0,
                accuracy      = 1.0f,
                effectToApply = "",
                effectChance  = 0f,
                buffStat      = StatType.Defence,
                statModifier  = 1.5f,   // +50% defence for a turn
            },
            new Move
            {
                moveName      = "Absurd Belch",
                moveType      = MoveType.Special,
                elementalType = ElementalType.Absurd,
                baseDamage    = 22,
                baseHealing   = 0,
                accuracy      = 0.70f,
                effectToApply = "Confusion",
                effectChance  = 0.50f,
            },
        };

        Debug.Log($"[TestEnemyUnit] {enemy.unitName} appeared! " +
                  $"HP: {enemy.maxHealth} | ATK: {enemy.attackP} | " +
                  $"DEF: {enemy.defence} | SPD: {enemy.speed} | " +
                  $"Behaviour: {enemy.behaviour}");
    }
}
