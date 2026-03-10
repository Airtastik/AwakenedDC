using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRoom : MonoBehaviour
{

    [SerializeField]
    private string bossBattle;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Testing Collision");
        if (other.CompareTag("staircase"))
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(bossBattle);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            player.transform.position = new Vector3(0, 1, 0);
            Debug.Log(player.transform.position);
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
