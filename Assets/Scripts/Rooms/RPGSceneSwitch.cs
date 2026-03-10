using UnityEngine;
using UnityEngine.SceneManagement;

public class RPGSceneSwitch : MonoBehaviour
{
    public Vector3 respawnPointForRpg;

    [SerializeField]
    private string rpgScene;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Testing Collision W/ Enemy");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            respawnPointForRpg = player.transform.position;
            SceneManager.LoadScene(rpgScene);
        }
    }
}
