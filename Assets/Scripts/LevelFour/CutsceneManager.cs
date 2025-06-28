using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("Dialogue Audio Clips")]
    public AudioClip sophieDialogue1Clip; // "Wow that was a really sad memory you collected"
    public AudioClip sophieDialogue2Clip; // "Make sure to collect all of this around you're dream world"
    public AudioClip sophieDialogue3Clip; // "Oh my gosh, run Andy's coming!!"
    public AudioClip glassBreakingSoundClip;
    
    [Header("Music Audio Clips")]
    public AudioClip backgroundMusic;
    [Range(0, 1)] public float backgroundMusicVolume = 0.5f;
    public AudioClip chasingMusic;
    [Range(0, 1)] public float chasingMusicVolume = 0.7f;
    
    [Header("Scene References")]
    public Camera playerCamera;
    public Camera camera2;
    public GameObject monster;
    public FPSHorrorPlayer3 playerMovementScript; // Updated to reference the specific script
    
    [Header("Monster Position")]
    public Vector3 monsterPosition = new Vector3(30.41f, -0.09f, -117.09f);
    
    [Header("Subtitle Settings")]
    [TextArea(2, 5)]
    public string subtitle1 = "Sophie the Cat: Wow that was a really sad memory you collected";
    [TextArea(2, 5)]
    public string subtitle2 = "Sophie the cat: Make sure to collect all of this around you're dream world";
    [TextArea(2, 5)]
    public string subtitle3 = "Sophie the cat: Oh my gosh, run Andy's coming!!";
    public Color subtitleTextColor = new Color(1f, 0.8f, 0.2f); // Gold-ish color
    public Color subtitleOutlineColor = Color.black;
    public float subtitleOutlineWidth = 0.2f;
    public TMP_FontAsset subtitleFont;
    public float subtitleFontSize = 32f;
    
    [Header("Subtitle Timing")]
    public float subtitle1DisplayTime = 4f;
    public float subtitle2DisplayTime = 4f;
    public float subtitle3DisplayTime = 4f;
    public float additionalDelayBetweenSubtitles = 0.5f;
    public float readingSpeedCharsPerSecond = 15f; // Average reading speed
    
    [Header("Objective Text Settings")]
    public string objectiveText = "Run";
    public Color objectiveTextColor = Color.red;
    public Color objectiveOutlineColor = Color.black;
    public float objectiveOutlineWidth = 0.2f;
    public TMP_FontAsset objectiveFont;
    public float objectiveFontSize = 42f;
    
    [Header("Timing")]
    public float camera2Duration = 5f;
    public float objectiveDisplayDuration = 10f;
    public float glassBreakingPauseTime = 1f;
    public float playerHeadStartTime = 6f; // Time player gets to run before monster starts chasing
    
    private Canvas cutsceneCanvas;
    private TextMeshProUGUI subtitleTextComponent;
    private TextMeshProUGUI objectiveTextComponent;
    private AudioSource dialogueAudioSource;
    private AudioSource soundEffectAudioSource;
    private AudioSource musicAudioSource;
    private MonsterController monsterController;
    private Coroutine cursorCoroutine;
    
    private void Awake()
    {
        // Create audio sources
        GameObject dialogueAudioObj = new GameObject("DialogueAudio");
        dialogueAudioObj.transform.parent = transform;
        dialogueAudioSource = dialogueAudioObj.AddComponent<AudioSource>();
        
        GameObject sfxAudioObj = new GameObject("SFXAudio");
        sfxAudioObj.transform.parent = transform;
        soundEffectAudioSource = sfxAudioObj.AddComponent<AudioSource>();
        
        GameObject musicAudioObj = new GameObject("MusicAudio");
        musicAudioObj.transform.parent = transform;
        musicAudioSource = musicAudioObj.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        
        // Get monster controller
        if (monster != null)
            monsterController = monster.GetComponent<MonsterController>();
    }
    
    private void Start()
    {
        // Create UI elements
        SetupUI();
        
        // Start the cutscene
        StartCoroutine(PlayCutscene());
        
        // Start background music
        if (backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.volume = backgroundMusicVolume;
            musicAudioSource.Play();
        }
    }
    
    private void SetupUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("CutsceneCanvas");
        cutsceneCanvas = canvasObj.AddComponent<Canvas>();
        cutsceneCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create subtitle text directly on canvas (no panel)
        GameObject subtitleTextObj = new GameObject("SubtitleText");
        subtitleTextObj.transform.SetParent(cutsceneCanvas.transform, false);
        subtitleTextComponent = subtitleTextObj.AddComponent<TextMeshProUGUI>();
        
        // Apply subtitle text styling
        subtitleTextComponent.alignment = TextAlignmentOptions.Center;
        subtitleTextComponent.fontSize = subtitleFontSize;
        subtitleTextComponent.color = subtitleTextColor;
        if (subtitleFont != null)
            subtitleTextComponent.font = subtitleFont;
        
        // Add outline/stroke to subtitle text
        subtitleTextComponent.enableVertexGradient = false;
        subtitleTextComponent.outlineWidth = subtitleOutlineWidth;
        subtitleTextComponent.outlineColor = subtitleOutlineColor;
        
        // Position subtitle text at middle center of screen
        RectTransform textRect = subtitleTextComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.2f, 0.45f); // Center vertically
        textRect.anchorMax = new Vector2(0.8f, 0.55f); // Center vertically
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Create objective text
        GameObject objectiveTextObj = new GameObject("ObjectiveText");
        objectiveTextObj.transform.SetParent(cutsceneCanvas.transform, false);
        objectiveTextComponent = objectiveTextObj.AddComponent<TextMeshProUGUI>();
        
        // Apply objective text styling
        objectiveTextComponent.alignment = TextAlignmentOptions.Left;
        objectiveTextComponent.fontSize = objectiveFontSize;
        objectiveTextComponent.fontStyle = FontStyles.Bold;
        objectiveTextComponent.color = objectiveTextColor;
        objectiveTextComponent.text = objectiveText;
        if (objectiveFont != null)
            objectiveTextComponent.font = objectiveFont;
        
        // Add outline/stroke to objective text
        objectiveTextComponent.enableVertexGradient = false;
        objectiveTextComponent.outlineWidth = objectiveOutlineWidth;
        objectiveTextComponent.outlineColor = objectiveOutlineColor;
        
        objectiveTextComponent.gameObject.SetActive(false);
        RectTransform objectiveRect = objectiveTextComponent.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(0, 1);
        objectiveRect.anchorMax = new Vector2(0, 1);
        objectiveRect.pivot = new Vector2(0, 1);
        objectiveRect.anchoredPosition = new Vector2(50, -50);
        objectiveRect.sizeDelta = new Vector2(200, 50);
        
        // Hide UI initially
        subtitleTextComponent.text = "";
    }
    
    private IEnumerator PlayCutscene()
    {
        // Validate audio clips
        if (sophieDialogue1Clip == null || sophieDialogue2Clip == null || 
            sophieDialogue3Clip == null || glassBreakingSoundClip == null)
        {
            Debug.LogError("One or more audio clips are missing. Please assign all required audio clips in the inspector.");
            yield break;
        }
        
        // Disable player movement using the static variable
        FPSHorrorPlayer3.allowMovement = false;
        
        // Show cursor - EXPLICITLY SET BOTH PROPERTIES
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Force cursor to be visible by setting it multiple times
        cursorCoroutine = StartCoroutine(EnsureCursorVisible());
        
        // Ensure player camera is active at start
        playerCamera.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
        
        // Hide monster initially
        monster.SetActive(false);
        
        // First dialogue
        yield return StartCoroutine(DisplaySubtitle(subtitle1, sophieDialogue1Clip, subtitle1DisplayTime));
        
        // Wait additional time between subtitles if needed
        yield return new WaitForSeconds(additionalDelayBetweenSubtitles);
        
        // Second dialogue line
        yield return StartCoroutine(DisplaySubtitle(subtitle2, sophieDialogue2Clip, subtitle2DisplayTime));
        
        // Wait additional time between subtitles if needed
        yield return new WaitForSeconds(additionalDelayBetweenSubtitles);
        
        // Glass breaking sound
        subtitleTextComponent.text = "";
        soundEffectAudioSource.clip = glassBreakingSoundClip;
        soundEffectAudioSource.Play();
        
        // Wait for sound effect
        yield return new WaitForSeconds(glassBreakingSoundClip.length + glassBreakingPauseTime);
        
        // Final dialogue
        yield return StartCoroutine(DisplaySubtitle(subtitle3, sophieDialogue3Clip, subtitle3DisplayTime));
        
        // Position monster at the specified position
        monster.transform.position = monsterPosition;
        
        // Make sure monster is not chasing yet
        if (monsterController != null)
            monsterController.StopChasing();
            
        // Activate monster but don't chase yet
        monster.SetActive(true);
        
        // Switch to camera 2 to show the monster
        playerCamera.gameObject.SetActive(false);
        camera2.gameObject.SetActive(true);
        
        // Clear subtitle immediately when camera switches
        subtitleTextComponent.text = "";
        
        // Switch to chasing music when camera changes and keep it playing
        if (chasingMusic != null)
        {
            musicAudioSource.clip = chasingMusic;
            musicAudioSource.volume = chasingMusicVolume;
            musicAudioSource.Play();
        }
        
        // Wait for camera2 duration
        yield return new WaitForSeconds(camera2Duration);
        
        // Switch back to player camera
        playerCamera.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
        
        // NOTE: We no longer switch back to background music here
        // The chase music will continue playing in a loop
        
        // Stop ensuring cursor is visible
        if (cursorCoroutine != null)
            StopCoroutine(cursorCoroutine);
        
        // IMPORTANT: Hide cursor and lock it for gameplay
        LockCursor();
        
        // Resume player movement
        FPSHorrorPlayer3.allowMovement = true;
        
        // Show objective
        objectiveTextComponent.gameObject.SetActive(true);
        
        // Start delayed monster chase
        StartCoroutine(StartMonsterChaseAfterDelay());
        
        // Hide objective after duration
        yield return new WaitForSeconds(objectiveDisplayDuration);
        objectiveTextComponent.gameObject.SetActive(false);
        
        // Make sure cursor is still locked after objective disappears
        LockCursor();
    }
    
    private void LockCursor()
    {
        // Properly lock and hide cursor for FPS gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Debug log to confirm cursor is locked
        Debug.Log("Cursor locked and hidden for gameplay");
    }
    
    private IEnumerator EnsureCursorVisible()
    {
        // Keep setting cursor visible every frame during cutscene
        while (true)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            yield return null;
        }
    }
    
    private IEnumerator StartMonsterChaseAfterDelay()
    {
        // Wait for the specified delay to give player head start
        yield return new WaitForSeconds(playerHeadStartTime);
        
        // Start monster chasing
        if (monsterController != null)
            monsterController.ResumeChasing();
    }
    
    private IEnumerator DisplaySubtitle(string text, AudioClip audioClip, float minDisplayTime)
    {
        // Display the subtitle
        subtitleTextComponent.text = text;
        
        // Play the audio
        if (audioClip != null)
        {
            dialogueAudioSource.clip = audioClip;
            dialogueAudioSource.Play();
        }
        
        // Calculate minimum time needed to read the text
        float readingTime = text.Length / readingSpeedCharsPerSecond;
        
        // Calculate total wait time (longer of audio duration, minimum display time, or reading time)
        float audioTime = (audioClip != null) ? audioClip.length : 0f;
        float waitTime = Mathf.Max(audioTime, minDisplayTime, readingTime);
        
        // Wait for the calculated time
        yield return new WaitForSeconds(waitTime);
    }
    
    // Make sure cursor stays locked if script is disabled
    private void OnDisable()
    {
        // Ensure cursor is locked when script is disabled
        LockCursor();
    }
}