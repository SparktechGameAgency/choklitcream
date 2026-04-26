using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Panels")]
    public GameObject youWinPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        Instance = this;

        // Make sure both panels are hidden at the start
        if (youWinPanel != null) youWinPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // ── Called when the timer runs out ───────────────────────
    public void ShowWinPanel()
    {
        if (youWinPanel == null) return;

        Time.timeScale = 0f;          // freeze the game
        youWinPanel.SetActive(true);
    }

    // ── Called when the player dies ──────────────────────────
    public void ShowGameOverPanel()
    {
        if (gameOverPanel == null) return;

        Time.timeScale = 0f;          // freeze the game
        gameOverPanel.SetActive(true);
    }

    // ── Quit button on the canvas ────────────────────────────
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Optional: restart from either panel ──────────────────
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}