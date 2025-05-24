using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float hold_time = 1.7f;
    void Start()
    {
        StartCoroutine(Hold());
    }

    IEnumerator Hold()
    {
        yield return new WaitForSeconds(hold_time);
        SceneManager.LoadScene(1);
    }
}
