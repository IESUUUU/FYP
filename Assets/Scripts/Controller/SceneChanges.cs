using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanges : MonoBehaviour
{
    static int lastSceneIndex;
    public void LoadScene(string sceneName)
    {
        lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneName);
    }
    public void ReturnToLastScene()
    {
        SceneManager.LoadScene(lastSceneIndex);
    }
}
