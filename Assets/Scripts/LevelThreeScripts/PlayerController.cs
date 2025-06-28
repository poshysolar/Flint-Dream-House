using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool canMove = true; // Controls whether the player can move

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            // Insert your player movement logic here (e.g., input handling, character controller, etc.)
        }
        // If canMove is false, movement logic is skipped
    }
}