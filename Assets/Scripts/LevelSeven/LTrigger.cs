using UnityEngine;
using UnityEngine.SceneManagement;

public class LTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("LevelEight");
        }
    }
}
