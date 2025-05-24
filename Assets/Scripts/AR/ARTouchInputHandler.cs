using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ObjectRotationController), typeof(ObjectScaleController), typeof(ObjectMovementController))]
public class ARTouchInputHandler : MonoBehaviour
{
    [SerializeField] private Button toggleMoveButton;
    [SerializeField] private Button cyclePrefabButton;

    private ObjectRotationController rotationController;
    private ObjectScaleController scaleController;
    private ObjectMovementController movementController;

    private const float DOUBLE_TAP_TIME = 0.3f;
    private const float LONG_PRESS_TIME = 0.7f; // Time needed to hold for delete
    private float lastTapTime;
    private float fingerDownTime;
    private bool isLongPressing;
    private Vector2 initialTouchPosition;
    private const float TOUCH_MOVEMENT_THRESHOLD = 10f; // Pixels of movement before considering it a drag

    private void Awake()
    {
        rotationController = GetComponent<ObjectRotationController>();
        scaleController = GetComponent<ObjectScaleController>();
        movementController = GetComponent<ObjectMovementController>();

        if (rotationController == null || scaleController == null || movementController == null)
        {
            Debug.LogError("One or more required controllers are missing!");
            enabled = false;
        }
    }

    private void Start()
    {
        if (toggleMoveButton != null)
        {
            toggleMoveButton.onClick.AddListener(ToggleMoveMode);
            Debug.Log("Toggle move button initialized");
        }
        else
        {
            Debug.LogWarning("Toggle move button is not assigned!");
        }

        if (cyclePrefabButton != null)
        {
            cyclePrefabButton.onClick.AddListener(CyclePrefab);
            Debug.Log("Cycle prefab button initialized");
        }
        else
        {
            Debug.LogWarning("Cycle prefab button is not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (toggleMoveButton != null)
        {
            toggleMoveButton.onClick.RemoveListener(ToggleMoveMode);
        }

        if (cyclePrefabButton != null)
        {
            cyclePrefabButton.onClick.RemoveListener(CyclePrefab);
        }
    }

    private void ToggleMoveMode()
    {
        movementController.ToggleMoveMode();
    }

    private void CyclePrefab()
    {
        if (ARObjectManager.Instance == null)
        {
            Debug.LogError("ARObjectManager.Instance is null!");
            return;
        }

        ARObjectManager.Instance.CyclePrefab();
    }

    private void OnEnable()
    {
        try
        {
            EnhancedTouch.TouchSimulation.Enable();
            EnhancedTouch.EnhancedTouchSupport.Enable();
            EnhancedTouch.Touch.onFingerDown += FingerDown;
            EnhancedTouch.Touch.onFingerMove += FingerMove;
            EnhancedTouch.Touch.onFingerUp += FingerUp;
            Debug.Log("Touch input handlers enabled successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to enable touch input: {e.Message}");
            enabled = false;
        }
    }

    private void OnDisable()
    {
        try
        {
            EnhancedTouch.Touch.onFingerDown -= FingerDown;
            EnhancedTouch.Touch.onFingerMove -= FingerMove;
            EnhancedTouch.Touch.onFingerUp -= FingerUp;
            EnhancedTouch.EnhancedTouchSupport.Disable();
            EnhancedTouch.TouchSimulation.Disable();
            Debug.Log("Touch input handlers disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during touch input disable: {e.Message}");
        }
    }

    private void Update()
    {
        // Check for long press
        if (EnhancedTouch.Touch.activeTouches.Count == 1 && !isLongPressing)
        {
            var touch = EnhancedTouch.Touch.activeTouches[0];
            float touchMovement = Vector2.Distance(initialTouchPosition, touch.screenPosition);

            // Only consider it a long press if the finger hasn't moved much
            if (Time.time - fingerDownTime >= LONG_PRESS_TIME && touchMovement < TOUCH_MOVEMENT_THRESHOLD)
            {
                HandleLongPress();
                isLongPressing = true;
                Debug.Log("Long press detected");
            }
        }
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        fingerDownTime = Time.time;
        isLongPressing = false;
        initialTouchPosition = finger.currentTouch.screenPosition;

        float timeSinceLastTap = Time.time - lastTapTime;
        if (timeSinceLastTap <= DOUBLE_TAP_TIME)
        {
            HandleDoubleTap(finger.currentTouch.screenPosition);
            Debug.Log("Double tap detected");
        }
        lastTapTime = Time.time;
    }

    private void HandleDoubleTap(Vector2 touchPosition)
    {
        if (ARObjectManager.Instance == null)
        {
            Debug.LogError("ARObjectManager.Instance is null!");
            return;
        }

        // Check if movement is enabled
        if (movementController.IsMoveEnabled())
        {
            // If movement is enabled, try to move the current object
            if (ARObjectManager.Instance.TryGetCurrentObject(out GameObject currentObject))
            {
                // Raycast to get the new position
                if (ARObjectManager.Instance.RaycastToPlane(touchPosition, out Pose pose))
                {
                    // Set up movement to the new position
                    Vector3 floatPosition = pose.position + Vector3.up * 0.1f; // Add a small float height
                    movementController.SetupObjectMovement(floatPosition, pose.rotation);
                    movementController.MoveSelectedBone();
                    Debug.Log("Double tap: Moving object to new position");
                }
                else
                {
                    Debug.Log("Double tap: No plane detected for movement");
                }
            }
            else
            {
                Debug.Log("Double tap: No object to move, spawning new object");
                ARObjectManager.Instance.SpawnOrMoveObject(touchPosition);
            }
        }
        else
        {
            // If movement is disabled, spawn or move the object
            Debug.Log("Double tap: Movement disabled, spawning or moving object");
            ARObjectManager.Instance.SpawnOrMoveObject(touchPosition);
        }
    }

    private void HandleLongPress()
    {
        if (ARObjectManager.Instance == null)
        {
            Debug.LogError("ARObjectManager.Instance is null!");
            return;
        }

        if (ARObjectManager.Instance.TryGetCurrentObject(out _))
        {
            ARObjectManager.Instance.DeleteCurrentObject();
        }
        else
        {
            Debug.Log("No object selected to delete");
        }
    }

    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (EnhancedTouch.Touch.activeTouches.Count == 2)
        {
            try
            {
                var touch0 = EnhancedTouch.Touch.activeTouches[0];
                var touch1 = EnhancedTouch.Touch.activeTouches[1];
                scaleController.HandlePinchToScale(touch0.screenPosition, touch1.screenPosition);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during pinch scaling: {e.Message}");
            }
        }
        else if (EnhancedTouch.Touch.activeTouches.Count == 1 && !isLongPressing)
        {
            try
            {
                rotationController.HandleRotation(finger.currentTouch.screenPosition);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during rotation: {e.Message}");
            }
        }
    }

    private void FingerUp(EnhancedTouch.Finger finger)
    {
        rotationController.ResetTouchPosition();
        scaleController.ResetScaling();
        isLongPressing = false;
        initialTouchPosition = Vector2.zero;
    }
}