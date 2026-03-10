using UnityEngine;
public class StatsManager : MonoBehaviour
{

    public GameObject statsUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleStats();
        }
    }

    public void ToggleStats()
    {
        bool isActive = !statsUI.activeSelf;
        statsUI.SetActive(isActive);

        Time.timeScale = isActive ? 0f : 1f;
    }
}
