using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DynamicPauseMenu : MonoBehaviour
{
    private GameObject canvasObject;
    private GameObject pausePanel;
    private bool isPaused = false;
    private bool canTogglePause = true; // New variable to track if player can toggle pause

    public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
    public Color buttonColor = Color.white;
    public Color textColor = Color.black;
    public Font buttonFont;
    public int fontSize = 24;

    void Start()
    {
        CreatePauseMenuUI();
        pausePanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused && canTogglePause)
        {
            PauseGame();
        }
    }

    void CreatePauseMenuUI()
    {
        canvasObject = new GameObject("PauseCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Add EventSystem if it doesn't exist in the scene
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasObject.transform, false);

        Image panelImage = pausePanel.AddComponent<Image>();
        panelImage.color = panelColor;

        RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateButton("Resume", new Vector2(0, 60), ResumeGame);
        CreateButton("Quit", new Vector2(0, -60), QuitToMainMenu);
    }

    void CreateButton(string label, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        GameObject buttonObj = new GameObject(label + "Button");
        buttonObj.transform.SetParent(pausePanel.transform, false);

        Button button = buttonObj.AddComponent<Button>();
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;

        // Add a ColorBlock to make the button visually respond to interactions
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        button.colors = colors;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 70); // Increased size
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = rect.sizeDelta;
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;

        button.onClick.AddListener(callback);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pausePanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        canTogglePause = true; // Allow toggling pause again after resume button is clicked
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        pausePanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        canTogglePause = false; // Prevent toggling pause until resume button is clicked
    }
}