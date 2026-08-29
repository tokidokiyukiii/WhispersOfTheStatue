using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    [Header("UI References")]
    public GameObject startPanel; // Optional: if you have animations/toggles

    [Header("Scene Settings")]
    public string gameSceneName = "BestVer";

    void Start()
    {
        // Ensure time is normal in menu (no gameplay running)
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        Debug.Log("[MainMenu] Starting game...");

        // Load the game scene (additive or single)
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
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

    // Optional: if you want a "Return to Menu" button from game over UI
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
