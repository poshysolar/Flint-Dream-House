using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class AudioSubtitlesSystem : MonoBehaviour
{
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont;

    [Header("AUDIO SETTINGS")]
    public AudioClip voiceOverClip;

    [Header("SUBTITLE SETTINGS")]
    [TextArea(3, 5)]
    public string subtitleText = "Sophie The Cat: Don't worry about Andy he should be fine, the machine didn't crush him";
    public Color subtitleColor = Color.white;
    public int subtitleSize = 24;
    [Range(0f, 1f)] public float subtitlePosition = 0.15f;

    [Header("OBJECTIVE SETTINGS")] 
    public string objectiveText = "Find her";
    public Color objectiveColor = Color.red;
    public int objectiveSize = 36;
    [Range(0f, 1f)] public float objectivePosition = 0.9f;
    public float objectiveDisplayTime = 12f;

    private AudioSource audioSource;
    private TextMeshProUGUI subtitleTMP;
    private TextMeshProUGUI objectiveTMP;
    private Canvas canvas;

    void Start()
    {
        // Set up audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = voiceOverClip;

        // Create UI
        CreateCanvasWithText();
        
        // Start sequence
        StartCoroutine(PlaySequence());
    }

    void CreateCanvasWithText()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("SubtitleCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        // Add necessary canvas components
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Subtitle Text
        GameObject subtitleGO = new GameObject("SubtitleText");
        subtitleGO.transform.SetParent(canvasGO.transform, false);

        RectTransform subtitleRT = subtitleGO.AddComponent<RectTransform>();
        subtitleRT.anchorMin = new Vector2(0, subtitlePosition);
        subtitleRT.anchorMax = new Vector2(1, subtitlePosition);
        subtitleRT.pivot = new Vector2(0.5f, 0.5f);
        subtitleRT.sizeDelta = new Vector2(0, 100);
        subtitleRT.anchoredPosition = Vector2.zero;

        subtitleTMP = subtitleGO.AddComponent<TextMeshProUGUI>();
        subtitleTMP.text = subtitleText;
        subtitleTMP.color = subtitleColor;
        subtitleTMP.fontSize = subtitleSize;
        subtitleTMP.alignment = TextAlignmentOptions.Center;
        subtitleTMP.enableWordWrapping = true;
        if (customFont != null)
            subtitleTMP.font = customFont;

        // Create Objective Text
        GameObject objectiveGO = new GameObject("ObjectiveText");
        objectiveGO.transform.SetParent(canvasGO.transform, false);

        RectTransform objectiveRT = objectiveGO.AddComponent<RectTransform>();
        objectiveRT.anchorMin = new Vector2(0, objectivePosition);
        objectiveRT.anchorMax = new Vector2(1, objectivePosition);
        objectiveRT.pivot = new Vector2(0.5f, 0.5f);
        objectiveRT.sizeDelta = new Vector2(0, 100);
        objectiveRT.anchoredPosition = Vector2.zero;

        objectiveTMP = objectiveGO.AddComponent<TextMeshProUGUI>();
        objectiveTMP.text = objectiveText;
        objectiveTMP.color = objectiveColor;
        objectiveTMP.fontSize = objectiveSize;
        objectiveTMP.alignment = TextAlignmentOptions.Center;
        objectiveTMP.enableWordWrapping = true;
        if (customFont != null)
            objectiveTMP.font = customFont;
        
        // Set initial alpha for objective text
        objectiveTMP.alpha = 0;

        // Add outline to both texts
        SetupTextOutline(subtitleTMP);
        SetupTextOutline(objectiveTMP);
    }

    void SetupTextOutline(TextMeshProUGUI tmp)
    {
        tmp.enableVertexGradient = true;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;
    }

    IEnumerator PlaySequence()
    {
        // Play audio and show subtitle
        audioSource.Play();
        subtitleTMP.text = subtitleText;

        // Wait for clip to finish
        yield return new WaitForSeconds(voiceOverClip.length);
        subtitleTMP.text = "";

        // Show objective with fade in/out
        yield return StartCoroutine(FadeObjective(true));
        yield return new WaitForSeconds(objectiveDisplayTime);
        yield return StartCoroutine(FadeObjective(false));
    }

    IEnumerator FadeObjective(bool fadeIn)
    {
        float duration = 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            objectiveTMP.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed/duration);
            yield return null;
        }
        objectiveTMP.alpha = targetAlpha;
    }
}