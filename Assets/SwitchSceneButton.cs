using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSceneButton : MonoBehaviour
{
    public void SwitchScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
