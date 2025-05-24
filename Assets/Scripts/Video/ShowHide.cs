using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHide : MonoBehaviour
{
    [SerializeField] private GameObject buttonFullScreen;
    [SerializeField] private GameObject buttonMinScreen;
    [SerializeField] private GameObject titleCover;

    public void showHide()
    {
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            if (buttonMinScreen.activeSelf)
            {
                buttonMinScreen.SetActive(false);
                titleCover.SetActive(false);
            }
            else
            {
                buttonMinScreen.SetActive(true);
                titleCover.SetActive(true);
            }
        }
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            if (buttonFullScreen.activeSelf)
            {
                buttonFullScreen.SetActive(false);
                titleCover.SetActive(false);
            }
            else
            {
                buttonFullScreen.SetActive(true);
                titleCover.SetActive(true);
            }
        }
    }
}
