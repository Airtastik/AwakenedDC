using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroToDC : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event
            SceneManager.LoadScene("AwakenedDCBuild");
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
            Debug.Log("[Intro] Player found on scene load. Setting position and rotation.");
            player.transform.rotation = Quaternion.identity; // Set to desired spawn rotation
        }
        else
        {
            Debug.LogWarning("[Intro] Player not found on scene transition.");
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to prevent multiple calls
    }
}