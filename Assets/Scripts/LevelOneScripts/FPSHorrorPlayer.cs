using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FPSHorrorPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private Transform playerCamera;
    private float xRotation = 0f;

    [Header("Head Bobbing")]
    [SerializeField] private float walkBobSpeed = 4f; // Slower for horror feel
    [SerializeField] private float walkBobAmount = 0.015f; // Subtler vertical bob
    [SerializeField] private float walkSideBobAmount = 0.01f; // Added slight side bob
    [SerializeField] private float runBobSpeed = 5f;
    [SerializeField] private float runBobAmount = 0.025f;
    [SerializeField] private float runSideBobAmount = 0.015f;
    private float defaultYPos = 0;
    private float defaultXPos = 0; // Added for side bob
    private float timer = 0;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepDelayWalk = 0.5f;
    [SerializeField] private float footstepDelayRun = 0.3f;
    private float nextFootstepTime = 0;

    [Header("Crosshair")]
    [SerializeField] private Texture2D crosshairTexture;
    [SerializeField] private float crosshairSize = 20f;

    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 velocity;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        
        if (playerCamera != null)
        {
            defaultYPos = playerCamera.localPosition.y;
            defaultXPos = playerCamera.localPosition.x; // Store default X position
        }

        if (crosshairTexture == null)
        {
            CreateDefaultCrosshair();
        }
    }

    private void CreateDefaultCrosshair()
    {
        int size = 32; // Higher resolution for smoother circle
        crosshairTexture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float center = size / 2f;
        float radius = size / 2f - 2; // Slightly smaller to avoid edge clipping
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (distance <= radius) // Filled circle
                    pixels[y * size + x] = Color.white;
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
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        HandleHeadBobbing();
        HandleFootsteps();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp vertical rotation to prevent flipping
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 move = (transform.forward * Input.GetAxis("Vertical") + 
                       transform.right * Input.GetAxis("Horizontal")) * speed;
        
        controller.Move(move * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void HandleHeadBobbing()
    {
        if (playerCamera == null) return;

        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
        
        if (isMoving)
        {
            float bobSpeed = Input.GetKey(KeyCode.LeftShift) ? runBobSpeed : walkBobSpeed;
            float bobAmount = Input.GetKey(KeyCode.LeftShift) ? runBobAmount : walkBobAmount;
            float sideBobAmount = Input.GetKey(KeyCode.LeftShift) ? runSideBobAmount : walkSideBobAmount;

            timer += Time.deltaTime * bobSpeed;
            playerCamera.localPosition = new Vector3(
                defaultXPos + Mathf.Sin(timer * 0.5f) * sideBobAmount, // Slower side bob
                defaultYPos + Mathf.Sin(timer) * bobAmount,
                playerCamera.localPosition.z
            );
        }
        else
        {
            timer = 0;
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                new Vector3(defaultXPos, defaultYPos, 0),
                Time.deltaTime * 10f
            );
        }
    }

    private void HandleFootsteps()
    {
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
        
        if (isGrounded && isMoving && Time.time > nextFootstepTime)
        {
            float delay = Input.GetKey(KeyCode.LeftShift) ? footstepDelayRun : footstepDelayWalk;
            nextFootstepTime = Time.time + delay;
            
            if (footstepSounds.Length > 0)
            {
                audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
            }
        }
    }
}