using UnityEngine;
using UnityEngine.SceneManagement;

public class PreBoss : MonoBehaviour
{
    private int level = 0;
    public GenerationManager manager;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Staircase"))
        {
            if (level > 2) {
                // Debug.Log("[PreBoss] Player entered pre-boss area. Transitioning to boss scene...");
                SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event
                SceneManager.LoadScene("PreBoss");
            } else 
                manager.buildEnviornment(++level);

        } 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            player.transform.position = new Vector3(0,1,0);

            if (controller != null) controller.enabled = true;
            Debug.Log("[PreBoss] Player found on scene load. Setting position and rotation.");
            player.transform.rotation = Quaternion.identity; // Set to desired spawn rotation
        }
        else
        {
            Debug.LogWarning("[PreBoss] Player not found on scene transition.");
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to prevent multiple calls
    }
}
