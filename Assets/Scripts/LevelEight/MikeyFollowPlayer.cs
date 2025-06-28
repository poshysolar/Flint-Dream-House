using UnityEngine;

public class MikeyFollowPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTarget; // The player to follow
    [SerializeField] private GameObject mikeyTalkingModel; // The talking Mikey model
    [SerializeField] private GameObject mikeyRunningModel; // The running Mikey model with Run animator
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float followDistance = 3f; // Distance to maintain from player
    [SerializeField] private float stopDistance = 2f; // Distance at which Mikey stops moving
    [SerializeField] private float groundCheckDistance = 10f; // Distance to check for ground
    [SerializeField] private LayerMask groundLayerMask = -1; // What layers count as ground
    
    private bool isFollowing = false;
    private Animator runningAnimator;
    
    private void Start()
    {
        // Get the animator from the running model
        if (mikeyRunningModel != null)
        {
            runningAnimator = mikeyRunningModel.GetComponent<Animator>();
            
            // Set the animator controller to "BRunning"
            RuntimeAnimatorController bRunningController = Resources.Load<RuntimeAnimatorController>("BRunning");
            if (bRunningController != null)
            {
                runningAnimator.runtimeAnimatorController = bRunningController;
            }
            else
            {
                Debug.LogWarning("BRunning animator controller not found in Resources folder!");
            }
        }
        
        // Hide both models at start - they will be shown when needed
        if (mikeyTalkingModel != null)
            mikeyTalkingModel.SetActive(false);
        if (mikeyRunningModel != null)
            mikeyRunningModel.SetActive(false);
            
        // Hide the entire Mikey2 model until needed
        gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (isFollowing && playerTarget != null)
        {
            FollowPlayer();
            KeepOnGround();
        }
    }
    
    private void KeepOnGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
        {
            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y;
            transform.position = newPosition;
        }
    }
    
    public void StartFollowing()
    {
        // Show the Mikey2 model when starting to follow
        gameObject.SetActive(true);
        
        isFollowing = true;
        
        // Switch from talking model to running model
        if (mikeyTalkingModel != null)
            mikeyTalkingModel.SetActive(false);
        if (mikeyRunningModel != null)
            mikeyRunningModel.SetActive(true);
            
        Debug.Log("Mikey started following the player!");
    }
    
    public void StopFollowing()
    {
        isFollowing = false;
        
        // Stop running animation
        if (runningAnimator != null)
        {
            runningAnimator.SetBool("IsRunning", false);
        }
        
        Debug.Log("Mikey stopped following the player!");
    }
    
    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Only move if player is far enough away
        if (distanceToPlayer > stopDistance)
        {
            // Calculate direction to player
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            
            // Move towards player but maintain follow distance
            Vector3 targetPosition = playerTarget.position - (directionToPlayer * followDistance);
            
            // Move Mikey towards the target position (only X and Z, let ground check handle Y)
            Vector3 currentPos = transform.position;
            Vector3 newPos = Vector3.MoveTowards(currentPos, targetPosition, followSpeed * Time.deltaTime);
            newPos.y = currentPos.y; // Keep current Y position, ground check will adjust it
            transform.position = newPos;
            
            // Rotate to look at player
            Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Set running animation to true
            if (runningAnimator != null)
            {
                runningAnimator.SetBool("IsRunning", true);
            }
        }
        else
        {
            // Player is close enough, stop running animation but still look at player
            if (runningAnimator != null)
            {
                runningAnimator.SetBool("IsRunning", false);
            }
            
            // Still rotate to look at player even when not moving
            Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    // Public method to set the player target (in case you need to change it)
    public void SetPlayerTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }
    
    // Public method to get current following status
    public bool IsFollowing()
    {
        return isFollowing;
    }
}