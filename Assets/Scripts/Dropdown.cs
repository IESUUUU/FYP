using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dropdown : MonoBehaviour, IPointerClickHandler
{
    public string ContainerName;
    public RectTransform container;
    public float openHeight = 200f;
    public float closedHeight = 0f;
    public bool isOpen = false;
    public float animationSpeed = 12f;
    private float currentHeight;
    private float targetHeight;
    private bool containerHidden = true;
    public bool isMainDropdown = true;
    public GameObject[] buttonsToShow; // Buttons that should always be visible (like NOF)

    void Start()
    {
        // Initialize the container
        if (container == null)
        {
            container = transform.Find(ContainerName)?.GetComponent<RectTransform>();
        }

        if (container != null)
        {
            // Set up Vertical Layout Group
            VerticalLayoutGroup verticalLayout = container.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            verticalLayout.padding = new RectOffset(5, 5, 5, 5);
            verticalLayout.spacing = 5f;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;

            // Initial state
            isOpen = false;
            currentHeight = closedHeight;
            targetHeight = closedHeight;
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, closedHeight);

            // Initially hide container but keep specified buttons visible
            container.gameObject.SetActive(false);

            // Make sure specified buttons are visible
            if (buttonsToShow != null)
            {
                foreach (var button in buttonsToShow)
                {
                    if (button != null)
                    {
                        button.SetActive(true);
                    }
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleDropdown();
        eventData.Use();
    }

    private void ToggleDropdown()
    {
        if (container != null)
        {
            isOpen = !isOpen;
            targetHeight = isOpen ? openHeight : closedHeight;

            if (isOpen)
            {
                container.gameObject.SetActive(true);
                containerHidden = false;
                
                // Show all children in container
                foreach (Transform child in container.transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
            else
            {
                // When closing, hide all except specified buttons
                foreach (Transform child in container.transform)
                {
                    bool shouldStayVisible = false;
                    if (buttonsToShow != null)
                    {
                        foreach (var button in buttonsToShow)
                        {
                            if (button == child.gameObject)
                            {
                                shouldStayVisible = true;
                                break;
                            }
                        }
                    }
                    
                    if (!shouldStayVisible)
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                // Close any child dropdowns
                Dropdown[] childDropdowns = container.GetComponentsInChildren<Dropdown>(true);
                foreach (var childDropdown in childDropdowns)
                {
                    if (childDropdown != this)
                    {
                        childDropdown.ForceClose();
                    }
                }
            }
        }
    }

    public void ForceClose()
    {
        if (isOpen)
        {
            isOpen = false;
            targetHeight = closedHeight;
            if (container != null)
            {
                // Keep specified buttons visible even when forced to close
                foreach (Transform child in container.transform)
                {
                    bool shouldStayVisible = false;
                    if (buttonsToShow != null)
                    {
                        foreach (var button in buttonsToShow)
                        {
                            if (button == child.gameObject)
                            {
                                shouldStayVisible = true;
                                break;
                            }
                        }
                    }
                    
                    child.gameObject.SetActive(shouldStayVisible);
                }
                
                container.gameObject.SetActive(false);
                containerHidden = true;
            }
        }
    }

    void Update()
    {
        if (container != null)
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * animationSpeed);
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);

            if (!isOpen && Mathf.Abs(currentHeight - closedHeight) < 0.01f && !containerHidden)
            {
                container.gameObject.SetActive(false);
                containerHidden = true;
            }
        }
    }
}
