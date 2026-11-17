
using UnityEngine;

public class TrainHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;

    private int currentHP;
    private bool isGameOver;

    public int CurrentHP => currentHP;

    [Header("Train Upgrade")]
    public int trainLevel = 1;
    public int maxLevel = 5;
    public int upgradeCost = 100;
    public int hpIncrease = 50;

    void Start()
    {
        currentHP = maxHP;
        Debug.Log("Train HP: " + currentHP);
    }

    public void TakeDamage(int damage)
    {
        if (isGameOver) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log("Train HP: " + currentHP);

        if (currentHP <= 0)
            GameOver();
    }

    public void UpgradeTrain()
    {
        if (trainLevel >= maxLevel)
        {
            Debug.Log("Train Level Max!");
            return;
        }

        if (ScrapManager.Instance == null)
        {
            Debug.Log("ไม่พบ ScrapManager");
            return;
        }

        if (ScrapManager.Instance.scrap < upgradeCost)
        {
            Debug.Log("Scrap ไม่เพียงพอ!");
            return;
        }

        ScrapManager.Instance.scrap -= upgradeCost;

        trainLevel++;
        maxHP += hpIncrease;
        currentHP = maxHP;

        Debug.Log(
            "Train Level: " + trainLevel +
            " | HP: " + currentHP + "/" + maxHP +
            " | Scrap: " + ScrapManager.Instance.scrap
        );
    }

    void GameOver()
    {
        isGameOver = true;

        Debug.Log("GAME OVER - TRAIN DESTROYED");
        Time.timeScale = 0f;
    }
}
