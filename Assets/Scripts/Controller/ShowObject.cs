using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowObject : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    public void show()
    {
        obj.SetActive(true);
    }
}
