using UnityEngine;

/// <summary>
/// Place this in the RPG battle scene. It listens for the battle to end
/// and automatically returns to the overworld after a short delay.
///
/// SETUP:
/// - Add this to any GameObject in the battle scene (e.g. BattleManager)
/// - Assign your BattleSystem reference in the Inspector
/// - It handles both Victory and Defeat
/// </summary>
public class BattleReturnTrigger : MonoBehaviour
{
    [Header("References")]
    public BattleSystem battleSystem;

    [Header("Settings")]
    public float returnDelayWin  = 3.0f;  // Seconds after victory before returning
    public float returnDelayLoss = 4.0f;  // Seconds after defeat before returning

    void OnEnable()
    {
        BattleSystem.OnStateChanged += OnBattleStateChanged;
    }

    void OnDisable()
    {
        BattleSystem.OnStateChanged -= OnBattleStateChanged;
    }

    private void OnBattleStateChanged(BattleState state)
    {
        if (state == BattleState.Won)
        {
            Debug.Log($"[BattleReturn] Victory! Returning in {returnDelayWin}s.");
            Invoke(nameof(ReturnToOverworld), returnDelayWin);
        }
        else if (state == BattleState.Lost)
        {
            Debug.Log($"[BattleReturn] Defeat. Returning in {returnDelayLoss}s.");
            Invoke(nameof(ReturnToOverworld), returnDelayLoss);
        }
    }

    private void ReturnToOverworld()
    {
        SceneTransitionManager.ReturnToOverworld();
    }
}
