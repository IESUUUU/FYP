using UnityEngine;

public class ObjectScaleController : MonoBehaviour
{
    private float initialDistance;
    private Vector3 initialScale;

    // Scale constraints to prevent objects from becoming too small or too large
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 5.0f;
    [SerializeField] private float scaleMultiplier = 1.0f;

    private ARObjectManager arObjectManager;

    private void Start()
    {
        Debug.Log("Scale controller initialized");
        arObjectManager = FindObjectOfType<ARObjectManager>();
    }

    public void HandlePinchToScale(Vector2 touch0Position, Vector2 touch1Position)
    {
        if (ARObjectManager.Instance == null)
        {
            Debug.LogError("ARObjectManager.Instance is null!");
            return;
        }

        if (!ARObjectManager.Instance.TryGetCurrentObject(out GameObject currentObject))
        {
            return;
        }

        float currentDistance = Vector2.Distance(touch0Position, touch1Position);

        // Ignore very small distances to prevent division by zero
        if (currentDistance < 0.1f)
        {
            return;
        }

        if (initialDistance == 0)
        {
            initialDistance = currentDistance;
            initialScale = currentObject.transform.localScale;
            return;
        }

        float scaleFactor = currentDistance / initialDistance;

        // Apply scale multiplier for more intuitive scaling
        scaleFactor = 1.0f + ((scaleFactor - 1.0f) * scaleMultiplier);

        // Calculate new scale
        Vector3 newScale = initialScale * scaleFactor;

        // Clamp scale to min/max values
        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
        newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

        currentObject.transform.localScale = newScale;
    }

    public void ResetScaling()
    {
        initialDistance = 0f;
    }

    public void OnObjectScaled()
    {
        if (arObjectManager != null)
        {
            arObjectManager.NotifyObjectScaled();
        }
    }
}