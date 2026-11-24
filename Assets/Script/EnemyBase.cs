using System;
using System.Collections;
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

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.2f;

    private Vector2 knockbackVelocity;
    private float knockbackTimer;

    [Header("Damage Feedback")]
    public AudioSource audioSource;
    public AudioClip[] hitSounds = new AudioClip[2];

    [Range(0f, 1f)]
    public float hitVolume = 0.7f;

    public Color damageColor = Color.red;
    public float flashDuration = 0.15f;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    [Header("Rewards (เมื่อตาย)")]
    public int scoreReward = 100;
    public int scrapReward = 10;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<EnemyBase> OnDied;

    public static event Action<EnemyBase> OnBossSpawned;
    public static event Action<int, int> OnBossHealthChanged;
    public static event Action OnBossDefeated;

    private Transform targetPoint;
    private TrainHealth trainHealth;
    private float attackTimer;

    void Awake()
    {
        // ค้นหา SpriteRenderer ทั้งตัวศัตรูและลูกทั้งหมด
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // จดจำสีเดิมของแต่ละ SpriteRenderer
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        // ค้นหา AudioSource อัตโนมัติ
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

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
        if (CurrentHP <= 0)
            CurrentHP = maxHP;
    }

    void Update()
    {
        if (IsDead || targetPoint == null) return;

        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;

            if (state != GameState.Gameplay &&
                state != GameState.WaitingWaveClear)
                return;
        }

        // กำลังถูก Knockback
        if (knockbackTimer > 0f)
        {
            transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);

            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector2.zero;
            }

            return;
        }

        float dist = Vector2.Distance(transform.position, targetPoint.position);

        if (dist > attackRange)
        {
            Vector2 dir = ((Vector2)targetPoint.position -
                           (Vector2)transform.position).normalized;

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

        // เล่นเสียงเมื่อได้รับ Damage
        PlayHitSound();

        // เปลี่ยนสีเป็นสีแดงชั่วคราว
        PlayDamageFlash();

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

    void PlayHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0)
            return;

        // รวบรวมเสียงที่กำหนดไว้
        System.Collections.Generic.List<AudioClip> availableSounds =
            new System.Collections.Generic.List<AudioClip>();

        foreach (AudioClip clip in hitSounds)
        {
            if (clip != null)
                availableSounds.Add(clip);
        }

        if (availableSounds.Count == 0)
            return;

        // สุ่มเลือกเสียง
        int randomIndex = UnityEngine.Random.Range(0, availableSounds.Count);
        AudioClip randomSound = availableSounds[randomIndex];

        // เล่นเสียงแยกจากตัวศัตรู
        AudioSource.PlayClipAtPoint(
            randomSound,
            transform.position,
            hitVolume
        );
    }

    void PlayDamageFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        // หากโดนซ้ำ ให้เริ่มเอฟเฟกต์ใหม่
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(DamageFlashCoroutine());
    }

    IEnumerator DamageFlashCoroutine()
    {
        // เปลี่ยนเป็นสีแดง
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = damageColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // คืนสีเดิม
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }

        flashCoroutine = null;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (IsDead) return;

        // บังคับให้ Knockback ไปทางซ้ายเท่านั้น
        knockbackVelocity = Vector2.left * force;
        knockbackTimer = knockbackDuration;
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
