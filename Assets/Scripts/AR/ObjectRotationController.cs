using UnityEngine;
using UnityEngine.UI;

public class ObjectRotationController : MonoBehaviour
{
    [SerializeField] private Button rotateToggleButton;
    private const float ROTATE_SPEED = 0.2f;

    private bool isRotationEnabled;
    private Vector2 previousTouchPosition;

    private ARObjectManager arObjectManager;

    private void Start()
    {
        if (rotateToggleButton == null)
        {
            Debug.LogError("Rotate toggle button is not assigned!");
            return;
        }

        rotateToggleButton.onClick.AddListener(ToggleRotation);
        Debug.Log("Rotation controller initialized");

        arObjectManager = FindObjectOfType<ARObjectManager>();
    }

    private void OnDestroy()
    {
        if (rotateToggleButton != null)
        {
            rotateToggleButton.onClick.RemoveListener(ToggleRotation);
        }
    }

    private void ToggleRotation()
    {
        isRotationEnabled = !isRotationEnabled;
        Debug.Log($"Rotation {(isRotationEnabled ? "enabled" : "disabled")}");
    }

    public void HandleRotation(Vector2 currentPosition)
    {
        if (!isRotationEnabled)
        {
            return;
        }

        if (ARObjectManager.Instance == null)
        {
            Debug.LogError("ARObjectManager.Instance is null!");
            return;
        }

        if (!ARObjectManager.Instance.TryGetCurrentObject(out GameObject currentObject))
        {
            return;
        }

        if (previousTouchPosition == Vector2.zero)
        {
            previousTouchPosition = currentPosition;
            return;
        }

        Vector2 delta = currentPosition - previousTouchPosition;

        // Ignore very small movements to prevent jitter
        if (delta.sqrMagnitude < 1f)
        {
            return;
        }

        Transform objectTransform = currentObject.transform;

        objectTransform.Rotate(Vector3.up, delta.x * ROTATE_SPEED, Space.World);
        objectTransform.Rotate(Vector3.right, -delta.y * ROTATE_SPEED, Space.World);

        previousTouchPosition = currentPosition;
    }

    public void ResetTouchPosition()
    {
        previousTouchPosition = Vector2.zero;
    }

    public void OnObjectRotated()
    {
        if (arObjectManager != null)
        {
            arObjectManager.NotifyObjectRotated();
        }
    }
}