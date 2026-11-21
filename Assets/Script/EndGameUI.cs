using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    [Header("Panel & Texts")]
    public GameObject endGamePanel;
    public TMP_Text titleText;
    public TMP_Text statsText;

    [Header("Title Colors")]
    public Color victoryColor = new Color(1f, 0.85f, 0.2f, 1f);  // สีทอง
    public Color gameOverColor = new Color(1f, 0.25f, 0.25f, 1f); // สีแดง

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Settings")]
    [Tooltip("ชื่อซีนหน้าเริ่มเกม ต้องตรงตัวพิมพ์เล็ก-ใหญ่")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState state)
    {
        if (state != GameState.GameOver && state != GameState.Victory)
            return;

        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        bool isWin = (state == GameState.Victory);

        if (titleText != null)
        {
            titleText.text = isWin ? "VICTORY!" : "GAME OVER";
            titleText.color = isWin ? victoryColor : gameOverColor;
        }

        if (statsText != null && GameManager.Instance != null)
        {
            int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 1;
            int maxWaves = WaveManager.Instance != null ? WaveManager.Instance.maxWaves : 5;

            statsText.text =
                $"Wave: {wave}/{maxWaves}\n" +
                $"Kills: {GameManager.Instance.KillCount}\n" +
                $"Score: {GameManager.Instance.Score}";
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // กดจุด 3 จุดที่หัวสคริปต์นี้ตอนกด Play เพื่อเทสต์หน้าจบเกมได้ทันที
    [ContextMenu("Test Victory")]
    void TestVictory()
    {
        GameManager.Instance?.TriggerVictory();
    }

    [ContextMenu("Test Game Over")]
    void TestGameOver()
    {
        GameManager.Instance?.TriggerGameOver();
    }
}