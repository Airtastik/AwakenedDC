using UnityEngine;
using System.Collections.Generic;

public class TriggerInteractable : Interactable
{
    [Tooltip("List of triggers for the interact")]
    public List<int> triggerList = new List<int>();

    [Tooltip("Environmental Trigger")]
    public EnviornmentalTriggerManager manager;

    protected override void OnInteract()
    {
        for (int i = 0; i < triggerList.Count; i++)
        {
            manager.progress(triggerList[i]);
        }
    }
}
