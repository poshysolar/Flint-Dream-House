using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CreditsScene : MonoBehaviour
{
    [Header("Credits Appearance")]
    [TextArea(5, 30)]
    public string creditsText = 
        "All level 3D designs were under the free and open-source license.\n\n" +
        "Most of the 3D designs were created by Icy Solar\n\n" +
        "Coder/Programmer: Icy Solar\n\n" +
        "Audio: Youtube/Freesounds.org\n\n" +
        "Voice Acting: Icy Solar\n\n" +
        "Huge shoutout to all of the wonderful Youtubers who will play this game!";
    
    public float scrollSpeed = 50f;
    public Color textColor = Color.white;
    public TMP_FontAsset fontAsset;
    public int fontSize = 32;

    [Header("Background Audio")]
    public AudioClip backgroundAudio;

    private RectTransform textTransform;
    private float screenHeight;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Get screen height for positioning calculations
        screenHeight = Screen.height;

        // Canvas setup
        var canvasGO = new GameObject("CreditsCanvas", typeof(Canvas));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Add EventSystem for UI interaction
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Background
        var bgGO = new GameObject("Background", typeof(Image));
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bg = bgGO.GetComponent<Image>();
        bg.color = Color.black;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Credits Text
        var textGO = new GameObject("CreditsText", typeof(TextMeshProUGUI));
        textGO.transform.SetParent(canvasGO.transform, false);
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = creditsText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = textColor;
        tmp.fontSize = fontSize;
        tmp.enableWordWrapping = true;
        if (fontAsset) tmp.font = fontAsset;

        textTransform = tmp.rectTransform;
        textTransform.sizeDelta = new Vector2(800, 2000);
        textTransform.anchorMin = new Vector2(0.5f, 0);
        textTransform.anchorMax = new Vector2(0.5f, 0);
        textTransform.pivot = new Vector2(0.5f, 0);
        textTransform.anchoredPosition = new Vector2(0, -textTransform.sizeDelta.y + screenHeight * 0.02f);

        // Background Audio Setup
        if (backgroundAudio != null)
        {
            var audioGO = new GameObject("BackgroundAudio", typeof(AudioSource));
            var audioSource = audioGO.GetComponent<AudioSource>();
            audioSource.clip = backgroundAudio;
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.Play();
        }

        CreateBackButton(canvasGO.transform);
    }

    void Update()
    {
        if (textTransform != null)
        {
            textTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.unscaledDeltaTime);

            if (textTransform.anchoredPosition.y > textTransform.sizeDelta.y + screenHeight)
            {
                textTransform.anchoredPosition = new Vector2(0, textTransform.anchoredPosition.y);
            }
        }
    }

    void CreateBackButton(Transform parent)
    {
        GameObject buttonGO = new GameObject("BackButton", typeof(RectTransform), typeof(Button), typeof(Image));
        buttonGO.transform.SetParent(parent, false);
        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(140, 50);

        var img = buttonGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        img.raycastTarget = true;

        GameObject txtGO = new GameObject("ButtonText", typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(buttonGO.transform, false);
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Back";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var txtRect = tmp.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;

        var btn = buttonGO.GetComponent<Button>();
        btn.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        btn.navigation = new Navigation { mode = Navigation.Mode.Automatic };
    }
}