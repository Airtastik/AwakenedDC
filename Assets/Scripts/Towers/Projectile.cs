using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float distance;
    public int damage; 
    private Vector3 initialPosition;

    public void Init(float distance, int damage, float scale)
    {
        this.distance = distance;
        this.damage = damage;
        transform.localScale = new Vector3(scale, scale, scale);
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if ((initialPosition - transform.position).magnitude >= distance)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Debug.Log("Hit an enemy: " + collision.gameObject.name);
            // need actual enemy scripting data done by now
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null) {
                enemyHealth.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }

}
