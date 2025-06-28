using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [Header("Customizable Settings")]
    public Texture2D backgroundTexture;
    public Font tmpFont;
    public Color buttonColor = Color.white;
    public Color textColor = Color.black;
    public int fontSize = 36;
    
    [Header("Button References")]
    public GameObject playButton;
    public GameObject creditsButton;
    public GameObject quitButton;
    
    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    
    [Header("Copyright Settings")]
    public Color copyrightTextColor = Color.white;
    public int copyrightFontSize = 24;
    
    [Header("Corner Image Settings")]
    public Texture cornerImage;
    public Vector2 cornerImageSize = new Vector2(150, 150);
    public Vector2 cornerImageOffset = new Vector2(100, 100); // Offset from bottom-left corner
    
    private Canvas canvas;
    private AudioSource audioSource;
    private GameObject copyrightText;
    private GameObject cornerImageObject;
    
    [ExecuteInEditMode]
    void Awake()
    {
        SetupUI();
        
        if (Application.isPlaying)
        {
            SetupAudio();
        }
    }
    
    void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.volume = musicVolume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    void Start()
    {
        // Ensure cursor is visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (playButton != null)
        {
            Button button = playButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SceneManager.LoadScene("Intro"));
            }
        }
        
        if (creditsButton != null)
        {
            Button button = creditsButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SceneManager.LoadScene("CreditsScene"));
            }
        }
        
        if (quitButton != null)
        {
            Button button = quitButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(QuitGame);
            }
            else
            {
                Debug.LogError("Quit button is missing Button component!");
            }
        }
        else
        {
            Debug.LogError("Quit button reference is null!");
        }
    }
    
    private void QuitGame()
    {
        Debug.Log("Quit button clicked! Calling Application.Quit()");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void SetupUI()
    {
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
            }
        }
        
        Transform bgTransform = canvas.transform.Find("Background");
        if (bgTransform == null && backgroundTexture != null)
        {
            CreateBackground();
        }
        else if (bgTransform != null && backgroundTexture != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                Sprite bgSprite = Sprite.Create(
                    backgroundTexture,
                    new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
                bgImage.sprite = bgSprite;
            }
        }
        
        if (playButton == null)
        {
            playButton = CreateButton("Play", "Intro", new Vector2(0, 100), new Vector2(300, 80));
        }
        
        if (creditsButton == null)
        {
            creditsButton = CreateButton("Credits", "CreditsScene", new Vector2(0, 0), new Vector2(300, 80));
        }
        
        if (quitButton == null)
        {
            quitButton = CreateQuitButton("Quit", new Vector2(0, -100), new Vector2(300, 80));
        }
        
        // Create copyright text
        if (copyrightText == null)
        {
            CreateCopyrightText();
        }
        
        // Create corner image
        if (cornerImageObject == null && cornerImage != null)
        {
            CreateCornerImage();
        }
        
        UpdateButtonAppearance(playButton, "Play");
        UpdateButtonAppearance(creditsButton, "Credits");
        UpdateButtonAppearance(quitButton, "Quit");
        UpdateCopyrightText();
        UpdateCornerImage();
        
        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }
    }
    
    void CreateCopyrightText()
    {
        GameObject copyrightGO = new GameObject("CopyrightText");
        copyrightGO.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI copyrightLabel = copyrightGO.AddComponent<TextMeshProUGUI>();
        copyrightLabel.text = "© Icy Solar 2025";
        copyrightLabel.fontSize = copyrightFontSize;
        copyrightLabel.color = copyrightTextColor;
        copyrightLabel.alignment = TextAlignmentOptions.Center;
        
        if (tmpFont != null)
            copyrightLabel.font = TMP_FontAsset.CreateFontAsset(tmpFont);
        
        RectTransform copyrightRect = copyrightGO.GetComponent<RectTransform>();
        copyrightRect.anchorMin = new Vector2(0.5f, 0f);
        copyrightRect.anchorMax = new Vector2(0.5f, 0f);
        copyrightRect.pivot = new Vector2(0.5f, 0f);
        copyrightRect.sizeDelta = new Vector2(400, 50);
        
        // Position it below the quit button with some spacing
        if (quitButton != null)
        {
            RectTransform quitRect = quitButton.GetComponent<RectTransform>();
            if (quitRect != null)
            {
                float yPosition = quitRect.anchoredPosition.y - quitRect.sizeDelta.y/2 - 60;
                copyrightRect.anchoredPosition = new Vector2(0, yPosition);
            }
        }
        else
        {
            copyrightRect.anchoredPosition = new Vector2(0, 50);
        }
        
        copyrightText = copyrightGO;
    }
    
    void CreateCornerImage()
    {
        if (cornerImage == null) return;
        
        GameObject cornerGO = new GameObject("CornerImage");
        cornerGO.transform.SetParent(canvas.transform, false);
        
        RawImage rawImage = cornerGO.AddComponent<RawImage>();
        rawImage.texture = cornerImage;
        
        RectTransform cornerRect = cornerGO.GetComponent<RectTransform>();
        cornerRect.anchorMin = new Vector2(0f, 0f);
        cornerRect.anchorMax = new Vector2(0f, 0f);
        cornerRect.pivot = new Vector2(0f, 0f);
        cornerRect.sizeDelta = cornerImageSize;
        cornerRect.anchoredPosition = cornerImageOffset;
        
        cornerImageObject = cornerGO;
    }
    
    void UpdateCopyrightText()
    {
        if (copyrightText != null)
        {
            TextMeshProUGUI label = copyrightText.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.fontSize = copyrightFontSize;
                label.color = copyrightTextColor;
                
                if (tmpFont != null)
                {
                    if (label.font == null || label.font.name != tmpFont.name)
                    {
                        label.font = TMP_FontAsset.CreateFontAsset(tmpFont);
                    }
                }
            }
            
            // Update position relative to quit button
            if (quitButton != null)
            {
                RectTransform quitRect = quitButton.GetComponent<RectTransform>();
                RectTransform copyrightRect = copyrightText.GetComponent<RectTransform>();
                if (quitRect != null && copyrightRect != null)
                {
                    float yPosition = quitRect.anchoredPosition.y - quitRect.sizeDelta.y/2 - 60;
                    copyrightRect.anchoredPosition = new Vector2(0, yPosition);
                }
            }
        }
    }
    
    void UpdateCornerImage()
    {
        if (cornerImageObject != null)
        {
            RawImage rawImage = cornerImageObject.GetComponent<RawImage>();
            RectTransform cornerRect = cornerImageObject.GetComponent<RectTransform>();
            
            if (rawImage != null)
            {
                rawImage.texture = cornerImage;
            }
            
            if (cornerRect != null)
            {
                cornerRect.sizeDelta = cornerImageSize;
                cornerRect.anchoredPosition = cornerImageOffset;
            }
        }
        else if (cornerImage != null)
        {
            CreateCornerImage();
        }
    }
    
    void UpdateButtonAppearance(GameObject buttonGO, string text)
    {
        if (buttonGO == null) return;
        
        Image image = buttonGO.GetComponent<Image>();
        if (image != null)
        {
            image.color = buttonColor;
        }
        
        Transform textTransform = buttonGO.transform.Find("Text");
        if (textTransform != null)
        {
            TextMeshProUGUI label = textTransform.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
                label.fontSize = fontSize;
                label.color = textColor;
                
                if (tmpFont != null)
                {
                    // Only create font asset if we don't already have one
                    if (label.font == null || label.font.name != tmpFont.name)
                    {
                        label.font = TMP_FontAsset.CreateFontAsset(tmpFont);
                    }
                }
            }
        }
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        canvasGO.transform.SetParent(this.transform);
        
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
    }

    void CreateBackground()
    {
        if (backgroundTexture == null) return;

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvas.transform, false);

        Image bgImage = bgGO.AddComponent<Image>();
        RectTransform rect = bgGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Sprite bgSprite = Sprite.Create(
            backgroundTexture,
            new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        bgImage.sprite = bgSprite;
        bgImage.preserveAspect = false;
    }

    GameObject CreateButton(string text, string sceneName, Vector2 position, Vector2 size)
    {
        GameObject buttonGO = new GameObject(text + "Button");
        buttonGO.transform.SetParent(canvas.transform, false);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        Image image = buttonGO.AddComponent<Image>();
        image.color = buttonColor;

        Button button = buttonGO.AddComponent<Button>();
        button.interactable = true;

        GameObject labelGO = new GameObject("Text");
        labelGO.transform.SetParent(buttonGO.transform, false);

        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAlignmentOptions.Center;

        if (tmpFont != null)
            label.font = TMP_FontAsset.CreateFontAsset(tmpFont);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        return buttonGO;
    }

    GameObject CreateQuitButton(string text, Vector2 position, Vector2 size)
    {
        GameObject buttonGO = new GameObject(text + "Button");
        buttonGO.transform.SetParent(canvas.transform, false);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        Image image = buttonGO.AddComponent<Image>();
        image.color = buttonColor;

        Button button = buttonGO.AddComponent<Button>();
        button.interactable = true;

        GameObject labelGO = new GameObject("Text");
        labelGO.transform.SetParent(buttonGO.transform, false);

        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAlignmentOptions.Center;

        if (tmpFont != null)
            label.font = TMP_FontAsset.CreateFontAsset(tmpFont);
                    if (tmpFont != null)
            label.font = TMP_FontAsset.CreateFontAsset(tmpFont);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        return buttonGO;
    }
    
    #if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        UnityEditor.EditorApplication.delayCall += () => {
            if (this == null) return;
            SetupUI();
        };
    }
    #endif
    
    // Fixed OnApplicationQuit method
    void OnApplicationQuit()
    {
        #if UNITY_EDITOR
        // Only mark scene dirty in editor and when not playing
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
        #endif
    }
}