using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    [Header("Game Speed Settings")]
    public float normalSpeed = 1f;
    public float fastSpeed = 2f;
    public float veryFastSpeed = 3f;

    public float CurrentSpeed { get; private set; } = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = normalSpeed;
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    // เปลี่ยนเป็นความเร็วปกติ
    public void SetSpeed1x()
    {
        SetGameSpeed(normalSpeed);
    }

    // เร่งความเร็ว 2 เท่า
    public void SetSpeed2x()
    {
        SetGameSpeed(fastSpeed);
    }

    // เร่งความเร็ว 3 เท่า
    public void SetSpeed3x()
    {
        SetGameSpeed(veryFastSpeed);
    }

    void SetGameSpeed(float speed)
    {
        if (!CanChangeSpeed()) return;

        CurrentSpeed = speed;
        Time.timeScale = CurrentSpeed;

        Debug.Log($"Game Speed: {CurrentSpeed}x");
    }

    bool CanChangeSpeed()
    {
        if (GameManager.Instance == null) return false;

        GameState state = GameManager.Instance.CurrentState;

        return state == GameState.Gameplay ||
               state == GameState.WaitingWaveClear;
    }

    void HandleStateChanged(GameState state)
    {
        // ออกจาก Gameplay ให้กลับเป็นความเร็วปกติ
        if (state == GameState.Station ||
            state == GameState.GameOver ||
            state == GameState.Victory)
        {
            Time.timeScale = normalSpeed;
            return;
        }

        // กลับเข้าสู่ Gameplay ใช้ความเร็วที่เลือกไว้
        if (state == GameState.Gameplay ||
            state == GameState.WaitingWaveClear)
        {
            Time.timeScale = CurrentSpeed;
        }
        else
        {
            Time.timeScale = normalSpeed;
        }
    }
}
