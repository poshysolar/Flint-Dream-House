using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.0f; // Editable in Inspector
    [SerializeField] private GameObject player; // Drag player GameObject here in Inspector
    private Image fadeImage;
    private MonoBehaviour playerScript; // Reference to the player's script

    private void Start()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Ensure it's on top

        // Add CanvasScaler for proper scaling
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.offsetMin = Vector2.zero;
        fadeImage.rectTransform.offsetMax = Vector2.zero;

        // Get player's script component
        if (player != null)
        {
            playerScript = player.GetComponent<MonoBehaviour>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            StartCoroutine(FadeAndLoadLevel());
        }
    }

    private IEnumerator FadeAndLoadLevel()
    {
        // Pause player movement
        if (playerScript != null)
        {
            // Try setting a 'canMove' boolean if it exists
            var canMoveField = playerScript.GetType().GetField("canMove");
            if (canMoveField != null)
            {
                canMoveField.SetValue(playerScript, false);
            }
            // Alternatively, try calling a 'SetMovement' method if it exists
            var setMovementMethod = playerScript.GetType().GetMethod("SetMovement");
            if (setMovementMethod != null)
            {
                setMovementMethod.Invoke(playerScript, new object[] { false });
            }
        }

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));
        
        // Load LevelSeven
        SceneManager.LoadScene("LevelSeven");
        
        // Fade from black in the new scene
        yield return StartCoroutine(Fade(1f, 0f));

        // Re-enable player movement
        if (playerScript != null)
        {
            var canMoveField = playerScript.GetType().GetField("canMove");
            if (canMoveField != null)
            {
                canMoveField.SetValue(playerScript, true);
            }
            var setMovementMethod = playerScript.GetType().GetMethod("SetMovement");
            if (setMovementMethod != null)
            {
                setMovementMethod.Invoke(playerScript, new object[] { true });
            }
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color imageColor = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha); // Black color
            yield return null;
        }

        // Ensure final alpha is set
        fadeImage.color = new Color(0, 0, 0, endAlpha);
    }
}