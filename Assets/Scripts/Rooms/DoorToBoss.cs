using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToBoss : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[DoorToBoss] Player entered boss door area. Transitioning to pre-boss scene...");
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event
            SceneManager.LoadScene("RPG");
        } 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Do whatever you need to do to get the boss fight to load here.
    }

}
