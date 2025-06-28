using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float minDistanceToPlayer = 1.5f; // Minimum distance to maintain from player

    [Header("Performance Settings")]
    public float updateInterval = 0.1f; // How often to update path (lower = more CPU usage)
    public float groundCheckInterval = 0.5f; // How often to check ground
    public float soundCheckInterval = 1.0f; // How often to check for playing sounds

    [Header("Ground Detection")]
    public float groundCheckDistance = 10f; // How far to check for ground
    public LayerMask groundLayer; // Layer for ground detection
    public float heightOffset = 0f; // Adjust monster height above ground

    [Header("Audio")]
    public AudioSource monsterAudioSource;
    public AudioClip[] monsterSounds;
    public float soundInterval = 3f;

    [Header("References")]
    public Transform player; // Reference to player's Transform
    private Animator animator; // Reference to Animator component

    private bool isChasing = false; // Start with chasing disabled
    private float lastSoundTime;
    private float lastUpdateTime;
    private float lastGroundCheckTime;
    private float lastSoundCheckTime;
    private Vector3 targetPosition;

    private void Start()
    {
        // Set up animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Set up audio source if not assigned
        if (monsterAudioSource == null && GetComponent<AudioSource>() != null)
            monsterAudioSource = GetComponent<AudioSource>();
        
        // Set default ground layer if not assigned
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Default");
        
        // Start with running animation but not chasing
        if (animator != null)
            animator.SetBool("IsRunning", true);
        
        // Initialize timers
        lastUpdateTime = 0f;
        lastGroundCheckTime = 0f;
        lastSoundCheckTime = 0f;
        lastSoundTime = 0f;
    }

    private void OnEnable()
    {
        // Reset sound timer
        lastSoundTime = Time.time;
    
        // Ensure monster is on ground when enabled
        SnapToGround();
    }

    private void Update()
    {
        if (!isChasing || player == null)
            return;
        
        float currentTime = Time.time;
    
        // Update target position at intervals (not every frame)
        if (currentTime - lastUpdateTime > updateInterval)
        {
            UpdateTargetPosition();
            lastUpdateTime = currentTime;
        }
    
        // Move towards target position
        MoveTowardsTarget();
    
        // Check ground at intervals
        if (currentTime - lastGroundCheckTime > groundCheckInterval)
        {
            SnapToGround();
            lastGroundCheckTime = currentTime;
        }
    
        // Check for playing sounds at intervals
        if (currentTime - lastSoundCheckTime > soundCheckInterval)
        {
            CheckForPlayingSound(currentTime);
            lastSoundCheckTime = currentTime;
        }
    }

    private void UpdateTargetPosition()
    {
        // Look at player (Y-axis only to avoid tilting)
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
    
        if (direction != Vector3.zero)
        {
            // Calculate distance to player
            float distanceToPlayer = direction.magnitude;
        
            // Rotate towards player
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        
            // Set target position if not too close
            if (distanceToPlayer > minDistanceToPlayer)
            {
                targetPosition = player.position;
                // Keep Y position unchanged
                targetPosition.y = transform.position.y;
            }
        }
    }

    private void MoveTowardsTarget()
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
    }

    private void SnapToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f; // Start slightly above current position
    
        // Cast ray downward to find ground
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // Set position to ground point plus height offset
            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y + heightOffset;
            transform.position = newPosition;
        }
    }

    private void CheckForPlayingSound(float currentTime)
    {
        // Play random monster sounds at intervals
        if (monsterAudioSource != null && monsterSounds != null && monsterSounds.Length > 0)
        {
            if (currentTime - lastSoundTime > soundInterval)
            {
                PlayRandomMonsterSound();
                lastSoundTime = currentTime;
            }
        }
    }

    private void PlayRandomMonsterSound()
    {
        if (monsterAudioSource != null && monsterSounds != null && monsterSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, monsterSounds.Length);
            monsterAudioSource.clip = monsterSounds[randomIndex];
            monsterAudioSource.Play();
        }
    }

    public void StopChasing()
    {
        isChasing = false;
        if (animator != null)
            animator.SetBool("IsRunning", false);
    }

    public void ResumeChasing()
    {
        isChasing = true;
        if (animator != null)
            animator.SetBool("IsRunning", true);
    }
}