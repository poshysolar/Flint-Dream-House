using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class TriggerSequence : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public GameObject player;         // The player GameObject
    public GameObject mainCamera;     // The initial active camera
    public GameObject camera3;        // The camera to switch to
    public GameObject monsterPrefab;  // The monster prefab to spawn
    public GameObject rock;           // The rock GameObject to slide
    public AudioClip rockSlideAudio;  // Audio clip for rock sliding
    public AudioClip subtitleAudio;   // Audio clip for subtitle dialogue
    public float playerMoveDuration = 2f; // Duration for player roll over
    public float rockMoveDuration = 1f;   // Duration for rock sliding
    public float rollRotationSpeed = 360f; // Degrees per second for rolling effect
    public Font subtitleFont;         // Font for subtitle
    public int subtitleFontSize = 40; // Increased font size for better visibility
    public Color subtitleColor = Color.white; // Color for subtitle
    public float subtitleDuration = 3f; // Duration to display subtitle
    public float fadeDuration = 1f;   // Duration for fade to black
    public Color fadeImageColor = Color.black; // Color for fade image

    private bool sequenceStarted = false;
    private GameObject subtitleTextObj;
    private AudioSource audioSource;  // AudioSource component for playing clips
    private AudioSource subtitleAudioSource; // Separate AudioSource for subtitles

    void Start()
    {
        // Ensure cameras are in correct initial state
        mainCamera.SetActive(true);
        camera3.SetActive(false);

        // Add AudioSource components to this GameObject
        audioSource = gameObject.AddComponent<AudioSource>();
        subtitleAudioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player and sequence hasn't started
        if (other.CompareTag("Player") && !sequenceStarted)
        {
            sequenceStarted = true;
            // Show mouse cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            StartCoroutine(RollOverSequence());
        }
    }

    IEnumerator RollOverSequence()
    {
        // Step 1: Roll the player to the target position with rotation
        Vector3 startPos = new Vector3(20.41f, 6.38f, 331.22f);
        Vector3 targetPos = new Vector3(20.41f, 6.38f, 356.26f);
        Quaternion startRot = player.transform.rotation;
        float elapsed = 0f;

        while (elapsed < playerMoveDuration)
        {
            // Lerp position
            player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / playerMoveDuration);
            // Add rolling rotation (around X-axis for forward roll)
            player.transform.Rotate(Vector3.right, rollRotationSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.transform.position = targetPos;
        player.transform.rotation = startRot; // Reset rotation after rolling

        // Step 2: Switch to Camera3 and show subtitle with audio
        mainCamera.SetActive(false);
        camera3.SetActive(true);

        // Create subtitle UI
        CreateSubtitle("Sophie The Cat: NO! Andy you are not allowed to hurt Flint!!");
        
        // Play subtitle audio and wait for it to finish
        if (subtitleAudio != null)
        {
            subtitleAudioSource.clip = subtitleAudio;
            subtitleAudioSource.Play();
            yield return new WaitForSeconds(subtitleAudio.length);
        }
        else
        {
            // If no audio, just wait for the subtitle duration
            yield return new WaitForSeconds(subtitleDuration);
        }

        // Step 3: Spawn the monster and play "Fall" animation
        GameObject monster = Instantiate(monsterPrefab, new Vector3(27.7f, -0.09f, 329.2f), Quaternion.identity);
        Animator monsterAnimator = monster.GetComponent<Animator>();
        if (monsterAnimator != null)
        {
            monsterAnimator.Play("Fall");
        }

        // Step 4: Slide the rock down on Y-axis only and play rock slide audio
        Vector3 rockStartPos = rock.transform.position; // Assumes rock is at Y=35.26001 initially
        Vector3 rockTargetPos = new Vector3(rockStartPos.x, 19.66f, rockStartPos.z);
        elapsed = 0f;

        if (rockSlideAudio != null)
        {
            audioSource.clip = rockSlideAudio;
            audioSource.Play();
        }

        while (elapsed < rockMoveDuration)
        {
            rock.transform.position = Vector3.Lerp(rockStartPos, rockTargetPos, elapsed / rockMoveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rock.transform.position = rockTargetPos;

        // Step 5: Wait for 3 seconds (or until rock slide audio finishes if longer)
        float remainingWaitTime = 3f;
        if (rockSlideAudio != null && audioSource.isPlaying)
        {
            remainingWaitTime = Mathf.Max(remainingWaitTime, rockSlideAudio.length - rockMoveDuration);
        }
        yield return new WaitForSeconds(remainingWaitTime);

        // Step 6: Remove subtitle
        if (subtitleTextObj != null)
        {
            Destroy(subtitleTextObj);
        }

        // Step 7: Switch back to main camera, reset player, hide cursor, stop audio
        camera3.SetActive(false);
        mainCamera.SetActive(true);
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        if (subtitleAudioSource != null)
        {
            subtitleAudioSource.Stop();
        }

        // Teleport player to final position with rotation reset
        player.transform.position = new Vector3(4.32f, 6.38f, 383.92f);
        player.transform.rotation = Quaternion.identity; // Rotation all 0

        // Hide mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Step 8: Fade to black and load LevelFive
        yield return StartCoroutine(FadeToBlack());
        SceneManager.LoadScene("LevelFive");
    }

    void CreateSubtitle(string text)
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Text
        subtitleTextObj = new GameObject("SubtitleText");
        subtitleTextObj.transform.SetParent(canvasObj.transform, false);
        Text subtitleText = subtitleTextObj.AddComponent<Text>();
        subtitleText.text = text;
        subtitleText.font = subtitleFont != null ? subtitleFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.color = subtitleColor;
        subtitleText.alignment = TextAnchor.MiddleCenter;

        // Add Outline component for text stroke
        Outline outline = subtitleTextObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        // Position at middle center, 20% from bottom for better visibility
        RectTransform rectTransform = subtitleText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.2f); // 20% from bottom
        rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(1800, 200); // Wide enough for full text

        // Destroy canvas when audio finishes or after duration
        if (subtitleAudio != null)
        {
            Destroy(canvasObj, subtitleAudio.length);
        }
        else
        {
            Destroy(canvasObj, subtitleDuration);
        }
    }

    IEnumerator FadeToBlack()
    {
        // Create Canvas for fade
        GameObject fadeCanvasObj = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = fadeCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Image for fade
        GameObject fadeImageObj = new GameObject("FadeImage");
        fadeImageObj.transform.SetParent(fadeCanvasObj.transform, false);
        Image fadeImage = fadeImageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeImageColor.r, fadeImageColor.g, fadeImageColor.b, 0f); // Start transparent

        // Set Image to cover screen
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Fade to black
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(fadeImageColor.r, fadeImageColor.g, fadeImageColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(fadeImageColor.r, fadeImageColor.g, fadeImageColor.b, 1f); // Fully opaque
    }
}