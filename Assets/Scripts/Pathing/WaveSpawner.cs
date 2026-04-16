using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int count;
    public float spawnInterval = 0.5f; // time between each enemy in this group
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";
    public List<EnemyGroup> groups;
    public float timeBeforeNextWave = 5f;
}

public class WaveSpawner : MonoBehaviour
{
    public List<Wave> waves;
    public Transform spawnPoint;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    void Start()
    {
        if (spawnPoint == null && Waypoints.points != null && Waypoints.points.Length > 0)
        {
            spawnPoint = Waypoints.points[0];
        }

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave wave = waves[currentWaveIndex];
            Debug.Log($"Starting {wave.waveName}");

            yield return StartCoroutine(SpawnWave(wave));

            // Wait until all enemies from this wave are dead before next wave
            while (enemiesAlive > 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(wave.timeBeforeNextWave);
            currentWaveIndex++;
        }

        Debug.Log("All waves complete!");
    }

    IEnumerator SpawnWave(Wave wave)
    {
        foreach (EnemyGroup group in wave.groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        GameObject enemy = Instantiate(prefab, spawnPoint.position, prefab.transform.rotation);
        enemiesAlive++;

        // Subscribe to its death so we can track count
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleEnemyDeath;
        }
    }

    void HandleEnemyDeath()
    {
        enemiesAlive--;
    }
}