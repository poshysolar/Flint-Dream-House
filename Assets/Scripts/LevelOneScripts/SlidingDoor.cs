using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public void OpenDoor(float targetX, float speed)
    {
        Vector3 targetPosition = new Vector3(
            targetX,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }
}