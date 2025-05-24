using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ObjectInteraction : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private float floatHeight = 0.1f;

    [Header("UI Elements")]
    [SerializeField] private Button rotateToggleButton;
    [SerializeField] private Button moveBoneButton;
    [SerializeField] private Button cyclePrefabButton;

    private const int MAX_OBJECTS = 3;
    private const float DOUBLE_TAP_TIME = 0.3f;
    private const float ROTATE_SPEED = 0.2f;

    private ARRaycastManager aRRaycastManager;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private readonly Dictionary<int, GameObject> spawnedObjects = new Dictionary<int, GameObject>();
    private readonly Queue<int> objectSpawnOrder = new Queue<int>();

    private bool isMoveEnabled;
    private bool isRotationEnabled;
    private Vector2 previousTouchPosition;
    private float lastTapTime;

    // Scale gesture variables
    private float initialDistance;
    private Vector3 initialScale;
    private Vector3 moveBonePosition;
    private Quaternion moveBoneRotation;
    private int selectedPrefabIndex;

    private void Awake()
    {
        InitializeComponents();
        SetupButtonListeners();
    }

    private void InitializeComponents()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
    }

    private void SetupButtonListeners()
    {
        rotateToggleButton.onClick.AddListener(ToggleRotation);
        moveBoneButton.onClick.AddListener(MoveSelectedBone);
        cyclePrefabButton.onClick.AddListener(SelectNextPrefab);
    }

    private void OnEnable()
    {
        EnableTouchInput();
    }

    private void OnDisable()
    {
        DisableTouchInput();
        RemoveButtonListeners();
    }

    private void EnableTouchInput()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
        EnhancedTouch.Touch.onFingerMove += FingerMove;
        EnhancedTouch.Touch.onFingerUp += FingerUp;
    }

    private void DisableTouchInput()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.Touch.onFingerMove -= FingerMove;
        EnhancedTouch.Touch.onFingerUp -= FingerUp;
    }

    private void RemoveButtonListeners()
    {
        rotateToggleButton.onClick.RemoveListener(ToggleRotation);
        moveBoneButton.onClick.RemoveListener(MoveSelectedBone);
        cyclePrefabButton.onClick.RemoveListener(SelectNextPrefab);
    }

    private void MoveSelectedBone()
    {
        if (!spawnedObjects.TryGetValue(selectedPrefabIndex, out GameObject selectedObject) || !isMoveEnabled)
            return;

        selectedObject.transform.SetPositionAndRotation(moveBonePosition, moveBoneRotation);
        isMoveEnabled = false;
    }

    private void ToggleRotation()
    {
        isRotationEnabled = !isRotationEnabled;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        float timeSinceLastTap = Time.time - lastTapTime;
        if (timeSinceLastTap <= DOUBLE_TAP_TIME)
        {
            HandleDoubleTap(finger.currentTouch.screenPosition);
        }
        lastTapTime = Time.time;
    }

    private void HandleDoubleTap(Vector2 touchPosition)
    {
        if (!aRRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose pose = hits[0].pose;
        Vector3 floatPosition = pose.position + pose.up * floatHeight;

        if (spawnedObjects.TryGetValue(selectedPrefabIndex, out GameObject existingObject))
        {
            SetupObjectMovement(floatPosition, pose.rotation);
        }
        else
        {
    
        }

        OrientObjectTowardsCamera(pose.up);
    }

    private void SetupObjectMovement(Vector3 position, Quaternion rotation)
    {
        moveBonePosition = position;
        moveBoneRotation = rotation;
        isMoveEnabled = true;
    }



    private void RemoveOldestObject()
    {
        int oldestPrefabIndex = objectSpawnOrder.Dequeue();
        if (spawnedObjects.TryGetValue(oldestPrefabIndex, out GameObject oldestObject))
        {
            Destroy(oldestObject);
            spawnedObjects.Remove(oldestPrefabIndex);
        }
    }

    private void OrientObjectTowardsCamera(Vector3 upDirection)
    {
        if (!spawnedObjects.TryGetValue(selectedPrefabIndex, out GameObject currentObject))
            return;

        Vector3 directionToFace = Camera.main.transform.position - currentObject.transform.position;
        directionToFace.y = 0;
        currentObject.transform.rotation = Quaternion.LookRotation(directionToFace, upDirection);
    }



    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (!spawnedObjects.ContainsKey(selectedPrefabIndex)) return;

        if (EnhancedTouch.Touch.activeTouches.Count == 2)
        {
            HandlePinchToScale();
        }
        else if (EnhancedTouch.Touch.activeTouches.Count == 1 && isRotationEnabled)
        {
            HandleRotation(finger);
        }

        previousTouchPosition = finger.screenPosition;
    }

    private void HandlePinchToScale()
    {
        var touch0 = EnhancedTouch.Touch.activeTouches[0];
        var touch1 = EnhancedTouch.Touch.activeTouches[1];
        float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

        if (initialDistance == 0)
        {
            initialDistance = currentDistance;
            initialScale = spawnedObjects[selectedPrefabIndex].transform.localScale;
            return;
        }

        float scaleFactor = currentDistance / initialDistance;
        spawnedObjects[selectedPrefabIndex].transform.localScale = initialScale * scaleFactor;
    }

    private void HandleRotation(EnhancedTouch.Finger finger)
    {
        if (previousTouchPosition == Vector2.zero) return;

        Vector2 delta = finger.screenPosition - previousTouchPosition;
        Transform objectTransform = spawnedObjects[selectedPrefabIndex].transform;

        objectTransform.Rotate(Vector3.up, delta.x * ROTATE_SPEED, Space.World);
        objectTransform.Rotate(Vector3.right, -delta.y * ROTATE_SPEED, Space.World);
    }

    private void FingerUp(EnhancedTouch.Finger finger)
    {
        initialDistance = 0f;
        previousTouchPosition = Vector2.zero;
    }

    private void EnsureColliders(GameObject obj)
    {
        obj.tag = "MainBone";

        if (!obj.TryGetComponent<BoxCollider>(out _))
        {
            obj.AddComponent<BoxCollider>();
        }

        foreach (var meshFilter in obj.GetComponentsInChildren<MeshFilter>())
        {
            if (!meshFilter.TryGetComponent<MeshCollider>(out var meshCollider))
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
            }
            meshFilter.gameObject.tag = "Bone";
        }
    }

    private void SelectNextPrefab()
    {
        selectedPrefabIndex = (selectedPrefabIndex + 1) % prefabs.Count;
    }
}

