using UnityEngine;
using System.Collections.Generic;

public class DoorToRoomEC : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Transform teleportTarget;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();
            controller.enabled = false;
            other.transform.position = teleportTarget.position;
            other.transform.rotation = teleportTarget.rotation;
            controller.enabled = true;
        }
    }
}
