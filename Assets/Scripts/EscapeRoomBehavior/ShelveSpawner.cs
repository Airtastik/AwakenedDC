using UnityEngine;

public class ShelveSpawner : MonoBehaviour
{
    public GameObject Shelve1;
    public GameObject Shelve2;
    public GameObject Shelve3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnDestroy()
    {
        Shelve1.SetActive(true);
        Shelve2.SetActive(true);
        Shelve3.SetActive(true);
    }
}
