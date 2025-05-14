using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainMenuUI : MonoBehaviour
{
    // 点击按钮时调用，传入场景名
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

    }


    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}