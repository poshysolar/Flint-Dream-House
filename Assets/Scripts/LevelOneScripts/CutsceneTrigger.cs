using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera playerCamera;
    public Camera camera3;
    public float cameraMoveSpeed = 5f;

    [Header("Phone Model")]
    public Transform phoneModel;

    [Header("Audio Settings")]
    public AudioClip phoneRing;
    public List<AudioClip> dialogueAudio;
    [Range(0, 1)] public float npcAudioVolume = 0.7f;
    [Range(0, 1)] public float playerAudioVolume = 1f;

    [Header("Subtitle Settings")]
    public Font subtitleFont;
    public int subtitleFontSize = 24;
    public Color subtitleColor = Color.white;
    public Color subtitleOutlineColor = Color.black;
    public float subtitleOutlineWidth = 2f;
    public Vector2 subtitlePosition = new Vector2(0, -100);
    public float subtitleDuration = 3f;
    public float endTextDuration = 10f;

    [Header("End Text Settings")]
    public string endText = "Explore your creation";
    public Font endTextFont;
    public int endTextFontSize = 36;
    public Color endTextColor = Color.white;
    public Vector2 endTextPosition = new Vector2(200, 200);

    private GameObject player;
    private FPSHorrorPlayer fpsController;
    private GameObject subtitleCanvas;
    private bool isTriggered = false;
    private bool wasCursorVisible;
    private CursorLockMode previousCursorLockState;

    private List<string> subtitles = new List<string>()
    {
        "Hello... Hello is there anybody here?",
        "Yes? where am i exactly?",
        "Oh my gosh, its so good to hear your voice Flint!\nI thought you were going to be asleep forever!",
        "I was asleep?",
        "How could you not remember this place!?\nThis is the dream house you created after all",
        "I'm not sure anymore, is it safe here??",
        "umm.. not really\nSomething you created placed a curse over your dreams!",
        "Look I can't say much, just please explore your dream house world you created!"
    };

    private List<bool> useCamera3 = new List<bool>()
    {
        true,    // NPC line (camera3)
        false,   // Player line (main camera)
        true,    // NPC line
        false,   // Player line
        true,    // NPC line
        false,   // Player line
        true,    // NPC line
        true     // NPC line
    };

    void Start()
    {
        // Ensure only player camera is active at start
        playerCamera.gameObject.SetActive(true);
        camera3.gameObject.SetActive(false);

        // Create subtitle canvas if it doesn't exist
        CreateSubtitleCanvas();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            player = other.gameObject;
            fpsController = player.GetComponent<FPSHorrorPlayer>();

            // Save current cursor state
            wasCursorVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;

            // Show and unlock cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Freeze player movement
            if (fpsController != null)
            {
                fpsController.enabled = false;
            }

            StartCoroutine(CutsceneSequence());
        }
    }

    IEnumerator CutsceneSequence()
    {
        // Switch to camera3
        playerCamera.gameObject.SetActive(false);
        camera3.gameObject.SetActive(true);

        // Set initial camera position
        Vector3 startPos = new Vector3(14.94f, 7.66f, 58.21f);
        Vector3 endPos = new Vector3(14.94f, 7.66f, 19.06f);
        camera3.transform.position = startPos;

        // Move camera slowly
        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;
        float distanceCovered;
        float fractionOfJourney;

        while (Vector3.Distance(camera3.transform.position, endPos) > 0.1f)
        {
            distanceCovered = (Time.time - startTime) * cameraMoveSpeed;
            fractionOfJourney = distanceCovered / journeyLength;
            camera3.transform.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            yield return null;
        }

        // Ensure camera is exactly at end position
        camera3.transform.position = endPos;

        // Phone rings
        AudioSource phoneAudioSource = gameObject.AddComponent<AudioSource>();
        phoneAudioSource.spatialBlend = 0f; // 2D audio for consistency
        phoneAudioSource.clip = phoneRing;
        phoneAudioSource.volume = npcAudioVolume;
        phoneAudioSource.Play();
        yield return new WaitForSeconds(phoneRing.length);

        // Play dialogue sequence
        AudioSource dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        dialogueAudioSource.spatialBlend = 0f; // 2D audio for consistency
        for (int i = 0; i < dialogueAudio.Count && i < subtitles.Count; i++)
        {
            // Switch camera based on sequence
            if (useCamera3[i])
            {
                playerCamera.gameObject.SetActive(false);
                camera3.gameObject.SetActive(true);
            }
            else
            {
                playerCamera.gameObject.SetActive(true);
                camera3.gameObject.SetActive(false);
            }

            // Play audio with appropriate volume
            float volume = useCamera3[i] ? npcAudioVolume : playerAudioVolume;
            dialogueAudioSource.clip = dialogueAudio[i];
            dialogueAudioSource.volume = volume;
            dialogueAudioSource.Play();

            // Show subtitle with outline
            ShowSubtitleWithOutline(subtitles[i]);

            // Wait for audio to finish or minimum duration
            float waitTime = Mathf.Max(dialogueAudio[i].length, subtitleDuration);
            yield return new WaitForSeconds(waitTime);

            // Clear subtitle
            ClearSubtitle();
        }

        // End of cutscene
        playerCamera.gameObject.SetActive(true);
        camera3.gameObject.SetActive(false);

        // Resume player movement
        if (fpsController != null)
        {
            fpsController.enabled = true;
        }

        // Show end text
        ShowEndText();

        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Wait for end text to display
        yield return new WaitForSeconds(endTextDuration);

        // Disable the trigger after use
        gameObject.SetActive(false);
    }

    void CreateSubtitleCanvas()
    {
        subtitleCanvas = new GameObject("SubtitleCanvas");
        Canvas canvas = subtitleCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        subtitleCanvas.AddComponent<CanvasScaler>();
        subtitleCanvas.AddComponent<GraphicRaycaster>();
    }

    void ShowSubtitleWithOutline(string text)
    {
        // Clear any existing subtitles
        ClearSubtitle();

        // Create subtitle text object
        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(subtitleCanvas.transform);

        // Add Outline component
        Outline outline = subtitleObj.AddComponent<Outline>();
        outline.effectColor = subtitleOutlineColor;
        outline.effectDistance = new Vector2(subtitleOutlineWidth, subtitleOutlineWidth);

        // Add Text component
        Text subtitleText = subtitleObj.AddComponent<Text>();
        subtitleText.text = text;
        subtitleText.font = subtitleFont;
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.color = subtitleColor;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        subtitleText.verticalOverflow = VerticalWrapMode.Overflow;

        // Position subtitle
        RectTransform rect = subtitleObj.GetComponent<RectTransform>();
        rect.anchoredPosition = subtitlePosition;
        rect.sizeDelta = new Vector2(1000, 100);
    }

    void ClearSubtitle()
    {
        foreach (Transform child in subtitleCanvas.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void ShowEndText()
    {
        // Create end text object
        GameObject endTextObj = new GameObject("EndText");
        endTextObj.transform.SetParent(subtitleCanvas.transform);

        // Add Outline component
        Outline outline = endTextObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);

        // Add Text component
        Text endTextComponent = endTextObj.AddComponent<Text>();
        endTextComponent.text = endText;
        endTextComponent.font = endTextFont;
        endTextComponent.fontSize = endTextFontSize;
        endTextComponent.color = endTextColor;
        endTextComponent.alignment = TextAnchor.MiddleLeft;

        // Position end text in the top-left-middle-center
        RectTransform rect = endTextObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); // Top-left anchor
        rect.anchorMax = new Vector2(0, 1); // Top-left anchor
        rect.pivot = new Vector2(0, 1); // Pivot at top-left
        rect.anchoredPosition = new Vector2(50, -50); // Offset from top-left corner
        rect.sizeDelta = new Vector2(600, 100); // Wider size for objective text

        // Add fade effect
        StartCoroutine(FadeOutText(endTextComponent, endTextDuration));
    }

    IEnumerator FadeOutText(Text text, float duration)
    {
        yield return new WaitForSeconds(duration - 1f); // Wait before starting fade

        float fadeDuration = 1f;
        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            currentTime += Time.deltaTime;
            yield return null;
        }

        Destroy(text.gameObject);
    }
}