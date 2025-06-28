using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LaptopInteraction2 : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip mikeyClip1; // First Mikey dialogue
    [SerializeField] private AudioClip sophieClip1; // First Sophie dialogue
    [SerializeField] private AudioClip mikeyClip2; // Second Mikey dialogue
    [SerializeField] private AudioClip sophieClip2; // Second Sophie dialogue

    [Header("UI Settings")]
    [SerializeField] private TMP_FontAsset textFont; // Customizable font (default LiberationSans if null)
    [SerializeField] private float textSize = 36f;  // Customizable text size
    [SerializeField] private Color strokeColor = Color.black; // Color for text stroke
    [SerializeField] private float strokeWidth = 0.1f; // Width of the text stroke
    [SerializeField] private float promptYOffset = 0f; // Y offset for prompt position (editable in Inspector)

    [Header("Interaction Settings")]
    [SerializeField] private float maxInteractionDistance = 3f; // Max distance for prompt and interaction

    [Header("Door Settings")]
    [SerializeField] private Transform slidingDoor; // Reference to the sliding door transform
    [SerializeField] private float doorStartX = 44.26f; // Editable initial X position
    [SerializeField] private float doorTargetX = 69.8f; // Editable target X position
    [SerializeField] private float doorY = 27.6881f; // Fixed Y position
    [SerializeField] private float doorZ = 430.9378f; // Fixed Z position
    [SerializeField] private float doorMoveSpeed = 5f; // Speed of door movement

    private Vector3 originalDoorPosition;
    private Vector3 targetDoorPosition;
    private TextMeshProUGUI subtitleText;
    private TextMeshProUGUI promptText;
    private bool isPlaying = false;
    private float audioTimer = 0f;
    private bool moveDoor = false;
    private int currentClipIndex = 0;
    private float[] subtitleTimings;
    private string[] subtitles = {
        "Mikey: I never felt this good before, it's like I can tear anyone and anything in my way!",
        "Sophie The Cat: Please stop Mikey, it's the curse it makes you think things that aren't real",
        "Mikey: Stop, you're also affected by the curse, you just won't admit to it",
        "Sophie The Cat: This isn't about me. I'll find your secret room, you better watch out!",
        ""
    };
    private Color[] subtitleColors = {
        new Color(1f, 0.5f, 0f), // Orange for Mikey
        new Color(0.5f, 0f, 0.5f), // Purple for Sophie
        new Color(1f, 0.5f, 0f), // Orange for Mikey
        new Color(0.5f, 0f, 0.5f), // Purple for Sophie
        Color.white // Default for empty subtitle
    };
    private AudioClip[] audioClips;
    private Camera mainCamera;

    void Start()
    {
        // Initialize main camera
        mainCamera = Camera.main;

        // Initialize door positions
        originalDoorPosition = new Vector3(doorStartX, doorY, doorZ);
        targetDoorPosition = new Vector3(doorTargetX, doorY, doorZ);

        // Initialize audio clips array
        audioClips = new AudioClip[] { mikeyClip1, sophieClip1, mikeyClip2, sophieClip2 };

        // Calculate subtitle timings based on clip lengths
        subtitleTimings = new float[audioClips.Length + 1];
        float cumulativeTime = 0f;
        subtitleTimings[0] = 0f;
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i] != null)
            {
                cumulativeTime += audioClips[i].length;
                subtitleTimings[i + 1] = cumulativeTime;
            }
            else
            {
                Debug.LogWarning($"Audio clip at index {i} is not assigned!");
                subtitleTimings[i + 1] = cumulativeTime + 5f; // Fallback duration
            }
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Subtitle TextMeshProUGUI
        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(canvasObj.transform, false);
        subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();

        // Configure subtitle text
        subtitleText.font = textFont != null ? textFont : Resources.GetBuiltinResource<TMP_FontAsset>("LiberationSans SDF");
        subtitleText.fontSize = textSize;
        subtitleText.fontMaterial.EnableKeyword("OUTLINE_ON");
        subtitleText.fontMaterial.SetFloat("_Outline", strokeWidth);
        subtitleText.fontMaterial.SetColor("_OutlineColor", strokeColor);
        RectTransform subtitleRect = subtitleText.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0f);
        subtitleRect.pivot = new Vector2(0.5f, 0f);
        subtitleRect.anchoredPosition = new Vector2(0f, 100f); // 100 pixels from bottom
        subtitleRect.sizeDelta = new Vector2(1600f, 200f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.text = "";

        // Create Prompt TextMeshProUGUI
        GameObject promptObj = new GameObject("PromptText");
        promptObj.transform.SetParent(canvasObj.transform, false);
        promptText = promptObj.AddComponent<TextMeshProUGUI>();
        promptText.font = textFont != null ? textFont : Resources.GetBuiltinResource<TMP_FontAsset>("LiberationSans SDF");
        promptText.fontSize = textSize;
        promptText.color = new Color(1f, 0.5f, 0f); // Orange for prompt
        promptText.fontMaterial.EnableKeyword("OUTLINE_ON");
        promptText.fontMaterial.SetFloat("_Outline", strokeWidth);
        promptText.fontMaterial.SetColor("_OutlineColor", strokeColor);
        RectTransform promptRect = promptText.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.anchoredPosition = new Vector2(0f, promptYOffset); // Editable Y offset
        promptRect.sizeDelta = new Vector2(400f, 100f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.text = "Left Mouse Click";
        promptText.enabled = false;

        // Set initial door position
        if (slidingDoor != null)
        {
            slidingDoor.position = originalDoorPosition;
        }
    }

    void Update()
    {
        // Raycast to check if player is looking at laptop and within distance
        bool isLookingAtLaptop = false;
        if (mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance))
            {
                if (hit.collider.gameObject == gameObject && Vector3.Distance(mainCamera.transform.position, transform.position) <= maxInteractionDistance)
                {
                    isLookingAtLaptop = true;
                }
            }
        }
        promptText.enabled = isLookingAtLaptop && !isPlaying;

        // Handle audio and subtitle timing
        if (isPlaying)
        {
            audioTimer += Time.deltaTime;

            // Update subtitles based on timing
            for (int i = 0; i < subtitleTimings.Length - 1; i++)
            {
                if (audioTimer >= subtitleTimings[i] && audioTimer < subtitleTimings[i + 1])
                {
                    if (currentClipIndex != i && i < audioClips.Length)
                    {
                        if (audioClips[i] != null)
                        {
                            // Play audio as 2D sound (no spatial panning)
                            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                            audioSource.spatialBlend = 0f; // 2D audio
                            audioSource.clip = audioClips[i];
                            audioSource.Play();
                            Destroy(audioSource, audioClips[i].length); // Clean up after playing
                        }
                        currentClipIndex = i;
                    }
                    subtitleText.text = subtitles[i];
                    subtitleText.color = subtitleColors[i];
                    break;
                }
            }

            // Check if all clips have finished
            if (audioTimer >= subtitleTimings[subtitleTimings.Length - 1])
            {
                isPlaying = false;
                subtitleText.text = "";
                subtitleText.color = Color.white;
                moveDoor = true;
                currentClipIndex = 0;
            }
        }

        // Move door to target position after audio finishes
        if (moveDoor && slidingDoor != null)
        {
            slidingDoor.position = Vector3.Lerp(slidingDoor.position, targetDoorPosition, doorMoveSpeed * Time.deltaTime);
            if (Vector3.Distance(slidingDoor.position, targetDoorPosition) < 0.1f)
            {
                slidingDoor.position = targetDoorPosition;
                moveDoor = false;
            }
        }
    }

    void OnMouseDown()
    {
        // Play audio clips and start subtitle sequence if looking at laptop, within distance, and not already playing
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance) && hit.collider.gameObject == gameObject)
        {
            if (!isPlaying && audioClips[0] != null && Vector3.Distance(mainCamera.transform.position, transform.position) <= maxInteractionDistance)
            {
                // Play first clip as 2D sound
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f; // 2D audio
                audioSource.clip = audioClips[0];
                audioSource.Play();
                Destroy(audioSource, audioClips[0].length); // Clean up after playing
                isPlaying = true;
                audioTimer = 0f;
                currentClipIndex = 0;
            }
        }
    }
}