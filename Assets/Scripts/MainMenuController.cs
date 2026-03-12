using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

//Used ChatGPT 5.3
//Prompt: I want to whenever the player clicks on the "btn-controls" button, the ui switches to a umxl file called Controls. And when the player clicks a button called "btn-back" in the Controls file it switches back to the main menu umxl file. How would I do this?

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    public VisualTreeAsset mainMenuUXML;
    public VisualTreeAsset controlsUXML;

    UIDocument uiDocument;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        var root = uiDocument.rootVisualElement;

        root.Clear();
        mainMenuUXML.CloneTree(root);

        var btnAwaken = root.Q<Button>("btn-awaken");
        var btnControls = root.Q<Button>("btn-controls");

        btnAwaken.clicked += () =>
        {
            Debug.Log("Awaken clicked");
            SceneManager.LoadScene(2);
        };

        btnControls.clicked += () =>
        {
            Debug.Log("Controls clicked");
            ShowControlsMenu();
        };
    }

    void ShowControlsMenu()
    {
        var root = uiDocument.rootVisualElement;

        root.Clear();
        controlsUXML.CloneTree(root);

        var btnBack = root.Q<Button>("btn-back");

        btnBack.clicked += () =>
        {
            Debug.Log("Back clicked");
            ShowMainMenu();
        };
    }
}