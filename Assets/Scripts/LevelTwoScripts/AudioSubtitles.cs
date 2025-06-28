using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioSubtitles : MonoBehaviour
{
    public AudioClip subtitleAudioClip;
    private AudioSource audioSource;

    private GameObject player;

    private Text subtitleText;
    private GameObject canvas;
    private GameObject textObject;

    [Header("Subtitle Settings")]
    public float fontSize = 24f;
    public Color textColor = Color.white;
    public Font textFont;
    public Color strokeColor = Color.black;
    public float strokeWidth = 0.5f;

    private bool forcingCursorVisible = false;

    private GameObject visualCursor;
    private Image cursorImage;

    private readonly Subtitle[] subtitles = new Subtitle[]
    {
        new Subtitle("Sophie The Cat: Well you have found Andy's room", 0f, 3f),
        new Subtitle("Sophie The Cat: He is was the first monster you created!", 3f, 6f),
        new Subtitle("Sophie The Cat: Any way have fun looking around", 6f, 9f)
    };

    private float timer = 0f;
    private bool isAudioPlaying = false;

    private struct Subtitle
    {
        public string text;
        public float startTime;
        public float endTime;

        public Subtitle(string text, float startTime, float endTime)
        {
            this.text = text;
            this.startTime = startTime;
            this.endTime = endTime;
        }
    }

    private void CreateVisualCursor()
    {
        visualCursor = new GameObject("VisualCursor");
        visualCursor.transform.SetParent(canvas.transform, false);
        
        cursorImage = visualCursor.AddComponent<Image>();
        
        Texture2D cursorTexture = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (distance <= 8)
                    cursorTexture.SetPixel(x, y, Color.white);
                else if (distance <= 10)
                    cursorTexture.SetPixel(x, y, Color.black);
                else
                    cursorTexture.SetPixel(x, y, Color.clear);
            }
        }
        cursorTexture.Apply();
        
        cursorImage.sprite = Sprite.Create(cursorTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        cursorImage.rectTransform.sizeDelta = new Vector2(32, 32);
        
        Canvas cursorCanvas = visualCursor.AddComponent<Canvas>();
        cursorCanvas.overrideSorting = true;
        cursorCanvas.sortingOrder = 9999;
    }

    private void ForceCursorVisible()
    {
        forcingCursorVisible = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        for (int i = 0; i < 3; i++)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HideCursor()
    {
        forcingCursorVisible = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (visualCursor != null)
        {
            visualCursor.SetActive(false);
        }
    }

    void Start()
    {
        canvas = new GameObject("SubtitleCanvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 1000;
        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        CreateVisualCursor();
        ForceCursorVisible();
        StartCoroutine(EnsureCursorVisible());

        player = GameObject.Find("FPSHorrorPlayer2");

        textObject = new GameObject("SubtitleText");
        textObject.transform.SetParent(canvas.transform, false);
        subtitleText = textObject.AddComponent<Text>();

        subtitleText.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        subtitleText.fontSize = Mathf.RoundToInt(fontSize);
        subtitleText.color = textColor;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.rectTransform.sizeDelta = new Vector2(800, 200);
        subtitleText.rectTransform.anchoredPosition = new Vector2(0, -150);

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = strokeColor;
        outline.effectDistance = new Vector2(strokeWidth, strokeWidth);

        subtitleText.text = "";

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = subtitleAudioClip;
        audioSource.playOnAwake = false;

        if (subtitleAudioClip != null)
        {
            audioSource.Play();
            isAudioPlaying = true;
            FPSHorrorPlayer2.allowMovement = false;
        }
    }

    private IEnumerator EnsureCursorVisible()
    {
        while (forcingCursorVisible && isAudioPlaying)
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            
            if (visualCursor != null && cursorImage != null)
            {
                Vector2 screenPos = Input.mousePosition;
                cursorImage.rectTransform.position = screenPos;
                visualCursor.SetActive(true);
            }
            
            yield return new WaitForSeconds(0.02f);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            if (forcingCursorVisible)
            {
                ForceCursorVisible();
            }
        }

        if (forcingCursorVisible && (!Cursor.visible || Cursor.lockState != CursorLockMode.None))
        {
            ForceCursorVisible();
        }

        if (forcingCursorVisible && visualCursor != null && cursorImage != null)
        {
            cursorImage.rectTransform.position = Input.mousePosition;
        }

        if (!isAudioPlaying) return;

        timer += Time.deltaTime;

        bool subtitleDisplayed = false;
        foreach (Subtitle subtitle in subtitles)
        {
            if (timer >= subtitle.startTime && timer <= subtitle.endTime)
            {
                subtitleText.text = subtitle.text;
                subtitleDisplayed = true;
                break;
            }
        }

        if (!subtitleDisplayed)
        {
            subtitleText.text = "";
        }

        if (!audioSource.isPlaying && isAudioPlaying)
        {
            subtitleText.text = "";
            isAudioPlaying = false;
            Destroy(canvas, 1f);

            HideCursor();
            FPSHorrorPlayer2.allowMovement = true;
        }
    }

    void OnDestroy()
    {
        if (canvas != null)
        {
            Destroy(canvas);
        }

        forcingCursorVisible = false;
        HideCursor();
        FPSHorrorPlayer2.allowMovement = true;
    }
}