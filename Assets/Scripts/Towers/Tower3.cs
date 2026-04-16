using UnityEngine;

public class Tower3 : TowerParent
{
    public float stasisDuration;
    private float stasisTimer = 0;
    private GameObject heldEnemy;
    private float oldSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        if (heldEnemy != null) {
            // hold enemy
            // not quite sure how the logic is going to work since enemies
            // haven't been added yet but whatever.
            EnemyMovement movement = heldEnemy.GetComponent<EnemyMovement>();
            if (movement != null && oldSpeed == 0) {
                oldSpeed = movement.speed;
                movement.speed = 0;
            }
                

            if (stasisTimer > 0)
                stasisTimer -= FRAME_TICK_CONSTANT;
            else {
                movement.speed = oldSpeed;
                oldSpeed = 0;
                heldEnemy = null;
            }

        } else {
            if (recoil <= 0) {
                GameObject nearestEnemy = findNearestEnemy();
                if (nearestEnemy != null && (nearestEnemy.transform.position - transform.position).magnitude <= range) {
                    heldEnemy = nearestEnemy;
                    stasisTimer = stasisDuration + FRAME_TICK_CONSTANT;
                    recoil = fireDelay + FRAME_TICK_CONSTANT;
                }
                
            } else
                recoil -= FRAME_TICK_CONSTANT;
        }
        
    }

}
