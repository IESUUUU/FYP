using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveNotes : MonoBehaviour
{
    [SerializeField] private string title;
    [SerializeField] private GameObject notes;
    [SerializeField] private GameObject placeHolder;
    void Start()
    {
        if (PlayerPrefs.HasKey(title))
            placeHolder.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString(title);
    }
    public void SaveNote()
    {
        PlayerPrefs.SetString(title, notes.GetComponent<TMP_InputField>().text);
        // set the timestamp also
        Utility.SavePrefsTimestamp();

        //Debug.Log("Start time (upload to firebase): " + Time.time * 1000);
        StartCoroutine(Utility.uploadFirebase());
        //Debug.Log("Finish time (upload to firebase): " + Time.time * 1000);
    }
}
