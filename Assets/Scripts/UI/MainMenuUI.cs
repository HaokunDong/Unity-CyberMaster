using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // 场景名必须与 Build Settings 中一致
    public string bossSceneName = "Bossroom";

    public void StartGame()
    {
        SceneManager.LoadScene(bossSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
