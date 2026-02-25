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

    /// <summary>True once parties are populated and InitHealth has been called.
    /// The UI polls this before trying to build cards.</summary>
    public bool IsReady { get; private set; }

    // ── Settings ──────────────────────────────────────────────────────────────
    [Header("Settings")]
    public float actionDelay = 1.2f;

    // ── Turn Order ────────────────────────────────────────────────────────────
    private List<Unit> turnOrder = new List<Unit>();
    private int turnIndex = 0;

    // ── Events ────────────────────────────────────────────────────────────────
    public delegate void BattleEvent(string message);
    public static event BattleEvent OnBattleMessage;

    public delegate void BattleStateChanged(BattleState newState);
    public static event BattleStateChanged OnStateChanged;

    public delegate void PartyStatsChanged(List<PlayerUnit> players, List<EnemyUnit> enemies);
    public static event PartyStatsChanged OnStatsChanged;

    public delegate void ActiveUnitsChanged(PlayerUnit player, EnemyUnit enemy);
    public static event ActiveUnitsChanged OnActiveUnitsChanged;

    // Fired when a player unit faints mid-battle so the UI can prompt a switch
    public delegate void SwitchRequested(List<PlayerUnit> availableMembers);
    public static event SwitchRequested OnSwitchRequested;

    // ─────────────────────────────────────────────────────────────────────────
    #region Setup
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Wait one frame so that any TestPlayerUnit / TestEnemyUnit Awake()
        // scripts have had a chance to AddComponent and assign stats before
        // we read them and call InitHealth().
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return null; // skip one frame

        foreach (GameObject obj in playerPartyObjects)
        {
            PlayerUnit pu = obj.GetComponent<PlayerUnit>();
            if (pu != null) playerParty.Add(pu);
            else Debug.LogWarning($"{obj.name} has no PlayerUnit and was skipped.");
        }

        foreach (GameObject obj in enemyPartyObjects)
        {
            EnemyUnit eu = obj.GetComponent<EnemyUnit>();
            if (eu != null) enemyParty.Add(eu);
            else Debug.LogWarning($"{obj.name} has no EnemyUnit and was skipped.");
        }

        if (playerParty.Count == 0) { Debug.LogError("Player party is empty!"); yield break; }
        if (enemyParty.Count  == 0) { Debug.LogError("Enemy party is empty!");  yield break; }

        // Initialise HP now that all stats are guaranteed to be set
        foreach (var u in playerParty) u.InitHealth();
        foreach (var u in enemyParty)  u.InitHealth();

        IsReady = true;
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        SetState(BattleState.Start);

        activePlayer = playerParty[0];
        activeEnemy  = enemyParty[0];

        Log("A battle has begun!");
        Log($"Player party: {string.Join(", ", playerParty.Select(u => u.unitName))}");
        Log($"Enemy party:  {string.Join(", ", enemyParty.Select(u => u.unitName))}");
        yield return new WaitForSeconds(actionDelay);

        BuildTurnOrder();
        NotifyActiveUnitsChanged();
        NotifyStatsChanged();

        yield return new WaitForSeconds(actionDelay);
        yield return StartCoroutine(ProcessNextTurn());
    }

    /// <summary>
    /// Sorts all living units by speed (descending) to form the round's turn order.
    /// Rebuilt at the start of every new round so deaths and speed changes apply.
    /// </summary>
    private void BuildTurnOrder()
    {
        turnOrder = playerParty
            .Cast<Unit>()
            .Concat(enemyParty.Cast<Unit>())
            .Where(u => u.IsAlive)
            .OrderByDescending(u => u.speed)
            .ThenBy(_ => Random.value)   // Break speed ties randomly
            .ToList();

        turnIndex = 0;
        Log("Turn order: " + string.Join(" → ", turnOrder.Select(u => u.unitName)));
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Turn Loop
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator ProcessNextTurn()
    {
        if (CheckBattleOver()) yield break;

        // End of round — rebuild for the next one
        if (turnIndex >= turnOrder.Count)
        {
            Log("--- New Round ---");
            yield return new WaitForSeconds(actionDelay);
            BuildTurnOrder();
        }

        Unit current = turnOrder[turnIndex];

        // Skip units that died during the current round
        if (!current.IsAlive)
        {
            turnIndex++;
            yield return StartCoroutine(ProcessNextTurn());
            yield break;
        }

        if (current is PlayerUnit pu)
        {
            activePlayer = pu;
            NotifyActiveUnitsChanged();
            SetState(BattleState.PlayerTurn);
            Log($"--- {pu.unitName}'s Turn ---");
            // Coroutine pauses here — UI calls PlayerUseMove / PlayerSwitch to continue
        }
        else if (current is EnemyUnit eu)
        {
            activeEnemy = eu;
            NotifyActiveUnitsChanged();
            SetState(BattleState.EnemyTurn);
            Log($"--- {eu.unitName}'s Turn ---");
            yield return new WaitForSeconds(actionDelay);
            yield return StartCoroutine(EnemyTakeTurn(eu));
        }
    }

    private IEnumerator EndCurrentTurn()
    {
        // Tick status effects for the unit that just acted
        yield return StartCoroutine(TickUnitEffects(turnOrder[turnIndex]));
        if (CheckBattleOver()) yield break;

        turnIndex++;
        yield return StartCoroutine(ProcessNextTurn());
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Player Actions (call these from your UI buttons)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Active player uses a move. Optionally specify a target enemy index
    /// from GetLivingEnemies(); defaults to the first living enemy.
    /// </summary>
    public void PlayerUseMove(int moveIndex, int targetEnemyIndex = -1)
    {
        if (state != BattleState.PlayerTurn) { Log("It's not your turn!"); return; }
        if (moveIndex < 0 || moveIndex >= activePlayer.moveList.Length) { Log("Invalid move."); return; }

        EnemyUnit target = GetTargetEnemy(targetEnemyIndex);
        if (target == null) { Log("No valid target."); return; }

        StartCoroutine(PlayerAttackTurn(activePlayer.moveList[moveIndex], target));
    }

    /// <summary>
    /// Switch the active player to another party member. Costs the current turn.
    /// Pass the index within the full playerParty list.
    /// </summary>
    public void PlayerSwitch(int partyIndex)
    {
        if (state != BattleState.PlayerTurn) { Log("It's not your turn!"); return; }
        if (partyIndex < 0 || partyIndex >= playerParty.Count) { Log("Invalid party index."); return; }

        PlayerUnit switchTarget = playerParty[partyIndex];
        if (switchTarget == activePlayer) { Log($"{switchTarget.unitName} is already active!"); return; }
        if (!switchTarget.IsAlive)        { Log($"{switchTarget.unitName} can't battle!"); return; }

        StartCoroutine(SwitchPlayerUnit(switchTarget));
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Player Turn Coroutines
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator PlayerAttackTurn(Move move, EnemyUnit target)
    {
        if (!AccuracyCheck(move))
        {
            Log($"{activePlayer.unitName} used {move.moveName}... but it missed!");
            yield return new WaitForSeconds(actionDelay);
            yield return StartCoroutine(EndCurrentTurn());
            yield break;
        }

        yield return StartCoroutine(ExecuteMove(move, activePlayer, target));
        yield return new WaitForSeconds(actionDelay);

        // If that enemy fainted, promote the next living one as focus
        if (!activeEnemy.IsAlive) PromoteNextEnemy();

        if (CheckBattleOver()) yield break;
        yield return StartCoroutine(EndCurrentTurn());
    }

    private IEnumerator SwitchPlayerUnit(PlayerUnit switchTarget)
    {
        Log($"{activePlayer.unitName} withdrew!");
        yield return new WaitForSeconds(actionDelay);

        activePlayer = switchTarget;
        NotifyActiveUnitsChanged();
        Log($"Go, {activePlayer.unitName}!");
        yield return new WaitForSeconds(actionDelay);

        yield return StartCoroutine(EndCurrentTurn());
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Enemy Turn
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator EnemyTakeTurn(EnemyUnit enemy)
    {
        PlayerUnit target = GetRandomLivingPlayer();
        if (target == null) yield break;

        Move move = enemy.ChooseMove(target);

        if (move == null)
        {
            Log($"{enemy.unitName} does nothing.");
        }
        else if (!AccuracyCheck(move))
        {
            Log($"{enemy.unitName} used {move.moveName}... but it missed!");
        }
        else
        {
            yield return StartCoroutine(ExecuteMove(move, enemy, target));
        }

        yield return new WaitForSeconds(actionDelay);

        // If the targeted player fainted, auto-promote and fire switch event for UI
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
        yield return StartCoroutine(EndCurrentTurn());
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
            case MoveType.Special:
                yield return StartCoroutine(ExecuteAttack(move, attacker, defender));
                break;
            case MoveType.Heal:
                yield return StartCoroutine(ExecuteHeal(move, attacker));
                break;
            case MoveType.Buff:
                yield return StartCoroutine(ExecuteBuff(move, attacker, attacker));
                break;
            case MoveType.Debuff:
                yield return StartCoroutine(ExecuteBuff(move, attacker, defender));
                break;
        }
    }

    private IEnumerator ExecuteAttack(Move move, Unit attacker, Unit defender)
    {
        int damage = attacker.CalculateDamage(move, defender);
        Log($"{attacker.unitName} used {move.moveName}!");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        float mult = ElementalChart.GetMultiplier(move.elementalType, defender.elementalType);
        if      (mult >= 2.0f) Log("It's super effective!");
        else if (mult <= 0.5f) Log("It's not very effective...");

        defender.TakeDamage(damage);
        Log($"{defender.unitName} took {damage} damage! ({defender.currentHealth}/{defender.maxHealth} HP)");
        NotifyStatsChanged();

        if (!string.IsNullOrEmpty(move.effectToApply) && Random.value < move.effectChance)
        {
            StatusEffect effect = BuildEffect(move.effectToApply, move.elementalType);
            if (effect != null)
            {
                defender.AddEffect(effect);
                Log($"{defender.unitName} is afflicted with {effect.effectName}!");
            }
        }

        yield return new WaitForSeconds(actionDelay * 0.5f);
    }

    private IEnumerator ExecuteHeal(Move move, Unit caster)
    {
        // Heal the most wounded living ally on the same side
        Unit healTarget = caster is PlayerUnit
            ? (Unit)GetMostWoundedPlayer()
            : (Unit)GetMostWoundedEnemy();

        if (healTarget == null) healTarget = caster;

        Log($"{caster.unitName} used {move.moveName} on {healTarget.unitName}!");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        healTarget.Heal(move.baseHealing);
        Log($"{healTarget.unitName} restored {move.baseHealing} HP! ({healTarget.currentHealth}/{healTarget.maxHealth} HP)");
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

        Log($"Processing status effects on {unit.unitName}...");
        yield return new WaitForSeconds(actionDelay * 0.5f);

        for (int i = unit.currentEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = unit.currentEffects[i];

            if (e.damagePerTurn > 0)
            {
                unit.TakeDamage(e.damagePerTurn);
                Log($"{unit.unitName} took {e.damagePerTurn} damage from {e.effectName}!");
                NotifyStatsChanged();
                yield return new WaitForSeconds(actionDelay * 0.5f);
            }

            e.duration--;
            if (e.duration <= 0)
            {
                Log($"{e.effectName} wore off on {unit.unitName}.");
                unit.currentEffects.RemoveAt(i);
            }
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
            default:
                Debug.LogWarning($"Unknown effect: {effectName}");
                return null;
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
        if (next != null)
        {
            activeEnemy = next;
            Log($"{next.unitName} steps forward!");
            NotifyActiveUnitsChanged();
        }
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
        Log("All enemies defeated! Victory!");
        yield return new WaitForSeconds(actionDelay);

        // Split XP evenly among surviving members
        int totalXP   = enemyParty.Sum(e => e.experienceReward);
        int totalGold = enemyParty.Sum(e => e.goldReward);
        var survivors = GetLivingPlayers();
        int xpShare   = survivors.Count > 0 ? totalXP / survivors.Count : 0;

        foreach (PlayerUnit pu in survivors)
        {
            pu.GainExperience(xpShare);
            Log($"{pu.unitName} gained {xpShare} XP!");
        }

        Log($"The party earned {totalGold} gold!");
        NotifyStatsChanged();
    }

    private IEnumerator BattleLost()
    {
        SetState(BattleState.Lost);
        Log("All party members have fainted...");
        yield return new WaitForSeconds(actionDelay);
        Log("Game Over.");
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
            case StatType.AttackP:      target.attackP      = Mathf.RoundToInt(target.attackP * modifier);      break;
            case StatType.Defence:      target.defence      = Mathf.RoundToInt(target.defence * modifier);      break;
            case StatType.Speed:        target.speed        = Mathf.RoundToInt(target.speed   * modifier);      break;
            case StatType.CriticalDMG:  target.criticalDMG  *= modifier;                                        break;
            case StatType.CriticalRate: target.criticalRate *= modifier;                                         break;
            case StatType.EffectRes:    target.effectRes    *= modifier;                                         break;
            case StatType.Health:
                int amount = Mathf.RoundToInt(target.maxHealth * (modifier - 1f));
                if (amount > 0) target.Heal(amount);
                else            target.TakeDamage(Mathf.Abs(amount));
                break;
        }
    }

    private void SetState(BattleState newState)      { state = newState; OnStateChanged?.Invoke(newState); }
    private void Log(string message)                 { Debug.Log($"[Battle] {message}"); OnBattleMessage?.Invoke(message); }
    private void NotifyStatsChanged()                => OnStatsChanged?.Invoke(playerParty, enemyParty);
    private void NotifyActiveUnitsChanged()          => OnActiveUnitsChanged?.Invoke(activePlayer, activeEnemy);

    #endregion
}
