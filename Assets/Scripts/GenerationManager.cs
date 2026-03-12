using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

public class GenerationManager : MonoBehaviour
{
    public List<RoomSpawner> generatorPrefabs;
    private RoomSpawner currentSpawner;

    void Start()
    {   
        currentSpawner = null;
        buildEnviornment(0);
    }

    public void buildEnviornment(int idx) {
        if (currentSpawner != null) {
            Destroy(currentSpawner.gameObject);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3(0, player.transform.position.y, 0);

        currentSpawner = Instantiate(generatorPrefabs[idx], new Vector3(0, 0, 0), Quaternion.identity);

        
    }
}
