using UnityEngine;

public class Tower3 : TowerParent
{
    public float stasisDuration;
    private float stasisTimer = 0;
    private GameObject heldEnemy;

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

        } else {
            if (recoil <= 0) {
                GameObject nearerstEnemy = findNearestEnemy();
                if ((nearerstEnemy.transform.position - transform.position).magnitude <= range) {
                    heldEnemy = nearerstEnemy;
                    stasisTimer = stasisDuration + FRAME_TICK_CONSTANT;
                    recoil = fireDelay + FRAME_TICK_CONSTANT;
                }
                
            } else
                recoil -= FRAME_TICK_CONSTANT;
        }
        
    }

}
