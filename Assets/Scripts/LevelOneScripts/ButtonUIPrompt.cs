using UnityEngine;
using UnityEngine.UI;

[HelpURL("Add your documentation URL here if you have one")]
public class ButtonUIPrompt : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("The texture to display as a prompt")]
    public Texture2D texturePrompt;
    
    [Space(10)]
    [Tooltip("World-space size of the UI element in meters")]
    public Vector2 uiSize = new Vector2(0.5f, 0.5f);
    
    [Tooltip("Height offset above the button")]
    public Vector3 uiOffset = new Vector3(0, 0.3f, 0);
    
    [Range(0.1f, 2f)]
    [Tooltip("Overall scale multiplier for the UI")]
    public float scaleFactor = 0.3f;

    [Header("Visibility Settings")]
    [Range(0.1f, 10f)]
    [Tooltip("Maximum distance to show the prompt")]
    public float maxDisplayDistance = 3f;
    
    [Range(5f, 90f)]
    [Tooltip("Viewing angle threshold to show prompt")]
    public float viewAngleThreshold = 45f;

    [Header("Rendering Settings")]
    [Tooltip("Sorting layer name (create in Project Settings > Tags and Layers)")]
    public string sortingLayer = "UI";
    
    [Range(0, 500)]
    [Tooltip("Higher values render on top of lower values")]
    public int sortingOrder = 100;
    
    [Tooltip("Should the UI always face the camera directly?")]
    public bool billboardEffect = true;
    
    [Tooltip("Should the UI stay upright (prevent tilting with camera)?")]
    public bool keepUpright = true;

    [Header("Appearance Settings")]
    [Tooltip("Base color tint for the prompt")]
    public Color tintColor = Color.white;
    
    [Tooltip("Enable outline effect for better visibility")]
    public bool useOutline = true;
    
    [Tooltip("Outline color")]
    [SerializeField] private Color outlineColor = Color.black;
    
    [Range(1f, 1.5f)]
    [Tooltip("Outline size multiplier")]
    [SerializeField] private float outlineSize = 1.1f;

    // Private references
    private GameObject promptUI;
    private Canvas canvas;
    private Camera mainCamera;
    private Image promptImage;

    void Start()
    {
        mainCamera = Camera.main;
        CreatePromptUI();
        UpdateAppearance();
    }

    void CreatePromptUI()
    {
        // Create canvas
        promptUI = new GameObject("WorldspacePromptCanvas");
        promptUI.transform.SetParent(transform); // Make button the parent
        promptUI.transform.localPosition = uiOffset;
        promptUI.transform.localScale = Vector3.one * scaleFactor;

        canvas = promptUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        canvas.sortingOrder = sortingOrder;
        canvas.sortingLayerName = sortingLayer;

        // Create image
        GameObject imageGO = new GameObject("PromptImage");
        imageGO.transform.SetParent(promptUI.transform, false);
        
        promptImage = imageGO.AddComponent<Image>();
        UpdateImageSprite();
        
        // Setup rect transform
        RectTransform rt = imageGO.GetComponent<RectTransform>();
        rt.sizeDelta = uiSize;
        rt.anchoredPosition3D = Vector3.zero;
        rt.localRotation = Quaternion.identity;

        // Add outline if enabled
        if (useOutline) AddOutlineEffect(imageGO);

        promptUI.SetActive(false);
    }

    void UpdateImageSprite()
    {
        if (texturePrompt != null)
        {
            promptImage.sprite = Sprite.Create(
                texturePrompt,
                new Rect(0, 0, texturePrompt.width, texturePrompt.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        promptImage.color = tintColor;
    }

    void AddOutlineEffect(GameObject parent)
    {
        GameObject outline = new GameObject("Outline");
        outline.transform.SetParent(parent.transform, false);
        Image outlineImg = outline.AddComponent<Image>();
        
        if (texturePrompt != null)
        {
            outlineImg.sprite = Sprite.Create(
                texturePrompt,
                new Rect(0, 0, texturePrompt.width, texturePrompt.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        outlineImg.color = outlineColor;
        
        RectTransform rt = outline.GetComponent<RectTransform>();
        rt.sizeDelta = uiSize * outlineSize;
        rt.SetAsFirstSibling();
    }

    void Update()
    {
        if (promptUI == null || mainCamera == null) return;

        Vector3 uiPosition = transform.TransformPoint(uiOffset);
        Vector3 toUI = uiPosition - mainCamera.transform.position;
        float distance = toUI.magnitude;

        bool shouldShow = distance <= maxDisplayDistance && 
                        Vector3.Angle(mainCamera.transform.forward, toUI.normalized) <= viewAngleThreshold;

        promptUI.SetActive(shouldShow);

        if (shouldShow)
        {
            UpdateUIPosition();
        }
    }

    void UpdateUIPosition()
    {
        // Position relative to button with offset
        promptUI.transform.localPosition = uiOffset;
        promptUI.transform.localScale = Vector3.one * scaleFactor;

        // Face the camera based on settings
        if (billboardEffect)
        {
            promptUI.transform.rotation = Quaternion.LookRotation(
                promptUI.transform.position - mainCamera.transform.position);
            
            if (keepUpright)
            {
                promptUI.transform.rotation = Quaternion.Euler(0, promptUI.transform.eulerAngles.y, 0);
            }
        }
        else
        {
            promptUI.transform.rotation = Quaternion.identity;
        }
    }

    void UpdateAppearance()
    {
        if (promptImage != null)
        {
            promptImage.color = tintColor;
            UpdateImageSprite();
        }
    }

    void OnValidate()
    {
        // This runs in the editor when values change
        if (!Application.isPlaying) return;

        if (promptUI != null)
        {
            // Update transform properties
            promptUI.transform.localPosition = uiOffset;
            promptUI.transform.localScale = Vector3.one * scaleFactor;

            // Update canvas properties
            canvas.sortingOrder = sortingOrder;
            canvas.sortingLayerName = sortingLayer;

            // Update appearance
            UpdateAppearance();

            // Update outline if it exists
            Transform outline = promptUI.transform.Find("PromptImage/Outline");
            if (outline != null)
            {
                outline.GetComponent<Image>().color = outlineColor;
                outline.GetComponent<RectTransform>().sizeDelta = uiSize * outlineSize;
                outline.gameObject.SetActive(useOutline);
            }
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw position indicator
        Gizmos.color = Color.cyan;
        Vector3 worldOffset = transform.TransformPoint(uiOffset);
        Gizmos.DrawWireSphere(worldOffset, 0.05f);
        Gizmos.DrawLine(transform.position, worldOffset);

        // Draw visibility range
        UnityEditor.Handles.color = new Color(0, 1, 1, 0.1f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, maxDisplayDistance);

        // Draw view angle
        UnityEditor.Handles.color = new Color(1, 1, 0, 0.2f);
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -viewAngleThreshold, 0) * forward;
        Vector3 right = Quaternion.Euler(0, viewAngleThreshold, 0) * forward;
        UnityEditor.Handles.DrawSolidArc(
            transform.position, 
            Vector3.up, 
            left, 
            viewAngleThreshold * 2, 
            maxDisplayDistance);
    }
    #endif
}