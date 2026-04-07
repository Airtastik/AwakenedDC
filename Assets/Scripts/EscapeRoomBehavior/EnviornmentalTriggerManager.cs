using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnviornmentalTriggerManager : MonoBehaviour
{
    /// parallel lists, should have same length
    public List<GameObject> dynamicObjects;
    public List<int> associatedTrigger;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (dynamicObjects.Count != associatedTrigger.Count)
            Debug.LogError($"Enviornmental Trigger Parallel Lists are not equal length");
    }

    // Update is called once per frame
    // void Update() {
        
    // }

    /// called by other objects upon valid player interrupt to update the enviornment by
    /// deleting objects in the list with the associated trigger int
    public void progress(int trigger) {
        for (int i = 0; i < dynamicObjects.Count; i++) {
            if (associatedTrigger[i] == trigger) {
                Debug.Log($"Trigger is {trigger}");
                Debug.Log($"Destroying number {associatedTrigger[i]}");
                Destroy(dynamicObjects[i]);
            }
        }
    }


}
