// Scripts/Systems/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float gameDuration = 600f; // 10 minutes
    public float timeElapsed;
    public bool isGameOver;

    void Awake() => Instance = this;

    void Update()
    {
        if (isGameOver) return;

        timeElapsed += Time.deltaTime;
        if (timeElapsed >= gameDuration)
            WinGame();
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        SceneManager.LoadScene("GameOver");
    }

    public void WinGame()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("You survived!");
    }
}