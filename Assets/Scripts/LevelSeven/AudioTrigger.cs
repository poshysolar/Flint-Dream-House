using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip1;
    [SerializeField] private AudioClip audioClip2;
    
    [Header("Subtitle Settings")]
    [SerializeField] private Color textColor = Color.Lerp(Color.red, Color.yellow, 0.5f); // Orange color
    [SerializeField] private float textSize = 30f;
    [SerializeField] private Font textFont;
    [SerializeField] private float subtitleOutlineSize = 2f;
    
    [Header("Objective Settings")]
    [SerializeField] private Color objectiveColor = Color.red;
    [SerializeField] private float objectiveSize = 50f;
    [SerializeField] private Font objectiveFont;
    [SerializeField] private float objectiveOutlineSize = 3f;

    private AudioSource audioSource;
    private GameObject subtitleUI;
    private Text subtitleText;
    private Image backgroundPanel;
    private GameObject objectiveUI;
    private Text objectiveText;
    private bool hasTriggered = false;
    private float audioEndTime;
    private bool isSecondAudioPlaying = false;

    void Start()
    {
        // Create AudioSource for 2D audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Ensure 2D audio
        audioSource.playOnAwake = false;

        // Create Subtitle UI
        CreateSubtitleUI();
        
        // Create Objective UI
        CreateObjectiveUI();
    }

    void CreateSubtitleUI()
    {
        // Create UI Canvas
        GameObject canvasObj = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Background Panel
        GameObject panelObj = new GameObject("SubtitlePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        backgroundPanel = panelObj.AddComponent<Image>();
        backgroundPanel.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.2f);
        panelRect.offsetMin = new Vector2(0, 0);
        panelRect.offsetMax = new Vector2(0, 0);

        // Create Text
        GameObject textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(panelObj.transform, false);
        subtitleText = textObj.AddComponent<Text>();
        subtitleText.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitleText.fontSize = Mathf.RoundToInt(textSize);
        subtitleText.color = textColor;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.text = "";
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);

        // Add Text Outline
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(subtitleOutlineSize, -subtitleOutlineSize);

        subtitleUI = canvasObj;
        subtitleUI.SetActive(false);
    }
    
    void CreateObjectiveUI()
    {
        // Create UI Canvas
        GameObject canvasObj = new GameObject("ObjectiveCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // Ensure it's on top
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create Text
        GameObject textObj = new GameObject("ObjectiveText");
        textObj.transform.SetParent(canvasObj.transform, false);
        objectiveText = textObj.AddComponent<Text>();
        objectiveText.font = objectiveFont != null ? objectiveFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        objectiveText.fontSize = Mathf.RoundToInt(objectiveSize);
        objectiveText.color = objectiveColor;
        objectiveText.alignment = TextAnchor.MiddleCenter;
        objectiveText.text = "FIND SOPHIE THE CAT";
        
        // Add Text Outline
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(objectiveOutlineSize, -objectiveOutlineSize);
        
        // Position at the top middle of the screen
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.85f);
        textRect.anchorMax = new Vector2(0.5f, 0.85f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 100);
        
        // Add Shadow for extra horror effect
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(4, -4);
        
        objectiveUI = canvasObj;
        objectiveUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            PlayAudioSequence();
        }
    }

    void PlayAudioSequence()
    {
        if (audioClip1 != null)
        {
            audioSource.clip = audioClip1;
            audioSource.Play();
            subtitleUI.SetActive(true);
            subtitleText.text = "Mikey: I'm not the only monster you created, this curse will grow bigger & bigger";
            audioEndTime = Time.time + audioClip1.length;
            Invoke("PlaySecondAudio", audioClip1.length + 0.5f);
        }
    }

    void PlaySecondAudio()
    {
        if (audioClip2 != null)
        {
            isSecondAudioPlaying = true;
            audioSource.clip = audioClip2;
            audioSource.Play();
            subtitleText.text = "Mikey: killing me will create more monsters, you will watch and see";
            audioEndTime = Time.time + audioClip2.length;
            Invoke("AudioComplete", audioClip2.length);
        }
        else
        {
            AudioComplete();
        }
    }
    
    void AudioComplete()
    {
        HideSubtitles();
        ShowObjective();
    }

    void HideSubtitles()
    {
        subtitleUI.SetActive(false);
    }
    
    void ShowObjective()
    {
        objectiveUI.SetActive(true);
        StartCoroutine(ObjectiveDisplayEffect());
    }
    
    IEnumerator ObjectiveDisplayEffect()
    {
        // Initial appearance effect
        CanvasGroup canvasGroup = objectiveUI.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        
        // Fade in with slight pulsing
        float duration = 1.0f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, normalizedTime);
            
            // Add slight scale pulsing for horror effect
            float pulse = 1 + 0.05f * Mathf.Sin(normalizedTime * Mathf.PI * 4);
            objectiveText.transform.localScale = new Vector3(pulse, pulse, 1);
            
            yield return null;
        }
        
        // Display for 10 seconds with subtle effects
        float displayTime = 10.0f;
        elapsed = 0;
        
        while (elapsed < displayTime)
        {
            elapsed += Time.deltaTime;
            
            // Subtle pulsing and movement
            float pulse = 1 + 0.02f * Mathf.Sin(Time.time * 3);
            objectiveText.transform.localScale = new Vector3(pulse, pulse, 1);
            
            // Subtle color variation
            float colorPulse = 0.9f + 0.1f * Mathf.Sin(Time.time * 2);
            objectiveText.color = new Color(
                objectiveColor.r * colorPulse,
                objectiveColor.g * colorPulse,
                objectiveColor.b * colorPulse,
                objectiveColor.a
            );
            
            yield return null;
        }
        
        // Fade out with distortion effect
        duration = 1.0f;
        elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Fade out
            canvasGroup.alpha = Mathf.Lerp(1, 0, normalizedTime);
            
            // Distortion effect - text gets slightly stretched and fades
            float distortionX = 1 + normalizedTime * 0.2f;
            float distortionY = 1 - normalizedTime * 0.1f;
            objectiveText.transform.localScale = new Vector3(distortionX, distortionY, 1);
            
            yield return null;
        }
        
        objectiveUI.SetActive(false);
    }

    void Update()
    {
        // Ensure subtitles are visible during second audio
        if (isSecondAudioPlaying && audioSource.isPlaying && !subtitleUI.activeSelf)
        {
            subtitleUI.SetActive(true);
        }
        
        // Ensure subtitles are hidden if audio stops unexpectedly
        if (subtitleUI.activeSelf && Time.time > audioEndTime && !audioSource.isPlaying)
        {
            subtitleUI.SetActive(false);
        }
    }
}