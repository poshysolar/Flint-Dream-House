using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GeneratorRepairSequence : MonoBehaviour
{
    [SerializeField] private EightSequence eightSequence;
    [SerializeField] private GameObject[] generators;
    [SerializeField] private GameObject cylinderOne;
    [SerializeField] private GameObject cylinderTwo;
    [SerializeField] private GameObject catModel; // Cat model to hide after door opens
    [SerializeField] private GameObject mikey2Model; // Running Mikey model for follow system
    [SerializeField] private GameObject mikeyTalkingModel; // Add reference to talking Mikey model
    [SerializeField] private AudioClip mikeyAudio;
    [SerializeField] private Font customFont;
    [SerializeField] private float skillCheckZoneWidth = 100f;
    [SerializeField] private float skillCheckSpeed = 150f;
    [SerializeField] private float interactionDistance = 3f;

    // Reference to the run script from Assets/Scripts/LevelEight
    [SerializeField] private MikeyFollowPlayer mikeyFollowScript;

    // Speech tracking system
    public static bool isMikeySpeaking = false;

    private Text objectiveText;
    private Canvas canvas;
    private GameObject skillCheckUI;
    private Image progressBar;
    private Text notifyTrackerText;
    private RectTransform skillCheckMarker;
    private RectTransform skillCheckSuccessZone;
    private int generatorsRepaired = 0;
    private bool[] generatorRepairedStatus;
    private bool isRepairing = false;
    private float progress = 0f;
    private float markerPosition = 0f;
    private bool markerMovingRight = true;
    private AudioSource audioSource;
    private GameObject mikeyInstance;
    private int currentGeneratorIndex = -1;
    private int currentRound = 0;
    private int maxRounds = 2; // Need 2 successful skill checks to complete
    private bool objectiveUIVisible = false; // Track if objective UI is visible

    private void Start()
    {
        // Initialize cameras at start
        if (eightSequence != null)
        {
            if (eightSequence.PlayerCamera != null)
            {
                eightSequence.PlayerCamera.enabled = true;
            }
            if (eightSequence.Camera2 != null)
            {
                eightSequence.Camera2.enabled = false;
            }
            if (eightSequence.Camera3 != null)
            {
                eightSequence.Camera3.enabled = false;
            }
            if (eightSequence.Camera4 != null)
            {
                eightSequence.Camera4.enabled = false;
            }
        }

        // Initialize generator status array
        generatorRepairedStatus = new bool[generators.Length];

        // Create UI
        SetupUI();

        // Get AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();

        // Hide initial objective text - will be shown after Mikey's speech
        objectiveText.text = "";
        objectiveText.gameObject.SetActive(false);
        notifyTrackerText.gameObject.SetActive(false);
        
        // Ensure cat model is ALWAYS visible and positioned correctly
        if (catModel != null)
        {
            catModel.SetActive(true);
            catModel.transform.position = new Vector3(-17.67f, 0.35f, -40.34f);
            Debug.Log("Cat model positioned and made visible at: " + catModel.transform.position);
        }
        
        // Hide Mikey2 Model at start
        if (mikey2Model != null)
        {
            mikey2Model.SetActive(false);
        }

        // Handle talking Mikey model based on speech tracking
        UpdateMikeyTalkingModelVisibility();
    }

    // Method to show the objective UI - called from EightSequence
    public void ShowObjectiveUI()
    {
        if (!objectiveUIVisible)
        {
            objectiveText.gameObject.SetActive(true);
            notifyTrackerText.gameObject.SetActive(true);
            objectiveText.text = "Repair the generators";
            notifyTrackerText.text = $"Generators {generatorsRepaired}/{generators.Length}";
            objectiveUIVisible = true;
            Debug.Log("Objective UI shown after Mikey's speech");
        }
    }

    private void Update()
    {
        // Update Mikey talking model visibility based on speech tracking
        UpdateMikeyTalkingModelVisibility();

        // Only process game logic if objective UI is visible (after Mikey's speech)
        if (!objectiveUIVisible)
            return;

        if (isRepairing)
        {
            // Handle skill check system
            float barWidth = 600f;
            markerPosition += (markerMovingRight ? 1 : -1) * skillCheckSpeed * Time.deltaTime;
            
            if (markerPosition > barWidth / 2) 
            {
                markerPosition = barWidth / 2;
                markerMovingRight = false;
            }
            if (markerPosition < -barWidth / 2) 
            {
                markerPosition = -barWidth / 2;
                markerMovingRight = true;
            }
            
            skillCheckMarker.anchoredPosition = new Vector2(markerPosition, 0);

            // Check for spacebar input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                float successZoneX = skillCheckSuccessZone.anchoredPosition.x;
                float successZoneHalfWidth = skillCheckZoneWidth / 2;
                
                // Check if marker is within success zone
                if (markerPosition >= successZoneX - successZoneHalfWidth && 
                    markerPosition <= successZoneX + successZoneHalfWidth)
                {
                    // Successful skill check
                    currentRound++;
                    progress = (float)currentRound / (float)maxRounds;
                    progressBar.fillAmount = progress;
                    
                    Debug.Log($"Successful skill check! Round {currentRound}/{maxRounds}, Progress: {progress}");
                    
                    if (currentRound >= maxRounds)
                    {
                        FinishRepair();
                    }
                    else
                    {
                        // Reset marker position for next skill check
                        markerPosition = -barWidth / 2;
                        markerMovingRight = true;
                        
                        // Randomize success zone position for next round
                        float newZoneX = UnityEngine.Random.Range(-200f, 200f);
                        skillCheckSuccessZone.anchoredPosition = new Vector2(newZoneX, 0);
                    }
                }
                else
                {
                    // Failed skill check - reset progress
                    currentRound = 0;
                    progress = 0f;
                    progressBar.fillAmount = progress;
                    
                    Debug.Log("Failed skill check! Progress reset.");
                    
                    // Reset marker position
                    markerPosition = -barWidth / 2;
                    markerMovingRight = true;
                    
                    // Reset success zone position
                    skillCheckSuccessZone.anchoredPosition = new Vector2(200, 0);
                }
            }
        }
        else
        {
            // Check for generator interaction when not repairing
            if (eightSequence != null && eightSequence.PlayerCamera != null)
            {
                for (int i = 0; i < generators.Length; i++)
                {
                    if (!generatorRepairedStatus[i] && 
                        Vector3.Distance(generators[i].transform.position, eightSequence.PlayerCamera.transform.position) <= interactionDistance)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            StartRepair(i);
                            break; // Only start one repair at a time
                        }
                    }
                }
            }
        }
    }

    private void UpdateMikeyTalkingModelVisibility()
    {
        if (mikeyTalkingModel != null)
        {
            // Show talking model only when Mikey is speaking
            if (isMikeySpeaking && !mikeyTalkingModel.activeInHierarchy)
            {
                mikeyTalkingModel.SetActive(true);
                mikeyTalkingModel.transform.position = new Vector3(-0.7f, -0.21f, -29.82f);
                Debug.Log("Mikey talking model shown - Mikey is speaking");
            }
            else if (!isMikeySpeaking && mikeyTalkingModel.activeInHierarchy)
            {
                mikeyTalkingModel.SetActive(false);
                Debug.Log("Mikey talking model hidden - Mikey not speaking");
            }
        }
    }

    private void SetupUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GeneratorCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        canvasObj.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Objective Text
        GameObject objTextObj = new GameObject("ObjectiveText");
        objTextObj.transform.SetParent(canvasObj.transform, false);
        objectiveText = objTextObj.AddComponent<Text>();
        objectiveText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        objectiveText.fontSize = 40;
        objectiveText.alignment = TextAnchor.UpperCenter;
        objectiveText.rectTransform.sizeDelta = new Vector2(1000, 150);
        objectiveText.rectTransform.anchoredPosition = new Vector2(0, 300);
        objectiveText.color = Color.white;

        // Create Notify Tracker Text - FIXED: Positioned in top left corner
        GameObject notifyObj = new GameObject("NotifyTrackerText");
        notifyObj.transform.SetParent(canvasObj.transform, false);
        notifyTrackerText = notifyObj.AddComponent<Text>();
        notifyTrackerText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        notifyTrackerText.fontSize = 30;
        notifyTrackerText.alignment = TextAnchor.UpperLeft;
        notifyTrackerText.rectTransform.sizeDelta = new Vector2(300, 70);
        notifyTrackerText.rectTransform.anchoredPosition = new Vector2(-810, 470); // Top left corner position
        notifyTrackerText.color = Color.white;
        notifyTrackerText.text = $"Generators {generatorsRepaired}/{generators.Length}";

        // Create Skill Check UI (hidden initially)
        skillCheckUI = new GameObject("SkillCheckUI");
        skillCheckUI.transform.SetParent(canvasObj.transform, false);
        skillCheckUI.SetActive(false);

        GameObject skillCheckBar = new GameObject("SkillCheckBar");
        skillCheckBar.transform.SetParent(skillCheckUI.transform, false);
        Image barImage = skillCheckBar.AddComponent<Image>();
        barImage.color = Color.gray;
        barImage.rectTransform.sizeDelta = new Vector2(600, 50);
        barImage.rectTransform.anchoredPosition = new Vector2(0, 0);

        GameObject successZone = new GameObject("SuccessZone");
        successZone.transform.SetParent(skillCheckBar.transform, false);
        skillCheckSuccessZone = successZone.AddComponent<RectTransform>();
        Image successImage = successZone.AddComponent<Image>();
        successImage.color = Color.green;
        successImage.rectTransform.sizeDelta = new Vector2(skillCheckZoneWidth, 50);
        successImage.rectTransform.anchoredPosition = new Vector2(200, 0);

        GameObject marker = new GameObject("SkillCheckMarker");
        marker.transform.SetParent(skillCheckBar.transform, false);
        skillCheckMarker = marker.AddComponent<RectTransform>();
        Image markerImage = marker.AddComponent<Image>();
        markerImage.color = Color.red;
        markerImage.rectTransform.sizeDelta = new Vector2(20, 50);
        markerImage.rectTransform.anchoredPosition = new Vector2(-300, 0);

        // Create Progress Bar Background
        GameObject progressBarBG = new GameObject("ProgressBarBackground");
        progressBarBG.transform.SetParent(canvasObj.transform, false);
        Image progressBGImage = progressBarBG.AddComponent<Image>();
        progressBGImage.color = Color.gray;
        progressBGImage.rectTransform.sizeDelta = new Vector2(600, 30);
        progressBGImage.rectTransform.anchoredPosition = new Vector2(0, -100);
        progressBarBG.SetActive(false);

        // Create Progress Bar
        GameObject progressBarObj = new GameObject("ProgressBar");
        progressBarObj.transform.SetParent(progressBarBG.transform, false);
        progressBar = progressBarObj.AddComponent<Image>();
        progressBar.color = Color.blue;
        progressBar.rectTransform.sizeDelta = new Vector2(600, 30);
        progressBar.rectTransform.anchoredPosition = new Vector2(0, 0);
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        progressBar.fillAmount = 0f;
        progressBar.type = Image.Type.Filled;
    }

    private void StartRepair(int generatorIndex)
    {
        isRepairing = true;
        currentGeneratorIndex = generatorIndex;
        currentRound = 0;
        progress = 0f;
        progressBar.fillAmount = 0f;
        skillCheckUI.SetActive(true);
        progressBar.transform.parent.gameObject.SetActive(true); // Show progress bar background
        markerPosition = -300f;
        markerMovingRight = true;

        // Reset success zone to default position
        skillCheckSuccessZone.anchoredPosition = new Vector2(200, 0);

        // FIXED: Pause player movement by disabling the EightSequence script
        if (eightSequence != null)
        {
            eightSequence.enabled = false;
        }

        // Show and unlock cursor for skill check system
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log($"Started repair on generator {generatorIndex}. Need {maxRounds} successful skill checks.");
    }

    private void FinishRepair()
    {
        isRepairing = false;
        skillCheckUI.SetActive(false);
        progressBar.transform.parent.gameObject.SetActive(false); // Hide progress bar background
        generatorsRepaired++;
        generatorRepairedStatus[currentGeneratorIndex] = true;
        notifyTrackerText.text = $"Generators {generatorsRepaired}/{generators.Length}";

        // Reset for next repair
        currentRound = 0;
        progress = 0f;

        // FIXED: Re-enable player movement
        if (eightSequence != null)
        {
            eightSequence.enabled = true;
        }

        // Hide and lock cursor back to normal gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"Generator {currentGeneratorIndex} repair completed! Generators left: {generators.Length - generatorsRepaired}");

        if (generatorsRepaired >= generators.Length)
        {
            StartCoroutine(FinalCutscene());
        }
    }

    private IEnumerator FinalCutscene()
    {
        objectiveText.text = "";

        if (eightSequence != null)
        {
            eightSequence.enabled = false;
            if (eightSequence.PlayerCamera != null) eightSequence.PlayerCamera.enabled = false;
            if (eightSequence.Camera2 != null) eightSequence.Camera2.enabled = true;
            if (eightSequence.Camera3 != null) eightSequence.Camera3.enabled = false;
            if (eightSequence.Camera4 != null) eightSequence.Camera4.enabled = false;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Vector3 cylinderOneTarget = new Vector3(-18.04f, cylinderOne.transform.position.y, cylinderOne.transform.position.z);
        Vector3 cylinderTwoTarget = new Vector3(-23.04f, cylinderTwo.transform.position.y, cylinderTwo.transform.position.z);
        float moveSpeed = 2f;
        // Hide the cat model when doors open
        if (catModel != null)
        {
            catModel.SetActive(false);
        }

        while (Vector3.Distance(cylinderOne.transform.position, cylinderOneTarget) > 0.01f || 
               Vector3.Distance(cylinderTwo.transform.position, cylinderTwoTarget) > 0.01f)
        {
            cylinderOne.transform.position = Vector3.MoveTowards(cylinderOne.transform.position, cylinderOneTarget, moveSpeed * Time.deltaTime);
            cylinderTwo.transform.position = Vector3.MoveTowards(cylinderTwo.transform.position, cylinderTwoTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (eightSequence != null && eightSequence.Camera2 != null) eightSequence.Camera2.enabled = false;
        if (eightSequence != null && eightSequence.Camera3 != null) eightSequence.Camera3.enabled = true;
        
        // Set Mikey as speaking to show talking model
        isMikeySpeaking = true;

        Text subtitleText = eightSequence.SubtitleText;
        if (subtitleText != null)
        {
            subtitleText.color = new Color(1f, 0.5f, 0f);
            subtitleText.text = "Mikey: Urggh noo I won't let you get away this time!";
        }
        if (audioSource != null && mikeyAudio != null)
        {
            audioSource.clip = mikeyAudio;
            audioSource.Play();
            yield return new WaitForSeconds(mikeyAudio.length);
        }

        // Set Mikey as not speaking to hide talking model
        isMikeySpeaking = false;

        // Show Mikey2 Model (running model) when needed
        if (mikey2Model != null)
        {
            mikey2Model.SetActive(true);
            mikeyInstance = mikey2Model;
        }
        else if (eightSequence != null && eightSequence.MikeyPrefab != null)
        {
            mikeyInstance = eightSequence.MikeyPrefab;
            mikeyInstance.SetActive(true);
        }

        if (eightSequence != null && eightSequence.Camera3 != null) eightSequence.Camera3.enabled = false;
        if (eightSequence != null && eightSequence.PlayerCamera != null) eightSequence.PlayerCamera.enabled = true;
        if (subtitleText != null) subtitleText.text = "";

        // Start Mikey following the player using the referenced script from Assets/Scripts/LevelEight
        if (mikeyFollowScript != null && eightSequence != null && eightSequence.PlayerCamera != null)
        {
            mikeyFollowScript.SetPlayerTarget(eightSequence.PlayerCamera.transform);
            mikeyFollowScript.StartFollowing();
        }

        if (eightSequence != null) eightSequence.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}