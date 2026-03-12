using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToBoss : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[DoorToBoss] Player entered boss door area. Transitioning to pre-boss scene...");
        } 
    }

}
