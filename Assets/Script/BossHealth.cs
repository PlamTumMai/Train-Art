
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Health")]
    public int maxHP = 1000;
    public int currentHP;

    private bool isDead;

    void Start()
    {
        currentHP = maxHP;

        Debug.Log("BOSS SPAWN! HP: " + currentHP);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        currentHP = Mathf.Max(0, currentHP);

        Debug.Log("BOSS HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("BOSS DEFEATED!");

        Destroy(gameObject);
    }
}
