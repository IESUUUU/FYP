using UnityEngine;
using UnityEngine.Events;

public class ObjectManipulationController : MonoBehaviour
{
    [Header("Manipulation Settings")]
    [SerializeField] private float snapRotationDegrees = 45f; // For snap rotation
    [SerializeField] private float heightAdjustmentSpeed = 0.1f;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float maxHeight = 2f;

    [Header("Animation")]
    [SerializeField] private float placementAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve placementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEvent onObjectPlaced;
    public UnityEvent onObjectMoved;
    public UnityEvent onObjectRotated;
    public UnityEvent onObjectScaled;

    private GameObject currentObject;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float placementAnimationTime;
    private bool isAnimating;

    public void SetCurrentObject(GameObject obj)
    {
        currentObject = obj;
        if (currentObject != null)
        {
            targetPosition = currentObject.transform.position;
            targetRotation = currentObject.transform.rotation;
        }
    }

    public void AdjustHeight(float delta)
    {
        if (currentObject == null) return;

        float newHeight = Mathf.Clamp(
            currentObject.transform.position.y + delta * heightAdjustmentSpeed,
            minHeight,
            maxHeight
        );
        
        Vector3 position = currentObject.transform.position;
        position.y = newHeight;
        currentObject.transform.position = position;
        onObjectMoved?.Invoke();
    }

    public void SnapRotation()
    {
        if (currentObject == null) return;

        float currentYRotation = currentObject.transform.eulerAngles.y;
        float snappedRotation = Mathf.Round(currentYRotation / snapRotationDegrees) * snapRotationDegrees;
        
        targetRotation = Quaternion.Euler(0, snappedRotation, 0);
        StartPlacementAnimation();
        onObjectRotated?.Invoke();
    }

    public void StartPlacementAnimation()
    {
        if (currentObject == null) return;

        isAnimating = true;
        placementAnimationTime = 0f;
    }

    private void Update()
    {
        if (isAnimating)
        {
            UpdatePlacementAnimation();
        }
    }

    private void UpdatePlacementAnimation()
    {
        placementAnimationTime += Time.deltaTime;
        float normalizedTime = placementAnimationTime / placementAnimationDuration;

        if (normalizedTime >= 1f)
        {
            isAnimating = false;
            currentObject.transform.position = targetPosition;
            currentObject.transform.rotation = targetRotation;
            onObjectPlaced?.Invoke();
        }
        else
        {
            float curveValue = placementCurve.Evaluate(normalizedTime);
            currentObject.transform.position = Vector3.Lerp(currentObject.transform.position, targetPosition, curveValue);
            currentObject.transform.rotation = Quaternion.Lerp(currentObject.transform.rotation, targetRotation, curveValue);
        }
    }

    public void ResetObjectTransform()
    {
        if (currentObject == null) return;

        targetPosition = currentObject.transform.position;
        targetRotation = Quaternion.identity;
        StartPlacementAnimation();
    }
} 