using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeTitle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Header;
    [SerializeField] private TextMeshProUGUI BtnText;
    private string originalTitle;

    private bool isToggled = false;
    void Start()
    {
        // Save the initial text as the original title
        originalTitle = Header.text;
    }

    // Function to change the title to the button's text
    public void changeTitle()
    {
        Header.text = BtnText.text;
    }

    // Toggle function to switch between two titles
    public void ToggleTitle()
    {
        if (isToggled)
        {
            Header.text = originalTitle;
        }
        else
        {
            
            Header.text = BtnText.text;
        }

        isToggled = !isToggled;
    }
}

