using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSixTrigger : MonoBehaviour
{
    [Header("Cat Models")]
    public GameObject catWalkingModel;
    public GameObject catTalkingModel;
    public GameObject catIdleModel;
    
    [Header("Animation")]
    public Animator catWalkingAnimator;
    public Animator catTalkingAnimator;
    public Animator catIdleAnimator;
    
    [Header("Movement Settings")]
    [Range(0.5f, 200.0f)]
    public float walkSpeed = 11.0f; // Default walk speed set to 11
    [Range(0.5f, 5.0f)]
    public float rotationSpeed = 3.0f;
    
    [Header("Audio")]
    public AudioClip sophieDialogue1;
    public AudioClip sophieDialogue2;
    public AudioClip sophieDialogue3;
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float dialogueVolume = 1.0f;
    [Range(0f, 1f)]
    public float backgroundMusicVolume = 0.5f;
    
    [Header("Subtitle Settings")]
    [Range(24, 48)]
    public int subtitleFontSize = 32;
    public Color subtitleTextColor = Color.white;
    public Color subtitleOutlineColor = Color.black;
    [Range(0.1f, 0.5f)]
    public float subtitleOutlineThickness = 0.2f;
    public TMP_FontAsset subtitleFont;
    
    [Header("Fade Settings")]
    [Range(0.5f, 3.0f)]
    public float fadeInOutDuration = 1.5f;
    public Color fadeColor = Color.black;
    
    [Header("Player Reference")]
    public GameObject player;
    public MonoBehaviour playerMovementScript;
    
    // Walking points
    private float[] walkPoints = new float[] { -6.08f, 3f, 11.02f, 19.04f };
    private int currentPointIndex = 0;
    
    // UI elements
    private Canvas subtitleCanvas;
    private TextMeshProUGUI subtitleText;
    private Image backgroundPanel;
    private AudioSource dialogueAudioSource;
    private AudioSource backgroundMusicSource;
    
    // Fade control
    private Canvas fadeCanvas;
    private Image fadePanel;
    private bool isFading = false;
    
    private void Awake()
    {
        // Create dialogue audio source
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        dialogueAudioSource.playOnAwake = false;
        dialogueAudioSource.spatialBlend = 0f; // 2D sound
        dialogueAudioSource.volume = dialogueVolume;
        
        // Create background music audio source
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.playOnAwake = false;
        backgroundMusicSource.spatialBlend = 0f; // 2D sound
        backgroundMusicSource.volume = backgroundMusicVolume;
        backgroundMusicSource.loop = true; // Loop the background music
        
        // Explicitly show cursor at initialization
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void Start()
    {
        // Explicitly show cursor at start to ensure visibility
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Initialize cat position and rotation
        if (catWalkingModel != null)
        {
            Vector3 startPos = catWalkingModel.transform.position;
            startPos.x = walkPoints[0];
            catWalkingModel.transform.position = startPos;
            
            Vector3 startRot = catWalkingModel.transform.eulerAngles;
            startRot.y = -280f;
            catWalkingModel.transform.eulerAngles = startRot;
            
            // Make sure walking model is active, others are hidden
            catWalkingModel.SetActive(true);
        }
        
        // Hide talking and idle cat models initially
        if (catTalkingModel != null)
        {
            catTalkingModel.SetActive(false);
        }
        
        if (catIdleModel != null)
        {
            catIdleModel.SetActive(false);
        }
        
        // Create UI elements
        CreateUIElements();
        CreateFadeSystem();
        
        // Start the cutscene
        StartCoroutine(StartCutscene());
    }
    
    private void OnValidate()
    {
        // Update audio volumes when changed in inspector
        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.volume = dialogueVolume;
        }
        
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = backgroundMusicVolume;
        }
    }
    
    private void CreateUIElements()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("SubtitleCanvas");
        canvasObj.transform.SetParent(transform);
        subtitleCanvas = canvasObj.AddComponent<Canvas>();
        subtitleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        subtitleCanvas.sortingOrder = 10; // Make sure it's above other UI
        
        // Add canvas scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // Balance width and height
        
        // Add raycaster to block input
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background panel
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        backgroundPanel = panelObj.AddComponent<Image>();
        backgroundPanel.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.05f);
        panelRect.anchorMax = new Vector2(0.9f, 0.2f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create text
        GameObject textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        subtitleText = textObj.AddComponent<TextMeshProUGUI>();
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.color = subtitleTextColor;
        subtitleText.text = "Test Subtitle Text";
        
        // Set font if provided
        if (subtitleFont != null)
        {
            subtitleText.font = subtitleFont;
        }
        
        // Add outline
        subtitleText.enableVertexGradient = false;
        subtitleText.outlineWidth = subtitleOutlineThickness;
        subtitleText.outlineColor = subtitleOutlineColor;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.1f);
        textRect.anchorMax = new Vector2(0.95f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Hide UI initially
        subtitleCanvas.gameObject.SetActive(false);
    }
    
    private void CreateFadeSystem()
    {
        // Create separate canvas for fade
        GameObject fadeCanvasObj = new GameObject("FadeCanvas");
        fadeCanvasObj.transform.SetParent(transform);
        fadeCanvas = fadeCanvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 20; // Make sure it's above everything
        
        // Add canvas scaler
        CanvasScaler fadeScaler = fadeCanvasObj.AddComponent<CanvasScaler>();
        fadeScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        fadeScaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create fade panel
        GameObject fadePanelObj = new GameObject("FadePanel");
        fadePanelObj.transform.SetParent(fadeCanvasObj.transform, false);
        
        fadePanel = fadePanelObj.AddComponent<Image>();
        fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0); // Start transparent
        
        RectTransform fadePanelRect = fadePanelObj.GetComponent<RectTransform>();
        fadePanelRect.anchorMin = Vector2.zero;
        fadePanelRect.anchorMax = Vector2.one;
        fadePanelRect.offsetMin = Vector2.zero;
        fadePanelRect.offsetMax = Vector2.zero;
    }
    
    private IEnumerator StartCutscene()
    {
        // Ensure cursor is visible at the start of the cutscene
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Disable player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
        
        // Start background music
        if (backgroundMusic != null)
        {
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.volume = backgroundMusicVolume;
            backgroundMusicSource.Play();
        }
        
        // Start walking animation
        if (catWalkingAnimator != null)
        {
            catWalkingAnimator.SetBool("Walking", true);
        }
        
        // Walk through all points
        while (currentPointIndex < walkPoints.Length - 1)
        {
            yield return StartCoroutine(WalkToNextPoint());
            currentPointIndex++;
        }
        
        // Rotate to final rotation
        yield return StartCoroutine(RotateToAngle(-8f));
        
        // Switch to talking cat model
        SwitchToTalkingCat();
        
        // Show dialogue
        subtitleCanvas.gameObject.SetActive(true);
        
        // Play dialogues
        yield return StartCoroutine(ShowDialogue("Sophie The Cat: Flint I'm happy you found my room!", sophieDialogue1));
        yield return StartCoroutine(ShowDialogue("Sophie The Cat: The curse is going to make it hard for your memory to get back", sophieDialogue2));
        yield return StartCoroutine(ShowDialogue("Sophie The Cat: I been trying for months to see if this would go away but i think this curse is up to you to stop it, not me", sophieDialogue3));
        
        // Hide subtitles
        subtitleCanvas.gameObject.SetActive(false);
        
        // Start music fade-out and screen fade simultaneously
        if (backgroundMusicSource.isPlaying)
        {
            StartCoroutine(FadeOutBackgroundMusic(fadeInOutDuration));
        }
        
        // Fade out
        yield return StartCoroutine(FadeScreen(true, fadeInOutDuration));
        
        // Switch to idle cat model and teleport
        SwitchToIdleCat();
        
        // Fade in
        yield return StartCoroutine(FadeScreen(false, fadeInOutDuration));
        
        // Re-enable player movement and hide cursor
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private IEnumerator WalkToNextPoint()
    {
        Vector3 currentPos = catWalkingModel.transform.position;
        float targetX = walkPoints[currentPointIndex + 1];
        
        while (Mathf.Abs(currentPos.x - targetX) > 0.1f)
        {
            currentPos = catWalkingModel.transform.position;
            float newX = Mathf.MoveTowards(currentPos.x, targetX, walkSpeed * Time.deltaTime);
            catWalkingModel.transform.position = new Vector3(newX, currentPos.y, currentPos.z);
            yield return null;
        }
        
        // Ensure exact position
        currentPos = catWalkingModel.transform.position;
        catWalkingModel.transform.position = new Vector3(targetX, currentPos.y, currentPos.z);
    }
    
    private IEnumerator RotateToAngle(float targetYAngle)
    {
        Quaternion startRotation = catWalkingModel.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, targetYAngle, 0);
        
        float elapsedTime = 0;
        float rotationDuration = 1.0f;
        
        while (elapsedTime < rotationDuration)
        {
            catWalkingModel.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }
        
        // Ensure exact rotation
        catWalkingModel.transform.rotation = targetRotation;
    }
    
    private void SwitchToTalkingCat()
    {
        if (catWalkingAnimator != null)
        {
            catWalkingAnimator.SetBool("Walking", false);
        }
        
        if (catWalkingModel != null && catTalkingModel != null)
        {
            // Position the talking cat at the same position as the walking cat
            catTalkingModel.transform.position = catWalkingModel.transform.position;
            catTalkingModel.transform.rotation = catWalkingModel.transform.rotation;
            
            // Hide walking cat, show talking cat
            catWalkingModel.SetActive(false);
            catTalkingModel.SetActive(true);
            
            // Start talking animation on the talking cat model
            if (catTalkingAnimator != null)
            {
                catTalkingAnimator.SetBool("Talking", true);
            }
        }
    }
    
    private void SwitchToIdleCat()
    {
        if (catTalkingAnimator != null)
        {
            catTalkingAnimator.SetBool("Talking", false);
        }
        
        if (catTalkingModel != null && catIdleModel != null)
        {
            // Hide talking cat, show idle cat at the teleport position
            catTalkingModel.SetActive(false);
            catIdleModel.SetActive(true);
            
            // Set the idle cat to the teleport position
            catIdleModel.transform.position = new Vector3(-21.95f, 0.02f, 14.5f);
            catIdleModel.transform.rotation = Quaternion.Euler(0, 90, 0);
            
            // Start idle animation on the idle cat model
            if (catIdleAnimator != null)
            {
                catIdleAnimator.SetBool("Idle", true);
            }
        }
    }
    
    private IEnumerator ShowDialogue(string text, AudioClip clip)
    {
        // Set text
        subtitleText.text = text;
        
        // Play audio if available
        float audioDuration = 3.0f; // Default duration
        if (clip != null)
        {
            dialogueAudioSource.clip = clip;
            dialogueAudioSource.volume = dialogueVolume;
            dialogueAudioSource.Play();
            audioDuration = clip.length;
        }
        
        // Wait for audio to finish
        yield return new WaitForSeconds(audioDuration + 0.5f);
    }
    
    private IEnumerator FadeScreen(bool fadeOut, float duration)
    {
        if (isFading) yield break;
        isFading = true;
        
        float startAlpha = fadeOut ? 0 : 1;
        float endAlpha = fadeOut ? 1 : 0;
        float elapsedTime = 0;
        
        Color color = fadePanel.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            
            color.a = alpha;
            fadePanel.color = color;
            
            yield return null;
        }
        
        // Ensure final alpha
        color.a = endAlpha;
        fadePanel.color = color;
        
        isFading = false;
    }
    
    private IEnumerator FadeOutBackgroundMusic(float duration)
    {
        float startVolume = backgroundMusicSource.volume;
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / duration);
            yield return null;
        }
        
        // Ensure music stops and volume is reset
        backgroundMusicSource.Stop();
        backgroundMusicSource.volume = backgroundMusicVolume;
    }
}