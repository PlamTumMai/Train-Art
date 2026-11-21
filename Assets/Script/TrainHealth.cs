using UnityEngine;

public class TrainHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;

    public int CurrentHP => currentHP;
    public bool IsMaxLevel => trainLevel >= maxLevel;

    [Header("Train Upgrade")]
    public int trainLevel = 1;
    public int maxLevel = 5;
    public int upgradeCost = 100;
    public int hpIncrease = 50;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        if (currentHP <= 0)
        {
            GameManager.Instance?.TriggerGameOver();
        }
    }

    // เรียกจากปุ่ม "Upgrade Train" บนหน้า Station UI ได้เลย
    public bool TryUpgradeTrain()
    {
        if (IsMaxLevel) return false;
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.CurrentState != GameState.Station) return false;
        if (!GameManager.Instance.SpendScrap(upgradeCost)) return false;

        trainLevel++;
        maxHP += hpIncrease;
        currentHP = maxHP; // ฮีลเต็มหลอดเมื่ออัปเกรด
        return true;
    }
}