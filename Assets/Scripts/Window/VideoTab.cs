using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoTab : MonoBehaviour
{
    public GameObject tabbutton1;
    public GameObject tabbutton2;
    public GameObject tabbutton3;

    public GameObject tabcontent1;
    public GameObject tabcontent2;
    public GameObject tabcontent3;

    private void HideAllTabs()
    {
        tabcontent1.SetActive(false);
        tabcontent2.SetActive(false);
        tabcontent3.SetActive(false);

        tabbutton1.GetComponent<Button>().image.color = new Color32(199, 233, 217, 255);
        tabbutton2.GetComponent<Button>().image.color = new Color32(199, 233, 217, 255);
        tabbutton3.GetComponent<Button>().image.color = new Color32(199, 233, 217, 255);
    }

    public void ShowTab1()
    {
        HideAllTabs();
        tabcontent1.SetActive(true);
        tabbutton1.GetComponent<Button>().image.color = new Color32(176, 221, 193, 255);
    }

    public void ShowTab2()
    {
        HideAllTabs();
        tabcontent2.SetActive(true);
        tabbutton2.GetComponent<Button>().image.color = new Color32(176, 221, 193, 255);
    }

    public void ShowTab3()
    {
        HideAllTabs();
        tabcontent3.SetActive(true);
        tabbutton3.GetComponent<Button>().image.color = new Color32(176, 221, 193, 255);
    }
}
