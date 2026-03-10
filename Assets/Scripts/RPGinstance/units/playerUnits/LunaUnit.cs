using UnityEngine;

/// <summary>
/// Luna — a fragile NPC. No move list, no battle role.
/// Attach to her GameObject alongside a Unit or a standalone NPC component.
/// Her stats exist so she can be referenced in story events, cutscenes,
/// or conditional checks (e.g. is Luna still alive? what is her HP?).
/// </summary>
public class LunaUnit : MonoBehaviour
{
    [Header("Identity")]
    public string unitName      = "Luna";
    public ElementalType elementalType = ElementalType.Normal;

    [Header("Stats")]
    public int maxHealth        = 30;
    public int currentHealth    = 30;
    public bool isAlive         => currentHealth > 0;

    [Header("NPC Flags")]
    [Tooltip("Set true by story events when Luna joins the party temporarily.")]
    public bool isActiveInParty = false;

    [Tooltip("Tracks whether the player has spoken to Luna this scene.")]
    public bool hasSpokenTo     = false;

    // ── Story event hooks ─────────────────────────────────────────────────────

    /// <summary>Called by story/event scripts to damage Luna in a cutscene.</summary>
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (!isAlive) OnLunaFainted();
    }

    /// <summary>Called by story/event scripts to restore Luna's HP.</summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void OnLunaFainted()
    {
        Debug.Log("[Luna] Luna has fainted.");
        // Hook: trigger game over, cutscene, or story branch here
    }
}
