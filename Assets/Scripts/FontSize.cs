using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FontSize : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            GetComponent<TextMeshProUGUI>().fontSize = GetScaledFontSize(18);
        }
    }

    private int GetScaledFontSize(int baseFontSize)
    {
        float uiScale = Screen.width / Screen.height;
        int scaledFontSize = Mathf.RoundToInt(baseFontSize * uiScale);
        //Debug.Log(Mathf.Min(Screen.height, Screen.width));
        Debug.Log(uiScale);
        return scaledFontSize;
    }
}
