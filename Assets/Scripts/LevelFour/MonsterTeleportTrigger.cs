using UnityEngine;

public class MonsterTeleportTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        other.transform.position = new Vector3(27.7f, -80f, 222.2f);
    }
}