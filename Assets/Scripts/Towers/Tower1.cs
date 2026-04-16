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
                SpawnInDirectionWithVelocity(Projectile, angle, 1, 30);
                SpawnInDirectionWithVelocity(Projectile, angle + 90, 1, 30);
                SpawnInDirectionWithVelocity(Projectile, angle + 180, 1, 30);
                SpawnInDirectionWithVelocity(Projectile, angle + 270, 1, 30);
            }
            recoil = fireDelay + FRAME_TICK_CONSTANT;
        }
        if (recoil > 0)
            recoil -= FRAME_TICK_CONSTANT;
    }

}
