using UnityEngine;

public class ObjectMovementController : MonoBehaviour
{
    private bool isMoveEnabled;
    private Vector3 moveBonePosition;
    private Quaternion moveBoneRotation;
    private ARObjectManager arObjectManager;

    private void Start()
    {
        Debug.Log("Movement controller initialized");
        arObjectManager = FindObjectOfType<ARObjectManager>();
    }

    public void SetupObjectMovement(Vector3 position, Quaternion rotation)
    {
        moveBonePosition = position;
        moveBoneRotation = rotation;
        Debug.Log($"Object movement set up at position: {position}");
    }

    public bool IsMoveEnabled()
    {
        return isMoveEnabled;
    }

    public void MoveSelectedBone()
    {
        if (ARObjectManager.Instance == null)
        {
            Debug.LogError("ARObjectManager.Instance is null!");
            return;
        }

        if (!ARObjectManager.Instance.TryGetCurrentObject(out GameObject selectedObject))
        {
            Debug.LogWarning("No object selected for movement");
            return;
        }

        selectedObject.transform.SetPositionAndRotation(moveBonePosition, moveBoneRotation);
        Debug.Log($"Moved object to position: {moveBonePosition}");
        OnObjectMoved();
    }

    public void ToggleMoveMode()
    {
        isMoveEnabled = !isMoveEnabled;
        Debug.Log($"Movement mode {(isMoveEnabled ? "enabled" : "disabled")}");
    }

    public void OnObjectMoved()
    {
        if (arObjectManager != null)
        {
            arObjectManager.NotifyObjectMoved();
        }
    }
}