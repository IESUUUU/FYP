using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class ARTutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    [Header("Tutorial Images")]
    [SerializeField] private Sprite[] tutorialImages;

    [Header("AR References")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARObjectManager objectManager;

    private int currentTutorialStep = 0;
    private bool hasShownPlaneDetectionTutorial = false;
    private bool hasShownAutoSpawnTutorial = false;

    private readonly string[] tutorialSteps = new string[]
    {
        "Welcome! Please move your device around slowly to detect surfaces.",
        "Surface detected! An object will be automatically placed.",
        "Great! Now you can:\n• Pinch to scale the object\n• Drag with one finger to move\n• Rotate with two fingers",
        "Additional controls:\n• Switch between objects using the cycle button\n• Delete objects using the delete button\n• Reset placement using the reset button",
    };

    private void Awake()
    {
        Debug.Log("[Tutorial] Awake called");
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (tutorialPanel == null)
        {
            Debug.LogError("[Tutorial] Tutorial panel is not assigned!");
        }
        if (tutorialText == null)
        {
            Debug.LogError("[Tutorial] Tutorial text component is not assigned!");
        }
        if (nextButton == null)
        {
            Debug.LogError("[Tutorial] Next button is not assigned!");
        }
        if (skipButton == null)
        {
            Debug.LogError("[Tutorial] Skip button is not assigned!");
        }
    }

    private void Start()
    {
        Debug.Log("[Tutorial] Start called");

        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
            Debug.Log($"[Tutorial] Found ARPlaneManager: {planeManager != null}");
        }
        
        if (objectManager == null)
        {
            objectManager = FindObjectOfType<ARObjectManager>();
            Debug.Log($"[Tutorial] Found ARObjectManager: {objectManager != null}");
        }

        // Setup UI buttons
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextTutorialStep);
            Debug.Log("[Tutorial] Next button listener added");
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(EndTutorial);
            Debug.Log("[Tutorial] Skip button listener added");
        }

        // Subscribe to object placement event
        if (objectManager != null)
        {
            objectManager.onFirstObjectPlaced.AddListener(OnFirstObjectPlaced);
            Debug.Log("[Tutorial] Subscribed to first object placed event");
        }

        // Show initial tutorial
        ShowTutorialStep(0);
    }

    private void Update()
    {
        // Check for plane detection
        if (!hasShownPlaneDetectionTutorial && planeManager != null)
        {
            int planeCount = planeManager.trackables.count;
            if (planeCount > 0)
            {
                Debug.Log($"[Tutorial] Planes detected: {planeCount}");
                hasShownPlaneDetectionTutorial = true;
                ShowTutorialStep(1);
            }
        }
    }

    private void OnFirstObjectPlaced()
    {
        Debug.Log("[Tutorial] First object placed event received");
        if (!hasShownAutoSpawnTutorial)
        {
            hasShownAutoSpawnTutorial = true;
            ShowTutorialStep(2);
        }
    }

    private void ShowTutorialStep(int step)
    {
        Debug.Log($"[Tutorial] Showing step {step}");
        
        if (step >= tutorialSteps.Length)
        {
            Debug.Log("[Tutorial] No more steps, ending tutorial");
            EndTutorial();
            return;
        }

        currentTutorialStep = step;
        
        // Show tutorial panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Debug.Log("[Tutorial] Panel activated");
        }
        else
        {
            Debug.LogError("[Tutorial] Cannot show tutorial - panel is null!");
            return;
        }

        // Update text
        if (tutorialText != null)
        {
            tutorialText.text = tutorialSteps[step];
            Debug.Log($"[Tutorial] Updated text: {tutorialSteps[step]}");
        }

        // Update image if available
        if (tutorialImage != null && tutorialImages != null && step < tutorialImages.Length)
        {
            tutorialImage.gameObject.SetActive(true);
            tutorialImage.sprite = tutorialImages[step];
            Debug.Log("[Tutorial] Updated image");
        }
        else if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
            Debug.Log("[Tutorial] No image for this step, hiding image component");
        }
    }

    public void ShowNextTutorialStep()
    {
        Debug.Log("[Tutorial] Next button clicked");
        ShowTutorialStep(currentTutorialStep + 1);
    }

    private void EndTutorial()
    {
        Debug.Log("[Tutorial] Ending tutorial");
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(ShowNextTutorialStep);
        
        if (skipButton != null)
            skipButton.onClick.RemoveListener(EndTutorial);

        if (objectManager != null)
        {
            objectManager.onFirstObjectPlaced.RemoveListener(OnFirstObjectPlaced);
        }
        Debug.Log("[Tutorial] Cleaned up event listeners");
    }
} 