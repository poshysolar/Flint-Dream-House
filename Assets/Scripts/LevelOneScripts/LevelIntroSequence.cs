using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelIntroSequence : MonoBehaviour
{
    [Header("Player Settings")]
    public Vector3 startingPosition = new Vector3(-3.37f, 3.17f, 91.01f);
    [Tooltip("Use either teleportPosition OR teleportDestination (not both)")]
    public Vector3 teleportPosition = new Vector3(1.85f, 0.53f, 78.02f);
    public Transform teleportDestination;
    public GameObject player;
    public FPSHorrorPlayer01 fpsHorrorPlayer01;
    public Transform playerCamera;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera camera2;
    public float camera2Duration = 8f;

    [Header("Eye Effect")]
    public float eyesClosedDuration = 2f;
    public float eyesOpenDuration = 6f;
    public float eyeTransitionDuration = 1.5f;
    private Image eyeOverlay;
    private GameObject eyeCanvas;

    [Header("Camera 2 Movement")]
    public float camera2SwayAmount = 30f;
    public float camera2SwaySpeed = 0.8f;
    private Vector3 camera2OriginalRotation;

    [Header("Where Am I? UI Settings")]
    public string uiText = "Where am I?";
    public float uiFontSize = 48f;
    public Color uiTextColor = Color.white;
    public TMP_FontAsset uiFontAsset; // Assign custom font if needed
    public AudioClip whereAmISound; // <- ONLY ADDED THIS LINE

    [Header("Cursor Tracking")]
    [SerializeField] private bool debugCursor = true; // Enable to see cursor state in inspector
    [SerializeField] private bool currentCursorState;
    private bool originalCursorState;

    [HideInInspector]
    public TextMeshProUGUI whereAmIText;
    private GameObject textCanvas;

    private void Start()
    {
        // Store original cursor state
        originalCursorState = Cursor.visible;
        
        // Force show cursor at the very start
        SetCursorState(true);
        
        // Force set player starting position
        player.transform.position = startingPosition;

        // Initialize cameras
        mainCamera.enabled = true;
        camera2.enabled = false;
        camera2OriginalRotation = camera2.transform.eulerAngles;

        // Disable player control
        fpsHorrorPlayer01.enabled = false;

        // Create UI systems
        CreateEyeEffectSystem();
        CreateWhereAmIText();

        // Play sound (MODIFIED THESE LINES)
        if (whereAmISound != null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = whereAmISound;
            audioSource.spatialBlend = 0f; // 0 = 2D (full stereo), 1 = 3D
            audioSource.volume = 1f; // Full volume
            audioSource.Play();
        }

        // Start the sequence
        StartCoroutine(IntroSequence());
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        currentCursorState = visible;
        
        if (debugCursor)
        {
            Debug.Log($"Cursor set to: {(visible ? "VISIBLE" : "HIDDEN")} - Lock State: {Cursor.lockState}");
        }
    }

    private void Update()
    {
        // Update inspector value for debugging
        if (debugCursor)
        {
            currentCursorState = Cursor.visible;
        }
    }

    private void CreateEyeEffectSystem()
    {
        eyeCanvas = new GameObject("EyeEffectCanvas");
        Canvas canvas = eyeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imageObj = new GameObject("EyeOverlay");
        imageObj.transform.SetParent(eyeCanvas.transform);
        eyeOverlay = imageObj.AddComponent<Image>();

        Texture2D eyeTexture = CreateEyeTexture();
        eyeOverlay.sprite = Sprite.Create(eyeTexture, new Rect(0, 0, eyeTexture.width, eyeTexture.height), Vector2.zero);

        eyeOverlay.rectTransform.anchorMin = Vector2.zero;
        eyeOverlay.rectTransform.anchorMax = Vector2.one;
        eyeOverlay.rectTransform.offsetMin = Vector2.zero;
        eyeOverlay.rectTransform.offsetMax = Vector2.zero;

        eyeOverlay.color = Color.black;
    }

    private void CreateWhereAmIText()
    {
        textCanvas = new GameObject("WhereAmICanvas");
        Canvas canvas = textCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = textCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        textCanvas.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("WhereAmIText");
        textGO.transform.SetParent(textCanvas.transform);

        whereAmIText = textGO.AddComponent<TextMeshProUGUI>();
        whereAmIText.text = uiText;
        whereAmIText.alignment = TextAlignmentOptions.Center;
        whereAmIText.fontSize = uiFontSize;
        whereAmIText.color = uiTextColor;

        if (uiFontAsset != null)
        {
            whereAmIText.font = uiFontAsset;
        }

        RectTransform rect = whereAmIText.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 200);
        rect.anchorMin = new Vector2(0.5f, 0.1f); // Bottom center
        rect.anchorMax = new Vector2(0.5f, 0.1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    private Texture2D CreateEyeTexture()
    {
        int width = 512;
        int height = 512;
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inLeftEye = Mathf.Pow((x - width * 0.3f) / (width * 0.1f), 2) + Mathf.Pow((y - height * 0.5f) / (height * 0.15f), 2) < 1;
                bool inRightEye = Mathf.Pow((x - width * 0.7f) / (width * 0.1f), 2) + Mathf.Pow((y - height * 0.5f) / (height * 0.15f), 2) < 1;

                texture.SetPixel(x, y, (inLeftEye || inRightEye) ? Color.clear : Color.black);
            }
        }

        texture.Apply();
        return texture;
    }

    private IEnumerator IntroSequence()
    {
        // Ensure cursor is visible at start of sequence
        SetCursorState(true);
        yield return new WaitForEndOfFrame(); // Give it a frame to apply

        // Eyes closed
        eyeOverlay.color = Color.black;
        yield return new WaitForSeconds(eyesClosedDuration);

        // Fade open
        yield return StartCoroutine(FadeEyeEffect(Color.black, Color.clear, eyeTransitionDuration));

        // Eyes open hold - cursor should still be visible
        yield return new WaitForSeconds(eyesOpenDuration);

        // Switch to camera 2 - keep cursor visible
        mainCamera.enabled = false;
        camera2.enabled = true;
        SetCursorState(true); // Ensure it's still visible

        float swayTimer = 0f;
        while (swayTimer < camera2Duration)
        {
            swayTimer += Time.deltaTime;
            float sway = Mathf.Sin(swayTimer * camera2SwaySpeed) * camera2SwayAmount;
            camera2.transform.rotation = Quaternion.Euler(
                camera2OriginalRotation.x,
                camera2OriginalRotation.y + sway,
                camera2OriginalRotation.z
            );
            yield return null;
        }

        // Back to main camera - NOW hide cursor
        camera2.enabled = false;
        mainCamera.enabled = true;
        SetCursorState(false); // Hide cursor when returning to player

        // TELEPORT PLAYER
        fpsHorrorPlayer01.enabled = false;
        if (teleportDestination != null)
        {
            player.transform.position = teleportDestination.position;
        }
        else
        {
            player.transform.position = teleportPosition;
        }

        yield return new WaitForSeconds(0.5f);

        // Re-enable controls - cursor should remain hidden for FPS gameplay
        fpsHorrorPlayer01.enabled = true;

        // Clean up
        Destroy(eyeCanvas);
        Destroy(textCanvas);
    }

    private IEnumerator FadeEyeEffect(Color startColor, Color endColor, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            eyeOverlay.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }
        eyeOverlay.color = endColor;
    }

    private void OnDestroy()
    {
        // Restore original cursor state when this object is destroyed
        if (debugCursor)
        {
            Debug.Log("Restoring original cursor state");
        }
        Cursor.visible = originalCursorState;
    }
}