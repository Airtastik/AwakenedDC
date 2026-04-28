using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
    public GameObject path5;

    [Header("Camera")]
    public GameObject camera;
    public List<Transform> targetPosition;
    public float duration = 2f;
    public float arcStrength = 5f;

    private bool running = false;

    private int currentWaveIndex = 0;
    private int enemiesAlive     = 0;

    /// <summary>Read by TowerDefenseHUD every frame to show remaining enemies.</summary>
    public int EnemiesAlive => enemiesAlive;

    void Start()
    {
        if (spawnPoints == null && Waypoints.points != null && Waypoints.points.Length > 0)
            spawnPoints[0] = Waypoints.points[0];
    }

    public void Clicked()
    {
        StartCoroutine(RunWave());
        running = true;
    }

    public bool getRunning()
    {
        return running;
    }

    IEnumerator RunWave()
    {
        if (currentWaveIndex < waves.Count)
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
                spawnNumber++;
                SwitchPath(spawnNumber);
                StartCoroutine(MoveCamera(spawnNumber, currentWaveIndex));
                running = false;
                yield return new WaitForSeconds(wave.timeBeforeNextWave);
            }
            if (TowerDefenseHUD.Instance?.GetHealth() == 0)
            {
                TowerDefenseHUD.Instance?.ShowWaveMessage("GAME OVER", 5f);
                yield return new WaitForSeconds(10f);
                SceneManager.LoadScene("MainMenu");
            }
            
            currentWaveIndex++;
        }
        if (currentWaveIndex == waves.Count && enemiesAlive == 0)
        {
            TowerDefenseHUD.Instance?.ShowWaveMessage("ALL WAVES COMPLETE!", 5f);
            Debug.Log("All waves complete!");
            yield return new WaitForSeconds(10f);
            SceneManager.LoadScene("MainMenu");
        }
        
    }

    IEnumerator MoveCamera(float multiplier, int index)
    {
        Vector3 startPos = camera.transform.position;
        Quaternion startRot = camera.transform.rotation;

        Vector3 endPos = targetPosition[index].position;

        float rotation = 90.383f * multiplier;

        // 90° rotation (Y axis)
        Quaternion targetRot = Quaternion.Euler(40.031f, -90f + rotation, -0.036f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Optional smoothing
            t = Mathf.SmoothStep(0, 1, t);

            // Straight line movement
            camera.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Smooth rotation
            camera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = endPos;
        camera.transform.rotation = targetRot;
        Debug.Log(startRot.eulerAngles);
        Debug.Log(targetRot.eulerAngles);
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
        } else if (pathNumber == 4)
        {
            path4.SetActive(false);
            path5.SetActive(true);
        }
    }

    void HandleEnemyDeath() => enemiesAlive--;
}
