using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ToggleRotation : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private Button toggleButton;  // Reference to the UI Button

    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject spawnedObject;
    private bool isObjectSelected = false;
    private bool isRotationMode = false;  // Track the state of the rotation mode
    private Vector2 previousTouchPosition;

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();

        // Add a listener to the button to toggle rotation mode
        toggleButton.onClick.AddListener(ToggleRotationMode);
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
        EnhancedTouch.Touch.onFingerMove += FingerMove;
        EnhancedTouch.Touch.onFingerUp += FingerUp;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.Touch.onFingerMove -= FingerMove;
        EnhancedTouch.Touch.onFingerUp -= FingerUp;
    }

    private void ToggleRotationMode()
    {
        isRotationMode = !isRotationMode;  // Toggle the rotation mode on button press
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0 || isRotationMode) return;

        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(prefab, pose.position, pose.rotation);
                spawnedObject.transform.localScale *= 0.5f;  // Initial scaling factor
                isObjectSelected = true;
            }
            else
            {
                spawnedObject.transform.position = pose.position;
                spawnedObject.transform.rotation = pose.rotation;
                isObjectSelected = true;
            }

            if (aRPlaneManager.GetPlane(hits[0].trackableId).alignment == PlaneAlignment.HorizontalUp)
            {
                Vector3 position = spawnedObject.transform.position;
                Vector3 cameraPosition = Camera.main.transform.position;
                Vector3 direction = cameraPosition - position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                spawnedObject.transform.rotation = targetRotation;
            }
        }
    }

    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (!isObjectSelected || spawnedObject == null) return;

        if (isRotationMode)
        {
            // Rotate the object when in rotation mode
            Vector2 delta = finger.screenPosition - previousTouchPosition;
            float rotateSpeed = 0.2f;
            spawnedObject.transform.Rotate(Vector3.up, delta.x * rotateSpeed, Space.World);
            spawnedObject.transform.Rotate(Vector3.right, -delta.y * rotateSpeed, Space.World);
        }
        else if (EnhancedTouch.Touch.activeTouches.Count == 2)
        {
            // Handle pinch-to-scale
            var touch0 = EnhancedTouch.Touch.activeTouches[0];
            var touch1 = EnhancedTouch.Touch.activeTouches[1];

            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

            if (initialDistance == 0)
            {
                initialDistance = currentDistance;
                initialScale = spawnedObject.transform.localScale;
            }

            float scaleFactor = currentDistance / initialDistance;
            spawnedObject.transform.localScale = initialScale * scaleFactor;
        }
        else
        {
            initialDistance = 0;
            initialScale = Vector3.zero;
        }

        previousTouchPosition = finger.screenPosition;
    }

    private void FingerUp(EnhancedTouch.Finger finger)
    {
        previousTouchPosition = Vector2.zero;
    }

    // Variables for pinch-to-scale
    private float initialDistance;
    private Vector3 initialScale;
}
