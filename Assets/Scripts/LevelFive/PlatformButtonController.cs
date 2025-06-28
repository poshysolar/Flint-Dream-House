using UnityEngine;
using System.Collections;

public class PlatformButtonController : MonoBehaviour
{
    [Header("UI Prompt Settings")]
    public Texture2D promptTexture;
    public float promptWidth = 100f;
    public float promptHeight = 100f;
    public float promptDisplayDistance = 5f;
    private bool showPrompt = false;
    public float promptOffsetY = 50f;

    [Header("Button Settings")]
    public bool isUpButton = true;
    private Material buttonMaterial;
    private Renderer buttonRenderer;
    private bool isActive = false;

    [Header("Platform Settings")]
    public Transform platform;
    public float moveSpeed = 2f;
    public float upPosition = 0.1f;
    private Vector3 downPosition = new Vector3(18.297f, -73.8f, 565.7806f);
    [Tooltip("Delay at top before descending")] 
    public float waitTimeAtTop = 3f;

    [Header("Audio Settings")]
    public AudioClip buttonClickSound;
    public AudioClip platformMoveSound;
    private AudioSource buttonAudioSource;
    private AudioSource platformAudioSource;

    private Coroutine movementCoroutine;
    private bool platformIsMoving = false;
    private Camera mainCamera;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        buttonMaterial = new Material(Shader.Find("Standard"));
        buttonRenderer.material = buttonMaterial;
        UpdateButtonColor();

        buttonAudioSource = gameObject.AddComponent<AudioSource>();
        buttonAudioSource.playOnAwake = false;
        buttonAudioSource.clip = buttonClickSound;

        platformAudioSource = platform.gameObject.AddComponent<AudioSource>();
        platformAudioSource.playOnAwake = false;
        platformAudioSource.clip = platformMoveSound;
        platformAudioSource.loop = true;

        mainCamera = Camera.main;
    }

    void Update()
    {
        CheckPlayerView();
    }

    void CheckPlayerView()
    {
        if (mainCamera == null) return;

        Vector3 directionToButton = transform.position - mainCamera.transform.position;
        float distanceToButton = directionToButton.magnitude;

        if (distanceToButton <= promptDisplayDistance)
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                showPrompt = hit.transform == transform;
            }
            else
            {
                showPrompt = false;
            }
        }
        else
        {
            showPrompt = false;
        }
    }

    void OnGUI()
    {
        if (showPrompt && promptTexture != null)
        {
            float xPos = (Screen.width - promptWidth) / 2;
            float yPos = (Screen.height - promptHeight) / 2 - promptOffsetY;

            GUI.DrawTexture(
                new Rect(xPos, yPos, promptWidth, promptHeight),
                promptTexture,
                ScaleMode.ScaleToFit
            );
        }
    }

    void OnMouseDown()
    {
        if (!platformIsMoving)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
            TogglePlatformMovement();
        }
    }

    void TogglePlatformMovement()
    {
        isActive = !isActive;
        UpdateButtonColor();

        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(MovePlatform());
    }

    void UpdateButtonColor()
    {
        Color baseColor = isActive ? Color.green : Color.red;
        buttonMaterial.color = baseColor;
        buttonMaterial.SetColor("_EmissionColor", baseColor * 0.8f);
        buttonMaterial.EnableKeyword("_EMISSION");
    }

    IEnumerator MovePlatform()
    {
        platformIsMoving = true;
        platformAudioSource.Play();
        
        Vector3 targetPosition;
        if (isActive)
        {
            targetPosition = isUpButton ? 
                new Vector3(platform.position.x, upPosition, platform.position.z) : 
                downPosition;
        }
        else
        {
            targetPosition = isUpButton ? 
                downPosition : 
                new Vector3(platform.position.x, upPosition, platform.position.z);
        }

        while (Vector3.Distance(platform.position, targetPosition) > 0.01f)
        {
            platform.position = Vector3.MoveTowards(
                platform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        platform.position = targetPosition;

        if (!isUpButton && platform.position.y >= upPosition)
        {
            platformAudioSource.Stop();
            yield return new WaitForSeconds(waitTimeAtTop);
            platformAudioSource.Play();
            
            while (Vector3.Distance(platform.position, downPosition) > 0.01f)
            {
                platform.position = Vector3.MoveTowards(
                    platform.position, 
                    downPosition, 
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            platform.position = downPosition;
            isActive = false;
            UpdateButtonColor();
        }

        platformAudioSource.Stop();
        platformIsMoving = false;
    }
}