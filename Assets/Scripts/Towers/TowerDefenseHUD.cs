using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TowerDefenseHUD : MonoBehaviour
{
    public static TowerDefenseHUD Instance { get; private set; }

    [Header("References")]
    public WaveSpawner waveSpawner;
    public EnemyHealth enemyHealth;
    public EnemyMovement enemyMovement;

    [Header("Tower Prefabs (matched to buttons 1-4)")]
    public GameObject tower1Prefab;
    public GameObject tower2Prefab;
    public GameObject tower3Prefab;
    public GameObject tower4Prefab;

    [Header("Starting Values")]
    public int startingHealth   = 100;
    public int startingCurrency = 500;
    public int totalWaves       = 5;

    // ── State ─────────────────────────────────────────────────────────────────
    private int            health;
    private int            currency;
    private int            selectedShopIndex = -1;  // which shop button is active
    private TowerParent    selectedTower;            // a placed tower the player clicked

    // ── Tower names / costs ───────────────────────────────────────────────────
    private readonly string[] towerNames = { "CATHERINE", "ROMAN", "SILVIA", "DIMITRI" };
    private readonly int[]    towerCosts = { 100, 150, 200, 250 };

    // ── UI refs ───────────────────────────────────────────────────────────────
    private Label         healthLabel, waveLabel, enemyLabel, currencyLabel, waveMessage;
    private VisualElement[] shopBtns     = new VisualElement[4];
    private VisualElement upgradePanel;
    private Label         upgradeTowerName, upgradeLevel, upgradeRange, upgradeDamage;
    private Button        upgradeBtn, sellBtn, deselectBtn, startBtn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var root = GetComponent<UIDocument>().rootVisualElement;

        // Top bar
        healthLabel   = root.Q<Label>("health-value");
        waveLabel     = root.Q<Label>("wave-value");
        enemyLabel    = root.Q<Label>("enemy-value");
        currencyLabel = root.Q<Label>("currency-value");
        waveMessage   = root.Q<Label>("wave-message");

        // Shop buttons
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            shopBtns[i] = root.Q($"tower-btn-{i + 1}");
            shopBtns[i]?.RegisterCallback<ClickEvent>(_ => SelectShopTower(idx));
        }

        startBtn = root.Q<Button>("btn-start-wave");
        startBtn?.RegisterCallback<ClickEvent>(_ => StartWave());

        // Upgrade panel
        upgradePanel    = root.Q("upgrade-panel");
        upgradeTowerName= root.Q<Label>("upgrade-tower-name");
        upgradeLevel    = root.Q<Label>("upgrade-level");
        upgradeRange    = root.Q<Label>("upgrade-range");
        upgradeDamage   = root.Q<Label>("upgrade-damage");
        upgradeBtn      = root.Q<Button>("upgrade-btn");
        sellBtn         = root.Q<Button>("sell-btn");
        deselectBtn     = root.Q<Button>("deselect-btn");

        upgradeBtn?.RegisterCallback<ClickEvent>(_ => OnUpgradeClicked());
        sellBtn?.RegisterCallback<ClickEvent>(_ => OnSellClicked());
        deselectBtn?.RegisterCallback<ClickEvent>(_ => DeselectPlacedTower());

        health   = startingHealth;
        currency = startingCurrency;
        RefreshAll();
    }

    void Update()
    {
        // Hotkeys 1-4 for shop
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectShopTower(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectShopTower(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectShopTower(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectShopTower(3);
        if (Input.GetKeyDown(KeyCode.Escape)) DeselectAll();
        if (!isRunning())
        {
            startBtn.RemoveFromClassList("running");
        }

        if (waveSpawner != null)
            enemyLabel.text = waveSpawner.EnemiesAlive.ToString();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ═════════════════════════════════════════════════════════════════════════

    public void SetHealth(int v)   { health = Mathf.Max(0, v); healthLabel.text = health.ToString(); }
    public int GetHealth() { return health; }
    public void TakeDamage(int a)  => SetHealth(health - a);
    public void SetCurrency(int v) { currency = Mathf.Max(0, v); currencyLabel.text = currency.ToString(); RefreshAffordability(); }
    public void AddCurrency(int a) => SetCurrency(currency + a);
    public void SpendCurrency(int a) => SetCurrency(currency - a);
    public void SetWave(int current) => waveLabel.text = $"{current} / {totalWaves}";
    public int  GetCost(int index)   => (index >= 0 && index < towerCosts.Length) ? towerCosts[index] : 0;
    public bool CanAfford(int cost)  => currency >= cost;
    public bool isRunning() => waveSpawner.getRunning();

    public int SelectedShopIndex => selectedShopIndex;

    public void ShowWaveMessage(string msg, float duration = 2.5f)
    {
        waveMessage.text = msg;
        waveMessage.RemoveFromClassList("hidden");
        StartCoroutine(HideMessageAfter(duration));
    }

    /// <summary>
    /// Call this from your tower placement / click detection script when the
    /// player clicks on a placed tower in the world.
    /// </summary>
    public void SelectPlacedTower(TowerParent tower)
    {
        if (tower == null) return;
        selectedTower     = tower;
        selectedShopIndex = -1;
        RefreshShopSelection();
        RefreshUpgradePanel();
        upgradePanel.RemoveFromClassList("hidden");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ═════════════════════════════════════════════════════════════════════════

    private void SelectShopTower(int index)
    {
        // Clicking the same button again deselects
        selectedShopIndex = (selectedShopIndex == index) ? -1 : index;
        Debug.Log("Selected Tower: " + selectedShopIndex);
        DeselectPlacedTower();
        RefreshShopSelection();
    }

    private void DeselectPlacedTower()
    {
        selectedTower = null;
        upgradePanel.AddToClassList("hidden");
    }

    private void StartWave()
    {
        if (isRunning())
        {
            return;
        }
        waveSpawner.Clicked();
        startBtn.AddToClassList("running");
    }

    private void DeselectAll()
    {
        selectedShopIndex = -1;
        DeselectPlacedTower();
        RefreshShopSelection();
    }

    private void OnUpgradeClicked()
    {
        if (selectedTower == null) return;
        if (selectedTower.IsMaxLevel) return;

        int cost = selectedTower.UpgradeCost;
        if (!CanAfford(cost))
        {
            ShowWaveMessage("NOT ENOUGH CREDITS", 1.5f);
            return;
        }

        SpendCurrency(cost);
        selectedTower.TryUpgrade();
        RefreshUpgradePanel();
    }

    private void OnSellClicked()
    {
        if (selectedTower == null) return;
        int refund = selectedTower.SellValue;
        AddCurrency(refund);
        Destroy(selectedTower.gameObject);
        DeselectPlacedTower();
    }

    private void RefreshUpgradePanel()
    {
        if (selectedTower == null) return;

        upgradeTowerName.text = selectedTower.gameObject.name.Replace("(Clone)", "").Trim();
        upgradeLevel.text     = $"LVL {selectedTower.Level + 1}";
        upgradeRange.text     = $"RNG  {selectedTower.Range:0}";
        upgradeDamage.text    = $"DMG  {selectedTower.Damage}";

        if (selectedTower.IsMaxLevel)
        {
            upgradeBtn.text = "MAX LEVEL";
            upgradeBtn.AddToClassList("max-level");
            upgradeBtn.SetEnabled(false);
        }
        else
        {
            int cost = selectedTower.UpgradeCost;
            upgradeBtn.text = $"UPGRADE  ◈ {cost}";
            upgradeBtn.RemoveFromClassList("max-level");
            upgradeBtn.SetEnabled(CanAfford(cost));
        }

        sellBtn.text = $"SELL  ◈ {selectedTower.SellValue}";
    }

    private void RefreshShopSelection()
    {
        for (int i = 0; i < shopBtns.Length; i++)
        {
            if (shopBtns[i] == null) continue;
            if (i == selectedShopIndex) shopBtns[i].AddToClassList("selected");
            else                        shopBtns[i].RemoveFromClassList("selected");
        }
    }

    private void RefreshAffordability()
    {
        for (int i = 0; i < shopBtns.Length; i++)
        {
            if (shopBtns[i] == null) continue;
            if (CanAfford(towerCosts[i])) shopBtns[i].RemoveFromClassList("unaffordable");
            else                          shopBtns[i].AddToClassList("unaffordable");
        }
        // Refresh upgrade button affordability if a tower is selected
        if (selectedTower != null) RefreshUpgradePanel();
    }

    private void RefreshAll()
    {
        healthLabel.text   = health.ToString();
        currencyLabel.text = currency.ToString();
        waveLabel.text     = $"1 / {totalWaves}";
        enemyLabel.text    = "0";
        RefreshAffordability();
    }

    private IEnumerator HideMessageAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        waveMessage.AddToClassList("hidden");
    }
}
