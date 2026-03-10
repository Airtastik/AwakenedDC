using UnityEngine;


public class BossSpawn : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;
    }
}
