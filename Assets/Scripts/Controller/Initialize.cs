using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    [SerializeField] private GameObject signInButton;
    [SerializeField] private GameObject loggedInMenu;
    [SerializeField] private GameObject profileButton;
    [SerializeField] private TMP_Text welcomeText;
    private bool firstStartUp = false;
    void Start()
    {
        if (PlayerPrefs.HasKey(Utility.PrefsUserID) &&
            PlayerPrefs.GetString(Utility.PrefsUserID) != "")
        {
            signInButton.SetActive(false);
            welcomeText.text += PlayerPrefs.GetString(Utility.PrefUsername);
            loggedInMenu.SetActive(true);
            profileButton.SetActive(true);
        }
        else
        {
            loggedInMenu.SetActive(false);
        }

        /*if (!PlayerPrefs.HasKey(Utility.FirstStartUp))
        {
            PlayerPrefs.SetInt(Utility.FirstStartUp, 1);
            firstStartUp = PlayerPrefs.GetInt(Utility.Mute) == 0;
        }
        else
        {
            PlayerPrefs.SetInt(Utility.FirstStartUp, 0);
        }*/
    }
}
