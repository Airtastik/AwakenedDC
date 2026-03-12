using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroToDC : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            player.transform.position = new Vector3(0,1,0);
            if (controller != null) controller.enabled = true;
            player.transform.rotation = Quaternion.identity;
        } 
    }
}