using UnityEngine;

public class Tower4 : TowerParent
{
    private Vector3 initalPosition;
    private GameObject target;
    public float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        base.Start();
        initalPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (target == null) {
            GameObject NearestEnemy = findNearestEnemy();
            if (NearestEnemy != null && (NearestEnemy.transform.position - initalPosition).magnitude <= range)
                target = NearestEnemy;
            else {
                Vector3 direction = initalPosition - transform.position;
                direction.y = 0f;
                if (direction.magnitude > .1f) {
                    float offset = speed;
                    if (direction.magnitude < speed)
                        offset = direction.magnitude;
                    rb.linearVelocity = direction * offset;
                }
            }
        }

        if (target != null) {
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            float offset = speed;
            if (direction.magnitude < speed)
                offset = direction.magnitude;
            rb.linearVelocity = direction * offset;
        } 

        if (recoil > 0)
            recoil -= FRAME_TICK_CONSTANT;
        
        if (target != null && IsWithinXZRange(target)) {
            if (recoil <= 0) {
                EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
                if (enemyHealth != null) {
                    enemyHealth.TakeDamage(damage);
                }
                recoil = fireDelay + FRAME_TICK_CONSTANT;
            }
        }

    }

    private bool IsWithinXZRange(GameObject obj)
    {
        Vector3 diff = transform.position - obj.transform.position;
        diff.y = 0f;
        float width = GetComponent<Renderer>().bounds.size.x;
        return diff.sqrMagnitude <= width * width;
    }

}
