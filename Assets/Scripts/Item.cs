// Generated with Gemini 3 Fast on 2/22/2026
// Prompt: I want to create an inventory and Item pickup system. I want to start
// with a simple health potion. I want to have the option to pick up the health potion when I get close enough.

using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName = "Health Potion";
    public int healthAmount = 20;

    // This is what happens when we "consume" the item
    public void Use()
    {
        Debug.Log("Used " + itemName + "! Healed for " + healthAmount);
        // Add actual healing logic here later
    }
}