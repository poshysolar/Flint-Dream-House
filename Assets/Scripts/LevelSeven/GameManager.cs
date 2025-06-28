using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Setup")]
    [Tooltip("Assign the 4 cubes in order: Cube1 (Red), Cube2 (Blue), Cube3 (Yellow), Cube4 (Green)")]
    [SerializeField] private GameObject[] colorCubes; // Array of 4 cubes

    [Tooltip("Assign the door GameObject that will be deleted after completing all levels")]
    [SerializeField] private GameObject doorToUnlock; // Door to unlock/delete

    [Tooltip("Assign the empty GameObject with a collider set as a trigger to start the game")]
    [SerializeField] private GameObject triggerZone; // Trigger to start the game

    [Header("UI Settings")]
    [Tooltip("The color of the UI text")]
    [SerializeField] private Color textColor = Color.white;
    
    [Tooltip("The font size of the UI text")]
    [SerializeField] private int fontSize = 50;
    
    [Tooltip("Optional: Custom font for UI text (leave empty for default)")]
    [SerializeField] private Font customFont;

    [Tooltip("Width of the text area")]
    [SerializeField] private float textWidth = 500f;
    
    [Tooltip("Height of the text area")]
    [SerializeField] private float textHeight = 100f;
    
    [Tooltip("Text outline/stroke size")]
    [SerializeField] private float textStrokeSize = 1f;
    
    [Tooltip("Text outline/stroke color")]
    [SerializeField] private Color textStrokeColor = Color.black;

    private Material[] colorMaterials; // Materials for each cube (red, blue, yellow, green)
    private Material defaultMaterial;  // Material for hidden state (white)
    private List<int> sequence;        // Current sequence of cube indices
    private int currentLevel = 0;      // Current level (1 to 4)
    private int currentStep = 0;       // Current step in player's input
    private bool isShowingSequence = false; // Is the sequence being displayed?
    private bool isWaitingForInput = false; // Is the game waiting for player input?
    private bool gameStarted = false;   // Has the game started?
    private Text uiText;               // UI Text for messages

    void Start()
    {
        // Create materials programmatically
        CreateMaterials();

        // Create UI Canvas and Text programmatically
        SetupUI();

        // Set all cubes to default (white) material
        foreach (var cube in colorCubes)
        {
            if (cube != null && cube.GetComponent<MeshRenderer>() != null)
            {
                cube.GetComponent<MeshRenderer>().material = defaultMaterial;
            }
        }

        // Make sure trigger has a collider set as trigger
        if (triggerZone != null)
        {
            Collider triggerCollider = triggerZone.GetComponent<Collider>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;
            }
        }
    }

    void CreateMaterials()
    {
        // Initialize the materials array
        colorMaterials = new Material[4];

        // Create White Material (no emission)
        defaultMaterial = new Material(Shader.Find("Standard"));
        defaultMaterial.color = Color.white;

        // Create Red Material (Glowing)
        colorMaterials[0] = new Material(Shader.Find("Standard"));
        colorMaterials[0].color = Color.red;
        colorMaterials[0].SetColor("_EmissionColor", Color.red * 2f); // Increased intensity
        colorMaterials[0].EnableKeyword("_EMISSION");

        // Create Blue Material (Glowing)
        colorMaterials[1] = new Material(Shader.Find("Standard"));
        colorMaterials[1].color = Color.blue;
        colorMaterials[1].SetColor("_EmissionColor", Color.blue * 2f); // Increased intensity
        colorMaterials[1].EnableKeyword("_EMISSION");

        // Create Yellow Material (Glowing)
        colorMaterials[2] = new Material(Shader.Find("Standard"));
        colorMaterials[2].color = Color.yellow;
        colorMaterials[2].SetColor("_EmissionColor", Color.yellow * 2f); // Increased intensity
        colorMaterials[2].EnableKeyword("_EMISSION");

        // Create Green Material (Glowing)
        colorMaterials[3] = new Material(Shader.Find("Standard"));
        colorMaterials[3].color = Color.green;
        colorMaterials[3].SetColor("_EmissionColor", Color.green * 2f); // Increased intensity
        colorMaterials[3].EnableKeyword("_EMISSION");
    }

    void SetupUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Text
        GameObject textObj = new GameObject("GameUIText");
        textObj.transform.SetParent(canvasObj.transform, false);
        uiText = textObj.AddComponent<Text>();
        
        // Apply custom font if provided, otherwise use default
        if (customFont != null)
        {
            uiText.font = customFont;
        }
        else
        {
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        // Apply inspector settings
        uiText.fontSize = fontSize;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = textColor;

        // Add outline/stroke component
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = textStrokeColor;
        textOutline.effectDistance = new Vector2(textStrokeSize, textStrokeSize);
        textOutline.useGraphicAlpha = true;

        // Position Text in center of screen
        RectTransform rectTransform = uiText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

        // Initially disable Text
        uiText.enabled = false;
    }

    // This method needs to be on the same GameObject that has the trigger collider
    void OnTriggerEnter(Collider other)
    {
        if (!gameStarted && other.CompareTag("Player"))
        {
            gameStarted = true;
            StartCoroutine(StartGameWithMessage());
        }
    }

    // Make sure this script is attached to the GameObject with the trigger collider
    // or add this component to the triggerZone GameObject
    void OnValidate()
    {
        if (triggerZone != null && triggerZone != gameObject)
        {
            // Validation logic remains but without debug message
        }
    }

    // Alternative method if the trigger is on a different GameObject
    public void StartGameFromTrigger()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(StartGameWithMessage());
        }
    }

    IEnumerator StartGameWithMessage()
    {
        yield return StartCoroutine(ShowMessage("STARTING PUZZLE", 2f));
        StartLevel(1);
    }

    void StartLevel(int level)
    {
        currentLevel = level;
        sequence = new List<int>();

        if (level == 1)
        {
            // Fixed sequence for first level: Cube3 (Yellow), Cube4 (Green), Cube1 (Red)
            sequence.AddRange(new int[] { 2, 3, 0 }); // Fixed index for Cube1 (should be 0, not 1)
        }
        else
        {
            // Random sequence for levels 2-4, length = level + 1
            for (int i = 0; i < level + 1; i++)
            {
                sequence.Add(Random.Range(0, 4));
            }
        }

        currentStep = 0;
        isShowingSequence = true;
        isWaitingForInput = false;
        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(0.5f); // Short pause before showing sequence
        
        foreach (int index in sequence)
        {
            if (index >= 0 && index < colorCubes.Length && colorCubes[index] != null)
            {
                MeshRenderer renderer = colorCubes[index].GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = colorMaterials[index];
                    yield return new WaitForSeconds(1f);
                    renderer.material = defaultMaterial;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        
        isShowingSequence = false;
        isWaitingForInput = true;
        StartCoroutine(ShowMessage("YOUR TURN", 1f));
    }

    void Update()
    {
        if (isWaitingForInput && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                int index = System.Array.IndexOf(colorCubes, clicked);
                if (index != -1) // Valid cube clicked
                {
                    StartCoroutine(FlashCube(index));
                    if (index == sequence[currentStep])
                    {
                        currentStep++;
                        if (currentStep == sequence.Count)
                        {
                            StartCoroutine(LevelComplete());
                        }
                    }
                    else
                    {
                        StartCoroutine(WrongPattern());
                    }
                }
            }
        }
    }

    IEnumerator FlashCube(int index)
    {
        MeshRenderer renderer = colorCubes[index].GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = colorMaterials[index];
            yield return new WaitForSeconds(0.5f);
            renderer.material = defaultMaterial;
        }
    }

    IEnumerator LevelComplete()
    {
        isWaitingForInput = false;
        yield return StartCoroutine(ShowMessage("PATTERN CORRECT!", 2f));
        
        if (currentLevel < 4)
        {
            yield return StartCoroutine(ShowMessage("NEXT PATTERN", 2f));
            StartLevel(currentLevel + 1);
        }
        else
        {
            if (doorToUnlock != null)
            {
                // Visual effect before destroying the door
                StartCoroutine(DoorUnlockEffect());
            }
            else
            {
                yield return StartCoroutine(ShowMessage("PUZZLE COMPLETE!", 3f));
            }
        }
    }

    IEnumerator DoorUnlockEffect()
    {
        yield return StartCoroutine(ShowMessage("PUZZLE COMPLETE! DOOR UNLOCKED", 2f));
        
        if (doorToUnlock != null)
        {
            // Optional: Add visual effect to the door before destroying it
            MeshRenderer doorRenderer = doorToUnlock.GetComponent<MeshRenderer>();
            if (doorRenderer != null)
            {
                // Create a glowing material for the door
                Material glowMaterial = new Material(Shader.Find("Standard"));
                glowMaterial.color = Color.cyan;
                glowMaterial.SetColor("_EmissionColor", Color.cyan * 3f);
                glowMaterial.EnableKeyword("_EMISSION");
                
                // Apply the glowing material
                doorRenderer.material = glowMaterial;
                
                // Wait a moment for the glow effect to be visible
                yield return new WaitForSeconds(1.5f);
            }
            
            // Destroy the door
            Destroy(doorToUnlock);
        }
    }

    IEnumerator WrongPattern()
    {
        isWaitingForInput = false;
        yield return StartCoroutine(ShowMessage("WRONG PATTERN", 2f));
        StartLevel(currentLevel);
    }

    IEnumerator ShowMessage(string message, float duration)
    {
        if (uiText != null)
        {
            uiText.text = message;
            uiText.enabled = true;
            yield return new WaitForSeconds(duration);
            uiText.enabled = false;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
    }
}