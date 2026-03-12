using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(UIDocument))]
public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("References")]
    public BattleSystem battleSystem;

    // ── Cached elements ───────────────────────────────────────────────────────
    private VisualElement root;
    private VisualElement enemyPanel;
    private VisualElement playerPanel;
    private VisualElement queueList;
    private Label         battleMessage;
    private VisualElement movesContainer;
    private VisualElement specialsContainer;
    private VisualElement itemsContainer;
    private Button        guardBtn;
    private VisualElement targetOverlay;
    private VisualElement targetList;
    private VisualElement resultOverlay;
    private Label         resultTitle;
    private Label         resultSubtitle;

    private readonly System.Action[] moveClickHandlers    = new System.Action[3];
    private readonly System.Action[] specialClickHandlers = new System.Action[3];

    // ── State ─────────────────────────────────────────────────────────────────
    private int  pendingMoveIndex = -1;
    private readonly Dictionary<Unit, VisualElement> unitCards = new();
    private bool battleReady = false;

    // ─────────────────────────────────────────────────────────────────────────
    #region Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    void OnEnable()
    {
        try
        {
            Debug.Log("[BattleUI] OnEnable started.");
            root = GetComponent<UIDocument>().rootVisualElement;
            if (root == null) { Debug.LogError("[BattleUI] rootVisualElement is null — is UXML assigned?"); return; }
            CacheElements();
            SubscribeEvents();
            Debug.Log("[BattleUI] OnEnable complete.");

            // Re-run battle ready wait when re-enabled by BattleStageManager
            if (battleSystem != null)
                StartCoroutine(WaitForBattleReady());
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
        if (battleSystem == null) { Debug.LogError("[BattleUI] battleSystem not assigned!"); return; }
        // WaitForBattleReady is started by OnEnable which always fires before Start
    }

    private IEnumerator WaitForBattleReady()
    {
        // Stop any previous wait
        battleReady = false;

        float timeout = 10f;
        while (!battleSystem.IsReady && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!battleSystem.IsReady)
        {
            Debug.LogError("[BattleUI] Timed out waiting for BattleSystem. Check Wait For Signal is ticked and BattleStageManager is in the scene.");
            yield break;
        }

        battleReady = true;
        BuildPartyCards();
        BuildMoveButtons();
        BuildSpecialButtons();
        BuildItemButtons();
        SetActionsEnabled(false);
        Debug.Log("[BattleUI] UI populated.");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Element Caching
    // ─────────────────────────────────────────────────────────────────────────

    private void CacheElements()
    {
        var rpgPanel = root.Q("RPGpanel");
        if (rpgPanel != null) { rpgPanel.style.visibility = Visibility.Visible; rpgPanel.style.opacity = 1f; }
        else Debug.LogError("[BattleUI] RPGpanel not found.");

        enemyPanel  = root.Q("EnemyPanel");
        playerPanel = root.Q("PlayerSlot1");
        queueList   = root.Q("queue-list");

        if (enemyPanel  == null) Debug.LogError("[BattleUI] EnemyPanel not found.");
        if (playerPanel == null) Debug.LogError("[BattleUI] PlayerSlot1 not found.");
        if (queueList   == null) Debug.LogError("[BattleUI] queue-list not found.");

        battleMessage = root.Q<Label>("battle-message");
        if (battleMessage == null) Debug.LogError("[BattleUI] battle-message not found.");

        var movesFoldout = root.Q<Foldout>("moves");
        if (movesFoldout == null) Debug.LogError("[BattleUI] Foldout 'moves' not found.");
        movesContainer = movesFoldout?.contentContainer;

        var specialsFoldout = root.Q<Foldout>("specials");
        if (specialsFoldout == null) Debug.LogError("[BattleUI] Foldout 'specials' not found.");
        specialsContainer = specialsFoldout?.contentContainer;

        var itemsFoldout = root.Q<Foldout>("items");
        if (itemsFoldout == null) Debug.LogError("[BattleUI] Foldout 'items' not found.");
        itemsContainer = itemsFoldout?.contentContainer;

        guardBtn = root.Q<Button>("guard");
        if (guardBtn == null) Debug.LogError("[BattleUI] Button 'guard' not found.");
        else guardBtn.clicked += OnGuardClicked;

        targetOverlay = root.Q("target-overlay");
        targetList    = root.Q("target-list");
        var cancelBtn = root.Q<Button>("target-cancel");
        if (cancelBtn != null) cancelBtn.clicked += HideTargetOverlay;
        else Debug.LogError("[BattleUI] target-cancel not found.");

        resultOverlay  = root.Q("result-overlay");
        resultTitle    = root.Q<Label>("result-title");
        resultSubtitle = root.Q<Label>("result-subtitle");
        var resultBtn  = root.Q<Button>("result-btn");
        if (resultBtn != null) resultBtn.clicked += () => resultOverlay.AddToClassList("hidden");
        else Debug.LogError("[BattleUI] result-btn not found.");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Events
    // ─────────────────────────────────────────────────────────────────────────

    private void SubscribeEvents()
    {
        BattleSystem.OnBattleMessage      += OnMessage;
        BattleSystem.OnStateChanged       += OnStateChanged;
        BattleSystem.OnStatsChanged       += OnStatsChanged;
        BattleSystem.OnActiveUnitsChanged += OnActiveUnitsChanged;
        BattleSystem.OnTurnQueueUpdated   += OnTurnQueueUpdated;
        BattleSystem.OnInventoryChanged   += OnInventoryChanged;
    }

    private void UnsubscribeEvents()
    {
        BattleSystem.OnBattleMessage      -= OnMessage;
        BattleSystem.OnStateChanged       -= OnStateChanged;
        BattleSystem.OnStatsChanged       -= OnStatsChanged;
        BattleSystem.OnActiveUnitsChanged -= OnActiveUnitsChanged;
        BattleSystem.OnTurnQueueUpdated   -= OnTurnQueueUpdated;
        BattleSystem.OnInventoryChanged   -= OnInventoryChanged;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Turn Queue
    // ─────────────────────────────────────────────────────────────────────────

    private void OnTurnQueueUpdated(List<Unit> queue)
    {
        if (!battleReady || queueList == null) return;
        queueList.Clear();

        for (int i = 0; i < queue.Count; i++)
        {
            Unit unit = queue[i];
            bool isPlayer = unit is PlayerUnit;
            bool isNext   = i == 0;

            var entry = new VisualElement();
            entry.AddToClassList("queue-entry");
            entry.AddToClassList(isPlayer ? "queue-player" : "queue-enemy");
            if (isNext) entry.AddToClassList("queue-next");

            // Position number
            var idx = new Label(isNext ? "▶" : (i + 1).ToString());
            idx.AddToClassList("queue-index");
            entry.Add(idx);

            // Unit name
            var name = new Label(unit.unitName);
            name.AddToClassList("queue-name");
            entry.Add(name);

            // Elemental type pip
            var pip = new Label(unit.elementalType.ToString().ToUpper().Substring(0, 3));
            pip.AddToClassList("queue-type-pip");
            pip.AddToClassList($"pip-{unit.elementalType.ToString().ToLower()}");
            entry.Add(pip);

            queueList.Add(entry);
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Party Cards
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildPartyCards()
    {
        if (enemyPanel == null || playerPanel == null)
        {
            Debug.LogError("[BattleUI] Panels are null — check UXML names.");
            return;
        }

        ClearChildrenAfterLabel(enemyPanel);
        ClearChildrenAfterLabel(playerPanel);
        unitCards.Clear();

        // Use the actual spawned unit lists, not the raw GameObject inspector lists
        foreach (EnemyUnit eu in battleSystem.EnemyParty)
        {
            if (eu == null) continue;
            var c = MakeUnitCard(eu, false);
            enemyPanel.Add(c);
            unitCards[eu] = c;
            Debug.Log($"[BattleUI] Enemy card: {eu.unitName}");
        }

        foreach (PlayerUnit pu in battleSystem.PlayerParty)
        {
            if (pu == null) continue;
            var c = MakeUnitCard(pu, true);
            playerPanel.Add(c);
            unitCards[pu] = c;
            Debug.Log($"[BattleUI] Player card: {pu.unitName}");
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
        {
            var spRow = new VisualElement();
            spRow.AddToClassList("sp-row");
            var spLbl = new Label("SP"); spLbl.AddToClassList("sp-lbl"); spRow.Add(spLbl);
            var diamonds = new VisualElement(); diamonds.AddToClassList("sp-diamonds"); diamonds.name = "sp-diamonds";
            for (int i = 0; i < 5; i++)
            {
                var d = new VisualElement(); d.AddToClassList("sp-diamond"); d.name = $"sp-d-{i}";
                diamonds.Add(d);
            }
            spRow.Add(diamonds);
            card.Add(spRow);
        }

        var effectsRow = new VisualElement();
        effectsRow.AddToClassList("effects-row");
        effectsRow.name = "effects-row";
        card.Add(effectsRow);

        RefreshCard(unit);
        return card;
    }

    private static VisualElement MakeBarRow(string labelText, string lblClass, string bgClass, string fillClass, string txtClass)
    {
        var row  = new VisualElement(); row.AddToClassList(lblClass.Replace("-lbl", "-row"));
        var lbl  = new Label(labelText); lbl.AddToClassList(lblClass); row.Add(lbl);
        var bg   = new VisualElement(); bg.AddToClassList(bgClass);
        var fill = new VisualElement(); fill.AddToClassList(fillClass); bg.Add(fill); row.Add(bg);
        var txt  = new Label(); txt.AddToClassList(txtClass); row.Add(txt);
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

        if (card.Q(className: "hp-txt") is Label hpTxt) hpTxt.text = $"{unit.currentHealth}/{unit.maxHealth}";

        // SP diamonds — light up filled ones, darken spent ones
        var diamonds = card.Q("sp-diamonds");
        if (diamonds != null && unit.maxSP > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                var d = card.Q($"sp-d-{i}");
                if (d == null) continue;
                bool filled = i < unit.currentSP;
                d.EnableInClassList("sp-empty", !filled);
            }
        }

        card.EnableInClassList("fainted-card", !unit.IsAlive);
        card.RemoveFromClassList("active-card");

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
        foreach (var card in unitCards.Values) card.RemoveFromClassList("active-card");
        if (activePlayer != null && unitCards.TryGetValue(activePlayer, out var pc)) pc.AddToClassList("active-card");
        if (activeEnemy  != null && unitCards.TryGetValue(activeEnemy,  out var ec)) ec.AddToClassList("active-card");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Move & Switch Buttons
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildMoveButtons()
    {
        if (movesContainer == null) { Debug.LogError("[BattleUI] movesContainer null."); return; }

        for (int i = 0; i < 3; i++)
        {
            var btn = root.Q<Button>($"move-btn-{i}");
            if (btn == null) continue;
            if (moveClickHandlers[i] != null) btn.clicked -= moveClickHandlers[i];
            int idx = i;
            moveClickHandlers[i] = () => OnMoveClicked(idx);
            btn.clicked += moveClickHandlers[i];
        }
        RefreshMoveButtons();
    }

    private void BuildSpecialButtons()
    {
        if (specialsContainer == null) { Debug.LogError("[BattleUI] specialsContainer null."); return; }

        for (int i = 0; i < 3; i++)
        {
            var btn = root.Q<Button>($"special-btn-{i}");
            if (btn == null) continue;
            if (specialClickHandlers[i] != null) btn.clicked -= specialClickHandlers[i];
            int idx = i + 3; // specials are moves 3, 4, 5
            specialClickHandlers[i] = () => OnMoveClicked(idx);
            btn.clicked += specialClickHandlers[i];
        }
        RefreshSpecialButtons();
    }

    private void RefreshMoveButtons()
    {
        if (!battleReady) return;
        RefreshMoveBtnSet(battleSystem.ActivePlayer, "move-btn-", 0, 3);
    }

    private void RefreshSpecialButtons()
    {
        if (!battleReady) return;
        RefreshMoveBtnSet(battleSystem.ActivePlayer, "special-btn-", 3, 3);
    }

    private void RefreshMoveBtnSet(PlayerUnit activePlayer, string btnPrefix, int moveOffset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var btn = root.Q<Button>($"{btnPrefix}{i}");
            if (btn == null) continue;

            foreach (var t in new[] { "fire","water","nature","normal","absurd" })
                btn.RemoveFromClassList($"move-type-{t}");
            btn.RemoveFromClassList("move-no-sp");

            int moveIndex = moveOffset + i;
            if (activePlayer?.moveList != null && moveIndex < activePlayer.moveList.Length)
            {
                Move move      = activePlayer.moveList[moveIndex];
                bool canAfford = activePlayer.HasSP(move.spCost);
                string spTag   = move.spCost > 0 ? $"  [{move.spCost} SP]" : "  [free]";
                btn.text = move.moveName + spTag;
                btn.SetEnabled(canAfford);
                btn.AddToClassList($"move-type-{move.elementalType.ToString().ToLower()}");
                if (!canAfford) btn.AddToClassList("move-no-sp");
            }
            else
            {
                btn.text = "—";
                btn.SetEnabled(false);
            }
        }
    }

    private void BuildItemButtons()
    {
        if (itemsContainer == null) return;
        itemsContainer.Clear();

        var inventory = PlayerInventory.Instance;
        if (inventory == null || inventory.items.Count == 0)
        {
            var empty = new Label("No items.");
            empty.AddToClassList("empty-label");
            itemsContainer.Add(empty);
            return;
        }

        // Show one button per unique item type, with count badge
        foreach (Item item in inventory.GetUniqueItems())
        {
            int count = inventory.CountOf(item.itemName);
            var btn   = new Button();
            btn.AddToClassList("item-btn");
            btn.text = $"{item.itemName}  x{count}";
            btn.tooltip = item.description;

            Item captured = item;
            btn.clicked += () => OnItemClicked(captured);
            itemsContainer.Add(btn);
        }
    }

    private void RefreshItemButtons()
    {
        if (!battleReady) return;
        BuildItemButtons(); // rebuild is simpler than diffing counts
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Message
    // ─────────────────────────────────────────────────────────────────────────

    private void ShowMessage(string message)
    {
        if (battleMessage == null) return;
        battleMessage.text = message;
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
            var eu = enemies[i];
            var btn = new Button();
            btn.AddToClassList("target-btn");
            btn.text = $"{eu.unitName}  {eu.currentHealth}/{eu.maxHealth} HP";
            int capturedI = i;
            btn.clicked += () => { HideTargetOverlay(); battleSystem.PlayerUseMove(pendingMoveIndex, capturedI); };
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
    #region Actions Enable
    // ─────────────────────────────────────────────────────────────────────────

    private void SetActionsEnabled(bool enabled)
    {
        for (int i = 0; i < 3; i++) root.Q<Button>($"move-btn-{i}")?.SetEnabled(enabled);
        for (int i = 0; i < 3; i++) root.Q<Button>($"special-btn-{i}")?.SetEnabled(enabled);
        guardBtn?.SetEnabled(enabled);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Button Callbacks
    // ─────────────────────────────────────────────────────────────────────────

    private void OnMoveClicked(int index)
    {
        if (battleSystem.state != BattleState.PlayerTurn) return;
        SetActionsEnabled(false);
        if (battleSystem.GetLivingEnemies().Count > 1) ShowTargetOverlay(index);
        else battleSystem.PlayerUseMove(index, 0);
    }

    private void OnGuardClicked()
    {
        if (battleSystem.state != BattleState.PlayerTurn) return;
        ShowMessage("Guarding...");
        SetActionsEnabled(false);
    }

    private void OnItemClicked(Item item)
    {
        if (battleSystem.state != BattleState.PlayerTurn) return;
        SetActionsEnabled(false);
        // Items that need a specific target show the party picker; others auto-target
        if (item.effect == ItemEffect.ReviveTarget)
            ShowPartyTargetOverlay(item);
        else
            battleSystem.PlayerUseItem(item);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region BattleSystem Event Handlers
    // ─────────────────────────────────────────────────────────────────────────

    private void OnMessage(string msg) => ShowMessage(msg);

    private void OnStateChanged(BattleState state)
    {
        switch (state)
        {
            case BattleState.PlayerTurn:
                if (!battleReady) break;
                RefreshMoveButtons();
                RefreshSpecialButtons();
                SetActionsEnabled(true);
                break;
            case BattleState.EnemyTurn:
                SetActionsEnabled(false);
                break;
            case BattleState.Won:
                SetActionsEnabled(false);
                StartCoroutine(ShowResultDelayed(state, "All enemies defeated!\nXP distributed."));
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
    }

    private void OnActiveUnitsChanged(PlayerUnit activePlayer, EnemyUnit activeEnemy)
    {
        if (!battleReady) return;
        SetActiveHighlights(activePlayer, activeEnemy);
        RefreshMoveButtons();
        RefreshSpecialButtons();
    }

    private void OnInventoryChanged(List<Item> items)
    {
        if (!battleReady) return;
        RefreshItemButtons();
    }

    private void ShowPartyTargetOverlay(Item item)
    {
        targetList.Clear();
        var allPlayers = battleSystem.playerPartyObjects
            .Select((o, i) => (unit: o?.GetComponent<PlayerUnit>(), index: i))
            .Where(p => p.unit != null && !p.unit.IsAlive)
            .ToList();

        foreach (var (unit, index) in allPlayers)
        {
            var btn = new Button();
            btn.AddToClassList("target-btn");
            btn.text = $"{unit.unitName}  (fainted)";
            int captured = index;
            btn.clicked += () => { HideTargetOverlay(); battleSystem.PlayerUseItem(item, captured); };
            targetList.Add(btn);
        }

        if (targetList.childCount == 0)
        {
            // No fainted targets — just use it normally
            HideTargetOverlay();
            battleSystem.PlayerUseItem(item);
            return;
        }

        targetOverlay.RemoveFromClassList("hidden");
    }

    private IEnumerator ShowResultDelayed(BattleState state, string subtitle)
    {
        yield return new WaitForSeconds(1.5f);
        ShowResultOverlay(state, subtitle);
    }

    #endregion
}
