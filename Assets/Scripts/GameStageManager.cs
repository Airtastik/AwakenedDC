using UnityEngine;

/// <summary>
/// Global singleton that tracks the current game stage (0, 1, 2 ...).
/// Set the stage from anywhere:  GameStageManager.SetStage(1);
/// Read it from anywhere:        GameStageManager.CurrentStage
/// 
/// Survives scene loads. Pair with DontDestroyOnLoad or place on a persistent GO.
/// </summary>
public class GameStageManager : MonoBehaviour
{
    public static GameStageManager Instance { get; private set; }

    public static int CurrentStage { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Advance to the next stage.</summary>
    public static void NextStage() => SetStage(CurrentStage + 1);

    /// <summary>Set an explicit stage number.</summary>
    public static void SetStage(int stage)
    {
        CurrentStage = stage;
        Debug.Log($"[GameStageManager] Stage → {stage}");
    }
}
