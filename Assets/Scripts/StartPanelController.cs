using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class StartPanelController : MonoBehaviour
{
    private const string MainSceneName = "Main1";


    public void EnterMainScene()
    {
        SceneManager.LoadScene(MainSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
