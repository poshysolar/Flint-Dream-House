using UnityEngine;

public class TeleportAfterPause : MonoBehaviour
{
    public Vector3 targetPosition = new Vector3(-18.62f, 11.05f, 0.1452637f);
    public float delay = 2f;

    private CharacterController controller;
    private bool isTeleporting = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            Debug.LogWarning("CharacterController not found on this GameObject!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isTeleporting)
        {
            StartCoroutine(TeleportAfterDelay());
        }
    }

    System.Collections.IEnumerator TeleportAfterDelay()
    {
        isTeleporting = true;

        // Optional: disable movement if needed
        if (controller != null) controller.enabled = false;

        yield return new WaitForSeconds(delay);

        transform.position = targetPosition;

        // Re-enable movement
        if (controller != null) controller.enabled = true;

        isTeleporting = false;
    }
}
