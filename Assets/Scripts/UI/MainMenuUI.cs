using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    // 加载指定场景
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 退出游戏
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 🔁 重新加载当前场景
    public void ReloadCurrentScene()
    {
        // 先销毁 DontDestroyOnLoad 中的指定对象
        DestroyIfExists("CameraManager");
        DestroyIfExists("UIManager");
        DestroyIfExists("AudioManager");
        DestroyIfExists("PlayerManager");
        DestroyIfExists("EnemyManager");
        DestroyIfExists("SkillManager");
        DestroyIfExists("EventSystem");

        // 重新加载当前场景
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void DestroyIfExists(string objName)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
