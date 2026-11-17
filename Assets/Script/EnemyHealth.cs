
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 100;

    private int currentHP;
    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
    }

    public void SetMaxHP(int hp)
    {
        maxHP = hp;
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;

        Debug.Log("Enemy HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (KillCounter.Instance != null)
        {
            KillCounter.Instance.AddKill();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(
                ScoreManager.Instance.scorePerKill
            );
        }

        if (ScrapManager.Instance != null)
        {
            ScrapManager.Instance.AddScrap(
                ScrapManager.Instance.scrapPerKill
            );
        }

        EnemyWaveMember waveMember = GetComponent<EnemyWaveMember>();

        if (waveMember != null)
        {
            waveMember.EnemyDied();
        }

        Destroy(gameObject);
    }
}
