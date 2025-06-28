using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EightSequence : MonoBehaviour
{
    [SerializeField] private MonoBehaviour playerScript; // Drag your player movement script here
    [SerializeField] private Camera camera2; // Camera2 for the first sequence
    [SerializeField] private Camera camera3; // Camera3 for Mikey's sequence
    [SerializeField] private Camera camera4; // Camera4 for the final sequence
    [SerializeField] private Camera playerCamera; // Player's main camera
    [SerializeField] private GameObject mikeyPrefab; // Mikey's 3D model prefab
    [SerializeField] private AudioClip audio1; // Sophie audio
    [SerializeField] private AudioClip audio2; // Mikey audio 1
    [SerializeField] private AudioClip audio3; // Mikey audio 2
    [SerializeField] private float textStrokeWidth = 0.5f; // Stroke width for subtitle text
    [SerializeField] private float cameraMoveSpeed = 5f; // Speed for camera movement, editable in Inspector
    [SerializeField] private Font subtitleFont; // Custom font for subtitles, assignable in Inspector
    [SerializeField] private GeneratorRepairSequence generatorRepairScript; // Reference to the generator script
    private float cameraRotateSpeed = 50f; // Speed for camera rotation

    private AudioSource audioSource;
    private Text subtitleText;
    private GameObject mikeyInstance;
    private Canvas canvas;

    // Public getters for GeneratorRepairSequence
    public Camera Camera2 => camera2;
    public Camera Camera3 => camera3;
    public Camera Camera4 => camera4;
    public Camera PlayerCamera => playerCamera;
    public GameObject MikeyPrefab => mikeyPrefab;
    public Text SubtitleText => subtitleText; // Expose subtitle text for reuse

    private void Start()
    {
        // Pause player movement
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Create UI for subtitles
        SetupSubtitleUI();

        // Get AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();

        // Start the sequence
        StartCoroutine(PlaySequence());
    }

    private void SetupSubtitleUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("SubtitleCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Text UI
        GameObject textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(canvasObj.transform, false);
        subtitleText = textObj.AddComponent<Text>();
        subtitleText.font = subtitleFont != null ? subtitleFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitleText.fontSize = 28;
        subtitleText.fontStyle = FontStyle.Bold;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.rectTransform.sizeDelta = new Vector2(900, 100);
        
        // Position the text at the bottom of the screen with proper spacing
        subtitleText.rectTransform.anchorMin = new Vector2(0.5f, 0);
        subtitleText.rectTransform.anchorMax = new Vector2(0.5f, 0);
        subtitleText.rectTransform.pivot = new Vector2(0.5f, 0);
        subtitleText.rectTransform.anchoredPosition = new Vector2(0, 100); // 100 pixels from bottom

        // Add Outline component for text stroke with improved settings
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(textStrokeWidth, textStrokeWidth);
        
        // Add Shadow component for better readability
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private IEnumerator PlaySequence()
    {
        // Disable player camera, enable Camera2
        playerCamera.enabled = false;
        camera2.enabled = true;
        camera3.enabled = false;
        camera4.enabled = false;

        // Move Camera2 to initial position and set Y rotation to -90
        camera2.transform.position = new Vector3(58.11f, 3.533f, 0.05999988f);
        camera2.transform.rotation = Quaternion.Euler(0, -90f, 0);

        // Move Camera2 X position to -17.17
        Vector3 targetPos1 = new Vector3(-17.17f, camera2.transform.position.y, camera2.transform.position.z);
        while (Vector3.Distance(camera2.transform.position, targetPos1) > 0.01f)
        {
            camera2.transform.position = Vector3.MoveTowards(camera2.transform.position, targetPos1, cameraMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Rotate Camera2 to Y: -180.672
        Quaternion targetRot = Quaternion.Euler(0, -180.672f, 0);
        while (Quaternion.Angle(camera2.transform.rotation, targetRot) > 0.01f)
        {
            camera2.transform.rotation = Quaternion.RotateTowards(camera2.transform.rotation, targetRot, cameraRotateSpeed * Time.deltaTime);
            yield return null;
        }

        // Move Camera2 Z position to -32.35
        Vector3 targetPos2 = new Vector3(camera2.transform.position.x, camera2.transform.position.y, -32.35f);
        while (Vector3.Distance(camera2.transform.position, targetPos2) > 0.01f)
        {
            camera2.transform.position = Vector3.MoveTowards(camera2.transform.position, targetPos2, cameraMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Play Audio1 and show subtitles
        subtitleText.color = new Color(0.8f, 0.4f, 0.9f); // Sophie's subtitles are purple (brighter)
        subtitleText.text = "Sophie The Cat: Flint! please find a way to unlock the cage";
        audioSource.clip = audio1;
        audioSource.Play();
        yield return new WaitForSeconds(audio1.length);

        // Switch to Camera3 and spawn Mikey
        camera2.enabled = false;
        camera3.enabled = true;
        
        // Set Mikey as speaking to show talking model
        GeneratorRepairSequence.isMikeySpeaking = true;
        
        mikeyInstance = Instantiate(mikeyPrefab, new Vector3(-0.7f, -0.21f, -29.82f), Quaternion.identity);
        Animator mikeyAnimator = mikeyInstance.GetComponent<Animator>();
        if (mikeyAnimator != null)
        {
            mikeyAnimator.Play("BTalking");
        }

        // Play Audio2 and show subtitles
        subtitleText.color = new Color(1f, 0.7f, 0.2f); // Mikey's subtitles are orange (brighter)
        subtitleText.text = "Mikey: Let's see if you can solve this next puzzle";
        audioSource.clip = audio2;
        audioSource.Play();
        yield return new WaitForSeconds(audio2.length);

        // Play Audio3 and show subtitles
        subtitleText.text = "Mikey: In order to save her you must complete the generators";
        audioSource.clip = audio3;
        audioSource.Play();
        yield return new WaitForSeconds(audio3.length);

        // Show objective UI after Mikey finishes speaking about generators
        if (generatorRepairScript != null)
        {
            generatorRepairScript.ShowObjectiveUI();
        }
        
        // Set Mikey as not speaking to hide talking model
        GeneratorRepairSequence.isMikeySpeaking = false;

        // Hide Mikey instance after audio3 finishes
        if (mikeyInstance != null)
        {
            mikeyInstance.SetActive(false);
            Debug.Log("Mikey instance hidden after audio3");
        }
        else
        {
            Debug.LogWarning("MikeyInstance is null, cannot hide Mikey");
        }

        // Switch to Camera4
        camera3.enabled = false;
        camera4.enabled = true;

        // Stay on Camera4 for 5 seconds
        yield return new WaitForSeconds(5f);

        // Switch back to player camera
        camera4.enabled = false;
        playerCamera.enabled = true;
        subtitleText.text = "";

        // Unpause player movement
        if (playerScript != null)
        {
            playerScript.enabled = true;
        }

        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public float TextStrokeWidth
    {
        get => textStrokeWidth;
        set
        {
            textStrokeWidth = value;
            if (subtitleText != null)
            {
                Outline outline = subtitleText.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectDistance = new Vector2(textStrokeWidth, textStrokeWidth);
                }
            }
        }
    }

    public float CameraMoveSpeed
    {
        get => cameraMoveSpeed;
        set => cameraMoveSpeed = value;
    }
}