using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GazeInteraction : MonoBehaviour
{
    public float gazeTime = 2.0f; // Time required to gaze before triggering interaction

    public Material active; // Material to apply when the object is in focus
    private Material originalMaterial; // Store the original material to revert back
    private GameObject gazedObject = null; // Object currently being gazed upon
    private float gazeTimer = 0.0f;

    [SerializeField]
    private Button deleteButton; // Button to delete the currently gazed object

    [SerializeField]
    private TextMeshProUGUI gazeObjectNameText; // TextMeshProUGUI to display the name of the gazed object

    private void Start()
    {
        // Add listener to the delete button
        deleteButton.onClick.AddListener(DeleteGazedObject);
    }

    private void Update()
    {
        HandleGazeInteraction();
    }

    private void HandleGazeInteraction()
    {
        // Use Camera.main instead of arCamera
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);

            // Check if the hit object is tagged as "Bone" or "MainBone"
            if (hit.collider.gameObject.CompareTag("Bone") || hit.collider.gameObject.CompareTag("MainBone"))
            {
                // If we hit the main bone's collider, try to find a child bone collider instead
                if (hit.collider.gameObject.CompareTag("MainBone") && hit.collider is BoxCollider)
                {
                    // Try to find a child bone collider
                    bool foundChildBone = false;
                    foreach (Transform child in hit.collider.transform)
                    {
                        if (child.CompareTag("Bone"))
                        {
                            // Cast a new ray to check if we're actually looking at this child
                            Ray childRay = new Ray(Camera.main.transform.position, (child.position - Camera.main.transform.position).normalized);
                            RaycastHit childHit;
                            if (Physics.Raycast(childRay, out childHit) && childHit.collider.gameObject == child.gameObject)
                            {
                                // We're looking at a child bone, use that instead
                                hit = childHit;
                                foundChildBone = true;
                                break;
                            }
                        }
                    }
                    
                    // If we didn't find a child bone we're actually looking at, ignore this hit
                    if (!foundChildBone)
                    {
                        if (gazedObject != null)
                        {
                            RevertMaterial(gazedObject);
                            gazedObject = null;
                            UpdateGazeObjectNameText(string.Empty);
                        }
                        gazeTimer = 0.0f;
                        return;
                    }
                }

                // Start or continue the gaze timer
                if (gazedObject == hit.transform.gameObject)
                {
                    gazeTimer += Time.deltaTime;

                    // Trigger the interaction if gaze time exceeds the threshold
                    if (gazeTimer >= gazeTime)
                    {
                        TriggerGazeInteraction(gazedObject);
                    }
                }
                else
                {
                    // If a new object is gazed at, reset the timer and revert the previous object's material
                    if (gazedObject != null)
                    {
                        RevertMaterial(gazedObject);
                    }

                    gazedObject = hit.transform.gameObject;
                    gazeTimer = 0.0f;

                    // Store the original material to revert back later
                    StoreOriginalMaterial(gazedObject);

                    // Update the TextMeshPro text with the gazed object's name
                    UpdateGazeObjectNameText(gazedObject.name);
                }
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit any object.");

            // Reset if no object is gazed at, and revert the previous object's material
            if (gazedObject != null)
            {
                RevertMaterial(gazedObject);
                gazedObject = null;

                // Clear the TextMeshPro text
                UpdateGazeObjectNameText(string.Empty);
            }
            gazeTimer = 0.0f;
        }
    }

    private void TriggerGazeInteraction(GameObject obj)
    {
        // Apply the active material to the object's mesh
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = active; // Change to active material
        }

        Debug.Log("Gaze interaction triggered on: " + obj.name);
    }

    private void RevertMaterial(GameObject obj)
    {
        // Revert the material of the object's mesh to the original
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = originalMaterial; // Revert to original material
        }

        Debug.Log("Material reverted on: " + obj.name);
    }

    private void StoreOriginalMaterial(GameObject obj)
    {
        // Store the original material of the object's mesh
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
    }

    private void UpdateGazeObjectNameText(string objectName)
    {
        // Update the TextMeshPro text with the object's name
        gazeObjectNameText.text = objectName;
    }

    private void DeleteGazedObject()
    {
        if (gazedObject != null)
        {
            // If we're gazing at a child bone, find its parent MainBone
            GameObject objectToDelete = gazedObject;
            
            // If the gazed object is a child bone, find its parent MainBone
            if (gazedObject.CompareTag("Bone"))
            {
                Transform parent = gazedObject.transform.parent;
                while (parent != null && !parent.CompareTag("MainBone"))
                {
                    parent = parent.parent;
                }
                
                if (parent != null)
                {
                    objectToDelete = parent.gameObject;
                    Debug.Log("Found parent MainBone: " + objectToDelete.name);
                }
                else
                {
                    Debug.LogWarning("Could not find parent MainBone for: " + gazedObject.name);
                    return;
                }
            }
            else if (!gazedObject.CompareTag("MainBone"))
            {
                Debug.Log("Cannot delete object without MainBone or Bone tag: " + gazedObject.name);
                return;
            }
            
            // Delete the object
            Destroy(objectToDelete);
            Debug.Log("Deleted object: " + objectToDelete.name);
            gazedObject = null;
            UpdateGazeObjectNameText(string.Empty);
        }
        else
        {
            Debug.Log("No valid object to delete.");
        }
    }
}
