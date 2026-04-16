using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Covers the case where the enemy reaches the end of the path
        // and EnemyMovement destroys it without calling Die()
        if (currentHealth > 0f)
        {
            OnDeath?.Invoke();
        }
    }
}