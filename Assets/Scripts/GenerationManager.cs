using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

public class GenerationManager : MonoBehaviour
{
    public List<RoomSpawner> generatorPrefabs;
    public GameObject playerInstance;
    private RoomSpawner currentSpawner;

    void Start()
    {   
        currentSpawner = null;
        buildEnviornment(0);
    }

    void buildEnviornment(int idx) {
        if (currentSpawner != null)
            currentSpawner.purgeEnviornment();
        currentSpawner = Instantiate(generatorPrefabs[idx], new Vector3(0, 0, 0), Quaternion.identity);

        Vector3 oldPos = playerInstance.transform.position;
        oldPos.x = 0;
        oldPos.z = 0;
        playerInstance.transform.position = oldPos;
    }
}
