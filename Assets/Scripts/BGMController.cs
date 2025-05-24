using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    public GameObject imgSound;
    public GameObject imgMute;
    private bool mute = false;

    void Start()
    {
        if (!PlayerPrefs.HasKey(Utility.Mute))
        {
            PlayerPrefs.SetInt(Utility.Mute, 0);
            Load();
        }
        else
        {
            Load();
        }
        UpdateButtonImage();
        AudioListener.pause = mute;
    }
    public void SoundOnOff()
    {
        if (mute)
        {
            Debug.Log("Continue playing BGM");
            AudioListener.pause = false;

            mute = false;
        }
        else
        {
            Debug.Log("Pause BGM");
            AudioListener.pause = true;

            mute = true;
        }
        UpdateButtonImage();
        Save();
    }

    private void UpdateButtonImage()
    {
        if (mute)
        {
            imgSound.SetActive(false);
            imgMute.SetActive(true);
        }
        else
        {
            imgSound.SetActive(true);
            imgMute.SetActive(false);
        }
    }
    private void Load()
    {
        mute = PlayerPrefs.GetInt(Utility.Mute) == 1;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(Utility.Mute, mute ? 1 : 0);
    }
}
