using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GemInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioClip sophieVoice;
    [SerializeField] private float gemHoldDistance = 2f;
    [SerializeField] private float gemScale = 0.3f;
    
    [Header("Interaction Prompt")]
    [SerializeField] private Texture2D promptTexture;
    [SerializeField] private Vector2 promptSize = new Vector2(100, 100);
    [SerializeField] private Vector2 promptOffset = new Vector2(0, 50);
    [SerializeField] private float promptDistance = 5f;
    
    [Header("Subtitles")]
    [SerializeField] private Font subtitleFont;
    [SerializeField] private int subtitleFontSize = 28;
    [SerializeField] private Color subtitleColor = Color.white;
    [SerializeField] private float subtitleDisplayTime = 3f;
    [SerializeField] private float timeBetweenLines = 0.5f;
    
    [Header("Screen Effects")]
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float shakeDuration = 1.5f;
    [SerializeField] private float flashDuration = 2.5f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private int flashCount = 4;
    
    private Camera mainCamera;
    private AudioSource audioSource;
    private GameObject gemHeld;
    private Canvas subtitleCanvas;
    private Text subtitleText;
    private Image flashImage;
    private bool isInteracting = false;
    private bool isLookingAtGem = false;
    private FPSHorrorPlayer2 playerController;
    
    private void Start()
    {
        mainCamera = Camera.main;
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Find player controller reference
        playerController = FindObjectOfType<FPSHorrorPlayer2>();
        
        // Create subtitle canvas and text
        CreateSubtitleUI();
        
        // Create flash effect image
        CreateFlashImage();
    }
    
    private void Update()
    {
        CheckIfLookingAtGem();
        
        if (!isInteracting && Input.GetMouseButtonDown(0) && isLookingAtGem)
        {
            StartCoroutine(HandleGemInteraction());
        }
    }
    
    private void CheckIfLookingAtGem()
    {
        if (isInteracting) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, promptDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                isLookingAtGem = true;
            }
            else
            {
                isLookingAtGem = false;
            }
        }
        else
        {
            isLookingAtGem = false;
        }
    }
    
    private void OnGUI()
    {
        if (isLookingAtGem && !isInteracting && promptTexture != null)
        {
            // Calculate the position to display the prompt (above the gem in screen space)
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            
            // Don't show if gem is behind camera
            if (screenPos.z < 0) return;
            
            // Apply offset
            screenPos.x += promptOffset.x;
            screenPos.y += promptOffset.y;
            
            // Draw the prompt texture
            Rect promptRect = new Rect(
                screenPos.x - (promptSize.x / 2),
                Screen.height - screenPos.y - (promptSize.y / 2), // GUI Y is inverted from screen Y
                promptSize.x,
                promptSize.y
            );
            
            GUI.DrawTexture(promptRect, promptTexture);
        }
    }
    
    private void CreateSubtitleUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("GemSubtitles");
        subtitleCanvas = canvasObj.AddComponent<Canvas>();
        subtitleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        subtitleCanvas.sortingOrder = 10; // Ensure it's on top
        
        // Add canvas scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create text object
        GameObject textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        subtitleText = textObj.AddComponent<Text>();
        
        // Use the font from inspector or fallback to Arial
        subtitleText.font = subtitleFont != null ? subtitleFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = subtitleColor;
        
        // Position the text at bottom center
        RectTransform rectTransform = subtitleText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = new Vector2(0, 150); // 150 pixels from bottom
        rectTransform.sizeDelta = new Vector2(1600, 200);
        
        // Add shadow
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);
        
        // Hide initially
        subtitleText.text = "";
        subtitleCanvas.enabled = false;
    }
    
    private void CreateFlashImage()
    {
        // Create a full-screen image for the flash effect
        GameObject flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(subtitleCanvas.transform, false);
        
        flashImage = flashObj.AddComponent<Image>();
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        
        RectTransform flashRect = flashImage.rectTransform;
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        
        // Make sure it covers the entire screen
        flashRect.anchoredPosition = Vector2.zero;
        flashRect.localScale = Vector3.one;
    }
    
    private IEnumerator HandleGemInteraction()
    {
        isInteracting = true;
        
        // Disable player movement and show cursor
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Create a copy of the gem in front of the camera
        gemHeld = Instantiate(gameObject);
        
        // Remove any scripts from the copy
        Destroy(gemHeld.GetComponent<GemInteraction>());
        
        // Position and scale the gem
        gemHeld.transform.localScale = Vector3.one * gemScale;
        gemHeld.transform.position = mainCamera.transform.position + mainCamera.transform.forward * gemHoldDistance;
        gemHeld.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward * -1);
        
        // Hide the original gem
        gameObject.GetComponent<Renderer>().enabled = false;
        
        // Play audio
        if (sophieVoice != null)
        {
            audioSource.clip = sophieVoice;
            audioSource.Play();
        }
        
        // Show subtitles
        subtitleCanvas.enabled = true;
        
        // First line
        subtitleText.text = "Sophie The Cat: Flint this is your memory gem";
        yield return new WaitForSeconds(subtitleDisplayTime);
        
        // Second line
        subtitleText.text = "Sophie The Cat: C'mon Flint look inside of it";
        yield return new WaitForSeconds(subtitleDisplayTime);
        
        // Hide subtitles
        subtitleText.text = "";
        
        // Make sure canvas stays enabled for flash effect
        subtitleCanvas.enabled = true;
        
        // Screen shake and flash effects run simultaneously
        StartCoroutine(ShakeCamera());
        yield return StartCoroutine(FlashScreen());
        
        // Load next scene
        SceneManager.LoadScene("LevelThree");
    }
    
    private IEnumerator ShakeCamera()
    {
        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            // Gentler shake with smoother movement
            float x = Mathf.Sin(elapsed * 15f) * shakeIntensity;
            float y = Mathf.Cos(elapsed * 17f) * shakeIntensity;
            
            mainCamera.transform.localPosition = new Vector3(x, y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.localPosition = originalPosition;
    }
    
    private IEnumerator FlashScreen()
    {
        // Make sure the flash image is visible
        flashImage.gameObject.SetActive(true);
        
        // Flash effect with distinct pulses
        float timePerFlash = flashDuration / flashCount;
        
        for (int i = 0; i < flashCount; i++)
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < timePerFlash / 2)
            {
                float t = elapsed / (timePerFlash / 2);
                flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Fade out
            elapsed = 0f;
            while (elapsed < timePerFlash / 2)
            {
                float t = 1 - (elapsed / (timePerFlash / 2));
                flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        // Final bright flash before scene change
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1.0f);
        yield return new WaitForSeconds(0.3f);
    }
}