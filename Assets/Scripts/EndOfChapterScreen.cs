using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class EndOfChapterScreen : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private string chapterText = "End Of Chapter One";
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Font textFont;
    [SerializeField] [Range(10, 150)] private int textSize = 72;

    [Header("Button Settings")]
    [SerializeField] private string buttonText = "Continue to Main Menu";
    [SerializeField] private Color buttonTextColor = Color.black;
    [SerializeField] private Color buttonNormalColor = Color.white;
    [SerializeField] private Color buttonHoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color buttonPressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] [Range(10, 100)] private int buttonTextSize = 36;
    [SerializeField] private Vector2 buttonSize = new Vector2(400, 80);
    [SerializeField] private float textButtonSpacing = 50f;

    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "MainMenu";

    private Canvas canvas;
    private GameObject backgroundPanel;
    private Text chapterTextComponent;
    private Button continueButton;
    private EventSystem eventSystem;

    void Start()
    {
        SetupCursor();
        CreateUIElements();
        CreateEventSystem();
    }

    private void SetupCursor()
    {
        // Make sure cursor is visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CreateEventSystem()
    {
        // Check if EventSystem already exists in the scene
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }
    }

    private void CreateUIElements()
    {
        // Create Canvas
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("EndOfChapterCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Ensure it's on top
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create Background
        if (backgroundPanel == null)
        {
            backgroundPanel = new GameObject("Background");
            backgroundPanel.transform.SetParent(canvas.transform);
            
            Image bgImage = backgroundPanel.AddComponent<Image>();
            bgImage.color = Color.black;
            
            RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }

        // Create Chapter Text
        if (chapterTextComponent == null)
        {
            GameObject textGO = new GameObject("ChapterText");
            textGO.transform.SetParent(canvas.transform);
            
            chapterTextComponent = textGO.AddComponent<Text>();
            chapterTextComponent.text = chapterText;
            chapterTextComponent.color = textColor;
            chapterTextComponent.font = textFont;
            chapterTextComponent.fontSize = textSize;
            chapterTextComponent.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(1000, 150);
            textRect.anchoredPosition = Vector2.zero;
        }

        // Create Continue Button
        if (continueButton == null)
        {
            GameObject buttonGO = new GameObject("ContinueButton");
            buttonGO.transform.SetParent(canvas.transform);
            
            // Add RectTransform first
            RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = buttonSize;
            
            continueButton = buttonGO.AddComponent<Button>();
            
            // Set button colors
            ColorBlock colors = continueButton.colors;
            colors.normalColor = buttonNormalColor;
            colors.highlightedColor = buttonHoverColor;
            colors.pressedColor = buttonPressedColor;
            colors.selectedColor = buttonHoverColor;
            colors.colorMultiplier = 1f;
            continueButton.colors = colors;
            
            // Add button image
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = buttonNormalColor;
            
            // Add outline
            Outline outline = buttonGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
            
            // Create button text
            GameObject buttonTextGO = new GameObject("ButtonText");
            buttonTextGO.transform.SetParent(buttonGO.transform);
            
            Text buttonTextComponent = buttonTextGO.AddComponent<Text>();
            buttonTextComponent.text = buttonText;
            buttonTextComponent.color = buttonTextColor;
            buttonTextComponent.font = textFont;
            buttonTextComponent.fontSize = buttonTextSize;
            buttonTextComponent.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            // Add click event
            continueButton.onClick.AddListener(LoadMainMenu);
        }

        // Position elements
        PositionElements();
    }

    private void LoadMainMenu()
    {
        Debug.Log("Loading Main Menu...");
        SceneManager.LoadScene(targetSceneName);
    }

    private void PositionElements()
    {
        // Add null checks for both components AND their RectTransforms
        if (chapterTextComponent == null || continueButton == null)
            return;

        RectTransform textRect = chapterTextComponent.GetComponent<RectTransform>();
        RectTransform buttonRect = continueButton.GetComponent<RectTransform>();
        
        if (textRect == null || buttonRect == null)
            return;

        // Position text at center
        textRect.anchoredPosition = Vector2.zero;

        // Position button below text with spacing
        buttonRect.anchoredPosition = new Vector2(
            0, 
            -textRect.sizeDelta.y/2 - buttonSize.y/2 - textButtonSpacing
        );
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && canvas != null)
        {
            // Update text properties
            if (chapterTextComponent != null)
            {
                chapterTextComponent.text = chapterText;
                chapterTextComponent.color = textColor;
                chapterTextComponent.font = textFont;
                chapterTextComponent.fontSize = textSize;
            }

            // Update button properties
            if (continueButton != null)
            {
                // Update colors
                ColorBlock colors = continueButton.colors;
                colors.normalColor = buttonNormalColor;
                colors.highlightedColor = buttonHoverColor;
                colors.pressedColor = buttonPressedColor;
                continueButton.colors = colors;

                // Update button image
                Image buttonImage = continueButton.GetComponent<Image>();
                if (buttonImage != null) buttonImage.color = buttonNormalColor;

                // Update button text
                Text buttonTextComponent = continueButton.GetComponentInChildren<Text>();
                if (buttonTextComponent != null)
                {
                    buttonTextComponent.text = buttonText;
                    buttonTextComponent.color = buttonTextColor;
                    buttonTextComponent.font = textFont;
                    buttonTextComponent.fontSize = buttonTextSize;
                }

                // Update button size - add null check for RectTransform
                RectTransform buttonRect = continueButton.GetComponent<RectTransform>();
                if (buttonRect != null) buttonRect.sizeDelta = buttonSize;
            }

            // Only call PositionElements if all components exist
            PositionElements();
        }
    }

    // Optional: Add keyboard support for accessibility
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (continueButton != null && continueButton.interactable)
            {
                LoadMainMenu();
            }
        }
    }
}