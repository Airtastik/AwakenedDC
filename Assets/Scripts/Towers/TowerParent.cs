using UnityEngine;

public class TowerParent : MonoBehaviour
{
    public GameObject Projectile;
    public float projectileScale;
    public float fireDelay;

    protected float recoil = 0;
    // level is 0 indexed for efficiency
    protected int level = 0;
    protected float range;
    protected int damage;
    protected int upgradeCost;
    protected float FRAME_TICK_CONSTANT = .016f;

    // tower specfic stuff
    public float[] RANGE;
    public int[] DAMAGE;
    public int[] UPGRADE_COST;

    protected int MAX_LEVEL = 3;

    public LayerMask interactLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start() {
        if (RANGE == null || RANGE.Length == 0)
        {
            Debug.LogError("RANGE array is not set on " + gameObject.name);
            return;
        }

        if (DAMAGE == null || DAMAGE.Length == 0)
        {
            Debug.LogError("DAMAGE array is not set on " + gameObject.name);
            return;
        }

        if (UPGRADE_COST == null || UPGRADE_COST.Length == 0)
        {
            Debug.LogError("UPGRADE_COST array is not set on " + gameObject.name);
            return;
        }

        int validLevels = Mathf.Min(RANGE.Length, DAMAGE.Length, UPGRADE_COST.Length);
        if (validLevels <= 0)
        {
            Debug.LogError("Tower upgrade arrays are not configured correctly for " + gameObject.name);
            return;
        }

        if (MAX_LEVEL > validLevels - 1)
        {
            MAX_LEVEL = validLevels - 1;
            Debug.LogWarning($"Tower '{gameObject.name}' MAX_LEVEL reduced to {MAX_LEVEL} because its configured arrays only have {validLevels} levels.");
        }

        ApplyLevelStats();

        //Projectile.transform.localScale = new Vector3(projectileScale, projectileScale, projectileScale);
        
    }

    // ── Public accessors for HUD ─────────────────────────────────────────────
    public int   Level      => level;
    public int   MaxLevel   => MAX_LEVEL;
    public float Range      => range;
    public int   Damage     => damage;
    public bool  IsMaxLevel => level >= MAX_LEVEL;
    public int   UpgradeCost => upgradeCost;

    public int SellValue
    {
        get
        {
            if (TowerDefenseHUD.Instance == null) return 50;
            int t = GetComponent<Tower1>() != null ? 0 :
                    GetComponent<Tower2>() != null ? 1 :
                    GetComponent<Tower3>() != null ? 2 :
                    GetComponent<Tower4>() != null ? 3 : -1;
            return t >= 0 ? Mathf.RoundToInt(TowerDefenseHUD.Instance.GetCost(t) * 0.5f) : 50;
        }
    }

    private void ApplyLevelStats()
    {
        range = RANGE[level];
        damage = DAMAGE[level];
        upgradeCost = level < MAX_LEVEL ? UPGRADE_COST[level] : 0;
    }

    // assume no alternative upgrade paths right now
    // returns true if there is enough balance. Pass it in by reference from the UI
    // so that it is modified within the values stored intrinsically to this object
    public bool upgrade(ref int balance) {
        if (balance >= upgradeCost && level < MAX_LEVEL) {
            balance -= upgradeCost;
            level++;
            ApplyLevelStats();
            return true;
        }
        return false;
    }

    // Called by HUD upgrade button — uses internal currency via TowerDefenseHUD
    public bool TryUpgrade()
    {
        if (IsMaxLevel) return false;
        int cost = upgradeCost;
        if (TowerDefenseHUD.Instance == null || !TowerDefenseHUD.Instance.CanAfford(cost)) return false;
        TowerDefenseHUD.Instance.SpendCurrency(cost);
        level++;
        ApplyLevelStats();
        return true;
    }

    // just use this for the display value
    public int getUpgradeCost() {
        return upgradeCost;
    }

    protected GameObject SpawnInDirectionWithVelocity(GameObject prefab, float angleDegrees, float distance, float speed) {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        Vector3 spawnPosition = transform.position + direction * distance;
        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.LookRotation(direction));
        Projectile proj = obj.GetComponent<Projectile>();
        proj.Init(range, damage, projectileScale);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = direction * speed;
        }

        return obj;
    }

    protected GameObject findNearestEnemy() {
        Collider[] closeEnemies = Physics.OverlapSphere(transform.position, range, interactLayer);

        float closestDist = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (Collider col in closeEnemies)
        {
            if (col.gameObject.CompareTag("Enemy")) {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDist) { closestDist = dist; closestEnemy = col.gameObject; }
            }
        }

        return closestEnemy;
    }

    protected float GetAngleFromNorth(GameObject s) {
        if (s == null) return 0f;

        Vector3 direction = s.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        return angle;
    }

}
