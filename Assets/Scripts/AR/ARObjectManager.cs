using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ARObjectManager : MonoBehaviour
{
    public static ARObjectManager Instance { get; private set; }

    [Header("AR References")]
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private float floatHeight = 0.1f;
    [SerializeField] private GameObject femurVariant; // Single femur variant instead of a list

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentPrefabText;  // Reference to the UI text component
    [SerializeField] private Button switchModelButton; // Button to switch femur models

    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager arPlaneManager;
    [Header("Fader Reference")]
    [SerializeField] private PrefabTextFader prefabTextFader;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private readonly Dictionary<int, GameObject> spawnedObjects = new Dictionary<int, GameObject>();
    private readonly Queue<int> objectSpawnOrder = new Queue<int>();

    private const int MAX_OBJECTS = 2;
    public int SelectedPrefabIndex { get; private set; } = 0;
    private bool isUsingVariant = false; // Track which model is currently being used

    [Header("Tutorial Events")]
    public UnityEvent onFirstObjectPlaced = new UnityEvent();
    public UnityEvent onObjectMoved = new UnityEvent();
    public UnityEvent onObjectScaled = new UnityEvent();
    public UnityEvent onObjectRotated = new UnityEvent();

    private bool hasPlacedFirstObject = false;

    [Header("Auto Spawn Settings")]
    [SerializeField] private bool autoSpawnEnabled = true;
    [SerializeField] private float autoSpawnDelay = 0.5f;
    private bool hasAutoSpawned = false;
    private float planeDetectionTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        aRRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();

        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogError("No prefabs assigned to ARObjectManager!");
            enabled = false;
        }

        // Initialize UI elements
        if (switchModelButton != null)
        {
            switchModelButton.onClick.AddListener(SwitchFemurModel);
            switchModelButton.gameObject.SetActive(false); // Hide button initially
        }

        // Update the initial text
        UpdateCurrentPrefabText();

        // Show the fader panel at start with the current prefab name
        if (prefabTextFader != null && prefabs != null && prefabs.Count > 0)
        {
            prefabTextFader.ShowPrefabText(prefabs[SelectedPrefabIndex].name);
        }

        // Subscribe to plane changed event
        if (arPlaneManager != null)
        {
            arPlaneManager.planesChanged += OnPlanesChanged;
        }
    }

    private void OnDestroy()
    {
        if (switchModelButton != null)
        {
            switchModelButton.onClick.RemoveListener(SwitchFemurModel);
        }

        // Unsubscribe from plane changed event
        if (arPlaneManager != null)
        {
            arPlaneManager.planesChanged -= OnPlanesChanged;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!autoSpawnEnabled || hasAutoSpawned) return;

        // Check if we have any valid planes
        if (arPlaneManager != null && arPlaneManager.trackables.count > 0)
        {
            // Start the timer
            planeDetectionTimer = autoSpawnDelay;
        }
    }

    private void Update()
    {
        // Handle auto-spawn timer
        if (autoSpawnEnabled && !hasAutoSpawned && planeDetectionTimer > 0)
        {
            planeDetectionTimer -= Time.deltaTime;
            
            if (planeDetectionTimer <= 0)
            {
                AutoSpawnObject();
            }
        }
    }

    private void AutoSpawnObject()
    {
        if (hasAutoSpawned) return;

        // Get the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (RaycastToPlane(screenCenter, out Pose pose))
        {
            Vector3 spawnPosition = pose.position + pose.up * floatHeight;
            
            // Spawn the object
            if (spawnedObjects.Count >= MAX_OBJECTS)
            {
                RemoveOldestObject();
            }
            SpawnNewObject(spawnPosition, pose);
            
            hasAutoSpawned = true;
            Debug.Log("Auto-spawned object on detected plane");
        }
    }

    // Add method to manually trigger auto-spawn
    public void TriggerAutoSpawn()
    {
        hasAutoSpawned = false;
        AutoSpawnObject();
    }

    public bool TryGetSpawnedObject(int index, out GameObject obj)
    {
        return spawnedObjects.TryGetValue(index, out obj);
    }

    public bool TryGetCurrentObject(out GameObject obj)
    {
        return spawnedObjects.TryGetValue(SelectedPrefabIndex, out obj);
    }

    public void DeleteCurrentObject()
    {
        if (spawnedObjects.TryGetValue(SelectedPrefabIndex, out GameObject obj))
        {
            // Hide switch model button if deleting the femur or any object
            if (switchModelButton != null)
            {
                switchModelButton.gameObject.SetActive(SelectedPrefabIndex == 0 && spawnedObjects.ContainsKey(SelectedPrefabIndex));
            }

            Destroy(obj);
            spawnedObjects.Remove(SelectedPrefabIndex);

            var newQueue = new Queue<int>(objectSpawnOrder.Where(index => index != SelectedPrefabIndex));
            objectSpawnOrder.Clear();
            foreach (var index in newQueue)
            {
                objectSpawnOrder.Enqueue(index);
            }

            Debug.Log($"Deleted object with index {SelectedPrefabIndex}. Remaining objects: {spawnedObjects.Count}");
        }
        else
        {
            Debug.LogWarning($"Attempted to delete object with index {SelectedPrefabIndex}, but it doesn't exist.");
        }
    }

    public bool RaycastToPlane(Vector2 screenPosition, out Pose pose)
    {
        pose = default;

        if (aRRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager is not initialized!");
            return false;
        }

        if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            pose = hits[0].pose;
            return true;
        }
        return false;
    }

    public void SpawnOrMoveObject(Vector2 touchPosition)
    {
        if (!RaycastToPlane(touchPosition, out Pose pose))
        {
            Debug.Log("No plane detected at touch position.");
            return;
        }

        Vector3 floatPosition = pose.position + pose.up * floatHeight;

        if (spawnedObjects.TryGetValue(SelectedPrefabIndex, out GameObject existingObject))
        {
            existingObject.transform.SetPositionAndRotation(floatPosition, pose.rotation);
            Debug.Log($"Moved object with index {SelectedPrefabIndex} to new position.");
        }
        else
        {
            if (spawnedObjects.Count >= MAX_OBJECTS)
            {
                RemoveOldestObject();
            }
            SpawnNewObject(floatPosition, pose);
        }

        OrientObjectTowardsCamera(pose.up);
    }

    private void SpawnNewObject(Vector3 position, Pose pose)
    {
        if (SelectedPrefabIndex < 0 || SelectedPrefabIndex >= prefabs.Count)
        {
            Debug.LogError($"Invalid prefab index: {SelectedPrefabIndex}. Available prefabs: {prefabs.Count}");
            return;
        }

        // Ensure the object limit is respected
        if (spawnedObjects.ContainsKey(SelectedPrefabIndex))
        {
            Destroy(spawnedObjects[SelectedPrefabIndex]);
            spawnedObjects.Remove(SelectedPrefabIndex);
            Queue<int> newQueue = new Queue<int>(objectSpawnOrder.Where(i => i != SelectedPrefabIndex));
            objectSpawnOrder.Clear();
            foreach (int i in new List<int>(newQueue))
            {
                objectSpawnOrder.Enqueue(i);
            }
        }

        GameObject newObject = Instantiate(prefabs[SelectedPrefabIndex], position, Quaternion.identity);
        
        // Log initial state
        Debug.Log($"[Spawn] Initial scale of new object: {newObject.transform.localScale}");
        
        // Get and apply the scale factor
        float scaleFactor = GetScalingFactorForPrefab(SelectedPrefabIndex);
        Vector3 assignedScale = prefabs[SelectedPrefabIndex].transform.localScale;
        
        // Apply scale: always use prefab's assigned scale from the Inspector
        if (assignedScale != Vector3.zero)
        {
            newObject.transform.localScale = assignedScale * scaleFactor;
            Debug.Log($"[Spawn] Applied scale: Assigned({assignedScale}) * Factor({scaleFactor}) = Final({newObject.transform.localScale})");
        }
        else
        {
            Debug.LogWarning($"[Spawn] Assigned prefab scale is zero! Setting default scale of {scaleFactor}");
            newObject.transform.localScale = Vector3.one * scaleFactor;
        }
        
        EnsureColliders(newObject);

        spawnedObjects[SelectedPrefabIndex] = newObject;
        objectSpawnOrder.Enqueue(SelectedPrefabIndex);

        // Show or hide switch model button based on current prefab
        if (switchModelButton != null)
        {
            switchModelButton.gameObject.SetActive(SelectedPrefabIndex == 0);
        }

        Debug.Log($"[Spawn] Final object scale after all operations: {newObject.transform.localScale}");

        // Notify tutorial system of first object placement
        if (!hasPlacedFirstObject)
        {
            hasPlacedFirstObject = true;
            onFirstObjectPlaced?.Invoke();
        }
    }

    private void RemoveOldestObject()
    {
        if (objectSpawnOrder.Count > 0)
        {
            int oldestPrefabIndex = objectSpawnOrder.Dequeue();
            if (spawnedObjects.TryGetValue(oldestPrefabIndex, out GameObject oldestObject))
            {
                Debug.Log($"Removing oldest object with index {oldestPrefabIndex} to make room for new object. Current count: {spawnedObjects.Count}");
                Destroy(oldestObject);
                spawnedObjects.Remove(oldestPrefabIndex);
            }
        }
        else
        {
            if (spawnedObjects.Count > 0)
            {
                var firstKey = spawnedObjects.Keys.First();
                if (spawnedObjects.TryGetValue(firstKey, out GameObject oldestObject))
                {
                    Debug.Log($"Queue empty, removing object with index {firstKey}. Current count: {spawnedObjects.Count}");
                    Destroy(oldestObject);
                    spawnedObjects.Remove(firstKey);
                }
            }
            else
            {
                Debug.LogWarning("Attempted to remove oldest object, but no objects exist.");
            }
        }
    }

    private void OrientObjectTowardsCamera(Vector3 upDirection)
    {
        if (!spawnedObjects.TryGetValue(SelectedPrefabIndex, out GameObject currentObject))
            return;

        if (Camera.main == null)
        {
            Debug.LogError("Camera.main is null! Make sure your camera is tagged as 'MainCamera'.");
            return;
        }

        Vector3 directionToFace = Camera.main.transform.position - currentObject.transform.position;
        directionToFace.y = 0;

        if (directionToFace.sqrMagnitude > 0.001f)
        {
            currentObject.transform.rotation = Quaternion.LookRotation(directionToFace, upDirection);
        }
    }

    private float GetScalingFactorForPrefab(int prefabIndex)
    {
        if (prefabs == null || prefabIndex < 0 || prefabIndex >= prefabs.Count)
        {
            Debug.LogError($"Invalid prefab index {prefabIndex} for scaling");
            return 1.0f;
        }

        string prefabName = prefabs[prefabIndex].name;
        Debug.Log($"[Scaling] Calculating scale for prefab: {prefabName} (index: {prefabIndex})");

        // Get the original scale of the prefab
        Vector3 prefabOriginalScale = prefabs[prefabIndex].transform.localScale;
        Debug.Log($"[Scaling] Original prefab scale: {prefabOriginalScale}");

        float scaleFactor = 1.0f;

        // First check if the prefab has a custom scale component
        if (prefabs[prefabIndex].TryGetComponent<CustomScale>(out var customScale))
        {
            scaleFactor = customScale.scaleFactor;
            Debug.Log($"[Scaling] Using custom scale factor: {scaleFactor} from CustomScale component");
        }
        // Then check prefab name keywords
        else if (prefabName.ToLower().Contains("small") || prefabName.ToLower().Contains("tiny"))
        {
            scaleFactor = 0.5f;
            Debug.Log($"[Scaling] Using small scale factor: {scaleFactor} based on name");
        }
        else if (prefabName.ToLower().Contains("large") || prefabName.ToLower().Contains("big"))
        {
            scaleFactor = 2.0f;
            Debug.Log($"[Scaling] Using large scale factor: {scaleFactor} based on name");
        }
        else
        {
            // Fallback to index-based scaling
            switch (prefabIndex)
            {
                case 0: // Femur
                    scaleFactor = 0.5f;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    scaleFactor = 25.0f;
                    break;
                case 5:
                case 6:
                case 7:
                    scaleFactor = 0.5f;
                    break;
                default:
                    scaleFactor = 1.0f;
                    break;
            }
            Debug.Log($"[Scaling] Using index-based scale factor: {scaleFactor} for index {prefabIndex}");
        }

        Debug.Log($"[Scaling] Final scale factor for {prefabName}: {scaleFactor}");
        return scaleFactor;
    }

    private void EnsureColliders(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("Cannot add colliders to null object!");
            return;
        }

        obj.tag = "MainBone";

        // Add BoxCollider to the main object if it doesn't exist
        if (!obj.TryGetComponent<BoxCollider>(out _))
        {
            var mainBoxCollider = obj.AddComponent<BoxCollider>();
            // Adjust box collider size based on mesh bounds
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                mainBoxCollider.size = meshFilter.sharedMesh.bounds.size;
                mainBoxCollider.center = meshFilter.sharedMesh.bounds.center;
            }
        }

        // Add MeshColliders to all child objects with MeshFilters
        foreach (var meshFilter in obj.GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter == null) continue;

            try
            {
                if (!meshFilter.TryGetComponent<MeshCollider>(out var meshCollider))
                {
                    meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                    
                    // Check if mesh is accessible
                    if (meshFilter.sharedMesh != null)
                    {
                        // For non-readable meshes, we'll use a simpler approach
                        if (!meshFilter.sharedMesh.isReadable)
                        {
                            Debug.LogWarning($"Mesh '{meshFilter.sharedMesh.name}' is not readable. Using a simplified collider setup.");
                            // Create a new mesh collider with the original mesh
                            // This will work as long as the mesh is properly imported
                            meshCollider.sharedMesh = meshFilter.sharedMesh;
                            
                            // If that fails, fall back to a box collider
                            if (meshCollider.sharedMesh == null)
                            {
                                var childBoxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                                childBoxCollider.size = meshFilter.sharedMesh.bounds.size;
                                childBoxCollider.center = meshFilter.sharedMesh.bounds.center;
                            }
                        }
                        else
                        {
                            meshCollider.sharedMesh = meshFilter.sharedMesh;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Mesh is null on object: {meshFilter.gameObject.name}");
                        // Fallback to a simple box collider if mesh is null
                        var childBoxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                        childBoxCollider.size = Vector3.one;
                        childBoxCollider.center = Vector3.zero;
                    }
                }
                meshFilter.gameObject.tag = "Bone";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up collider for {meshFilter.gameObject.name}: {e.Message}");
                // Fallback to a simple box collider if mesh collider fails
                if (!meshFilter.gameObject.TryGetComponent<BoxCollider>(out _))
                {
                    var childBoxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                    childBoxCollider.size = meshFilter.sharedMesh != null ? meshFilter.sharedMesh.bounds.size : Vector3.one;
                    childBoxCollider.center = meshFilter.sharedMesh != null ? meshFilter.sharedMesh.bounds.center : Vector3.zero;
                }
            }
        }
    }

    public void CyclePrefab()
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("Cannot cycle prefabs: prefab list is empty or null.");
            return;
        }

        SelectedPrefabIndex = (SelectedPrefabIndex + 1) % prefabs.Count;
        UpdateCurrentPrefabText();

        // Show the fader panel when cycling
        if (prefabTextFader != null && prefabs != null && prefabs.Count > 0)
        {
            prefabTextFader.ShowPrefabText(prefabs[SelectedPrefabIndex].name);
        }
    }

    private void UpdateCurrentPrefabText()
    {
        if (currentPrefabText != null && prefabs != null && prefabs.Count > 0)
        {
            string prefabName = prefabs[SelectedPrefabIndex].name;
            currentPrefabText.text = $"Current Prefab: {prefabName}";
        }
        // Show switchModelButton only for femur (index 0), hide otherwise
        if (switchModelButton != null)
        {
            switchModelButton.gameObject.SetActive(SelectedPrefabIndex == 0);
        }
    }

    public bool GetObjectPose(int index, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (spawnedObjects.TryGetValue(index, out GameObject obj))
        {
            position = obj.transform.position;
            rotation = obj.transform.rotation;
            return true;
        }

        return false;
    }

    private void SwitchFemurModel()
    {
        if (femurVariant == null)
        {
            Debug.LogWarning("No femur variant assigned!");
            return;
        }

        if (!spawnedObjects.TryGetValue(0, out GameObject currentFemur))
        {
            Debug.LogWarning("No femur object found to switch!");
            return;
        }

        try
        {
            // Store the current transform
            Vector3 position = currentFemur.transform.position;
            Quaternion rotation = currentFemur.transform.rotation;
            Vector3 scale = currentFemur.transform.localScale;

            // Destroy the current object and all its children
            foreach (Transform child in currentFemur.transform)
            {
                Destroy(child.gameObject);
            }
            Destroy(currentFemur);

            // Spawn new variant or original based on current state
            GameObject newFemur;
            if (!isUsingVariant)
            {
                newFemur = Instantiate(femurVariant, position, rotation);
                isUsingVariant = true;
            }
            else
            {
                newFemur = Instantiate(prefabs[0], position, rotation);
                isUsingVariant = false;
            }

            // Apply the same transform
            newFemur.transform.localScale = scale;

            // Set up colliders and tags
            if (newFemur != null)
            {
                // Set main object tag
                newFemur.tag = "MainBone";

                // Add BoxCollider to main object if it doesn't exist
                if (!newFemur.TryGetComponent<BoxCollider>(out _))
                {
                    var mainBoxCollider = newFemur.AddComponent<BoxCollider>();
                    // Adjust box collider size based on mesh bounds
                    var meshFilter = newFemur.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        mainBoxCollider.size = meshFilter.sharedMesh.bounds.size;
                        mainBoxCollider.center = meshFilter.sharedMesh.bounds.center;
                    }
                }

                // Add MeshColliders to all child objects with MeshFilters
                foreach (var meshFilter in newFemur.GetComponentsInChildren<MeshFilter>())
                {
                    if (meshFilter == null) continue;

                    try
                    {
                        // Set child object tag first
                        meshFilter.gameObject.tag = "Bone";

                        if (!meshFilter.TryGetComponent<MeshCollider>(out var meshCollider))
                        {
                            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                            meshCollider.convex = true;
                            
                            // Check if mesh is accessible
                            if (meshFilter.sharedMesh != null)
                            {
                                meshCollider.sharedMesh = meshFilter.sharedMesh;
                            }
                            else
                            {
                                Debug.LogError($"Mesh is null on object: {meshFilter.gameObject.name}");
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error setting up collider for {meshFilter.gameObject.name}: {e.Message}");
                        // Fallback to a simple box collider if mesh collider fails
                        if (!meshFilter.gameObject.TryGetComponent<BoxCollider>(out _))
                        {
                            var childBoxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                            childBoxCollider.size = meshFilter.sharedMesh != null ? meshFilter.sharedMesh.bounds.size : Vector3.one;
                            childBoxCollider.center = meshFilter.sharedMesh != null ? meshFilter.sharedMesh.bounds.center : Vector3.zero;
                        }
                    }
                }
            }

            // Update the spawned objects dictionary
            spawnedObjects[0] = newFemur;

            Debug.Log($"Switched femur model. Using variant: {isUsingVariant}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error switching femur model: {e.Message}");
        }
    }

   
    private void ApplyOriginalMaterials(GameObject newObj, Dictionary<Renderer, Material[]> originalMaterials)
    {
        Debug.Log("\n=== Applying Original Materials ===");
        var newRenderers = newObj.GetComponentsInChildren<Renderer>();
        
        foreach (var newRenderer in newRenderers)
        {
            Debug.Log($"\nProcessing renderer: {newRenderer.gameObject.name}");
            
            // Try to find a matching renderer in the original materials
            var matchingRenderer = originalMaterials.Keys.FirstOrDefault(r => 
                r.gameObject.name.Contains(newRenderer.gameObject.name) || 
                newRenderer.gameObject.name.Contains(r.gameObject.name));

            if (matchingRenderer != null && originalMaterials.TryGetValue(matchingRenderer, out var materials))
            {
                Debug.Log($"Found matching renderer: {matchingRenderer.gameObject.name}");
                Debug.Log($"Original materials count: {materials.Length}");
                Debug.Log($"New renderer materials count: {newRenderer.sharedMaterials.Length}");
                
                // Ensure material arrays match in length
                if (materials.Length != newRenderer.sharedMaterials.Length)
                {
                    Debug.LogWarning($"Material count mismatch! Original: {materials.Length}, New: {newRenderer.sharedMaterials.Length}");
                }
                
                newRenderer.sharedMaterials = materials;
                Debug.Log($"Applied {materials.Length} materials to {newRenderer.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"No matching original materials found for: {newRenderer.gameObject.name}");
            }
        }
        
        Debug.Log("=== End Applying Materials ===\n");
    }

    // Add these methods to notify the tutorial system
    public void NotifyObjectMoved()
    {
        onObjectMoved?.Invoke();
    }

    public void NotifyObjectScaled()
    {
        onObjectScaled?.Invoke();
    }

    public void NotifyObjectRotated()
    {
        onObjectRotated?.Invoke();
    }
}