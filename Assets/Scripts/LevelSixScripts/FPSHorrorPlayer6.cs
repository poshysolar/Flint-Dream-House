using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FPSHorrorPlayer6 : MonoBehaviour
{
    public static bool allowMovement = true; // Kept as requested

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -19.62f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float accelerationTime = 0.1f;
    [SerializeField] private float decelerationTime = 0.2f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2.0f; // Back to a standard value
    [SerializeField] private Transform playerCamera;
    private float xRotation = 0f;

    [Header("Head Bobbing")]
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float walkBobAmount = 0.015f;
    [SerializeField] private float runBobSpeed = 14f;
    [SerializeField] private float runBobAmount = 0.022f;
    [SerializeField] private float bobDamping = 8.0f;
    private Vector3 defaultCameraPosition;
    private float bobTimer = 0f;
    private float bobCycle = 0f;
    private float bobFactor = 0f;
    private Vector3 targetCameraPosition;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepDelayWalk = 0.5f;
    [SerializeField] private float footstepDelayRun = 0.3f;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float footstepRandomPitch = 0.1f;
    private float nextFootstepTime = 0f;

    [Header("Crosshair")]
    [SerializeField] private Texture2D crosshairTexture;
    [SerializeField] private float crosshairSize = 8f; // Smaller size for a more subtle dot
    [SerializeField] private Color crosshairColor = new Color(1f, 1f, 1f, 0.7f);

    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private Vector3 lastPosition;
    private float actualMovementSpeed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (playerCamera != null)
        {
            defaultCameraPosition = playerCamera.localPosition;
            targetCameraPosition = defaultCameraPosition;
        }

        if (crosshairTexture == null)
        {
            CreateDefaultCrosshair();
        }
        
        lastPosition = transform.position;
        
        // Set up camera to not see player capsule
        SetupCamera();
        
        // Remove player shadow
        RemovePlayerShadow();
    }
    
    private void SetupCamera()
    {
        if (playerCamera != null && playerCamera.GetComponent<Camera>() != null)
        {
            // Set near clip plane to prevent seeing inside the capsule
            playerCamera.GetComponent<Camera>().nearClipPlane = 0.05f;
            
            // Make sure camera is positioned correctly
            playerCamera.localPosition = new Vector3(0, 0.8f, 0); // Adjust height as needed
            
            // Ignore collisions between camera and player
            if (playerCamera.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                playerCamera.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }
    
    private void RemovePlayerShadow()
    {
        // Remove shadow casting from the player and all its children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        // Also disable the player's mesh renderer if it exists
        Renderer playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }
    }

    private void CreateDefaultCrosshair()
    {
        int size = 64;
        crosshairTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        float center = size / 2f;
        float radius = size / 2f - 2; // Slightly smaller to avoid edge clipping
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (distance <= radius) // Filled circle
                    pixels[y * size + x] = crosshairColor;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }
        
        crosshairTexture.SetPixels(pixels);
        crosshairTexture.Apply();
    }

    private void OnGUI()
    {
        if (crosshairTexture != null)
        {
            GUI.DrawTexture(
                new Rect(
                    Screen.width / 2 - crosshairSize / 2,
                    Screen.height / 2 - crosshairSize / 2,
                    crosshairSize,
                    crosshairSize
                ),
                crosshairTexture
            );
        }
    }

    private void Update()
    {
        if (!allowMovement)
            return;

        CheckGrounded();
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        CalculateHeadBob();
        ApplyHeadBob();
        HandleFootsteps();
        
        // Calculate actual movement speed for footsteps and head bob
        actualMovementSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, controller.height / 2f, 0), 
                                         groundCheckDistance, groundMask);
        
        // Apply a small downward force when grounded to stick to slopes
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMouseLook()
    {
        // Simple, reliable mouse look implementation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Update camera pitch (up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // Apply rotations
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Calculate target speed
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        targetSpeed = inputDirection.magnitude * (isRunning ? runSpeed : walkSpeed);
        
        // Smooth acceleration/deceleration
        float accelerationRate = targetSpeed > currentSpeed ? accelerationTime : decelerationTime;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / accelerationRate);
        
        // Calculate move direction in world space
        if (inputDirection.magnitude >= 0.1f)
        {
            moveDirection = transform.TransformDirection(inputDirection) * currentSpeed;
        }
        else
        {
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, Time.deltaTime * 8f);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        
        // Move the controller
        controller.Move(moveDirection * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            
            // Reset head bob when jumping
            bobTimer = 0f;
        }
    }

    private void CalculateHeadBob()
    {
        if (actualMovementSpeed > 0.1f && isGrounded)
        {
            // Determine bob parameters based on movement
            float bobSpeed = actualMovementSpeed > walkSpeed * 0.8f ? runBobSpeed : walkBobSpeed;
            float bobAmount = actualMovementSpeed > walkSpeed * 0.8f ? runBobAmount : walkBobAmount;
            
            // Increment bob timer
            bobTimer += Time.deltaTime * bobSpeed * (actualMovementSpeed / walkSpeed);
            
            // Calculate bob cycle (0 to 1)
            bobCycle = Mathf.Sin(bobTimer);
            
            // Calculate bob factor (0 to 1 based on movement speed)
            bobFactor = Mathf.Lerp(bobFactor, Mathf.Clamp01(actualMovementSpeed / walkSpeed), Time.deltaTime * 4f);
            
            // Calculate target camera position with bob
            targetCameraPosition = defaultCameraPosition + new Vector3(
                Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f, 
                Mathf.Abs(bobCycle) * bobAmount, 
                0f
            ) * bobFactor;
        }
        else
        {
            // Gradually reset bob when not moving
            bobFactor = Mathf.Lerp(bobFactor, 0f, Time.deltaTime * 4f);
            targetCameraPosition = defaultCameraPosition;
        }
    }

    private void ApplyHeadBob()
    {
        if (playerCamera != null)
        {
            // Smoothly move camera to target position
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition, 
                targetCameraPosition, 
                Time.deltaTime * bobDamping
            );
        }
    }

    private void HandleFootsteps()
    {
        if (isGrounded && actualMovementSpeed > 0.5f && Time.time > nextFootstepTime)
        {
            // Determine footstep delay based on movement speed
            float delay = actualMovementSpeed > walkSpeed * 0.8f ? footstepDelayRun : footstepDelayWalk;
            nextFootstepTime = Time.time + delay;
            
            if (footstepSounds.Length > 0)
            {
                // Play random footstep sound with slight pitch variation
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.pitch = 1f + Random.Range(-footstepRandomPitch, footstepRandomPitch);
                audioSource.PlayOneShot(clip, footstepVolume * (actualMovementSpeed / runSpeed));
            }
        }
    }
}