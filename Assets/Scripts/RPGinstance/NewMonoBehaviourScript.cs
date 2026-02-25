using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Attach to the same GameObject as your UIDocument.
/// Assign the RPGins.uxml UIDocument and your BattleSystem in the Inspector.
///
/// EVENT SYSTEM EXPLAINED:
/// BattleSystem fires static C# events (OnBattleMessage, OnStateChanged, etc.)
/// whenever something meaningful happens in the battle. This script subscribes
/// to those events in OnEnable and unsubscribes in OnDisable. That means:
///   - BattleSystem never needs to know the UI exists
///   - The UI never polls — it only reacts when told to
///   - No circular references between the two scripts
/// You only need to drag BattleSystem into the Inspector field below so
/// the UI can call back into it (PlayerUseMove, PlayerSwitch, etc.)
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("References")]
    public BattleSystem battleSystem;

    // ── Cached elements ───────────────────────────────────────────────────────
    private VisualElement root;
    private VisualElement enemyPanel;
    private VisualElement playerPanel;
    private VisualElement logContent;
    private ScrollView    logScroll;
    private VisualElement movesContainer;
    private VisualElement switchContainer;
    private Button        guardBtn;
    private VisualElement targetOverlay;
    private VisualElement targetList;
    private VisualElement resultOverlay;
    private Label         resultTitle;
    private Label         resultSubtitle;

    // ── Move button click handlers stored so we can cleanly remove them ───────
    private readonly System.Action[] moveClickHandlers = new System.Action[4];

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int  pendingMoveIndex = -1;
    private readonly Dictionary<Unit, VisualElement> unitCards = new();
    private bool battleReady = false;

    // ─────────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    void OnEnable()
    {
        try
        {
            Debug.Log("[BattleUI] OnEnable started.");
            root = GetComponent<UIDocument>().rootVisualElement;
            if (root == null) { Debug.LogError("[BattleUI] rootVisualElement is null. Is the UXML assigned on the UIDocument?"); return; }
            Debug.Log("[BattleUI] Root found. Caching elements...");
            CacheElements();
            Debug.Log("[BattleUI] CacheElements done. Subscribing events...");
            SubscribeEvents();
            Debug.Log("[BattleUI] OnEnable complete.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BattleUI] OnEnable crashed: {e.Message}\n{e.StackTrace}");
        }
    }

    void OnDisable() => UnsubscribeEvents();

    void Start()
    {
        Debug.Log("[BattleUI] Start fired.");
        if (battleSystem == null) { Debug.LogError("[BattleUI] battleSystem is not assigned in the Inspector!"); return; }
        StartCoroutine(WaitForBattleReady());
    }

    /// <summary>
    /// Waits until BattleSystem has fully initialised its party lists and
    /// called SetupBattle before we try to read unit data for the UI.
    /// BattleSystem.LateStart skips 1 frame then populates parties, so we
    /// skip 2 frames to be safe, then poll until units are present.
    /// </summary>
    private IEnumerator WaitForBattleReady()
    {
        yield return null;
        yield return null; // give BattleSystem.LateStart two frames to run

        // Poll until BattleSystem has living units (max 2 seconds)
        float timeout = 2f;
        while (battleSystem.GetLivingPlayers().Count == 0 && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (battleSystem.GetLivingPlayers().Count == 0)
        {
            Debug.LogError("[BattleUI] Timed out waiting for BattleSystem to populate parties. " +
                           "Check that your party GameObjects are assigned in the Inspector.");
            yield break;
        }

        battleReady = true;
        BuildPartyCards();
        BuildMoveButtons();
        BuildSwitchButtons();
        SetActionsEnabled(false);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Element Caching
    // ─────────────────────────────────────────────────────────────────────────

    private void CacheElements()
    {
        // Make the root panel visible — it may have been hidden in the UXML editor
        var rpgPanel = root.Q("RPGpanel");
        if (rpgPanel != null)
        {
            rpgPanel.style.visibility = Visibility.Visible;
            rpgPanel.style.opacity    = 1f;
        }
        else Debug.LogError("[BattleUI] Could not find RPGpanel in UXML.");

        enemyPanel  = root.Q("EnemyPanel");
        playerPanel = root.Q("PlayerSlot1");
        logContent  = root.Q("log-content");
        logScroll   = root.Q<ScrollView>("log-scroll");

        if (enemyPanel  == null) Debug.LogError("[BattleUI] EnemyPanel not found.");
        if (playerPanel == null) Debug.LogError("[BattleUI] PlayerSlot1 not found.");
        if (logContent  == null) Debug.LogError("[BattleUI] log-content not found.");
        if (logScroll   == null) Debug.LogError("[BattleUI] log-scroll not found.");

        var movesFoldout   = root.Q<Foldout>("moves");
        var specialFoldout = root.Q<Foldout>("special");

        if (movesFoldout   == null) Debug.LogError("[BattleUI] Foldout 'moves' not found.");
        if (specialFoldout == null) Debug.LogError("[BattleUI] Foldout 'special' not found.");

        movesContainer  = movesFoldout?.contentContainer;
        switchContainer = specialFoldout?.contentContainer;

        guardBtn = root.Q<Button>("guard");
        if (guardBtn == null) Debug.LogError("[BattleUI] Button 'guard' not found.");
        else guardBtn.clicked += OnGuardClicked;

        targetOverlay = root.Q("target-overlay");
        targetList    = root.Q("target-list");
        var cancelBtn = root.Q<Button>("target-cancel");
        if (cancelBtn != null) cancelBtn.clicked += HideTargetOverlay;
        else Debug.LogError("[BattleUI] Button 'target-cancel' not found.");

        resultOverlay  = root.Q("result-overlay");
        resultTitle    = root.Q<Label>("result-title");
        resultSubtitle = root.Q<Label>("result-subtitle");
        var resultBtn  = root.Q<Button>("result-btn");
        if (resultBtn != null) resultBtn.clicked += () => resultOverlay.AddToClassList("hidden");
        else Debug.LogError("[BattleUI] Button 'result-btn' not found.");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Event Subscriptions
    // ─────────────────────────────────────────────────────────────────────────

    // HOW THE EVENT SYSTEM WORKS:
    // BattleSystem declares static events like:
    //     public static event BattleEvent OnBattleMessage;
    // When BattleSystem calls Log(), it fires:
    //     OnBattleMessage?.Invoke(message);
    // This script subscribes with += in OnEnable and removes itself with -= in
    // OnDisable. That's all there is to it — the two scripts never directly
    // reference each other except through these event channels and the single
    // battleSystem Inspector reference used for sending commands back.

    private void SubscribeEvents()
    {
        BattleSystem.OnBattleMessage      += OnMessage;
        BattleSystem.OnStateChanged       += OnStateChanged;
        BattleSystem.OnStatsChanged       += OnStatsChanged;
        BattleSystem.OnActiveUnitsChanged += OnActiveUnitsChanged;
        BattleSystem.OnSwitchRequested    += OnSwitchRequested;
    }

    private void UnsubscribeEvents()
    {
        BattleSystem.OnBattleMessage      -= OnMessage;
        BattleSystem.OnStateChanged       -= OnStateChanged;
        BattleSystem.OnStatsChanged       -= OnStatsChanged;
        BattleSystem.OnActiveUnitsChanged -= OnActiveUnitsChanged;
        BattleSystem.OnSwitchRequested    -= OnSwitchRequested;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Party Card Construction
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildPartyCards()
    {
        if (enemyPanel == null || playerPanel == null)
        {
            Debug.LogError("[BattleUI] BuildPartyCards called but panels are null. Check UXML element names.");
            return;
        }

        ClearChildrenAfterLabel(enemyPanel);
        ClearChildrenAfterLabel(playerPanel);
        unitCards.Clear();

        // Use GetComponent — by this point TestUnit scripts have already run
        // AddComponent and assigned all stats via Awake
        foreach (GameObject obj in battleSystem.enemyPartyObjects)
        {
            var eu = obj.GetComponent<EnemyUnit>();
            if (eu != null)
            {
                var card = MakeUnitCard(eu, isPlayer: false);
                enemyPanel.Add(card);
                unitCards[eu] = card;
            }
            else
            {
                Debug.LogWarning($"[BattleUI] {obj.name} has no EnemyUnit component.");
            }
        }

        foreach (GameObject obj in battleSystem.playerPartyObjects)
        {
            var pu = obj.GetComponent<PlayerUnit>();
            if (pu != null)
            {
                var card = MakeUnitCard(pu, isPlayer: true);
                playerPanel.Add(card);
                unitCards[pu] = card;
            }
            else
            {
                Debug.LogWarning($"[BattleUI] {obj.name} has no PlayerUnit component.");
            }
        }
    }

    private static void ClearChildrenAfterLabel(VisualElement parent)
    {
        var toRemove = parent.Children().Where(c => c is not Label).ToList();
        foreach (var c in toRemove) parent.Remove(c);
    }

    private VisualElement MakeUnitCard(Unit unit, bool isPlayer)
    {
        var card = new VisualElement();
        card.AddToClassList("unit-card");
        card.AddToClassList(isPlayer ? "player-card" : "enemy-card");

        var nameLabel = new Label(unit.unitName);
        nameLabel.AddToClassList("unit-name");
        card.Add(nameLabel);

        var badge = new Label(unit.elementalType.ToString().ToUpper());
        badge.AddToClassList("type-badge");
        badge.AddToClassList($"type-{unit.elementalType.ToString().ToLower()}");
        card.Add(badge);

        card.Add(MakeBarRow("HP", "hp-lbl", "hp-bar-bg", "hp-bar-fill", "hp-txt"));

        if (isPlayer)
            card.Add(MakeBarRow("SP", "sp-lbl", "sp-bar-bg", "sp-bar-fill", "sp-txt"));

        var effectsRow = new VisualElement();
        effectsRow.AddToClassList("effects-row");
        effectsRow.name = "effects-row";
        card.Add(effectsRow);

        RefreshCard(unit);
        return card;
    }

    private static VisualElement MakeBarRow(
        string labelText, string lblClass, string bgClass, string fillClass, string txtClass)
    {
        var row = new VisualElement();
        row.AddToClassList(lblClass.Replace("-lbl", "-row"));

        var lbl = new Label(labelText);
        lbl.AddToClassList(lblClass);
        row.Add(lbl);

        var bg   = new VisualElement(); bg.AddToClassList(bgClass);
        var fill = new VisualElement(); fill.AddToClassList(fillClass);
        bg.Add(fill);
        row.Add(bg);

        var txt = new Label(); txt.AddToClassList(txtClass);
        row.Add(txt);

        return row;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Card Refresh
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshCard(Unit unit)
    {
        if (!unitCards.TryGetValue(unit, out var card)) return;

        float hpRatio = (float)unit.currentHealth / Mathf.Max(unit.maxHealth, 1);

        var hpFill = card.Q(className: "hp-bar-fill");
        if (hpFill != null)
        {
            hpFill.style.width = Length.Percent(hpRatio * 100f);
            hpFill.RemoveFromClassList("hp-mid");
            hpFill.RemoveFromClassList("hp-low");
            if      (hpRatio < 0.25f) hpFill.AddToClassList("hp-low");
            else if (hpRatio < 0.50f) hpFill.AddToClassList("hp-mid");
        }

        if (card.Q(className: "hp-txt") is Label hpTxt)
            hpTxt.text = $"{unit.currentHealth}/{unit.maxHealth}";

        if (card.Q(className: "sp-txt") is Label spTxt)
            spTxt.text = "—";

        card.EnableInClassList("fainted-card", !unit.IsAlive);
        card.RemoveFromClassList("active-card"); // cleared here, set in SetActiveHighlights

        var effectsRow = card.Q("effects-row");
        if (effectsRow != null)
        {
            effectsRow.Clear();
            foreach (var e in unit.currentEffects)
            {
                var b = new Label($"{e.effectName}({e.duration})");
                b.AddToClassList("effect-badge");
                effectsRow.Add(b);
            }
        }
    }

    private void RefreshAllCards(List<PlayerUnit> players, List<EnemyUnit> enemies)
    {
        foreach (var u in players) RefreshCard(u);
        foreach (var u in enemies) RefreshCard(u);
    }

    private void SetActiveHighlights(PlayerUnit activePlayer, EnemyUnit activeEnemy)
    {
        foreach (var card in unitCards.Values)
            card.RemoveFromClassList("active-card");

        if (activePlayer != null && unitCards.TryGetValue(activePlayer, out var pc))
            pc.AddToClassList("active-card");
        if (activeEnemy  != null && unitCards.TryGetValue(activeEnemy,  out var ec))
            ec.AddToClassList("active-card");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Move & Switch Buttons
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildMoveButtons()
    {
        if (movesContainer == null) { Debug.LogError("[BattleUI] movesContainer is null."); return; }
        for (int i = 0; i < 4; i++)
        {
            var btn = root.Q<Button>($"move-btn-{i}");
            if (btn == null) continue;

            // Remove any previously registered handler
            if (moveClickHandlers[i] != null)
                btn.clicked -= moveClickHandlers[i];

            int idx = i; // capture for closure
            moveClickHandlers[i] = () => OnMoveClicked(idx);
            btn.clicked += moveClickHandlers[i];
        }

        RefreshMoveButtons();
    }

    private void RefreshMoveButtons()
    {
        if (!battleReady) return;

        var activePlayer = battleSystem.GetLivingPlayers().FirstOrDefault();

        for (int i = 0; i < 4; i++)
        {
            var btn = root.Q<Button>($"move-btn-{i}");
            if (btn == null) continue;

            foreach (var t in new[] { "fire","water","nature","normal","absurd" })
                btn.RemoveFromClassList($"move-type-{t}");

            if (activePlayer?.moveList != null && i < activePlayer.moveList.Length)
            {
                Move move = activePlayer.moveList[i];
                btn.text = move.moveName;
                btn.SetEnabled(true);
                btn.AddToClassList($"move-type-{move.elementalType.ToString().ToLower()}");
            }
            else
            {
                btn.text = "—";
                btn.SetEnabled(false);
            }
        }
    }

    private void BuildSwitchButtons()
    {
        if (!battleReady) return;
        switchContainer.Clear();

        int partyIndex = 0;
        foreach (GameObject obj in battleSystem.playerPartyObjects)
        {
            var pu = obj.GetComponent<PlayerUnit>();
            if (pu == null) continue;

            var btn = new Button();
            btn.AddToClassList("switch-btn");
            btn.text = pu.unitName;
            btn.SetEnabled(pu.IsAlive);

            int captured = partyIndex;
            btn.clicked += () => OnSwitchClicked(captured);
            switchContainer.Add(btn);
            partyIndex++;
        }
    }

    private void RefreshSwitchButtons()
    {
        if (!battleReady) return;
        var btns    = switchContainer.Children().OfType<Button>().ToList();
        var players = battleSystem.playerPartyObjects
            .Select(o => o.GetComponent<PlayerUnit>()).Where(u => u != null).ToList();

        for (int i = 0; i < btns.Count && i < players.Count; i++)
            btns[i].SetEnabled(players[i].IsAlive);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Battle Log
    // ─────────────────────────────────────────────────────────────────────────

    private void AppendLog(string message)
    {
        var entry = new Label(message);
        entry.AddToClassList("log-entry");

        string lower = message.ToLower();
        if      (lower.Contains("super effective"))                          entry.AddToClassList("log-super");
        else if (lower.Contains("not very effective"))                       entry.AddToClassList("log-resist");
        else if (lower.Contains("afflicted") || lower.Contains("wore off"))  entry.AddToClassList("log-effect");
        else if (lower.Contains("---"))                                      entry.AddToClassList("log-system");

        logContent.Add(entry);
        logScroll.schedule.Execute(() => logScroll.ScrollTo(entry)).StartingIn(50);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Overlays
    // ─────────────────────────────────────────────────────────────────────────

    private void ShowTargetOverlay(int moveIndex)
    {
        pendingMoveIndex = moveIndex;
        targetList.Clear();

        var enemies = battleSystem.GetLivingEnemies();
        for (int i = 0; i < enemies.Count; i++)
        {
            var eu  = enemies[i];
            var btn = new Button();
            btn.AddToClassList("target-btn");
            btn.text = $"{eu.unitName}  {eu.currentHealth}/{eu.maxHealth} HP";

            int capturedI = i;
            btn.clicked += () =>
            {
                HideTargetOverlay();
                battleSystem.PlayerUseMove(pendingMoveIndex, capturedI);
            };
            targetList.Add(btn);
        }

        targetOverlay.RemoveFromClassList("hidden");
    }

    private void HideTargetOverlay() => targetOverlay.AddToClassList("hidden");

    private void ShowResultOverlay(BattleState result, string subtitle)
    {
        resultTitle.text = result == BattleState.Won ? "VICTORY" : "DEFEAT";

        resultTitle.style.color = result == BattleState.Won
            ? new StyleColor(new Color(0.94f, 0.75f, 0.25f))
            : new StyleColor(new Color(0.88f, 0.36f, 0.36f));

        resultSubtitle.text = subtitle;
        resultOverlay.RemoveFromClassList("hidden");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Action Panel Enable / Disable
    // ─────────────────────────────────────────────────────────────────────────

    private void SetActionsEnabled(bool enabled)
    {
        for (int i = 0; i < 4; i++)
            root.Q<Button>($"move-btn-{i}")?.SetEnabled(enabled);

        foreach (var btn in switchContainer.Children().OfType<Button>())
            btn.SetEnabled(enabled);

        guardBtn.SetEnabled(enabled);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Button Callbacks
    // ─────────────────────────────────────────────────────────────────────────

    private void OnMoveClicked(int index)
    {
        if (battleSystem.state != BattleState.PlayerTurn) return;
        SetActionsEnabled(false);

        if (battleSystem.GetLivingEnemies().Count > 1)
            ShowTargetOverlay(index);
        else
            battleSystem.PlayerUseMove(index, 0);
    }

    private void OnSwitchClicked(int partyIndex)
    {
        if (battleSystem.state != BattleState.PlayerTurn) return;
        SetActionsEnabled(false);
        battleSystem.PlayerSwitch(partyIndex);
    }

    private void OnGuardClicked()
    {
        if (battleSystem.state != BattleState.PlayerTurn) return;
        AppendLog("Guarding...");
        SetActionsEnabled(false);
        // Extend BattleSystem with a PlayerGuard() method when ready
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region BattleSystem Event Handlers
    // ─────────────────────────────────────────────────────────────────────────

    private void OnMessage(string msg) => AppendLog(msg);

    private void OnStateChanged(BattleState state)
    {
        switch (state)
        {
            case BattleState.PlayerTurn:
                if (!battleReady) break;
                RefreshMoveButtons();
                RefreshSwitchButtons();
                SetActionsEnabled(true);
                break;

            case BattleState.EnemyTurn:
                SetActionsEnabled(false);
                break;

            case BattleState.Won:
                SetActionsEnabled(false);
                StartCoroutine(ShowResultDelayed(state, "All enemies defeated!\nXP distributed to survivors."));
                break;

            case BattleState.Lost:
                SetActionsEnabled(false);
                StartCoroutine(ShowResultDelayed(state, "Your entire party has fallen."));
                break;
        }
    }

    private void OnStatsChanged(List<PlayerUnit> players, List<EnemyUnit> enemies)
    {
        if (!battleReady) return;
        RefreshAllCards(players, enemies);
        RefreshSwitchButtons();
    }

    private void OnActiveUnitsChanged(PlayerUnit activePlayer, EnemyUnit activeEnemy)
    {
        if (!battleReady) return;
        SetActiveHighlights(activePlayer, activeEnemy);
        RefreshMoveButtons();
    }

    private void OnSwitchRequested(List<PlayerUnit> available)
    {
        if (!battleReady) return;
        RefreshSwitchButtons();
    }

    private IEnumerator ShowResultDelayed(BattleState state, string subtitle)
    {
        yield return new WaitForSeconds(1.5f);
        ShowResultOverlay(state, subtitle);
    }

    #endregion
}
