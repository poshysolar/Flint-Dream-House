using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneInteraction : MonoBehaviour
{
    [SerializeField] private AudioClip phoneAudioClip;
    [SerializeField] private Texture2D promptTexture;
    [SerializeField] private float maxPromptDistance = 5f;
    [SerializeField] [TextArea(3, 10)] private string subtitleTextContent = "Urgh. why aren't the doctors listening!...\nFlint doesn't need medicine, he needs to wake up & fast!";
    [SerializeField] private Color subtitleColor = Color.white;
    [SerializeField] private TMP_FontAsset subtitleFont;
    [SerializeField] private float subtitleFontSize = 24f;
    [SerializeField] private Color strokeColor = Color.black;
    [SerializeField] [Range(0f, 0.05f)] private float strokeThickness = 0.01f;

    private AudioSource audioSource;
    private GameObject subtitleTextObject;
    private TMP_Text subtitleText;
    private GameObject promptImageObject;
    private RawImage promptImage;
    private bool hasPlayed = false;
    private Camera mainCamera;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = phoneAudioClip;
        audioSource.playOnAwake = false;
        mainCamera = Camera.main;

        SetupSubtitleUI();
        SetupPromptUI();
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        bool isLookingAtPhone = Physics.Raycast(ray, out hit, maxPromptDistance) && hit.transform == transform;

        promptImageObject.SetActive(isLookingAtPhone && !hasPlayed);

        if (Input.GetMouseButtonDown(0) && isLookingAtPhone && !hasPlayed)
        {
            PlayAudioAndShowSubtitles();
        }

        subtitleTextObject.SetActive(audioSource.isPlaying);
    }

    void SetupSubtitleUI()
    {
        GameObject canvasObject = new GameObject("SubtitleCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        subtitleTextObject = new GameObject("SubtitleText");
        subtitleTextObject.transform.SetParent(canvasObject.transform, false);
        subtitleText = subtitleTextObject.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "Sophie The Cat: " + subtitleTextContent;
        subtitleText.font = subtitleFont != null ? subtitleFont : Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.color = subtitleColor;
        subtitleText.alignment = TextAlignmentOptions.Center;

        Material fontMaterial = Instantiate(subtitleText.font.material);
        fontMaterial.shaderKeywords = new string[] { "OUTLINE_ON" };
        subtitleText.fontMaterial = fontMaterial;
        subtitleText.fontMaterial.SetColor("_OutlineColor", strokeColor);
        subtitleText.fontMaterial.SetFloat("_Outline", strokeThickness);

        RectTransform textRect = subtitleText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0);
        textRect.anchorMax = new Vector2(0.5f, 0);
        textRect.pivot = new Vector2(0.5f, 0);
        textRect.anchoredPosition = new Vector2(0, 50);
        textRect.sizeDelta = new Vector2(1000, 200);

        subtitleTextObject.SetActive(false);
    }

    void SetupPromptUI()
    {
        GameObject canvasObject = new GameObject("PromptCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        promptImageObject = new GameObject("PromptImage");
        promptImageObject.transform.SetParent(canvasObject.transform, false);
        promptImage = promptImageObject.AddComponent<RawImage>();
        promptImage.texture = promptTexture;

        RectTransform imageRect = promptImage.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = new Vector2(100, 100);

        promptImageObject.SetActive(false);
    }

    void PlayAudioAndShowSubtitles()
    {
        if (phoneAudioClip != null && !hasPlayed)
        {
            audioSource.Play();
            hasPlayed = true;
        }
    }

    void OnValidate()
    {
        if (subtitleText != null && subtitleText.fontMaterial != null)
        {
            subtitleText.fontMaterial.SetColor("_OutlineColor", strokeColor);
            subtitleText.fontMaterial.SetFloat("_Outline", strokeThickness);
            subtitleText.color = subtitleColor;
            subtitleText.fontSize = subtitleFontSize;
            subtitleText.text = "Sophie The Cat: " + subtitleTextContent;

            if (subtitleFont != null)
            {
                subtitleText.font = subtitleFont;
                Material fontMaterial = Instantiate(subtitleFont.material);
                fontMaterial.shaderKeywords = new string[] { "OUTLINE_ON" };
                subtitleText.fontMaterial = fontMaterial;
                subtitleText.fontMaterial.SetColor("_OutlineColor", strokeColor);
                subtitleText.fontMaterial.SetFloat("_Outline", strokeThickness);
            }
        }
    }
}
