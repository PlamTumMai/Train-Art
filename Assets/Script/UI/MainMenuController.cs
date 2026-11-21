using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Name")]
    [Tooltip("ใส่ชื่อซีน Gameplay ให้ตรงตัวพิมพ์เล็ก-ใหญ่")]
    public string gameplaySceneName = "SampleScene";

    public void StartGame()
    {
        // รีเซ็ตเวลากลับเป็นปกติ เผื่อผู้เล่นกดกลับมาหน้านี้หลังจาก Game Over
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

        // ช่วยให้ทดสอบกดปุ่มออกในหน้า Unity Editor ได้
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}