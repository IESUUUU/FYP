using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyperlink : MonoBehaviour
{
    public void OpenUrl(string link)
    {
        Application.OpenURL(link);
    }
}
