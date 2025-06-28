using UnityEngine;

public class Playertele : MonoBehaviour
{
    private Vector3 teleportPosition = new Vector3(-15.1f, 0.56f, 172.41f);
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (controller != null)
            {
                controller.enabled = false; // Temporarily disable to manually move
                transform.position = teleportPosition;
                controller.enabled = true; // Reactivate after teleporting
            }
            else
            {
                transform.position = teleportPosition;
            }
        }
    }
}
