using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that handles transitions between the overworld
/// and the RPG battle scene. Remembers where to return after a battle.
///
/// SETUP:
/// - Add this to a GameObject in your first loaded scene (MainMenu or Bootstrap)
/// - It will persist across all scenes automatically
/// - Call SceneTransitionManager.StartBattle(roster, level) from any encounter trigger
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Scene Names — must match exactly in Build Settings")]
    public string overworldSceneName = "AwakenedDCBuild";
    public string battleSceneName    = "RPGScene";

    // Where to return the player after a battle
    private string returnScene;
    private string returnSpawnID;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Called from encounter triggers ────────────────────────────────────────

    /// <summary>
    /// Prepare an encounter and load the battle scene.
    /// spawnID is the name of a SpawnPoint in the overworld to return to.
    /// </summary>
    public static void StartBattle(EnemyRoster roster, int level = 1, string spawnID = "")
    {
        if (Instance == null) { Debug.LogError("[Transition] No SceneTransitionManager in scene!"); return; }

        Instance.returnScene   = SceneManager.GetActiveScene().name;
        Instance.returnSpawnID = spawnID;

        EncounterManager.Prepare(roster, level);
        SceneManager.LoadScene(Instance.battleSceneName);
    }

    public static void StartBattle(EncounterData data, string spawnID = "")
    {
        if (Instance == null) { Debug.LogError("[Transition] No SceneTransitionManager in scene!"); return; }

        Instance.returnScene   = SceneManager.GetActiveScene().name;
        Instance.returnSpawnID = spawnID;

        EncounterManager.Prepare(data);
        SceneManager.LoadScene(Instance.battleSceneName);
    }

    // ── Called from battle scene after win/loss ───────────────────────────────

    /// <summary>
    /// Return to the overworld scene after a battle ends.
    /// BattleReturnTrigger calls this automatically.
    /// </summary>
    public static void ReturnToOverworld()
    {
        if (Instance == null) { Debug.LogError("[Transition] No SceneTransitionManager in scene!"); return; }

        string scene = Instance.returnScene;
        if (string.IsNullOrEmpty(scene)) scene = Instance.overworldSceneName;

        SceneManager.LoadScene(scene);
    }

    public string ReturnSpawnID => returnSpawnID;
}
