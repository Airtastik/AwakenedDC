using UnityEngine;

public enum ItemEffect
{
    HealTarget,      // Restore HP to one party member
    HealParty,       // Restore HP to all living party members
    ReviveTarget,    // Bring a fainted member back with partial HP
    BuffAttack,      // Raise one member's attack for the battle
    BuffDefence,     // Raise one member's defence for the battle
    CureEffects,     // Remove all status effects from one member
}

[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    [Header("Identity")]
    public string itemName    = "Health Potion";
    public string description = "Restores 30 HP to one ally.";

    [Header("Effect")]
    public ItemEffect effect      = ItemEffect.HealTarget;
    public int        power       = 30;    // HP restored, stat boost amount, etc.
    public float      statMult    = 1.25f; // Used for buff items

    [Header("World Pickup (optional)")]
    public int healthAmount = 20; // kept for backwards compat with world pickups
}
