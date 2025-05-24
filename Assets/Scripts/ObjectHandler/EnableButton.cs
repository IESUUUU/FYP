using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableButton : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private Image img_active;
    [SerializeField] private Image img_inactive;
    void Update()
    {
        if (img_active.IsActive())
        {
            btn.enabled = true;
        }
        else
        {
            btn.enabled = false;
        }
    }
}
