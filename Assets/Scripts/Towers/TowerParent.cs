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
    protected float FRAME_TICK_CONSTANT = .016f;

    // tower specfic stuff
    public float[] RANGE;
    public int[] DAMAGE;

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
        range = RANGE[level];
        damage = DAMAGE[level];

        //Projectile.transform.localScale = new Vector3(projectileScale, projectileScale, projectileScale);
        
    }

    // assume no alternative upgrade paths right now
    protected void upgrade() {
        level++;
        range = RANGE[level];
        damage = DAMAGE[level];
    }

    protected GameObject SpawnInDirectionWithVelocity(GameObject prefab, float angleDegrees, float distance, float speed) {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        Vector3 spawnPosition = transform.position + direction * distance * (1 + projectileScale);
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
