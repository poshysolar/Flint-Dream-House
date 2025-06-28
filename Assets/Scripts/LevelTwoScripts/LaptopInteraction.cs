using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LaptopInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float maxInteractionDistance = 5f;
    public float promptVisibleDistance = 8f;
    
    [Header("Prompt Settings")]
    public Texture2D promptTexture;
    public float promptWidth = 100f;
    public float promptHeight = 100f;
    public float promptOffsetX = 0f;
    public float promptOffsetY = 0f;

    [Header("Audio Settings")]
    public AudioClip dialogueAudio1;
    public AudioClip dialogueAudio2;
    
    [Header("Subtitle Settings")]
    [TextArea(3, 10)]
    public string subtitleText1 = "Android: Andy loves mother nature";
    [TextArea(3, 10)]
    public string subtitleText2 = "Android: Unfortunately Andy's anger can get the best of him";
    public float firstTextDelay = 9f; // Changed to 9 seconds
    public float delayBetweenLines = 0.5f;
    public Color subtitleColor = Color.white;
    public Color strokeColor = Color.black; // Added stroke color
    public float strokeWidth = 0.5f; // Added stroke width
    [Range(10, 100)]
    public int subtitleFontSize = 40;
    public Font subtitleFont;

    private Transform player;
    private AudioSource audioSource;
    private Canvas subtitleCanvas;
    private Text subtitleTextUI;
    private Outline textOutline; // Added outline component
    private bool isDisplayingSubtitles = false;
    private bool isInRange = false;

    void Start()
    {
        player = Camera.main.transform;
        audioSource = GetComponent<AudioSource>();
        CreateSubtitleUI();
    }

    void CreateSubtitleUI()
    {
        GameObject canvasGO = new GameObject("LaptopSub");
        subtitleCanvas = canvasGO.AddComponent<Canvas>();
        subtitleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(canvasGO.transform, false);
        subtitleTextUI = textGO.AddComponent<Text>();
        
        // Add outline component
        textOutline = textGO.AddComponent<Outline>();
        textOutline.effectColor = strokeColor;
        textOutline.effectDistance = new Vector2(strokeWidth, -strokeWidth);
        
        subtitleTextUI.text = "";
        subtitleTextUI.color = subtitleColor;
        subtitleTextUI.fontSize = subtitleFontSize;
        subtitleTextUI.font = subtitleFont != null ? subtitleFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitleTextUI.alignment = TextAnchor.UpperCenter;
        
        RectTransform textRect = subtitleTextUI.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0);
        textRect.anchorMax = new Vector2(0.5f, 0);
        textRect.pivot = new Vector2(0.5f, 0);
        textRect.anchoredPosition = new Vector2(0, 100);
        textRect.sizeDelta = new Vector2(1800, 300);
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        isInRange = distance <= promptVisibleDistance;

        if (Input.GetMouseButtonDown(0) && !isDisplayingSubtitles)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform && distance <= maxInteractionDistance)
                {
                    StartCoroutine(DisplaySubtitles());
                }
            }
        }
    }

    void OnGUI()
    {
        if (isInRange && !isDisplayingSubtitles && promptTexture != null)
        {
            float screenCenterX = Screen.width / 2;
            float screenCenterY = Screen.height / 2;
            float promptX = screenCenterX - (promptWidth / 2) + promptOffsetX;
            float promptY = screenCenterY - (promptHeight / 2) + promptOffsetY;

            GUI.DrawTexture(
                new Rect(promptX, promptY, promptWidth, promptHeight),
                promptTexture
            );
        }
    }

    IEnumerator DisplaySubtitles()
    {
        isDisplayingSubtitles = true;
        
        subtitleTextUI.color = subtitleColor;
        subtitleTextUI.fontSize = subtitleFontSize;
        subtitleTextUI.font = subtitleFont != null ? subtitleFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        textOutline.effectColor = strokeColor; // Update stroke color
        
        if (dialogueAudio1 != null)
        {
            audioSource.clip = dialogueAudio1;
            audioSource.Play();
            
            // Wait 9 seconds before showing first text
            yield return new WaitForSeconds(firstTextDelay);
            
            subtitleTextUI.text = subtitleText1;
            
            // Wait for first audio to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            // Clear first text
            subtitleTextUI.text = "";
        }

        yield return new WaitForSeconds(delayBetweenLines);

        if (dialogueAudio2 != null)
        {
            audioSource.clip = dialogueAudio2;
            audioSource.Play();
            subtitleTextUI.text = subtitleText2; // Only show second text
            
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        subtitleTextUI.text = "";
        isDisplayingSubtitles = false;
    }
}