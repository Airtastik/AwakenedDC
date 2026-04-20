using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controller for the Tower Defence HUD.
///
/// Setup:
///   1. Create empty GameObject "TowerDefenseUI".
///   2. Add UIDocument (Source: TowerDefenseHUD.uxml, PanelSettings sort 5).
///   3. Add this script.
///   4. Assign waveSpawner, and the four tower prefabs in the Inspector.
///   5. Call TowerDefenseHUD.Instance.SetHealth / SetCurrency etc. from your game logic.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class TowerDefenseHUD : MonoBehaviour
{
    public static TowerDefenseHUD Instance { get; private set; }

    [Header("References")]
    public WaveSpawner waveSpawner;

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
    private int health;
    private int currency;
    private int selectedTower = -1; // -1 = none

    // ── UI refs ───────────────────────────────────────────────────────────────
    private Label         healthLabel;
    private Label         waveLabel;
    private Label         enemyLabel;
    private Label         currencyLabel;
    private Label         waveMessage;
    private VisualElement[] towerBtns = new VisualElement[4];

    // ── Tower costs ───────────────────────────────────────────────────────────
    private readonly int[] towerCosts = { 100, 150, 200, 250 };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var root = GetComponent<UIDocument>().rootVisualElement;

        healthLabel   = root.Q<Label>("health-value");
        waveLabel     = root.Q<Label>("wave-value");
        enemyLabel    = root.Q<Label>("enemy-value");
        currencyLabel = root.Q<Label>("currency-value");
        waveMessage   = root.Q<Label>("wave-message");

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            towerBtns[i] = root.Q($"tower-btn-{i + 1}");
            towerBtns[i]?.RegisterCallback<ClickEvent>(_ => SelectTower(idx));
        }

        health   = startingHealth;
        currency = startingCurrency;

        RefreshAll();
    }

    void Update()
    {
        // Hotkeys 1-4 to select towers
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectTower(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectTower(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectTower(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectTower(3);

        // Escape deselects
        if (Input.GetKeyDown(KeyCode.Escape)) DeselectTower();

        // Sync enemy count from WaveSpawner every frame
        if (waveSpawner != null)
            enemyLabel.text = waveSpawner.EnemiesAlive.ToString();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PUBLIC API — call from WaveSpawner, EnemyHealth, etc.
    // ═════════════════════════════════════════════════════════════════════════

    public void SetHealth(int value)
    {
        health = Mathf.Max(0, value);
        healthLabel.text = health.ToString();
    }

    public void TakeDamage(int amount) => SetHealth(health - amount);

    public void SetCurrency(int value)
    {
        currency = Mathf.Max(0, value);
        currencyLabel.text = currency.ToString();
        RefreshAffordability();
    }

    public void AddCurrency(int amount) => SetCurrency(currency + amount);
    public void SpendCurrency(int amount) => SetCurrency(currency - amount);

    public void SetWave(int current)
    {
        waveLabel.text = $"{current} / {totalWaves}";
    }

    public void ShowWaveMessage(string msg, float duration = 2.5f)
    {
        waveMessage.text = msg;
        waveMessage.RemoveFromClassList("hidden");
        StartCoroutine(HideMessageAfter(duration));
    }

    public int SelectedTowerIndex => selectedTower;
    public int GetCost(int index) => (index >= 0 && index < towerCosts.Length) ? towerCosts[index] : 0;
    public bool CanAfford(int index) => currency >= GetCost(index);

    // ═════════════════════════════════════════════════════════════════════════
    // INTERNAL
    // ═════════════════════════════════════════════════════════════════════════

    private void SelectTower(int index)
    {
        if (!CanAfford(index)) return;

        selectedTower = (selectedTower == index) ? -1 : index; // toggle
        RefreshSelection();
    }

    private void DeselectTower()
    {
        selectedTower = -1;
        RefreshSelection();
    }

    private void RefreshAll()
    {
        healthLabel.text   = health.ToString();
        currencyLabel.text = currency.ToString();
        waveLabel.text     = $"1 / {totalWaves}";
        enemyLabel.text    = "0";
        RefreshAffordability();
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < towerBtns.Length; i++)
        {
            if (towerBtns[i] == null) continue;
            if (i == selectedTower)
                towerBtns[i].AddToClassList("selected");
            else
                towerBtns[i].RemoveFromClassList("selected");
        }
    }

    private void RefreshAffordability()
    {
        for (int i = 0; i < towerBtns.Length; i++)
        {
            if (towerBtns[i] == null) continue;
            if (CanAfford(i))
                towerBtns[i].RemoveFromClassList("unaffordable");
            else
                towerBtns[i].AddToClassList("unaffordable");
        }
    }

    private IEnumerator HideMessageAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        waveMessage.AddToClassList("hidden");
    }
}
