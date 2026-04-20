using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab;
    public int count;
    public float spawnInterval = 0.5f;
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
    private int enemiesAlive     = 0;

    /// <summary>Read by TowerDefenseHUD every frame to show remaining enemies.</summary>
    public int EnemiesAlive => enemiesAlive;

    void Start()
    {
        if (spawnPoint == null && Waypoints.points != null && Waypoints.points.Length > 0)
            spawnPoint = Waypoints.points[0];

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave wave = waves[currentWaveIndex];
            Debug.Log($"Starting {wave.waveName}");

            // Update HUD wave counter
            TowerDefenseHUD.Instance?.SetWave(currentWaveIndex + 1);
            TowerDefenseHUD.Instance?.ShowWaveMessage($"WAVE  {currentWaveIndex + 1}", 2.5f);

            yield return StartCoroutine(SpawnWave(wave));

            while (enemiesAlive > 0)
                yield return null;

            if (currentWaveIndex < waves.Count - 1)
            {
                TowerDefenseHUD.Instance?.ShowWaveMessage("WAVE CLEARED", 2f);
                yield return new WaitForSeconds(wave.timeBeforeNextWave);
            }

            currentWaveIndex++;
        }

        TowerDefenseHUD.Instance?.ShowWaveMessage("ALL WAVES COMPLETE!", 5f);
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

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleEnemyDeath;
            // Give currency on kill
            health.OnDeath += () => TowerDefenseHUD.Instance?.AddCurrency(25);
        }
    }

    void HandleEnemyDeath() => enemiesAlive--;
}
