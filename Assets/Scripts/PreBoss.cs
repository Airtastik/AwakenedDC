using UnityEngine;
using UnityEngine.SceneManagement;

public class PreBoss : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Staircase"))
        {
            Debug.Log("[PreBoss] Player entered pre-boss area. Transitioning to boss scene...");
            OnSceneTransition();
            SceneManager.LoadScene("PreBoss");
        } 
    }

    void OnSceneTransition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0, 1, 0); // Set to desired spawn position
            player.transform.rotation = Quaternion.identity; // Set to desired spawn rotation
        }
        else
        {
            Debug.LogWarning("[PreBoss] Player not found on scene transition.");
        }
    }
}
