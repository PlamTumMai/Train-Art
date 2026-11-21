using System;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Type")]
    public bool isBoss = false;

    [Header("Stats (ค่าเริ่มต้น - จะถูกปรับตามเวฟโดย WaveManager)")]
    public int maxHP = 30;
    public int damage = 5;
    public float moveSpeed = 1.5f;
    public float attackRange = 0.8f;
    public float attackInterval = 1.2f;

    [Header("Rewards (เมื่อตาย)")]
    public int scoreReward = 100;
    public int scrapReward = 10;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    // Event สำหรับแจ้ง WaveManager
    public event Action<EnemyBase> OnDied;

    // Static Events สำหรับแจ้ง Boss HP Bar UI (ทำงานเฉพาะตัวที่ติ๊ก isBoss)
    public static event Action<EnemyBase> OnBossSpawned;
    public static event Action<int, int> OnBossHealthChanged;
    public static event Action OnBossDefeated;

    private Transform targetPoint;
    private TrainHealth trainHealth;
    private float attackTimer;

    public void Initialize(Transform attackPoint, TrainHealth train, int hp, int dmg, float speed)
    {
        targetPoint = attackPoint;
        trainHealth = train;
        maxHP = hp;
        CurrentHP = maxHP;
        damage = dmg;
        moveSpeed = speed;

        if (isBoss)
        {
            OnBossSpawned?.Invoke(this);
            OnBossHealthChanged?.Invoke(CurrentHP, maxHP);
        }
    }

    void Start()
    {
        // กันเหนียวกรณีวางเทสต์ในฉากตรงๆ โดยไม่ได้ผ่าน WaveManager
        if (CurrentHP <= 0) CurrentHP = maxHP;
    }

    void Update()
    {
        if (IsDead || targetPoint == null) return;

        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;
            if (state != GameState.Gameplay && state != GameState.WaitingWaveClear)
                return;
        }

        float dist = Vector2.Distance(transform.position, targetPoint.position);

        if (dist > attackRange)
        {
            Vector2 dir = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            attackTimer = 0f;
        }
        else
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                if (trainHealth != null)
                {
                    trainHealth.TakeDamage(damage);
                }
                attackTimer = 0f;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        if (isBoss)
        {
            OnBossHealthChanged?.Invoke(CurrentHP, maxHP);
        }

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        GameManager.Instance?.AddKillAndScore(scoreReward, scrapReward);

        if (isBoss)
        {
            OnBossDefeated?.Invoke();
        }

        OnDied?.Invoke(this);
        Destroy(gameObject);
    }
}