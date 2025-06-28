using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private float holdDistance = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private LayerMask grabbableLayer;
    
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 10f;

    [Header("UI Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color textOutlineColor = Color.black;
    [SerializeField] private float textOutlineWidth = 0.2f;
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Vector2 textPosition = new Vector2(50f, 200f);

    [Header("Prompt Settings")]
    [SerializeField] private Texture2D promptTexture;
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector2 promptSize = new Vector2(64, 64);

    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    private Camera mainCamera;
    private TextMeshProUGUI controlsText;
    private GameObject uiCanvas;
    private Vector3? promptPosition = null;

    void Start()
    {
        mainCamera = Camera.main;
        SetupUI();
    }

    void SetupUI()
    {
        // Create Canvas
        uiCanvas = new GameObject("GrabDropCanvas");
        Canvas canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = uiCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        uiCanvas.AddComponent<GraphicRaycaster>();

        // Create Controls Text
        GameObject textObj = new GameObject("ControlsText");
        textObj.transform.SetParent(uiCanvas.transform, false);
        
        controlsText = textObj.AddComponent<TextMeshProUGUI>();
        controlsText.text = "Q: To Throw\nE: To Pick Up";
        controlsText.font = customFont;
        controlsText.color = textColor;
        controlsText.fontSize = fontSize;
        
        // Add outline/stroke to text
        controlsText.enableVertexGradient = false;
        controlsText.outlineWidth = textOutlineWidth;
        controlsText.outlineColor = textOutlineColor;

        RectTransform rectTransform = controlsText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0, 0);
        rectTransform.anchoredPosition = textPosition;
        rectTransform.sizeDelta = new Vector2(300, 100);

        controlsText.enabled = false;
    }

    void Update()
    {
        CheckForGrabbableObject();

        if (Input.GetKeyDown(KeyCode.E) && heldObject == null)
        {
            TryGrabObject();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && heldObject != null)
        {
            DropObject();
        }

        if (heldObject != null)
        {
            HoldObject();
        }
    }

    void CheckForGrabbableObject()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer) && heldObject == null)
        {
            Debug.DrawRay(ray.origin, ray.direction * grabDistance, Color.yellow);
            promptPosition = hit.point + promptOffset;
            Debug.Log("Found grabbable object: " + hit.collider.gameObject.name);
        }
        else
        {
            promptPosition = null;
        }
    }

    void OnGUI()
    {
        if (promptPosition.HasValue && promptTexture != null && heldObject == null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(promptPosition.Value);
            if (screenPos.z > 0)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(
                    new Rect(screenPos.x - (promptSize.x / 2),
                    Screen.height - screenPos.y - (promptSize.y / 2),
                    promptSize.x,
                    promptSize.y),
                    promptTexture,
                    ScaleMode.StretchToFill,
                    true
                );
            }
        }
    }

    void TryGrabObject()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        {
            heldObject = hit.collider.gameObject;
            heldRigidbody = heldObject.GetComponent<Rigidbody>();
            
            if (heldRigidbody != null)
            {
                heldRigidbody.useGravity = false;
                heldRigidbody.freezeRotation = true;
                heldRigidbody.velocity = Vector3.zero;
                heldRigidbody.angularVelocity = Vector3.zero;
                
                controlsText.enabled = true;
                promptPosition = null;
            }
        }
    }

    void HoldObject()
    {
        Vector3 holdPosition = mainCamera.transform.position + mainCamera.transform.forward * holdDistance;
        
        if (heldRigidbody != null)
        {
            Vector3 moveDirection = (holdPosition - heldObject.transform.position);
            heldRigidbody.velocity = moveDirection * smoothSpeed;
        }
    }

    void DropObject()
    {
        if (heldRigidbody != null)
        {
            heldRigidbody.useGravity = true;
            heldRigidbody.freezeRotation = false;
            heldRigidbody.AddForce(mainCamera.transform.forward * throwForce, ForceMode.Impulse);
        }
        
        controlsText.enabled = false;
        
        heldObject = null;
        heldRigidbody = null;
    }

    void OnValidate()
    {
        if (controlsText != null)
        {
            controlsText.font = customFont;
            controlsText.color = textColor;
            controlsText.fontSize = fontSize;
            controlsText.rectTransform.anchoredPosition = textPosition;
            controlsText.outlineWidth = textOutlineWidth;
            controlsText.outlineColor = textOutlineColor;
        }
    }

    void OnDestroy()
    {
        if (uiCanvas != null)
        {
            Destroy(uiCanvas);
        }
    }
}