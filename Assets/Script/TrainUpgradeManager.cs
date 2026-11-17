
using UnityEngine;
using UnityEngine.InputSystem;

public class TrainUpgradeManager : MonoBehaviour
{
    public static TrainUpgradeManager Instance;

    [Header("Upgrade Target")]
    public TrainHealth trainHealth;

    [Header("Game Loop")]
    public GameLoopManager gameLoopManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            UpgradeTrain();
        }
    }

    public void UpgradeTrain()
    {
        if (gameLoopManager == null)
        {
            Debug.LogError("กรุณาตั้ง GameLoopManager!");
            return;
        }

        if (!gameLoopManager.IsStation)
        {
            Debug.Log("Upgrade รถไฟได้เฉพาะตอน Station!");
            return;
        }

        if (trainHealth == null)
        {
            Debug.Log("ไม่พบ TrainHealth!");
            return;
        }

        trainHealth.UpgradeTrain();
    }
}
