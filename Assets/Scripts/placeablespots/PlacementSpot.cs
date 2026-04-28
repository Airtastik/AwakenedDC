using UnityEngine;

/// <summary>
/// Attach to each Cube child under "Placeable Spots".
/// Highlights green on hover if unoccupied, red if occupied.
/// Call Occupy() when a tower is placed here, Vacate() when sold.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PlacementSpot : MonoBehaviour
{
    [Header("Colours")]
    public Color normalColour    = new Color(1f, 1f, 1f, 0f);   // transparent / default
    public Color hoverFreeColour = new Color(0.2f, 1f, 0.2f, 0.6f);
    public Color hoverOccupied   = new Color(1f, 0.2f, 0.2f, 0.6f);

    // ── State ─────────────────────────────────────────────────────────────────
    public bool  IsOccupied { get; private set; } = false;
    private TowerParent placedTower;

    private Renderer  rend;
    private Material  mat;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        // Use instance material so highlights don't affect all cubes at once
        mat  = rend.material;
        SetColour(normalColour);
    }

    // ── Mouse events ──────────────────────────────────────────────────────────

    void OnMouseEnter()
    {
        SetColour(IsOccupied ? hoverOccupied : hoverFreeColour);
    }

    void OnMouseExit()
    {
        SetColour(normalColour);
    }

    void OnMouseDown()
    {
        if (IsOccupied)
        {
            // Clicking an occupied spot selects its tower in the HUD
            if (placedTower != null)
                TowerDefenseHUD.Instance?.SelectPlacedTower(placedTower);
            return;
        }

        // Clicking a free spot places the selected shop tower
        int idx = TowerDefenseHUD.Instance != null
                  ? TowerDefenseHUD.Instance.SelectedShopIndex : -1;

        if (idx < 0) return;
        if (!TowerDefenseHUD.Instance.CanAfford(TowerDefenseHUD.Instance.GetCost(idx))) return;

        PlaceTower(idx);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Occupy(TowerParent tower)
    {
        IsOccupied  = true;
        placedTower = tower;
    }

    public void Vacate()
    {
        IsOccupied  = false;
        placedTower = null;
        SetColour(normalColour);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void PlaceTower(int shopIndex)
    {
        var hud = TowerDefenseHUD.Instance;
        if (hud == null) return;

        GameObject prefab = shopIndex switch
        {
            0 => hud.tower1Prefab,
            1 => hud.tower2Prefab,
            2 => hud.tower3Prefab,
            3 => hud.tower4Prefab,
            _ => null
        };

        if (prefab == null) return;

        // Spawn tower slightly above the tile so it sits on top
        Vector3 spawnPos = transform.position + Vector3.up * (transform.localScale.y * 0.5f);
        GameObject obj   = Instantiate(prefab, spawnPos, Quaternion.identity);

        TowerParent tower = obj.GetComponent<TowerParent>();
        if (tower == null) { Destroy(obj); return; }

        hud.SpendCurrency(hud.GetCost(shopIndex));
        Occupy(tower);

        // Let the tower know which spot it owns so selling it vacates the spot
        TowerSpotLink link = obj.AddComponent<TowerSpotLink>();
        link.spot = this;

        Debug.Log($"[PlacementSpot] Placed {prefab.name} at {transform.position}");
    }

    private void SetColour(Color c)
    {
        if (mat != null) mat.color = c;
    }
}
