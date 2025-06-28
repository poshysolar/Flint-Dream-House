using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SlidingDoorSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InteractionSystem interactionSystem;
    [SerializeField] private GameObject slidingDoor;
    [SerializeField] private GameObject cylinderButton;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Texture2D promptTexture;

    [Header("Door Settings")]
    [SerializeField] private float doorSlideSpeed = 5f;
    private Vector3 doorStartPos = new Vector3(35.8095f, 0f, 0f);
    private Vector3 doorEndPos = new Vector3(53.04f, 0f, 0f);
    private bool isDoorOpen = false;

    [Header("Button Settings")]
    [SerializeField] private float maxPromptDistance = 5f;
    [SerializeField] private AudioClip buttonSoundEffect;
    [SerializeField] private float promptOffset = 1f;
    [SerializeField] private Vector2 promptSize = new Vector2(100f, 100f);
    private Material redGlowMaterial;
    private Material greenGlowMaterial;
    private Renderer buttonRenderer;
    private AudioSource buttonAudioSource;

    [Header("Prompt UI Settings")]
    private GameObject promptObject;
    private Image promptImage;
    private GameObject promptCanvasObject;

    [Header("Locked Text UI Settings")]
    [SerializeField] private TMP_FontAsset lockedTextFont;
    [SerializeField] private int lockedTextFontSize = 36;
    [SerializeField] private Color lockedTextColor = Color.white;
    [SerializeField] private Color lockedTextOutlineColor = Color.black;
    [SerializeField] private float lockedTextOutlineThickness = 0.015f;
    [SerializeField] private string lockedTextMessage = "Locked Door";
    [SerializeField] private float lockedTextDuration = 2.0f;
    private TextMeshProUGUI lockedText;
    private GameObject lockedTextObject;
    private GameObject lockedTextCanvasObject;
    private bool isLookingAtButton = false;

    private void Start()
    {
        if (slidingDoor != null)
        {
            doorStartPos = new Vector3(35.8095f, slidingDoor.transform.position.y, slidingDoor.transform.position.z);
            doorEndPos = new Vector3(53.04f, slidingDoor.transform.position.y, slidingDoor.transform.position.z);
            slidingDoor.transform.position = doorStartPos;
        }

        if (cylinderButton != null)
        {
            buttonRenderer = cylinderButton.GetComponent<Renderer>();
            buttonAudioSource = cylinderButton.AddComponent<AudioSource>();
            buttonAudioSource.playOnAwake = false;
            buttonAudioSource.clip = buttonSoundEffect;
            CreateGlowMaterials();
            buttonRenderer.material = redGlowMaterial;
        }

        CreatePromptUI();
        CreateLockedTextUI();
    }

    private void CreateGlowMaterials()
    {
        redGlowMaterial = new Material(Shader.Find("Standard"));
        redGlowMaterial.color = Color.red;
        redGlowMaterial.SetColor("_EmissionColor", Color.red * 2f);
        redGlowMaterial.EnableKeyword("_EMISSION");

        greenGlowMaterial = new Material(Shader.Find("Standard"));
        greenGlowMaterial.color = Color.green;
        greenGlowMaterial.SetColor("_EmissionColor", Color.green * 2f);
        greenGlowMaterial.EnableKeyword("_EMISSION");
    }

    private void CreatePromptUI()
    {
        promptCanvasObject = new GameObject("DoorPromptCanvas");
        Canvas canvas = promptCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        promptCanvasObject.AddComponent<CanvasScaler>();
        promptCanvasObject.AddComponent<BillboardCanvas>();

        if (cylinderButton != null)
        {
            promptCanvasObject.transform.position = cylinderButton.transform.position + Vector3.up * promptOffset;
        }

        promptCanvasObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        promptObject = new GameObject("PromptImage");
        promptObject.transform.SetParent(promptCanvasObject.transform, false);
        promptImage = promptObject.AddComponent<Image>();

        if (promptTexture != null)
        {
            promptImage.sprite = Sprite.Create(promptTexture, new Rect(0, 0, promptTexture.width, promptTexture.height), new Vector2(0.5f, 0.5f));
        }

        RectTransform promptRect = promptImage.GetComponent<RectTransform>();
        promptRect.sizeDelta = promptSize;
        promptRect.anchoredPosition = Vector2.zero;

        promptObject.SetActive(false);
    }

    private void CreateLockedTextUI()
    {
        lockedTextCanvasObject = new GameObject("LockedTextCanvas");
        Canvas canvas = lockedTextCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = lockedTextCanvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        lockedTextCanvasObject.AddComponent<GraphicRaycaster>();

        lockedTextObject = new GameObject("LockedText");
        lockedTextObject.transform.SetParent(lockedTextCanvasObject.transform, false);

        lockedText = lockedTextObject.AddComponent<TextMeshProUGUI>();
        lockedText.font = lockedTextFont;
        lockedText.fontSize = lockedTextFontSize;
        lockedText.color = lockedTextColor;
        lockedText.alignment = TextAlignmentOptions.Center;
        lockedText.text = lockedTextMessage;

        lockedText.outlineWidth = lockedTextOutlineThickness;
        lockedText.outlineColor = lockedTextOutlineColor;

        RectTransform textRect = lockedText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(400, 100);

        lockedTextObject.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxPromptDistance))
        {
            if (hit.transform.gameObject == cylinderButton)
            {
                isLookingAtButton = true;
                promptObject.SetActive(true);

                if (Input.GetMouseButtonDown(0))
                {
                    // Check if the InteractionSystem has been triggered
                    if (interactionSystem != null && !interactionSystem.hasCutscenePlayed)
                    {
                        // Door is still locked - show locked message and play sound
                        StartCoroutine(ShowLockedText());
                        if (buttonAudioSource != null && buttonSoundEffect != null)
                        {
                            buttonAudioSource.PlayOneShot(buttonSoundEffect);
                        }
                    }
                    else
                    {
                        // Door can now be opened
                        StartCoroutine(OpenDoor());
                    }
                }
            }
            else
            {
                isLookingAtButton = false;
                promptObject.SetActive(false);
            }
        }
        else
        {
            isLookingAtButton = false;
            promptObject.SetActive(false);
        }

        // Update prompt rotation to face camera
        if (isLookingAtButton && promptCanvasObject != null)
        {
            promptCanvasObject.transform.LookAt(playerCamera.transform);
            promptCanvasObject.transform.Rotate(0, 180, 0);
        }
    }

    private IEnumerator ShowLockedText()
    {
        if (lockedTextObject != null)
        {
            lockedTextObject.SetActive(true);
            
            if (lockedText != null)
            {
                lockedText.text = lockedTextMessage;
                lockedText.enabled = true;
            }
            
            yield return new WaitForSeconds(lockedTextDuration);
            
            if (lockedTextObject != null)
            {
                lockedTextObject.SetActive(false);
            }
        }
    }

    private IEnumerator OpenDoor()
    {
        if (isDoorOpen || slidingDoor == null) yield break;

        isDoorOpen = true;
        promptObject.SetActive(false);

        if (buttonAudioSource != null && buttonSoundEffect != null)
        {
            buttonAudioSource.PlayOneShot(buttonSoundEffect);
        }

        if (buttonRenderer != null)
        {
            buttonRenderer.material = greenGlowMaterial;
        }

        float t = 0;
        Vector3 startPos = slidingDoor.transform.position;
        while (t < 1)
        {
            t += Time.deltaTime * doorSlideSpeed;
            slidingDoor.transform.position = Vector3.Lerp(startPos, doorEndPos, t);
            yield return null;
        }
        slidingDoor.transform.position = doorEndPos;
    }
}

public class BillboardCanvas : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }
    }
}