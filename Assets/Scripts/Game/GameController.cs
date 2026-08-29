using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Game Settings")]
    public string menuSceneName = "MainMenu";

    public void ReturnToMenu()
    {
        Debug.Log("[GameController] Returning to menu...");
        Time.timeScale = 1f; // Unpause before scene change
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }
    public void QuitGame()
    {
        Debug.Log("Quit button pressed");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Debug.LogWarning("Quit not supported on WebGL");
#else
        Application.Quit();
#endif
    }
}
