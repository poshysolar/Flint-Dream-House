using UnityEngine;
using UnityEngine.UI;

public class ClickableDoorSwitch : MonoBehaviour {
    [Header("Door Settings")]
    public GameObject doorToOpen;
    public float doorOpenPosition = -12.32f;
    public float doorMoveSpeed = 2f;

    [Header("Visual Settings")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    public float emissionIntensity = 5f;
    public Texture2D promptTexture;  // Add this field for the prompt texture
    private GameObject promptObject;  // GameObject to hold the prompt UI

    [Header("Audio Settings")]
    public AudioClip clickSound;
    private AudioSource audioSource;
    
    private bool isDoorOpen = false;
    private Vector3 doorClosedPosition;
    private Renderer switchRenderer;
    private Material switchMaterial;

    private void Start() {
        // Get references
        switchRenderer = GetComponent<Renderer>();
        // Create dynamic material
        switchMaterial = new Material(Shader.Find("Standard"));
        switchRenderer.material = switchMaterial;

        // Store initial door position if (doorToOpen != null)
        if (doorToOpen != null) {
            doorClosedPosition = doorToOpen.transform.position;
        }

        // Initialize switch color (red = closed)
        UpdateSwitchVisuals();

        // Add AudioSource component if not present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Create prompt UI
        CreatePromptUI();
    }

    private void CreatePromptUI() {
        if (promptTexture != null) {
            // Create a canvas for the prompt
            promptObject = new GameObject("PromptCanvas");
            promptObject.transform.SetParent(transform);
            
            // Position it above the button
            promptObject.transform.localPosition = new Vector3(0, 1.5f, 0);
            
            // Add Canvas component
            Canvas canvas = promptObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            // Add CanvasScaler
            CanvasScaler scaler = promptObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;
            
            // Create image object for the texture
            GameObject imageObj = new GameObject("PromptImage");
            imageObj.transform.SetParent(promptObject.transform, false);
            
            // Add RawImage component and set the texture
            RawImage image = imageObj.AddComponent<RawImage>();
            image.texture = promptTexture;
            
            // Set size
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1, 1);
            
            // Make it face the camera
            promptObject.AddComponent<Billboard>();
        }
    }

    private void Update() {
        HandlePlayerClick();
        HandleDoorMovement();
    }

    private void HandlePlayerClick() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject == gameObject) {
                    ToggleDoor();
                }
            }
        }
    }

    private void HandleDoorMovement() {
        if (doorToOpen == null) return;
        Vector3 targetPosition = isDoorOpen ? new Vector3(doorOpenPosition, doorClosedPosition.y, doorClosedPosition.z) : doorClosedPosition;
        doorToOpen.transform.position = Vector3.MoveTowards(
            doorToOpen.transform.position, targetPosition, doorMoveSpeed * Time.deltaTime
        );
    }

    private void ToggleDoor() {
        isDoorOpen = !isDoorOpen;
        UpdateSwitchVisuals();
        // Play sound when toggled
        if (clickSound != null) {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void UpdateSwitchVisuals() {
        if (switchMaterial != null) {
            Color baseColor = isDoorOpen ? unlockedColor : lockedColor;
            switchMaterial.color = baseColor;
            switchMaterial.SetColor("_EmissionColor", baseColor * emissionIntensity);
            switchMaterial.EnableKeyword("_EMISSION");
            // Force lighting update
            RendererExtensions.UpdateGIMaterials(switchRenderer);
            DynamicGI.UpdateEnvironment();
        }
    }
}

// Helper class to make the prompt always face the camera
public class Billboard : MonoBehaviour {
    void Update() {
        if (Camera.main != null) {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }
}