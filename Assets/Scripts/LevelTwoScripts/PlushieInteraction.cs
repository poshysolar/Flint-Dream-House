using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlushieInteraction : MonoBehaviour
{
    [SerializeField] private Texture2D interactionPrompt; // Assign in Inspector
    [SerializeField] private GameObject plushieModel; // Assign in Inspector
    [SerializeField] private float rotationSpeed = 50f; // Rotation speed for inspection
    [SerializeField] private float moveSpeed = 2.5f; // Increased move speed for the plushie
    [SerializeField] private float interactionDistance = 3f; // Max interaction distance
    [SerializeField] private MonoBehaviour playerController; // Assign your player controller
    [SerializeField] private float inspectionScale = 0.75f; // Slightly larger scale
    [SerializeField] private float inspectionDistance = 3f; // Distance from camera

    private Camera mainCamera;
    private GameObject inspectionInstance;
    private Canvas inspectionCanvas;
    private Image promptImage;
    private Light spotlight;
    private bool isInspecting = false;
    private Vector3 inspectionPosition;
    private Quaternion originalCameraRotation;
    private Vector3 originalPlushiePosition;
    private Quaternion originalPlushieRotation;
    private Vector3 originalPlushieScale;
    private Button exitButton;
    private EventSystem eventSystem;
    private readonly Vector3 plushiePosition = new Vector3(-26.40779f, 6.975255f, -1.869101f);
    private GameObject promptCanvasObj;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Ensure a camera is tagged as 'MainCamera'.");
            return;
        }
        SetupPromptCanvas();
        SetupUICanvas();
        SetupSpotlight();
        SetupEventSystem();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.runInBackground = true; // Ensure cursor behaves in editor
    }

    void Update()
    {
        if (mainCamera == null) return;

        if (isInspecting)
        {
            HandleInspectionInput();
            // Ensure cursor is visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Hide prompt when inspecting
            if (promptCanvasObj != null)
            {
                promptCanvasObj.SetActive(false);
            }
        }
        else
        {
            HandleInteraction();
            UpdatePrompt();
        }
    }

    private void SetupPromptCanvas()
    {
        // Create a separate canvas for the interaction prompt
        promptCanvasObj = new GameObject("PromptCanvas");
        Canvas promptCanvas = promptCanvasObj.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler promptScaler = promptCanvasObj.AddComponent<CanvasScaler>();
        promptScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        promptScaler.referenceResolution = new Vector2(1920, 1080);
        promptCanvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Prompt Image
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(promptCanvas.transform, false);
        promptImage = promptObj.AddComponent<Image>();
        if (interactionPrompt != null)
        {
            promptImage.sprite = Sprite.Create(interactionPrompt, new Rect(0, 0, interactionPrompt.width, interactionPrompt.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning("Interaction Prompt Texture2D not assigned in Inspector.");
        }
        promptImage.rectTransform.sizeDelta = new Vector2(100, 100);
        promptImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        promptImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        promptImage.rectTransform.anchoredPosition = Vector2.zero;
        promptImage.enabled = false;
    }

    private void SetupEventSystem()
    {
        // Check if an EventSystem already exists in the scene
        eventSystem = FindObjectOfType<EventSystem>();
        
        // If no EventSystem exists, create one
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("Created new EventSystem");
        }
        else
        {
            Debug.Log("Using existing EventSystem");
        }
    }

    private void SetupSpotlight()
    {
        GameObject lightObj = new GameObject("Spotlight");
        spotlight = lightObj.AddComponent<Light>();
        spotlight.type = LightType.Spot;
        spotlight.transform.position = plushiePosition + new Vector3(0, 2f, -1f);
        spotlight.transform.LookAt(plushiePosition);
        spotlight.color = new Color(0.8f, 0.8f, 0.9f); // Blue-tinted for horror
        spotlight.intensity = 2f;
        spotlight.spotAngle = 60f;
        spotlight.range = 5f;
        spotlight.shadows = LightShadows.Soft;
        spotlight.gameObject.SetActive(false);
    }

    private void SetupUICanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("InspectionCanvas");
        inspectionCanvas = canvasObj.AddComponent<Canvas>();
        inspectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        inspectionCanvas.sortingOrder = 100; // Ensure canvas is on top
        inspectionCanvas.gameObject.SetActive(false);

        // Create Exit Button
        GameObject buttonObj = new GameObject("ExitButton");
        buttonObj.transform.SetParent(inspectionCanvas.transform, false);
        exitButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f); // Dark grey, high opacity

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.anchoredPosition = new Vector2(100, -50); // Visible position
        buttonRect.sizeDelta = new Vector2(150, 60); // Larger for visibility

        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "EXIT";
        buttonText.color = new Color(0.9f, 0.9f, 1f); // Light blue for horror
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Clear any existing listeners and add our exit function
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(ExitInspectionFromButton);
    }

    private void UpdatePrompt()
    {
        if (promptImage == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            promptImage.enabled = (hit.transform == transform);
        }
        else
        {
            promptImage.enabled = false;
        }
    }

    private void HandleInteraction()
    {
        if (mainCamera == null || isInspecting) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.transform == transform && Input.GetMouseButtonDown(0))
            {
                StartInspection();
            }
        }
    }

    private void StartInspection()
    {
        if (plushieModel == null)
        {
            Debug.LogError("Plushie Model not assigned in Inspector.");
            return;
        }

        isInspecting = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Store original camera rotation and lock it
        originalCameraRotation = mainCamera.transform.rotation;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.05f); // Dark, eerie background

        spotlight.gameObject.SetActive(true);
        inspectionCanvas.gameObject.SetActive(true);
        
        // Hide the prompt canvas during inspection
        if (promptCanvasObj != null)
        {
            promptCanvasObj.SetActive(false);
        }

        // Store original plushie transform data
        originalPlushiePosition = plushieModel.transform.position;
        originalPlushieRotation = plushieModel.transform.rotation;
        originalPlushieScale = plushieModel.transform.localScale;

        // Position and scale plushie in front of camera
        inspectionPosition = mainCamera.transform.position + mainCamera.transform.forward * inspectionDistance;
        plushieModel.transform.position = inspectionPosition;
        plushieModel.transform.rotation = Quaternion.identity;
        plushieModel.transform.localScale = Vector3.one * inspectionScale;
        
        // Store reference to the model
        inspectionInstance = plushieModel;

        // Disable player controller if assigned
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Make sure the exit button is properly set up
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitInspectionFromButton);
            Debug.Log("Exit button listener added");
        }
        else
        {
            Debug.LogError("Exit button is null!");
        }
    }

    private void HandleInspectionInput()
    {
        if (inspectionInstance != null)
        {
            // Handle rotation with mouse movement
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

                // Rotate locally for natural movement
                inspectionInstance.transform.Rotate(Vector3.up * -mouseX, Space.World);
                inspectionInstance.transform.Rotate(Vector3.right * mouseY, Space.World);
            }
            
            // Handle movement with keyboard
            float horizontalInput = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float verticalInput = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            
            // Move left/right and up/down
            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0);
            inspectionInstance.transform.Translate(movement, Space.World);
            
            // Check for escape key as a backup exit method
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitInspection();
            }
        }
    }

    // Separate method specifically for the button click
    public void ExitInspectionFromButton()
    {
        Debug.Log("ExitInspectionFromButton called");
        ExitInspection();
    }

    public void ExitInspection()
    {
        Debug.Log("ExitInspection called");
        
        if (!isInspecting) return;
        
        isInspecting = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Restore camera
        mainCamera.clearFlags = CameraClearFlags.Skybox; // Restore original background
        mainCamera.transform.rotation = originalCameraRotation;

        spotlight.gameObject.SetActive(false);
        inspectionCanvas.gameObject.SetActive(false);
        
        // Show the prompt canvas again after exiting inspection
        if (promptCanvasObj != null)
        {
            promptCanvasObj.SetActive(true);
        }

        if (inspectionInstance != null)
        {
            // Return the plushie to its original position
            inspectionInstance.transform.position = originalPlushiePosition;
            inspectionInstance.transform.rotation = originalPlushieRotation;
            inspectionInstance.transform.localScale = originalPlushieScale;
            
            // Don't destroy the original model
            inspectionInstance = null; // Clear the reference
        }

        // Re-enable player controller if assigned
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Update prompt visibility after exiting
        UpdatePrompt();
    }

    // This ensures the exit button works in the Unity Editor
    private void OnValidate()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitInspectionFromButton);
        }
    }

    // Make sure to clean up when the script is destroyed
    private void OnDestroy()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
        }
    }
}