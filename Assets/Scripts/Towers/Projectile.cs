using UnityEngine;

public class Projectile : MonoBehaviour
{
    public double distance;
    public int damage; 
    private Vector3 initialPosition;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            Debug.Log("Hit an enemy: " + collision.gameObject.name);
            // need actual enemy scripting data done by now
        }
        Destroy(gameObject);
    }

}
