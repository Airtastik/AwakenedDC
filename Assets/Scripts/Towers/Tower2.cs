using UnityEngine;

public class Tower2 : TowerParent
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        if (recoil <= 0) {
            GameObject NearestEnemy = findNearestEnemy();
            if (NearestEnemy != null) {
                SpawnInDirectionWithVelocity(Projectile, 45, 1, 10);
                SpawnInDirectionWithVelocity(Projectile, 225, 1, 10);


            }
            recoil = fireDelay + FRAME_TICK_CONSTANT;
        }
        recoil -= FRAME_TICK_CONSTANT;
    }

}
