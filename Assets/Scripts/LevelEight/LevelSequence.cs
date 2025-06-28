using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSequence : MonoBehaviour
{
    [Header("Camera Settings")]
    public GameObject playerCamera;
    public GameObject introCamera;
    public float cameraMoveSpeed = 5f;

    [Header("Head Bobbing Settings")]
    [Tooltip("Amount of vertical head bob")]
    public float bobAmount = 0.1f;
    [Tooltip("Speed of the head bob")]
    public float bobSpeed = 4f;
    [Tooltip("How much the camera tilts side to side")]
    public float tiltAmount = 3f;
    [Tooltip("How fast the camera tilts")]
    public float tiltSpeed = 2f;
    [Tooltip("Smoothing factor for head bob")]
    public float bobSmoothness = 2f;

    [Header("Player Settings")]
    public Vector3 playerSpawnPosition = new Vector3(-106.83f, 2.237f, 5.81f);
    public Vector3 playerSpawnRotation = new Vector3(0f, -270f, 0f);
    public MonoBehaviour playerMovementScript;
    public string playerTag = "Player";
    public float playerGravityMultiplier = 5f; // Higher gravity for player

    [Header("Bear Settings")]
    public GameObject bearPrefab;
    public Vector3 bearSpawnPosition = new Vector3(-97.91f, 0f, 4.84f);
    public Vector3 bearSpawnRotation = new Vector3(-90f, -90f, 0f);
    public RuntimeAnimatorController bearThrowController;
    public float throwDuration = 1f;
    public Vector3 throwLandingPosition = new Vector3(-152.17f, 2.237f, 5.81f);

    [Header("Platform Settings")]
    public GameObject fallingPlatform;
    public float platformFallDelay = 1f;
    public float platformGravityMultiplier = 3f; // Higher gravity for platform
    public float waitAfterFall = 3f;

    [Header("Screen Transition")]
    public float fadeDuration = 3f;
    public string endScreenName = "EndScreen";

    private GameObject playerInstance;
    private GameObject bearInstance;
    private bool isThrowing = false;
    private bool platformFalling = false;
    private CanvasGroup fadeCanvasGroup;
    private Vector3 originalIntroCamPosition;
    private Vector3 originalIntroCamRotation;
    private float bobTimer = 0f;
    private float currentBobAmount = 0f;
    private float currentTiltAmount = 0f;

    void Start()
    {
        // Create fade UI automatically
        CreateFadeUI();
        
        // Initialize cameras
        playerCamera.SetActive(false);
        introCamera.SetActive(true);
        originalIntroCamPosition = introCamera.transform.localPosition;
        originalIntroCamRotation = introCamera.transform.localEulerAngles;

        // Find player in scene or create one if it doesn't exist
        playerInstance = GameObject.FindWithTag(playerTag);
        if (playerInstance == null)
        {
            playerInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerInstance.name = "Player";
            playerInstance.tag = playerTag;
        }
        
        // Ensure player has a Rigidbody for physics
        SetupPlayerPhysics();
        
        // Disable player renderer initially
        Renderer playerRenderer = playerInstance.GetComponent<Renderer>();
        if (playerRenderer != null)
            playerRenderer.enabled = false;

        // Disable player movement if script exists
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // Ensure falling platform has Rigidbody
        if (fallingPlatform != null)
        {
            SetupPlatformPhysics();
        }

        // Start the sequence
        StartCoroutine(LevelIntroSequenceCoroutine());
    }
    
    void SetupPlayerPhysics()
    {
        Rigidbody playerRb = playerInstance.GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            playerRb = playerInstance.AddComponent<Rigidbody>();
        }
        
        // Set up player rigidbody for good falling physics
        playerRb.isKinematic = true; // Start as kinematic (no physics)
        playerRb.useGravity = false; // Will enable gravity when needed
        playerRb.mass = 1f;
        playerRb.drag = 0f;
        playerRb.angularDrag = 0.05f;
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Add a box collider (better than capsule for platform sitting)
        Collider[] colliders = playerInstance.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            DestroyImmediate(col);
        }
        
        BoxCollider boxCol = playerInstance.AddComponent<BoxCollider>();
        boxCol.center = new Vector3(0, 0, 0);
        boxCol.size = new Vector3(1, 2, 1);
        
        Debug.Log("Player physics components configured");
    }
    
    void SetupPlatformPhysics()
    {
        Rigidbody platformRb = fallingPlatform.GetComponent<Rigidbody>();
        if (platformRb == null)
        {
            platformRb = fallingPlatform.AddComponent<Rigidbody>();
        }
        
        // Set up platform rigidbody
        platformRb.isKinematic = true; // Start kinematic
        platformRb.useGravity = false; // Will enable gravity when needed
        platformRb.mass = 50f; // Heavy platform
        platformRb.drag = 0f;
        platformRb.angularDrag = 0.05f;
        platformRb.interpolation = RigidbodyInterpolation.Interpolate;
        platformRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Ensure platform has a collider
        if (fallingPlatform.GetComponent<Collider>() == null)
        {
            BoxCollider platformCollider = fallingPlatform.AddComponent<BoxCollider>();
        }
        
        Debug.Log("Platform physics components configured");
    }

    void CreateFadeUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Fade Panel
        GameObject panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = Color.black;

        // Set up RectTransform
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Add Canvas Group
        fadeCanvasGroup = panelGO.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    IEnumerator LevelIntroSequenceCoroutine()
    {
        // Move intro camera along X axis
        float startX = -58.25f;
        float endX = -91.45f;
        float currentX = startX;

        introCamera.transform.position = new Vector3(startX, introCamera.transform.position.y, introCamera.transform.position.z);

        // Move camera
        while (currentX > endX)
        {
            currentX -= cameraMoveSpeed * Time.deltaTime;
            
            // Calculate head bob with smoothing
            bobTimer += Time.deltaTime * bobSpeed;
            
            // Target values
            float targetBob = Mathf.Sin(bobTimer) * bobAmount;
            float targetTilt = Mathf.Sin(bobTimer * 0.5f * tiltSpeed) * tiltAmount;
            
            // Smoothly interpolate to target values
            currentBobAmount = Mathf.Lerp(currentBobAmount, targetBob, bobSmoothness * Time.deltaTime);
            currentTiltAmount = Mathf.Lerp(currentTiltAmount, targetTilt, bobSmoothness * Time.deltaTime);
            
            // Apply movement and smoothed head bob
            introCamera.transform.position = new Vector3(
                currentX,
                originalIntroCamPosition.y + currentBobAmount,
                introCamera.transform.position.z
            );
            
            // Apply smoothed tilt
            introCamera.transform.localEulerAngles = new Vector3(
                originalIntroCamRotation.x,
                originalIntroCamRotation.y,
                currentTiltAmount
            );
            
            yield return null;
        }

        // Reset camera when done moving
        introCamera.transform.localEulerAngles = originalIntroCamRotation;
        introCamera.transform.position = new Vector3(endX, originalIntroCamPosition.y, introCamera.transform.position.z);

        // IMPORTANT: Force teleport player to spawn position regardless of current position
        playerInstance.transform.position = playerSpawnPosition;
        playerInstance.transform.eulerAngles = playerSpawnRotation;
        
        Debug.Log("Player positioned at: " + playerSpawnPosition);
        
        // Switch to player camera
        introCamera.SetActive(false);
        playerCamera.SetActive(true);

        // Enable player renderer
        Renderer playerRenderer = playerInstance.GetComponent<Renderer>();
        if (playerRenderer != null)
            playerRenderer.enabled = true;

        // Spawn bear with specified rotation
        if (bearPrefab != null)
        {
            bearInstance = Instantiate(bearPrefab, bearSpawnPosition, Quaternion.Euler(bearSpawnRotation));
            
            // Get the animator component
            Animator bearAnimator = bearInstance.GetComponent<Animator>();
            if (bearAnimator != null)
            {
                // Apply the throw controller to the bear if provided
                if (bearThrowController != null)
                {
                    bearAnimator.runtimeAnimatorController = bearThrowController;
                    Debug.Log("Applied custom throw controller to bear");
                }
                
                // Make sure animator is enabled
                bearAnimator.enabled = true;
                
                // Try multiple ways to trigger the throw animation
                bearAnimator.Play("Throw", 0, 0f);
                bearAnimator.SetTrigger("Throw");
                
                Debug.Log("Triggered bear throw animation");
            }
            else
            {
                Debug.LogError("Bear has no Animator component!");
            }
            
            // Give the bear animation a moment to start (short delay)
            yield return new WaitForSeconds(0.2f);
        }

        // THROW PLAYER
        Debug.Log("Starting player throw from: " + playerInstance.transform.position);
        yield return StartCoroutine(ThrowPlayer());
        Debug.Log("Player throw completed at: " + playerInstance.transform.position);

        // Position player directly on platform
        AttachPlayerToPlatform();
        
        // Wait before making platform fall
        yield return new WaitForSeconds(platformFallDelay);

        // Make platform fall with player
        StartFallingSequence();

        // Wait longer before fading to see the platform and player fall
        yield return new WaitForSeconds(waitAfterFall);

        // Fade screen (with slower fade)
        if (fadeCanvasGroup != null)
        {
            Debug.Log("Starting screen fade");
            float fadeStartTime = Time.time;
            while (Time.time - fadeStartTime < fadeDuration)
            {
                float progress = (Time.time - fadeStartTime) / fadeDuration;
                fadeCanvasGroup.alpha = progress;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // Load end screen
        if (!string.IsNullOrEmpty(endScreenName))
        {
            Debug.Log("Loading end screen: " + endScreenName);
            SceneManager.LoadScene(endScreenName);
        }
    }

    // DIRECT METHOD: Parent player to platform for falling
    void AttachPlayerToPlatform()
    {
        if (fallingPlatform != null && playerInstance != null)
        {
            // Get platform's top surface position
            Bounds platformBounds = new Bounds();
            
            Renderer platformRenderer = fallingPlatform.GetComponent<Renderer>();
            if (platformRenderer != null)
            {
                platformBounds = platformRenderer.bounds;
            }
            else
            {
                Collider platformCollider = fallingPlatform.GetComponent<Collider>();
                if (platformCollider != null)
                {
                    platformBounds = platformCollider.bounds;
                }
            }
            
            // Position player directly on top of platform
            float platformTopY = platformBounds.max.y;
            
            Vector3 playerPositionOnPlatform = new Vector3(
                throwLandingPosition.x,
                platformTopY + 0.1f, // Slightly above platform
                throwLandingPosition.z
            );
            
            playerInstance.transform.position = playerPositionOnPlatform;
            
            // MOST IMPORTANT: PARENT PLAYER TO PLATFORM
            playerInstance.transform.SetParent(fallingPlatform.transform);
            
            Debug.Log("Player PARENTED to platform at position: " + playerInstance.transform.position);
            
            // Also try adding a FixedJoint to connect player and platform
            FixedJoint joint = playerInstance.AddComponent<FixedJoint>();
            joint.connectedBody = fallingPlatform.GetComponent<Rigidbody>();
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            
            Debug.Log("Added FixedJoint to connect player to platform");
        }
    }

    void StartFallingSequence()
    {
        if (fallingPlatform != null && playerInstance != null)
        {
            Debug.Log("--- STARTING FALL SEQUENCE ---");
            
            // First, make sure player's rigidbody is set up for falling
            Rigidbody playerRb = playerInstance.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
                playerRb.useGravity = true;
                // Add extreme gravity force
                playerRb.AddForce(Vector3.down * 9.81f * playerGravityMultiplier, ForceMode.Acceleration);
                Debug.Log("Player gravity enabled with multiplier: " + playerGravityMultiplier);
            }
            
            // Make platform fall with extreme gravity
            Rigidbody platformRb = fallingPlatform.GetComponent<Rigidbody>();
            if (platformRb != null)
            {
                platformRb.isKinematic = false;
                platformRb.useGravity = true;
                // Add extreme gravity force
                platformRb.AddForce(Vector3.down * 9.81f * platformGravityMultiplier, ForceMode.Acceleration);
                Debug.Log("Platform gravity enabled with multiplier: " + platformGravityMultiplier);
            }
            
            platformFalling = true;
            
            // Start a coroutine to monitor the falling
            StartCoroutine(MonitorFalling());
        }
    }
    
    IEnumerator MonitorFalling()
    {
        float startY = fallingPlatform.transform.position.y;
        float startTime = Time.time;
        
        while (Time.time - startTime < waitAfterFall)
        {
            float currentY = fallingPlatform.transform.position.y;
            float distanceFallen = startY - currentY;
            
            Debug.Log("Platform falling: " + distanceFallen + " meters. Player position: " + 
                      playerInstance.transform.position + " (local: " + playerInstance.transform.localPosition + ")");
            
            // Add continuous downward force to both objects
            Rigidbody platformRb = fallingPlatform.GetComponent<Rigidbody>();
            Rigidbody playerRb = playerInstance.GetComponent<Rigidbody>();
            
            if (platformRb != null)
                platformRb.AddForce(Vector3.down * 9.81f * platformGravityMultiplier, ForceMode.Acceleration);
                
            if (playerRb != null)
                playerRb.AddForce(Vector3.down * 9.81f * playerGravityMultiplier, ForceMode.Acceleration);
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator ThrowPlayer()
    {
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null during throw!");
            yield break;
        }

        isThrowing = true;
        float throwStartTime = Time.time;
        Vector3 throwStartPosition = playerInstance.transform.position;
        
        Debug.Log("Throw starting - From: " + throwStartPosition + " To: " + throwLandingPosition);

        // Disable player physics during throw
        Rigidbody playerRb = playerInstance.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
        }

        // Create a clear visible throw arc
        while (Time.time - throwStartTime < throwDuration)
        {
            float progress = (Time.time - throwStartTime) / throwDuration;
            
            // Higher arc for visibility
            float height = Mathf.Sin(progress * Mathf.PI) * 10f;
            
            Vector3 position = Vector3.Lerp(throwStartPosition, throwLandingPosition, progress);
            position.y += height; // Add height for arc
            
            playerInstance.transform.position = position;
            
            yield return null;
        }

        // Ensure player is exactly at landing position
        playerInstance.transform.position = throwLandingPosition;
        Debug.Log("Player landed at: " + playerInstance.transform.position);

        isThrowing = false;
    }

    // OnDrawGizmos to visualize the path in the editor
    void OnDrawGizmos()
    {
        if (playerSpawnPosition != Vector3.zero && throwLandingPosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(playerSpawnPosition, 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(throwLandingPosition, 0.5f);
            
            // Draw throw path
            Gizmos.color = Color.yellow;
            Vector3 start = playerSpawnPosition;
            Vector3 end = throwLandingPosition;
            
            int segments = 20;
            Vector3 previous = start;
            
            for (int i = 1; i <= segments; i++)
            {
                float progress = (float)i / segments;
                float height = Mathf.Sin(progress * Mathf.PI) * 10f;
                
                Vector3 position = Vector3.Lerp(start, end, progress);
                position.y += height;
                
                Gizmos.DrawLine(previous, position);
                previous = position;
            }
        }
    }
}
