using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroCutscene : MonoBehaviour
{
    // Editable subtitle properties
    public Font subtitleFont = null; // Assign in Inspector, defaults to Arial if null
    public Color subtitleColor = Color.white; // Assign subtitle color in Inspector
    public int subtitleFontSize = 30; // Assign font size in Inspector
    public Color subtitleStrokeColor = Color.black; // Assign stroke color in Inspector
    public Vector2 subtitleStrokeDistance = new Vector2(1f, -1f); // Assign stroke distance in Inspector
    public AudioClip backgroundMusic; // Assign background music clip in Inspector
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.5f; // Assign background music volume in Inspector (0 to 1)
    public float cameraRotationDuration = 2f; // Duration for camera rotation in seconds

    // Subtitle data
    [Serializable]
    public struct SubtitleLine
    {
        [TextArea(3, 5)] // Makes the text field larger in Inspector
        public string text; // Subtitle text
        public AudioClip audioClip; // Audio for this subtitle
        public float duration; // Duration to display subtitle if no audio
    }

    public SubtitleLine[] subtitles = new SubtitleLine[]
    {
        new SubtitleLine
        {
            text = "Mother: Please... Please Flint you have to wake up from those nightmares",
            audioClip = null,
            duration = 5f
        },
        new SubtitleLine
        {
            text = "Mother: This cannot be real...",
            audioClip = null,
            duration = 5f
        }
    };

    private AudioSource backgroundAudioSource;
    private AudioSource dialogueAudioSource;

    void Start()
    {
        // Setup AudioSource for background music
        backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        backgroundAudioSource.clip = backgroundMusic;
        backgroundAudioSource.loop = true; // Loop the music during the cutscene
        backgroundAudioSource.playOnAwake = false; // We'll start it manually
        backgroundAudioSource.volume = backgroundMusicVolume;

        // Setup AudioSource for dialogue
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        dialogueAudioSource.loop = false; // Dialogue should not loop
        dialogueAudioSource.playOnAwake = false;
        dialogueAudioSource.volume = 1.0f; // Dialogue volume unchanged

        StartCoroutine(PlayCutscene());
    }

    void Update()
    {
        // Update background music volume in real-time if changed in Inspector
        if (backgroundAudioSource != null && backgroundAudioSource.clip != null)
        {
            backgroundAudioSource.volume = backgroundMusicVolume;
        }
    }

    IEnumerator PlayCutscene()
    {
        // Disable player movement
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.canMove = false;
            }
        }

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Set camera rotation to zero initially
        Camera.main.transform.eulerAngles = Vector3.zero;

        // Create canvas for UI
        GameObject canvasGO = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Create subtitle text UI element
        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(canvasGO.transform, false);
        Text subtitleText = textGO.AddComponent<Text>();
        subtitleText.font = subtitleFont != null ? subtitleFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.color = subtitleColor;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        RectTransform rectTransform = subtitleText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = new Vector2(0, 50); // 50 pixels from bottom
        rectTransform.sizeDelta = new Vector2(1000, 100);

        // Add Outline component for text stroke
        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor = subtitleStrokeColor;
        outline.effectDistance = subtitleStrokeDistance;

        // Create fade image UI element
        GameObject fadeGO = new GameObject("FadeImage");
        fadeGO.transform.SetParent(canvasGO.transform, false);
        Image fadeImage = fadeGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        RectTransform fadeRect = fadeImage.GetComponent<RectTransform>();
        fadeRect.anchorMin = new Vector2(0, 0);
        fadeRect.anchorMax = new Vector2(1, 1);
        fadeRect.sizeDelta = Vector2.zero;

        // Play background music
        if (backgroundMusic != null)
        {
            backgroundAudioSource.Play();
        }

        // Smoothly rotate camera to Y: 41.065
        Vector3 targetRotation = new Vector3(0, 41.065f, 0);
        float timer = 0;
        Quaternion startRotation = Camera.main.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);
        while (timer < cameraRotationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cameraRotationDuration;
            Camera.main.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        Camera.main.transform.rotation = endRotation; // Ensure exact rotation

        // Display subtitles
        foreach (var subtitle in subtitles)
        {
            subtitleText.text = subtitle.text;
            if (subtitle.audioClip != null)
            {
                dialogueAudioSource.PlayOneShot(subtitle.audioClip); // Play dialogue audio
                yield return new WaitForSeconds(subtitle.audioClip.length); // Wait for audio duration
            }
            else
            {
                yield return new WaitForSeconds(subtitle.duration); // Use specified duration
            }
        }

        // Stop background music
        if (backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Stop();
        }

        // Fade screen to black
        float fadeDuration = 1f;
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Switch to LevelFour
        SceneManager.LoadScene("LevelFour");
    }
}