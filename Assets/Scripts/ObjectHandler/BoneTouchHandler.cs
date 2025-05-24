using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoneTouchHandler : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    private Transform highlight;
    private Transform selection;
    private Material originalMaterial;

    [SerializeField] private Material highlightMaterial;
    [SerializeField] private GameObject infoLabel;
    [SerializeField] private TextMeshProUGUI infoText;

    void Awake()
    {
        // disable the collider, unless user trigger the rotate button
        GetComponent<Collider>().enabled = false;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Input.touchCount == 1)
        {
            if (highlight != null)
            {
                highlight.GetComponent<MeshRenderer>().material = originalMaterial;
            }
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // highlight selected part
                highlight = hit.transform;
                if (highlight.CompareTag(hit.collider.tag) && highlight != selection)
                {
                    if (hit.collider.tag != "Untagged")
                    {
                        if (highlight.GetComponent<MeshRenderer>().material != highlightMaterial)
                        {
                            originalMaterial = highlight.GetComponent<MeshRenderer>().material;
                            highlight.GetComponent<MeshRenderer>().material = highlightMaterial;
                        }
                        else
                        {
                            highlight = null;
                        }
                    }
                    else
                    {
                        highlight = null;
                    }
                }
                openLabel(hit.collider.tag);
            }
        }
    }

    private void openLabel(string label)
    {
        if (label != "Untagged" && label != "cube")
        {
            Debug.Log("Part Tag: " + label);
            infoLabel.SetActive(true);
            infoText.text = label;
        }
    }
}
