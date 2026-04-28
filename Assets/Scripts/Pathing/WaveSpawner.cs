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
    public List<Transform> spawnPoints;
    public int spawnNumber;

    [Header("Enemy Paths")]
    public GameObject path1;
    public GameObject path2;
    public GameObject path3;
    public GameObject path4;

    [Header("Level Object")]
    public GameObject level;
    public float duration = 1.5f;

    private int currentWaveIndex = 0;
    private int enemiesAlive     = 0;

    /// <summary>Read by TowerDefenseHUD every frame to show remaining enemies.</summary>
    public int EnemiesAlive => enemiesAlive;

    void Start()
    {
        if (spawnPoints == null && Waypoints.points != null && Waypoints.points.Length > 0)
            spawnPoints[0] = Waypoints.points[0];

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
            spawnNumber++;
            SwitchPath(spawnNumber);
            StartCoroutine(RotateBy90());
        }

        TowerDefenseHUD.Instance?.ShowWaveMessage("ALL WAVES COMPLETE!", 5f);
        Debug.Log("All waves complete!");
    }

    IEnumerator RotateBy90()
    {
        Quaternion startRot = level.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, -90, 0);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Smooth easing (optional but recommended)
            t = Mathf.SmoothStep(0, 1, t);

            level.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        level.transform.rotation = targetRot;
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
        GameObject enemy = Instantiate(prefab, spawnPoints[spawnNumber].position, prefab.transform.rotation);
        enemiesAlive++;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleEnemyDeath;
            // Give currency on kill
            health.OnDeath += () => TowerDefenseHUD.Instance?.AddCurrency(25);
        }
    }

    void SwitchPath(int pathNumber)
    {
        if (pathNumber == 1)
        {
            path1.SetActive(false);
            path2.SetActive(true);
        } else if (pathNumber == 2)
        {
            path2.SetActive(false);
            path3.SetActive(true);
        } else if (pathNumber == 3)
        {
            path3.SetActive(false);
            path4.SetActive(true);
        }
    }

    void HandleEnemyDeath() => enemiesAlive--;
}
