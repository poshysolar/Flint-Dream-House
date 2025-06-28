using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelNext : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("LevelSix");
        }
    }
}
