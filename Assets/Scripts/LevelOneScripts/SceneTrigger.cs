using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTrigger : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.5f; // Duration of the fade effect

    private Image fadeOverlay;
    private Canvas fadeCanvas;
    private bool isFading = false;

    private void Start()
    {
        // Create a full-screen Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // Ensure it's on top
        canvasGO.AddComponent<CanvasGroup>();

        // Create the fade overlay
        GameObject imageGO = new GameObject("FadeOverlay");
        imageGO.transform.parent = canvasGO.transform;
        fadeOverlay = imageGO.AddComponent<Image>();

        // Configure the black, full-screen image
        RectTransform rect = imageGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeOverlay.color = new Color(0, 0, 0, 0);
        fadeOverlay.raycastTarget = false; // Don’t block interactions
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFading)
        {
            StartCoroutine(FadeAndLoadScene("LevelTwo"));
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isFading = true;

        float elapsedTime = 0f;
        Color color = fadeOverlay.color;
        color.a = 0f;

        // Fade in to black
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeOverlay.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
