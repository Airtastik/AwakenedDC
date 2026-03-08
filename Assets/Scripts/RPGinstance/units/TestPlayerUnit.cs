using UnityEngine;

/// <summary>
/// SETUP: Add BOTH this script AND a PlayerUnit component to the same GameObject.
/// This script populates PlayerUnit's stats in Awake so they're ready before
/// BattleSystem reads them.
///
/// Alternatively, delete this script and fill in all the PlayerUnit fields
/// directly in the Inspector — either approach works fine.
/// </summary>
[RequireComponent(typeof(PlayerUnit))]
public class TestPlayerUnit : MonoBehaviour
{
    void Awake()
    {
        PlayerUnit player = GetComponent<PlayerUnit>();

        // ── Identity ──────────────────────────────────────────────────────────
        player.unitName      = "Aria";
        player.elementalType = ElementalType.Water;

        // ── Stats ─────────────────────────────────────────────────────────────
        player.maxHealth    = 100;
        player.attackP      = 18;
        player.defence      = 10;
        player.speed        = 12;
        player.criticalDMG  = 1.75f;
        player.criticalRate = 0.15f;
        player.effectRes    = 0.20f;

        // ── Progression ───────────────────────────────────────────────────────
        player.level                 = 1;
        player.experience            = 0;
        player.experienceToNextLevel = 100;
        player.statPointsAvailable   = 0;

        // ── Move List ─────────────────────────────────────────────────────────
        player.moveList = new Move[]
        {
            new Move
            {
                moveName      = "Aqua Slash",
                moveType      = MoveType.Attack,
                elementalType = ElementalType.Water,
                baseDamage    = 20,
                accuracy      = 0.95f,
                effectToApply = "Soggy",
                effectChance  = 0.25f,
            },
            new Move
            {
                moveName      = "Tidal Surge",
                moveType      = MoveType.Attack,
                elementalType = ElementalType.Water,
                baseDamage    = 35,
                accuracy      = 0.75f,
            },
            new Move
            {
                moveName      = "Healing Mist",
                moveType      = MoveType.Heal,
                elementalType = ElementalType.Normal,
                baseHealing   = 30,
                accuracy      = 1.0f,
            },
            new Move
            {
                moveName      = "Waterlog",
                moveType      = MoveType.Debuff,
                elementalType = ElementalType.Water,
                baseDamage    = 5,
                accuracy      = 0.90f,
                effectToApply = "Soggy",
                effectChance  = 0.80f,
                buffStat      = StatType.Speed,
                statModifier  = 0.5f,
            },
        };

        Debug.Log($"[TestPlayerUnit] {player.unitName} configured. Max HP: {player.maxHealth}");
    }
}
