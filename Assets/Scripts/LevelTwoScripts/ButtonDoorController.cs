using UnityEngine;
using UnityEngine.UI;

public class ButtonDoorController : MonoBehaviour
{
    [SerializeField] private GameObject slidingDoor;
    [SerializeField] [Range(1f, 20f)] private float slideSpeed = 10f; // Editable door slide speed
    [SerializeField] private Texture2D promptTexture; // Texture for UI prompt
    [SerializeField] [Range(1f, 10f)] private float promptDistance = 5f; // Distance for prompt visibility
    [SerializeField] [Range(1f, 40f)] private float clickDistance = 5f; // Distance for button click activation
    [SerializeField] private AudioClip buttonPressSound; // Sound effect for button press

    private Vector3 closedPosition = new Vector3(34.66f, 26.9528f, 45.82f);
    private Vector3 openPosition = new Vector3(53.27f, 26.9528f, 45.82f);
    private bool isOpen = false;
    
    private Material buttonMaterial;
    private Color redGlow = new Color(1f, 0f, 0f, 1f);
    private Color greenGlow = new Color(0f, 1f, 0f, 1f);
    
    private GameObject promptUI;
    private RawImage promptImage;
    private AudioSource audioSource;
    private Camera playerCamera;

    void Start()
    {
        // Set up player camera
        playerCamera = Camera.main;

        // Create and set up glowing material for the button
        buttonMaterial = new Material(Shader.Find("Standard"));
        buttonMaterial.EnableKeyword("_EMISSION");
        
        // Set the main color to match the emission color for full coverage
        buttonMaterial.SetColor("_Color", redGlow);
        buttonMaterial.SetColor("_EmissionColor", redGlow * 3f);
        
        // Apply material to the entire cylinder
        Renderer renderer = GetComponent<Renderer>();
        
        // Replace all materials on the cylinder with our emissive material
        Material[] materials = new Material[renderer.materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = buttonMaterial;
        }
        renderer.materials = materials;

        // Ensure door starts at closed position
        if (slidingDoor != null)
        {
            slidingDoor.transform.position = closedPosition;
        }

        // Set up audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = buttonPressSound;
        audioSource.playOnAwake = false;

        // Set up UI prompt
        SetupPromptUI();
    }

    void SetupPromptUI()
    {
        if (promptTexture == null) return;

        // Create a world space canvas that will follow the camera
        GameObject canvasObj = new GameObject("PromptCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 999; // Ensure it's on top of everything
        
        // Add necessary components
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create the prompt image
        GameObject imageObj = new GameObject("PromptImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        promptImage = imageObj.AddComponent<RawImage>();
        promptImage.texture = promptTexture;
        promptImage.raycastTarget = false;
        
        // Set up the rect transform for proper sizing
        RectTransform rectTransform = promptImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Adjust size as needed
        rectTransform.pivot = new Vector2(0.5f, 0);
        
        // Position the canvas in world space
        canvasObj.transform.position = transform.position + Vector3.up * 0.5f; // Position above button
        canvasObj.transform.localScale = Vector3.one * 0.01f; // Scale for world space
        
        promptUI = canvasObj;
        promptUI.SetActive(false); // Initially hidden
    }

    void Update()
    {
        // Handle door movement
        if (slidingDoor != null)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            slidingDoor.transform.position = Vector3.MoveTowards(
                slidingDoor.transform.position,
                targetPosition,
                slideSpeed * Time.deltaTime
            );
        }

        // Update prompt visibility and position
        UpdatePromptVisibility();

        // Detect mouse click on button
        if (Input.GetMouseButtonDown(0))
        {
            // Check distance to player
            float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
            if (distance <= clickDistance) // Only process click if within clickDistance
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform)
                    {
                        // Toggle door state
                        isOpen = !isOpen;
                        
                        // Update button color based on door state
                        Color newEmissionColor = (isOpen ? greenGlow : redGlow) * 3f;
                        Color newBaseColor = isOpen ? greenGlow : redGlow;
                        
                        // Apply to all materials on the cylinder
                        Renderer renderer = GetComponent<Renderer>();
                        foreach (Material mat in renderer.materials)
                        {
                            mat.SetColor("_Color", newBaseColor);
                            mat.SetColor("_EmissionColor", newEmissionColor);
                        }

                        // Play sound effect
                        if (audioSource != null && buttonPressSound != null)
                        {
                            audioSource.Play();
                        }
                    }
                }
            }
        }
    }

    void UpdatePromptVisibility()
    {
        if (promptUI == null || playerCamera == null) return;

        // Check distance to player
        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        bool withinDistance = distance <= promptDistance;

        // Check if player is looking at button
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        bool isLooking = false;

        if (Physics.Raycast(ray, out hit, promptDistance * 2f)) // Extended range for better detection
        {
            if (hit.transform == transform)
            {
                isLooking = true;
                
                // Position the prompt directly above where the player is looking
                Vector3 promptPosition = hit.point + (Vector3.up * 0.3f);
                promptUI.transform.position = promptPosition;
            }
        }

        // Show prompt only if within distance and looking at button
        promptUI.SetActive(withinDistance && isLooking);

        // Make prompt always face the camera
        if (promptUI.activeSelf)
        {
            // Calculate direction to face the camera
            Vector3 directionToCamera = playerCamera.transform.position - promptUI.transform.position;
            promptUI.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }

    // This ensures the material changes are applied in the editor
    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create material if it doesn't exist
            if (buttonMaterial == null)
            {
                buttonMaterial = new Material(Shader.Find("Standard"));
                buttonMaterial.EnableKeyword("_EMISSION");
            }
            
            // Set initial color (red since door starts closed)
            buttonMaterial.SetColor("_Color", redGlow);
            buttonMaterial.SetColor("_EmissionColor", redGlow * 3f);
            
            // Apply to renderer
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = buttonMaterial;
            }
            renderer.sharedMaterials = materials;
        }
    }

    // Draw a gizmo to visualize the click distance in the Scene view
    void OnDrawGizmos()
    {
        // Set the gizmo color (cyan for visibility)
        Gizmos.color = Color.cyan;
        // Draw a wireframe sphere centered at the button's position with radius equal to clickDistance
        Gizmos.DrawWireSphere(transform.position, clickDistance);
    }
}