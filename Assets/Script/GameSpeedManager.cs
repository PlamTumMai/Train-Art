using UnityEngine;
using TMPro;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    [Header("Game Speed Settings")]
    public float normalSpeed = 1f;
    public float fastSpeed = 2f;
    public float veryFastSpeed = 3f;

    [Header("UI References")]
    public TMP_Text speedButtonText;

    public float CurrentSpeed { get; private set; } = 1f;

    private int speedIndex = 0;

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
            Time.timeScale = 1f;
            Instance = null;
        }
    }

    // กดปุ่มเพื่อเปลี่ยนความเร็ว
    public void ToggleGameSpeed()
    {
        if (!CanChangeSpeed())
            return;

        speedIndex = (speedIndex + 1) % 3;

        switch (speedIndex)
        {
            case 0:
                CurrentSpeed = normalSpeed;
                break;

            case 1:
                CurrentSpeed = fastSpeed;
                break;

            case 2:
                CurrentSpeed = veryFastSpeed;
                break;
        }

        Time.timeScale = CurrentSpeed;

        UpdateSpeedButtonText();

        Debug.Log("Game Speed: " + CurrentSpeed + "x");
    }

    // ตรวจสอบว่าสามารถเปลี่ยนความเร็วได้หรือไม่
    bool CanChangeSpeed()
    {
        if (GameManager.Instance == null)
            return false;

        GameState state = GameManager.Instance.CurrentState;

        return state == GameState.Gameplay ||
               state == GameState.WaitingWaveClear;
    }

    // ตรวจสอบเมื่อ GameState เปลี่ยน
    void HandleStateChanged(GameState state)
    {
        if (state == GameState.Station)
        {
            ResetSpeed();
        }
    }

    // รีเซ็ตความเร็วกลับเป็น 1x
    public void ResetSpeed()
    {
        speedIndex = 0;
        CurrentSpeed = normalSpeed;

        Time.timeScale = normalSpeed;

        UpdateSpeedButtonText();

        Debug.Log("Game Speed Reset: 1x");
    }

    // อัปเดตข้อความบนปุ่ม
    void UpdateSpeedButtonText()
    {
        if (speedButtonText != null)
        {
            speedButtonText.text = CurrentSpeed.ToString("0") + "x";
        }
    }
}
