using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var btnAwaken = root.Q<Button>("btn-awaken");
        var btnRpg = root.Q<Button>("btn-rpg");

        Debug.Log($"btn-awaken found: {btnAwaken != null}");
        Debug.Log($"btn-rpg found: {btnRpg != null}");

        btnAwaken.clicked += () => { Debug.Log("Awaken clicked"); SceneManager.LoadScene(2); };
        btnRpg.clicked += () => { Debug.Log("RPG clicked"); SceneManager.LoadScene(1); };
    }
}
