using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BattleState
{
    Start,
    PlayerTurn,
    EnemyTurn,
    Won,
    Lost
}

public class BattleSystem : MonoBehaviour
{
    // ── Party References ──────────────────────────────────────────────────────
    [Header("Parties")]
    public List<GameObject> playerPartyObjects = new List<GameObject>();
    public List<GameObject> enemyPartyObjects  = new List<GameObject>();

    private List<PlayerUnit> playerParty = new List<PlayerUnit>();
    private List<EnemyUnit>  enemyParty  = new List<EnemyUnit>();

    // ── Active Units ──────────────────────────────────────────────────────────
    private PlayerUnit activePlayer;
    private EnemyUnit  activeEnemy;

    // ── State ─────────────────────────────────────────────────────────────────
    public BattleState state { get; private set; }
    public bool IsReady { get; private set; }

    // ── CTB Settings ──────────────────────────────────────────────────────────
    [Header("CTB Settings")]
    public float actionDelay    = 1.0f;
    public int   speedVariance  = 10;   // Random range added to speed roll
    public int   queuePreviewCount = 8; // How many upcoming turns to show in UI

    // ── Turn Queue ────────────────────────────────────────────────────────────
    // The public queue is what the UI reads for display
    public List<Unit> TurnQueue { get; private set; } = new List<Unit>();

    // ── Events ────────────────────────────────────────────────────────────────
    public delegate void BattleEvent(string message);
    public static event BattleEvent OnBattleMessage;

    public delegate void BattleStateChanged(BattleState newState);
    public static event BattleStateChanged OnStateChanged;

    public delegate void PartyStatsChanged(List<PlayerUnit> players, List<EnemyUnit> enemies);
    public static event PartyStatsChanged OnStatsChanged;

    public delegate void ActiveUnitsChanged(PlayerUnit player, EnemyUnit enemy);
    public static event ActiveUnitsChanged OnActiveUnitsChanged;

    public delegate void TurnQueueUpdated(List<Unit> queue);
    public static event TurnQueueUpdated OnTurnQueueUpdated;

    public delegate void SwitchRequested(List<PlayerUnit> availableMembers);
    public static event SwitchRequested OnSwitchRequested;

    public delegate void InventoryChanged(List<Item> items);
    public static event InventoryChanged OnInventoryChanged;

    // ─────────────────────────────────────────────────────────────────────────
    #region Setup
    // ─────────────────────────────────────────────────────────────────────────

    void Start() => StartCoroutine(LateStart());

    private IEnumerator LateStart()
    {
        yield return null; // let Awake() scripts run first

        // ── Player party ─────────────────────────────────────────────────────
        // If a PartyManager exists, use it as the source of truth for player
        // stats so HP/SP/XP persist across scenes.
        // Fall back to playerPartyObjects if no PartyManager is present
        // (useful for isolated test scenes).
        if (PartyManager.Instance != null && PartyManager.Instance.PartySize > 0)
        {
            var spawned = PartyManager.Instance.SpawnParty();
            foreach (GameObject obj in spawned)
            {
                PlayerUnit pu = obj.GetComponent<PlayerUnit>();
                if (pu != null) playerParty.Add(pu);
            }
            Debug.Log($"[BattleSystem] Party loaded from PartyManager ({playerParty.Count} members).");
        }
        else
        {
            foreach (GameObject obj in playerPartyObjects)
            {
                PlayerUnit pu = obj.GetComponent<PlayerUnit>();
                if (pu != null) playerParty.Add(pu);
                else Debug.LogWarning($"{obj.name} has no PlayerUnit.");
            }
            Debug.Log("[BattleSystem] No PartyManager found — using scene party objects.");
        }

        // ── Enemy party ──────────────────────────────────────────────────────
        // If EncounterManager has a prepared encounter, use it.
        // Otherwise fall back to enemyPartyObjects placed in the scene.
        if (EncounterManager.Instance != null && EncounterManager.HasPendingEncounter)
        {
            var spawned = EncounterManager.Instance.SpawnEnemies();
            foreach (GameObject obj in spawned)
            {
                EnemyUnit eu = obj.GetComponent<EnemyUnit>();
                if (eu != null) enemyParty.Add(eu);
            }
            Debug.Log($"[BattleSystem] Enemy party loaded from EncounterManager ({enemyParty.Count} enemies).");
        }
        else
        {
            foreach (GameObject obj in enemyPartyObjects)
            {
                EnemyUnit eu = obj.GetComponent<EnemyUnit>();
                if (eu != null) enemyParty.Add(eu);
                else Debug.LogWarning($"{obj.name} has no EnemyUnit.");
            }
            Debug.Log("[BattleSystem] No EncounterManager found — using scene enemy objects.");
        }

        if (playerParty.Count == 0) { Debug.LogError("Player party is empty!"); yield break; }
        if (enemyParty.Count  == 0) { Debug.LogError("Enemy party is empty!");  yield break; }

        // Players: skip InitHealth if PartyManager is handling persistence
        if (PartyManager.Instance == null)
            foreach (var u in playerParty) u.InitHealth();

        // Enemies: EncounterManager.SpawnEnemies() already called InitHealth;
        // only call it here for scene-placed fallback enemies.
        if (EncounterManager.Instance == null)
            foreach (var u in enemyParty) u.InitHealth();

        IsReady = true;
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        SetState(BattleState.Start);
        activePlayer = playerParty[0];
        activeEnemy  = enemyParty[0];

        Log("Battle begins!");
        yield return new WaitForSeconds(actionDelay);

        RebuildQueue();
        NotifyActiveUnitsChanged();
        NotifyStatsChanged();

        yield return new WaitForSeconds(actionDelay);
        yield return StartCoroutine(ProcessNextTurn());
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region CTB Queue
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Simulates queuePreviewCount turns ahead by repeatedly rolling
    /// speed + Random(0, speedVariance) for all living units and sorting.
    /// The first entry is always the unit who acts next.
    /// </summary>
    private void RebuildQueue()
    {
        var living = playerParty.Cast<Unit>()
                                .Concat(enemyParty.Cast<Unit>())
                                .Where(u => u.IsAlive)
                                .ToList();

        // Give each unit a rolled initiative score
        var initiatives = living.ToDictionary(u => u, u => RollInitiative(u));

        // Simulate the queue by repeatedly picking the highest initiative unit,
        // decrementing its score by a base cost, and repeating
        TurnQueue.Clear();
        var scores = new Dictionary<Unit, float>(initiatives);

        for (int i = 0; i < queuePreviewCount; i++)
        {
            if (scores.Count == 0) break;

            // Pick highest scorer
            Unit next = scores.OrderByDescending(kv => kv.Value).First().Key;
            TurnQueue.Add(next);

            // Decay that unit's score by its speed (faster units recover faster)
            scores[next] -= 100f;

            // Tick everyone else up by their initiative
            foreach (var u in living.Where(u => u != next))
                scores[u] += RollInitiative(u) * 0.5f;
        }

        OnTurnQueueUpdated?.Invoke(TurnQueue);
    }

    private float RollInitiative(Unit unit)
    {
        return unit.speed + Random.Range(0, speedVariance);
    }

    /// <summary>Returns the unit who acts next (first in queue).</summary>
    private Unit GetNextActor()
    {
        var living = playerParty.Cast<Unit>()
                                .Concat(enemyParty.Cast<Unit>())
                                .Where(u => u.IsAlive)
                                .ToList();

        // Roll fresh initiative for this turn decision
        return living.OrderByDescending(u => RollInitiative(u)).FirstOrDefault();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Turn Loop
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator ProcessNextTurn()
    {
        if (CheckBattleOver()) yield break;

        Unit actor = GetNextActor();
        if (actor == null) yield break;

        RebuildQueue();

        if (actor is PlayerUnit pu)
        {
            activePlayer = pu;
            NotifyActiveUnitsChanged();
            SetState(BattleState.PlayerTurn);
            Log($"{pu.unitName}'s turn!");
            // Pauses here — UI calls PlayerUseMove / PlayerSwitch to resume
        }
        else if (actor is EnemyUnit eu)
        {
            activeEnemy = eu;
            NotifyActiveUnitsChanged();
            SetState(BattleState.EnemyTurn);
            Log($"{eu.unitName}'s turn!");
            yield return new WaitForSeconds(actionDelay);
            yield return StartCoroutine(EnemyTakeTurn(eu));
        }
    }

    private IEnumerator EndCurrentTurn(Unit actor)
    {
        yield return StartCoroutine(TickUnitEffects(actor));
        if (CheckBattleOver()) yield break;
        yield return StartCoroutine(ProcessNextTurn());
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Player Actions
    // ─────────────────────────────────────────────────────────────────────────

    public void PlayerUseMove(int moveIndex, int targetEnemyIndex = -1)
    {
        if (state != BattleState.PlayerTurn) { Log("Not your turn!"); return; }
        if (moveIndex < 0 || moveIndex >= activePlayer.moveList.Length) { Log("Invalid move."); return; }

        Move move = activePlayer.moveList[moveIndex];
        if (!activePlayer.HasSP(move.spCost))
        {
            Log($"Not enough SP! ({move.moveName} costs {move.spCost} SP)");
            return;
        }

        EnemyUnit target = GetTargetEnemy(targetEnemyIndex);
        if (target == null) { Log("No valid target."); return; }

        StartCoroutine(PlayerAttackTurn(move, target));
    }

    public void PlayerSwitch(int partyIndex)
    {
        if (state != BattleState.PlayerTurn) { Log("Not your turn!"); return; }
        if (partyIndex < 0 || partyIndex >= playerParty.Count) { Log("Invalid index."); return; }

        PlayerUnit switchTarget = playerParty[partyIndex];
        if (switchTarget == activePlayer) { Log($"{switchTarget.unitName} is already active!"); return; }
        if (!switchTarget.IsAlive)        { Log($"{switchTarget.unitName} can't battle!"); return; }

        StartCoroutine(SwitchPlayerUnit(switchTarget));
    }

    /// <summary>
    /// Use an item from PlayerInventory on a party member.
    /// targetPartyIndex -1 = auto-target (most wounded / active player).
    /// </summary>
    public void PlayerUseItem(Item item, int targetPartyIndex = -1)
    {
        if (state != BattleState.PlayerTurn) { Log("Not your turn!"); return; }

        var inventory = PlayerInventory.Instance;
        if (inventory == null) { Log("No inventory found."); return; }
        if (!inventory.ConsumeItem(item))   { Log($"No {item.itemName} left."); return; }

        OnInventoryChanged?.Invoke(inventory.items);
        StartCoroutine(ItemUseTurn(item, targetPartyIndex));
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Player Turn Coroutines
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator PlayerAttackTurn(Move move, EnemyUnit target)
    {
        Unit actor = activePlayer;

        // Spend SP upfront — already validated in PlayerUseMove
        if (move.spCost > 0)
        {
            activePlayer.SpendSP(move.spCost);
            NotifyStatsChanged();
        }

        if (!AccuracyCheck(move))
        {
            Log($"{activePlayer.unitName} used {move.moveName}... but it missed!");
            yield return new WaitForSeconds(actionDelay);
            yield return StartCoroutine(EndCurrentTurn(actor));
            yield break;
        }

        yield return StartCoroutine(ExecuteMove(move, activePlayer, target));
        yield return new WaitForSeconds(actionDelay);

        if (!activeEnemy.IsAlive) PromoteNextEnemy();
        if (CheckBattleOver()) yield break;
        yield return StartCoroutine(EndCurrentTurn(actor));
    }

    private IEnumerator SwitchPlayerUnit(PlayerUnit switchTarget)
    {
        Unit actor = activePlayer;
        Log($"{activePlayer.unitName} withdrew!");
        yield return new WaitForSeconds(actionDelay);

        activePlayer = switchTarget;
        NotifyActiveUnitsChanged();
        Log($"Go, {activePlayer.unitName}!");
        yield return new WaitForSeconds(actionDelay);

        yield return StartCoroutine(EndCurrentTurn(actor));
    }

    private IEnumerator ItemUseTurn(Item item, int targetPartyIndex)
    {
        Unit actor = activePlayer;

        // Resolve target
        PlayerUnit target = (targetPartyIndex >= 0 && targetPartyIndex < playerParty.Count)
            ? playerParty[targetPartyIndex]
            : GetMostWoundedPlayer() ?? activePlayer;

        Log($"{activePlayer.unitName} used {item.itemName}!");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        switch (item.effect)
        {
            case ItemEffect.HealTarget:
                target.Heal(item.power);
                Log($"{target.unitName} recovered {item.power} HP!");
                NotifyStatsChanged();
                break;

            case ItemEffect.HealParty:
                foreach (var pu in GetLivingPlayers())
                {
                    pu.Heal(item.power);
                    Log($"{pu.unitName} recovered {item.power} HP!");
                }
                NotifyStatsChanged();
                break;

            case ItemEffect.ReviveTarget:
                // Target the first fainted member if no index given
                PlayerUnit fainted = (targetPartyIndex >= 0 && targetPartyIndex < playerParty.Count)
                    ? playerParty[targetPartyIndex]
                    : playerParty.FirstOrDefault(u => !u.IsAlive);
                if (fainted != null && !fainted.IsAlive)
                {
                    fainted.currentHealth = Mathf.RoundToInt(fainted.maxHealth * 0.3f);
                    Log($"{fainted.unitName} was revived with {fainted.currentHealth} HP!");
                    NotifyStatsChanged();
                }
                else Log("No fainted ally to revive.");
                break;

            case ItemEffect.BuffAttack:
                target.attackP = Mathf.RoundToInt(target.attackP * item.statMult);
                Log($"{target.unitName}'s Attack rose!");
                NotifyStatsChanged();
                break;

            case ItemEffect.BuffDefence:
                target.defence = Mathf.RoundToInt(target.defence * item.statMult);
                Log($"{target.unitName}'s Defence rose!");
                NotifyStatsChanged();
                break;

            case ItemEffect.CureEffects:
                target.currentEffects.Clear();
                Log($"{target.unitName}'s status effects were cured!");
                NotifyStatsChanged();
                break;
        }

        yield return new WaitForSeconds(actionDelay);
        yield return StartCoroutine(EndCurrentTurn(actor));
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Enemy Turn
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator EnemyTakeTurn(EnemyUnit enemy)
    {
        Unit actor = enemy;
        PlayerUnit target = GetRandomLivingPlayer();
        if (target == null) yield break;

        Move move = enemy.ChooseMove(target);

        if (move == null)
            Log($"{enemy.unitName} does nothing.");
        else if (!AccuracyCheck(move))
            Log($"{enemy.unitName} used {move.moveName}... but it missed!");
        else
            yield return StartCoroutine(ExecuteMove(move, enemy, target));

        yield return new WaitForSeconds(actionDelay);

        if (!activePlayer.IsAlive)
        {
            PlayerUnit next = GetRandomLivingPlayer();
            if (next != null)
            {
                Log($"{activePlayer.unitName} fainted! {next.unitName} steps in!");
                activePlayer = next;
                NotifyActiveUnitsChanged();
                OnSwitchRequested?.Invoke(GetLivingPlayers());
            }
        }

        if (CheckBattleOver()) yield break;
        yield return StartCoroutine(EndCurrentTurn(actor));
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Move Execution
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator ExecuteMove(Move move, Unit attacker, Unit defender)
    {
        switch (move.moveType)
        {
            case MoveType.Attack:
            case MoveType.Special: yield return StartCoroutine(ExecuteAttack(move, attacker, defender)); break;
            case MoveType.Heal:    yield return StartCoroutine(ExecuteHeal(move, attacker));              break;
            case MoveType.Buff:    yield return StartCoroutine(ExecuteBuff(move, attacker, attacker));    break;
            case MoveType.Debuff:  yield return StartCoroutine(ExecuteBuff(move, attacker, defender));    break;
        }
    }

    private IEnumerator ExecuteAttack(Move move, Unit attacker, Unit defender)
    {
        int damage = attacker.CalculateDamage(move, defender);
        Log($"{attacker.unitName} used {move.moveName}!");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        float mult = ElementalChart.GetMultiplier(move.elementalType, defender.elementalType);
        if      (mult >= 2.0f) Log("Super effective!");
        else if (mult <= 0.5f) Log("Not very effective...");

        defender.TakeDamage(damage);
        Log($"{defender.unitName} took {damage} damage! ({defender.currentHealth}/{defender.maxHealth} HP)");
        NotifyStatsChanged();

        if (!string.IsNullOrEmpty(move.effectToApply) && Random.value < move.effectChance)
        {
            StatusEffect effect = BuildEffect(move.effectToApply, move.elementalType);
            if (effect != null) { defender.AddEffect(effect); Log($"{defender.unitName} is {effect.effectName}!"); }
        }

        yield return new WaitForSeconds(actionDelay * 0.5f);
    }

    private IEnumerator ExecuteHeal(Move move, Unit caster)
    {
        Unit healTarget = caster is PlayerUnit ? (Unit)GetMostWoundedPlayer() : GetMostWoundedEnemy();
        if (healTarget == null) healTarget = caster;

        Log($"{caster.unitName} used {move.moveName} on {healTarget.unitName}!");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        healTarget.Heal(move.baseHealing);
        Log($"{healTarget.unitName} restored {move.baseHealing} HP!");
        NotifyStatsChanged();
    }

    private IEnumerator ExecuteBuff(Move move, Unit attacker, Unit target)
    {
        string action = move.moveType == MoveType.Buff ? "buffed" : "debuffed";
        Log($"{attacker.unitName} used {move.moveName}! {target.unitName}'s {move.buffStat} was {action}!");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        ApplyStatModifier(target, move.buffStat, move.statModifier);
        NotifyStatsChanged();

        if (move.moveType == MoveType.Debuff && move.baseDamage > 0)
        {
            int damage = attacker.CalculateDamage(move, target);
            target.TakeDamage(damage);
            Log($"{target.unitName} also took {damage} damage!");
            NotifyStatsChanged();
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Status Effects
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator TickUnitEffects(Unit unit)
    {
        if (unit.currentEffects.Count == 0) yield break;

        for (int i = unit.currentEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = unit.currentEffects[i];
            if (e.damagePerTurn > 0)
            {
                unit.TakeDamage(e.damagePerTurn);
                Log($"{unit.unitName} took {e.damagePerTurn} from {e.effectName}!");
                NotifyStatsChanged();
                yield return new WaitForSeconds(actionDelay * 0.5f);
            }
            e.duration--;
            if (e.duration <= 0) { Log($"{e.effectName} wore off on {unit.unitName}."); unit.currentEffects.RemoveAt(i); }
        }
    }

    private StatusEffect BuildEffect(string effectName, ElementalType source)
    {
        switch (effectName)
        {
            case "Burn":      return new StatusEffect { effectName = "Burn",      sourceElement = ElementalType.Fire,   duration = 3, damagePerTurn = 5, affectedStat = StatType.AttackP,      statModifier = 0.9f  };
            case "Soggy":     return new StatusEffect { effectName = "Soggy",     sourceElement = ElementalType.Water,  duration = 2, damagePerTurn = 0, affectedStat = StatType.Speed,        statModifier = 0.75f };
            case "Poison":    return new StatusEffect { effectName = "Poison",    sourceElement = ElementalType.Nature, duration = 4, damagePerTurn = 8, affectedStat = StatType.Defence,      statModifier = 0.85f };
            case "Confusion": return new StatusEffect { effectName = "Confusion", sourceElement = ElementalType.Absurd, duration = 2, damagePerTurn = 3, affectedStat = StatType.CriticalRate, statModifier = 0.5f  };
            default: Debug.LogWarning($"Unknown effect: {effectName}"); return null;
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Party Helpers
    // ─────────────────────────────────────────────────────────────────────────

    public List<PlayerUnit> GetLivingPlayers() => playerParty.Where(u => u.IsAlive).ToList();
    public List<EnemyUnit>  GetLivingEnemies()  => enemyParty.Where(u => u.IsAlive).ToList();

    private PlayerUnit GetRandomLivingPlayer()
    {
        var living = GetLivingPlayers();
        return living.Count > 0 ? living[Random.Range(0, living.Count)] : null;
    }

    private EnemyUnit GetTargetEnemy(int index)
    {
        var living = GetLivingEnemies();
        if (living.Count == 0) return null;
        return (index >= 0 && index < living.Count) ? living[index] : living[0];
    }

    private PlayerUnit GetMostWoundedPlayer() =>
        playerParty.Where(u => u.IsAlive).OrderBy(u => (float)u.currentHealth / u.maxHealth).FirstOrDefault();

    private EnemyUnit GetMostWoundedEnemy() =>
        enemyParty.Where(u => u.IsAlive).OrderBy(u => (float)u.currentHealth / u.maxHealth).FirstOrDefault();

    private void PromoteNextEnemy()
    {
        EnemyUnit next = GetLivingEnemies().FirstOrDefault();
        if (next != null) { activeEnemy = next; Log($"{next.unitName} steps forward!"); NotifyActiveUnitsChanged(); }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Battle Resolution
    // ─────────────────────────────────────────────────────────────────────────

    private bool CheckBattleOver()
    {
        if (GetLivingEnemies().Count == 0) { StartCoroutine(BattleWon()); return true; }
        if (GetLivingPlayers().Count == 0) { StartCoroutine(BattleLost()); return true; }
        return false;
    }

    private IEnumerator BattleWon()
    {
        SetState(BattleState.Won);
        Log("Victory!");
        yield return new WaitForSeconds(actionDelay);

        int totalXP   = enemyParty.Sum(e => e.experienceReward);
        int totalGold = enemyParty.Sum(e => e.goldReward);
        var survivors = GetLivingPlayers();
        int xpShare   = survivors.Count > 0 ? totalXP / survivors.Count : 0;

        foreach (PlayerUnit pu in survivors) { pu.GainExperience(xpShare); Log($"{pu.unitName} gained {xpShare} XP!"); }
        Log($"Party earned {totalGold} gold!");
        NotifyStatsChanged();

        // Persist HP/SP/XP back to PartyManager so state survives scene change
        PartyManager.Instance?.SaveParty(playerParty);
    }

    private IEnumerator BattleLost()
    {
        SetState(BattleState.Lost);
        Log("All party members have fainted...");
        yield return new WaitForSeconds(actionDelay);
        Log("Game Over.");

        // Save even on loss — members are clamped to 1 HP by PartyManager
        PartyManager.Instance?.SaveParty(playerParty);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Utility
    // ─────────────────────────────────────────────────────────────────────────

    private bool AccuracyCheck(Move move) => Random.value <= move.accuracy;

    private void ApplyStatModifier(Unit target, StatType stat, float modifier)
    {
        switch (stat)
        {
            case StatType.AttackP:      target.attackP      = Mathf.RoundToInt(target.attackP * modifier); break;
            case StatType.Defence:      target.defence      = Mathf.RoundToInt(target.defence * modifier); break;
            case StatType.Speed:        target.speed        = Mathf.RoundToInt(target.speed   * modifier); break;
            case StatType.CriticalDMG:  target.criticalDMG  *= modifier; break;
            case StatType.CriticalRate: target.criticalRate *= modifier; break;
            case StatType.EffectRes:    target.effectRes    *= modifier; break;
            case StatType.Health:
                int amount = Mathf.RoundToInt(target.maxHealth * (modifier - 1f));
                if (amount > 0) target.Heal(amount); else target.TakeDamage(Mathf.Abs(amount));
                break;
        }
    }

    private void SetState(BattleState newState)     { state = newState; OnStateChanged?.Invoke(newState); }
    private void Log(string message)                { Debug.Log($"[Battle] {message}"); OnBattleMessage?.Invoke(message); }
    private void NotifyStatsChanged()               => OnStatsChanged?.Invoke(playerParty, enemyParty);
    private void NotifyActiveUnitsChanged()         => OnActiveUnitsChanged?.Invoke(activePlayer, activeEnemy);

    #endregion
}
