using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollableCircle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rollSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Prompt Settings")]
    [SerializeField] private Texture2D promptTexture;
    [SerializeField] private Vector2 promptSize = new Vector2(128, 128); // Increased size
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 2f, 0); // Increased Y offset to place it higher
    [SerializeField] private float promptDisplayDistance = 3f;

    private Rigidbody rb;
    private bool isGrounded;
    private Camera mainCamera;
    private bool isPlayerLooking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationY;
        
        rb.mass = 1f;
        rb.drag = 1f;
        rb.angularDrag = 0.5f;

        mainCamera = Camera.main;
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Check if player is looking at this object
        CheckIfPlayerIsLooking();
    }

    void CheckIfPlayerIsLooking()
    {
        if (mainCamera == null) return;

        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        float distanceToCamera = directionToCamera.magnitude;

        if (distanceToCamera <= promptDisplayDistance)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                isPlayerLooking = hit.collider.gameObject == gameObject;
            }
            else
            {
                isPlayerLooking = false;
            }
        }
        else
        {
            isPlayerLooking = false;
        }
    }

    void OnGUI()
    {
        if (isPlayerLooking && promptTexture != null)
        {
            // Get the position above the sphere
            Vector3 promptPosition = transform.position + promptOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(promptPosition);

            // Only draw if the object is in front of the camera
            if (screenPosition.z > 0)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(
                    new Rect(
                        screenPosition.x - (promptSize.x / 2),
                        Screen.height - screenPosition.y - (promptSize.y / 2),
                        promptSize.x,
                        promptSize.y
                    ),
                    promptTexture,
                    ScaleMode.StretchToFill,
                    true
                );
            }
        }
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (movement != Vector3.zero)
        {
            rb.AddForce(movement * rollSpeed, ForceMode.Force);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        
        // Add a gizmo to show where the prompt will appear
        if (promptTexture != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + promptOffset, 0.1f);
        }
    }
}