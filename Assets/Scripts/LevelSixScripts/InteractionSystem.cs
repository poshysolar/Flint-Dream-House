using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractionSystem : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private GameObject player; // Drag player object here
    private MonoBehaviour playerMovementScript; // Reference to player's movement script
    [SerializeField] private Camera playerCamera; // Drag player's camera here

    [Header("Camera Settings")]
    [SerializeField] private Camera camera2; // Drag Camera2 here
    [SerializeField] private Camera camera3; // Drag Camera3 here

    [Header("Cube Settings")]
    [SerializeField] private GameObject cube; // Drag cube object here
    [SerializeField] private AudioClip cubeSoundEffect; // Sound effect for cube
    [SerializeField] private float cubeLowerSpeed = 1f; // Speed of cube lowering
    private Material redGlowMaterial;
    private Material whiteGlowMaterial;
    private AudioSource cubeAudioSource;

    [Header("Cat Settings")]
    [SerializeField] private GameObject catPrefab; // Drag cat prefab here
    private GameObject catInstance; // Runtime instance of cat
    [SerializeField] private Animator catAnimator; // Drag AnimatorController here
    private readonly string talkingAnimation = "Talking"; // Animation name

    [Header("Idle Model Settings")]
    [SerializeField] private GameObject idlePrefab; // Drag the Idle prefab here

    [Header("Audio Settings")]
    [SerializeField] private AudioClip audioClip1; // Sophie: Before the curse
    [SerializeField] private AudioClip audioClip2; // Sophie: He has lost his mind
    [SerializeField] private AudioClip audioClip3; // Android: ALERT ALERT
    [SerializeField] private AudioClip audioClip4; // Android: PLEASE COME
    [SerializeField] private AudioClip audioClip5; // Sophie: I got to hurry
    private AudioSource audioSource;

    [Header("Subtitle UI Settings")]
    [SerializeField] private TMP_FontAsset subtitleFont; // Assign TextMeshPro font
    [SerializeField] private int fontSize = 36; // Editable font size
    [SerializeField] private Color sophieTextColor = Color.white; // Editable Sophie text color
    [SerializeField] private Color androidTextColor = Color.white; // Android text color (set to white)
    [SerializeField] private Color outlineColor = Color.black; // Editable outline color
    [SerializeField] private float outlineThickness = 0.015f; // Increased outline thickness
    private TextMeshProUGUI subtitleText;
    private GameObject subtitleObject;

    [Header("Prompt UI Settings")]
    [SerializeField] private Texture2D promptTexture; // Drag Texture2D for prompt UI
    [SerializeField] private float maxPromptDistance = 13f; // Max distance to show prompt
    private GameObject promptObject;
    private Image promptImage;
    private bool isLookingAtQuad = false;

    [Header("Fade Settings")]
    private Image fadeImage;
    private GameObject fadeObject;
    private GameObject canvasObject; // Reference to canvas for destruction
    [SerializeField] private float fadeDuration = 1f; // Duration of fade to black

    private bool isCutscenePlaying = false;
    public bool hasCutscenePlayed = false; // Flag to ensure cutscene plays only once (changed to public)

    private void Start()
    {
        // Show mouse cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Get AudioSource component for dialogue
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Get AudioSource component for cube
        if (cube != null)
        {
            cubeAudioSource = cube.AddComponent<AudioSource>();
            cubeAudioSource.playOnAwake = false;
            cubeAudioSource.clip = cubeSoundEffect;
        }

        // Get player movement script dynamically
        if (player != null)
        {
            playerMovementScript = player.GetComponent<MonoBehaviour>();
        }

        // Set cube starting position and create materials
        if (cube != null)
        {
            cube.transform.position = new Vector3(9.9f, 53.33f, -0.82f);
            CreateGlowMaterials();
            cube.GetComponent<Renderer>().material = redGlowMaterial;
        }

        // Create Canvas and UI elements
        CreateSubtitleUI();
        CreatePromptUI();
        CreateFadeUI();

        // Ensure only player camera's AudioListener is active initially
        SetCameraActive(playerCamera, true);
        SetCameraActive(camera2, false);
        SetCameraActive(camera3, false);
    }

    private void CreateGlowMaterials()
    {
        // Create red glowing material
        redGlowMaterial = new Material(Shader.Find("Standard"));
        redGlowMaterial.color = Color.red;
        redGlowMaterial.SetColor("_EmissionColor", Color.red * 2f); // Bright glow
        redGlowMaterial.EnableKeyword("_EMISSION");

        // Create white glowing material
        whiteGlowMaterial = new Material(Shader.Find("Standard"));
        whiteGlowMaterial.color = Color.white;
        whiteGlowMaterial.SetColor("_EmissionColor", Color.white * 2f); // Bright glow
        whiteGlowMaterial.EnableKeyword("_EMISSION");
    }

    private void CreateSubtitleUI()
    {
        // Create Canvas
        canvasObject = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create TextMeshProUGUI for subtitles
        subtitleObject = new GameObject("SubtitleText");
        subtitleObject.transform.SetParent(canvasObject.transform, false);
        subtitleText = subtitleObject.AddComponent<TextMeshProUGUI>();

        // Configure TextMeshPro settings
        subtitleText.font = subtitleFont;
        subtitleText.fontSize = fontSize;
        subtitleText.color = sophieTextColor;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.enableWordWrapping = true;

        // Add outline (stroke) effect
        subtitleText.fontMaterial.EnableKeyword("OUTLINE_ON");
        subtitleText.outlineColor = outlineColor;
        subtitleText.outlineWidth = outlineThickness;

        // Position at bottom of canvas (1920x1080)
        RectTransform subtitleRect = subtitleText.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0f);
        subtitleRect.pivot = new Vector2(0.5f, 0f);
        subtitleRect.anchoredPosition = new Vector2(0, 50); // 50 pixels from bottom
        subtitleRect.sizeDelta = new Vector2(1600, 200); // Wide enough for text

        // Hide subtitles initially
        subtitleText.gameObject.SetActive(false);
    }

    private void CreatePromptUI()
    {
        // Use the same Canvas as subtitles
        if (canvasObject == null)
        {
            canvasObject = new GameObject("SubtitleCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Create Image for prompt
        promptObject = new GameObject("PromptImage");
        promptObject.transform.SetParent(canvasObject.transform, false);
        promptImage = promptObject.AddComponent<Image>();

        // Assign texture and set size
        if (promptTexture != null)
        {
            promptImage.sprite = Sprite.Create(promptTexture, new Rect(0, 0, promptTexture.width, promptTexture.height), new Vector2(0.5f, 0.5f));
        }
        RectTransform promptRect = promptImage.GetComponent<RectTransform>();
        promptRect.sizeDelta = new Vector2(100, 100); // 100x100 size
        promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.anchoredPosition = new Vector2(0, 0); // Center of screen

        // Hide prompt initially
        promptObject.SetActive(false);
    }

    private void CreateFadeUI()
    {
        // Use the same Canvas
        if (canvasObject == null)
        {
            canvasObject = new GameObject("SubtitleCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Create Image for fade
        fadeObject = new GameObject("FadeImage");
        fadeObject.transform.SetParent(canvasObject.transform, false);
        fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Transparent initially
        RectTransform fadeRect = fadeImage.GetComponent<RectTransform>();
        fadeRect.anchorMin = new Vector2(0, 0);
        fadeRect.anchorMax = new Vector2(1, 1);
        fadeRect.sizeDelta = Vector2.zero; // Full screen
    }

    private void Update()
    {
        if (isCutscenePlaying || hasCutscenePlayed)
        {
            promptObject.SetActive(false); // Hide prompt during or after cutscene
            return;
        }

        // Raycast from camera center to check if looking at quad
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        bool wasLookingAtQuad = isLookingAtQuad;
        isLookingAtQuad = Physics.Raycast(ray, out hit, maxPromptDistance) && hit.transform == transform;

        // Show/hide prompt based on raycast and distance
        if (isLookingAtQuad && !wasLookingAtQuad)
        {
            promptObject.SetActive(true);
        }
        else if (!isLookingAtQuad && wasLookingAtQuad)
        {
            promptObject.SetActive(false);
        }

        // Check for left mouse click
        if (isLookingAtQuad && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        isCutscenePlaying = true;
        hasCutscenePlayed = true; // Mark cutscene as played
        promptObject.SetActive(false); // Hide prompt during cutscene

        // Pause player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // Show and play first subtitle and audio (Sophie)
        subtitleText.gameObject.SetActive(true);
        subtitleText.color = sophieTextColor;
        subtitleText.text = "Sophie The Cat: Before the curse arrived Mikey and I were good friends";
        audioSource.clip = audioClip1;
        audioSource.Play();
        yield return new WaitForSeconds(audioClip1.length);

        // Show second subtitle and audio (Sophie)
        subtitleText.text = "Sophie The Cat: He has lost his mind, and he is willing to hurt both of us, if we don't try to stop him";
        audioSource.clip = audioClip2;
        audioSource.Play();
        yield return new WaitForSeconds(audioClip2.length);

        // Switch to Camera2
        SetCameraActive(playerCamera, false);
        SetCameraActive(camera2, true);

        // Spawn cat model
        if (catPrefab != null)
        {
            catInstance = Instantiate(catPrefab, new Vector3(-5.67f, 0.02f, 31.78f), Quaternion.Euler(0, 134.114f, 0));
        }

        // Lower cube, play sound effect, and start flashing
        if (cube != null && cubeAudioSource != null)
        {
            Vector3 startPos = new Vector3(9.9f, 53.33f, -0.82f);
            Vector3 endPos = new Vector3(9.9f, 49.72f, -0.82f);
            float t = 0;
            cubeAudioSource.Play();
            StartCoroutine(FlashCube());
            while (t < 1)
            {
                t += Time.deltaTime * cubeLowerSpeed;
                cube.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        // Play Android audio and subtitles (white text)
        subtitleText.color = androidTextColor;
        subtitleText.text = "Android: ALERT ALERT THERE HAS BEEN A SECURITY BREAK";
        audioSource.clip = audioClip3;
        audioSource.Play();
        yield return new WaitForSeconds(audioClip3.length);

        subtitleText.text = "Android: PLEASE COME TO THE MAIN LOBBY";
        audioSource.clip = audioClip4;
        audioSource.Play();
        yield return new WaitForSeconds(audioClip4.length);

        // Wait for cube sound effect to finish before switching to Camera3
        if (cubeAudioSource != null)
        {
            while (cubeAudioSource.isPlaying)
            {
                yield return null;
            }
        }

        // Switch to Camera3
        SetCameraActive(camera2, false);
        SetCameraActive(camera3, true);

        // Play cat talking animation
        if (catInstance != null && catAnimator != null)
        {
            catAnimator = catInstance.GetComponent<Animator>();
            if (catAnimator != null)
            {
                catAnimator.Play(talkingAnimation);
            }
        }

        // Play final Sophie audio and subtitle
        subtitleText.color = sophieTextColor;
        subtitleText.text = "Sophie The Cat: I got to hurry and stop this, Flint please get to the main lobby before something bad happens";
        audioSource.clip = audioClip5;
        audioSource.Play();
        yield return new WaitForSeconds(audioClip5.length);

        // Fade to black and destroy cat model and canvas
        yield return StartCoroutine(FadeToBlack());
        if (catInstance != null)
        {
            Destroy(catInstance);
        }
        if (canvasObject != null)
        {
            Destroy(canvasObject); // Destroy canvas to remove fade and subtitle UI
        }

        // Switch back to player camera
        SetCameraActive(camera3, false);
        SetCameraActive(playerCamera, true);

        // Hide subtitles (already destroyed with canvas)
        subtitleText.gameObject.SetActive(false);

        // Resume player movement and hide cursor
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        isCutscenePlaying = false;
    }

    private void SetCameraActive(Camera cam, bool active)
    {
        if (cam != null)
        {
            cam.gameObject.SetActive(active);
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = active; // Enable/disable AudioListener with camera
            }
        }
    }

    private IEnumerator FlashCube()
    {
        if (cube == null) yield break;
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        float flashDuration = 0.5f; // Duration of each flash
        while (isCutscenePlaying)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashDuration;
                cubeRenderer.material.Lerp(redGlowMaterial, whiteGlowMaterial, t);
                yield return null;
            }
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashDuration;
                cubeRenderer.material.Lerp(whiteGlowMaterial, redGlowMaterial, t);
                yield return null;
            }
        }
    }

    private IEnumerator FadeToBlack()
    {
        float t = 0;
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 1);
        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        // Destroy the Idle prefab after the fade is complete
        if (idlePrefab != null)
        {
            Destroy(idlePrefab);
        }
    }
}