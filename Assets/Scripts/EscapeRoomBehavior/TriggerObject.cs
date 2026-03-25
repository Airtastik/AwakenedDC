using UnityEngine;

public class TriggerObject : MonoBehaviour
{
    public EnviornmentalTriggerManager manager;

    // not actually a GameObject, but however items are stored in the inventory
    public GameObject key;

    // how this is going to progress the enviornment represented as an int state
    public int associatedTrigger;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    void Update() {
        // interaction conditonal for if player is interacting with object and then chooses the right key
        if (true) {
            manager.progress(associatedTrigger);
        }
    }
}
