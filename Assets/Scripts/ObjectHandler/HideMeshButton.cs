using UnityEngine;
using UnityEngine.UI;

public class HideSubMesh : MonoBehaviour
{
    // Array to hold all sub-mesh renderers
    private MeshRenderer[] subMeshRenderers;
    private bool[] subMeshVisibility;

    void Start()
    {
        // Get all MeshRenderer components in the child objects
        subMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        subMeshVisibility = new bool[subMeshRenderers.Length];

        // Initialize all sub-meshes to visible
        for (int i = 0; i < subMeshVisibility.Length; i++)
        {
            subMeshVisibility[i] = true;
        }
    }

    // Public method to toggle the visibility of a specific sub-mesh, called from the button
    public void ToggleSubMeshByIndex(int index)
    {
        if (index >= 0 && index < subMeshRenderers.Length)
        {
            // Toggle visibility
            subMeshVisibility[index] = !subMeshVisibility[index];
            subMeshRenderers[index].enabled = subMeshVisibility[index];
        }
        else
        {
            Debug.LogWarning("Sub-mesh index out of bounds!");
        }
    }
}
