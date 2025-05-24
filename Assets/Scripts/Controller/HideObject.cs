using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideObject : MonoBehaviour
{
    [SerializeField] private GameObject obj;
   
    public void Hide()
    {
        if (obj.activeSelf)
            obj.SetActive(false);
    }
}
