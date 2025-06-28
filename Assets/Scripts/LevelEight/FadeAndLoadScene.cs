using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeAndLoadScene : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    public string sceneToLoad = "Level Nine";

    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool hasFaded = false;

    void Start()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Fade Image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f); // Transparent black

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasFaded && other.CompareTag("Player"))
        {
            hasFaded = true;
            StartCoroutine(FadeOutAndLoad());
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 1f);
        SceneManager.LoadScene(sceneToLoad);
    }
}
