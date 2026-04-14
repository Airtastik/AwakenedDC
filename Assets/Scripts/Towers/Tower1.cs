using UnityEngine;

public class Tower1 : TowerParent
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
                float angle = GetAngleFromNorth(NearestEnemy);
                SpawnInDirectionWithVelocity(Projectile, angle, 1, 10);
                SpawnInDirectionWithVelocity(Projectile, angle + 90, 1, 10);
                SpawnInDirectionWithVelocity(Projectile, angle + 180, 1, 10);
                SpawnInDirectionWithVelocity(Projectile, angle + 270, 1, 10);
            }
            recoil = fireDelay + FRAME_TICK_CONSTANT;
        }
        recoil -= FRAME_TICK_CONSTANT;
    }

}
