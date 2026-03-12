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
            Debug.Log("Current Level: " + level);
            if (level > 1) {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                var controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;
                player.transform.position = new Vector3(200,201,200);
                if (controller != null) controller.enabled = true;
                player.transform.rotation = Quaternion.identity;
            } else 
                manager.buildEnviornment(++level);

        } 
    }
}
