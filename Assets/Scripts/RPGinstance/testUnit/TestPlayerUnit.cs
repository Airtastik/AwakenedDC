using UnityEngine;

public class TestPlayerUnit : MonoBehaviour
{
    private PlayerUnit player;

    void Awake()
    {
        player = gameObject.AddComponent<PlayerUnit>();

        // ── Identity ─────────────────────────────────────────────────────────
        player.unitName     = "Aria";
        player.elementalType = ElementalType.Water;

        // ── Stats ─────────────────────────────────────────────────────────────
        player.maxHealth     = 100;
        player.attackP       = 18;
        player.defence       = 10;
        player.speed         = 12;
        player.criticalDMG   = 1.75f;  // 175% damage on crit
        player.criticalRate  = 0.15f;  // 15% crit chance
        player.effectRes     = 0.20f;  // 20% chance to resist status effects

        // ── Progression ───────────────────────────────────────────────────────
        player.level                  = 1;
        player.experience             = 0;
        player.experienceToNextLevel  = 100;
        player.statPointsAvailable    = 0;

        // ── Move List ─────────────────────────────────────────────────────────
        player.moveList = new Move[]
        {
            new Move
            {
                moveName      = "Aqua Slash",
                moveType      = MoveType.Attack,
                elementalType = ElementalType.Water,
                baseDamage    = 20,
                baseHealing   = 0,
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
                baseHealing   = 0,
                accuracy      = 0.75f,
                effectToApply = "",
                effectChance  = 0f,
            },
            new Move
            {
                moveName      = "Healing Mist",
                moveType      = MoveType.Heal,
                elementalType = ElementalType.Normal,
                baseDamage    = 0,
                baseHealing   = 30,
                accuracy      = 1.0f,
                effectToApply = "",
                effectChance  = 0f,
            },
            new Move
            {
                moveName      = "Waterlog",
                moveType      = MoveType.Debuff,
                elementalType = ElementalType.Water,
                baseDamage    = 5,
                baseHealing   = 0,
                accuracy      = 0.90f,
                effectToApply = "Soggy",
                effectChance  = 0.80f,
                buffStat      = StatType.Speed,
                statModifier  = 0.5f,   // Halves target speed
            },
        };

        Debug.Log($"[TestPlayerUnit] {player.unitName} is ready! " +
                  $"HP: {player.maxHealth} | ATK: {player.attackP} | " +
                  $"DEF: {player.defence} | SPD: {player.speed}");
    }
}
