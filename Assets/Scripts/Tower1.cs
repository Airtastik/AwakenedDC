using UnityEngine;

public class Tower1 : MonoBehaviour
{
    public GameObject Projectile;
    public double fireDelay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        double recoil = 0;
        if (recoil <= 0) {
            // GameObject NearestEnemy = FindNearestObject("Enemy");
            // float angle = GetAngleFromNorth(NearestEnemy);
            float angle = 0;
            SpawnInDirectionWithVelocity(Projectile, angle, 1, 1);
            recoil = fireDelay + .016;
        }
        recoil -= .016;
    }

    GameObject SpawnInDirectionWithVelocity(GameObject prefab, float angleDegrees, float distance, float speed)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        Vector3 spawnPosition = transform.position + direction * distance;
        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.LookRotation(direction));

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = direction * speed;
        }

        return obj;
    }

    GameObject FindNearestObject(string name) {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        GameObject nearest = null;

        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name)
            {
                float distance = Vector3.Distance(currentPosition, obj.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = obj;
                }
            }
        }

        return nearest;
    }

    float GetAngleFromNorth(GameObject s) {
        if (s == null) return 0f;

        Vector3 direction = s.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        return angle;
    }



}
