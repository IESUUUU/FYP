using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeUI : MonoBehaviour
{
    [SerializeField] private GameObject[] showObjects;
    [SerializeField] private GameObject[] hideObjects;
    [SerializeField] private TMP_InputField[] inputFields;

    public void ShowHide()
    {
        for (int i = 0; i < hideObjects.Length; i++)
        {
            hideObjects[i].SetActive(false);
        }
        for (int i = 0; i < showObjects.Length; i++)
        {
            showObjects[i].SetActive(true);
        }
        for (int i = 0; i < inputFields.Length; i++)
        {
            inputFields[i].text = string.Empty;
        }
    }
}
