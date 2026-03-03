using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("btn-awaken").clicked += () => SceneManager.LoadScene("AwakenedDCBuild");
        root.Q<Button>("btn-rpg").clicked    += () => SceneManager.LoadScene("RPGtest");
    }
}
