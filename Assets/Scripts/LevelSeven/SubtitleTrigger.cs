using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SubtitleTrigger : MonoBehaviour
{
    [System.Serializable]
    public class SubtitleLine
    {
        [TextArea(2, 4)]
        public string text;
        public SubtitleCharacter character;
        public AudioClip audioClip;
    }

    public enum SubtitleCharacter { Mikey, Sophie }

    [Header("Subtitles Configuration")]
    public SubtitleLine[] subtitleLines = new SubtitleLine[3] {
        new SubtitleLine() { text = "Mikey: You're too late Flint. I already took your little kitty", character = SubtitleCharacter.Mikey },
        new SubtitleLine() { text = "Mikey: How fun would it be to play against me and your nightmares!", character = SubtitleCharacter.Mikey },
        new SubtitleLine() { text = "Sophie The Cat: Flint help, you must enter the room to find me!", character = SubtitleCharacter.Sophie }
    };

    [Header("Appearance Settings")]
    [Range(10, 60)] public int fontSize = 36;
    public TMP_FontAsset fontAsset;
    public Color outlineColor = Color.black;
    [Range(0, 1)] public float outlineWidth = 0.2f;
    public Color mikeyColor = new Color(1f, 0.5f, 0f); // Orange
    public Color sophieColor = new Color(0.7f, 0f, 1f); // Purple

    private GameObject subtitlePanel;
    private TextMeshProUGUI subtitleText;
    private bool hasTriggered = false;
    private AudioSource audioSource; // For 2D audio playback

    private void Awake()
    {
        // Create a dedicated AudioSource for 2D playback
        GameObject audioObject = new GameObject("SubtitleAudio");
        audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Set to 2D audio
        DontDestroyOnLoad(audioObject); // Persist across scenes if needed
    }

    private void CreateUIElements()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("SubtitlesCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Subtitle Panel
        subtitlePanel = new GameObject("SubtitlePanel");
        subtitlePanel.transform.SetParent(canvas.transform);
        
        // Panel positioning and sizing
        RectTransform panelRect = subtitlePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.25f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Panel background
        Image panelImage = subtitlePanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);

        // Create Subtitle Text
        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(subtitlePanel.transform);
        subtitleText = textGO.AddComponent<TextMeshProUGUI>();

        // Text positioning
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 10);
        textRect.offsetMax = new Vector2(-20, -10);

        // Set initial text properties
        UpdateTextAppearance();

        // Hide by default
        subtitlePanel.SetActive(false);
    }

    private void UpdateTextAppearance(SubtitleCharacter character = SubtitleCharacter.Mikey)
    {
        if (subtitleText != null)
        {
            subtitleText.fontSize = fontSize;
            subtitleText.font = fontAsset;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.enableWordWrapping = true;
            subtitleText.color = character == SubtitleCharacter.Mikey ? mikeyColor : sophieColor;
            subtitleText.outlineWidth = outlineWidth;
            subtitleText.outlineColor = outlineColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            CreateUIElements();
            StartCoroutine(PlaySubtitles());
            
            // Disable the collider so it can't trigger again
            GetComponent<Collider>().enabled = false;
        }
    }

    private IEnumerator PlaySubtitles()
    {
        subtitlePanel.SetActive(true);

        foreach (SubtitleLine line in subtitleLines)
        {
            if (line == null || string.IsNullOrEmpty(line.text)) continue;

            subtitleText.text = line.text;
            UpdateTextAppearance(line.character);

            if (line.audioClip != null)
            {
                // Play as 2D audio using our dedicated AudioSource
                audioSource.PlayOneShot(line.audioClip);
                yield return new WaitForSeconds(line.audioClip.length);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }
        }

        subtitlePanel.SetActive(false);
        
        // Optional: Destroy the UI after use
        Destroy(subtitlePanel.transform.parent.gameObject);
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (subtitleLines == null || subtitleLines.Length != 3)
        {
            System.Array.Resize(ref subtitleLines, 3);
        }
    }
    #endif
}