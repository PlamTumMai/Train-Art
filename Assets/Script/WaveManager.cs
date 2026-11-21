using System;
using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    public int maxWaves = 5;
    public int bossWave = 5;

    [Header("References")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    [Tooltip("ลาก AttackPoint ที่อยู่ท้ายขบวนรถไฟมาใส่")]
    public Transform attackPoint;
    public TrainHealth trainHealth;

    [Header("Spawn Area")]
    public float spawnDistance = 12f;
    public float spawnYRange = 1.5f;

    [Header("Enemy Scaling (ต่อเวฟ)")]
    public float baseSpawnInterval = 2.5f;
    public float intervalDecreasePerWave = 0.25f;
    public float minSpawnInterval = 0.8f;

    public int baseEnemyHP = 25;
    public int hpIncreasePerWave = 15;
    public int baseEnemyDamage = 5;
    public int damageIncreasePerWave = 2;
    public float baseMoveSpeed = 1.2f;
    public float speedIncreasePerWave = 0.25f;

    [Header("Boss Stats (Wave 5)")]
    public int bossHP = 600;
    public int bossDamage = 20;
    public float bossMoveSpeed = 1.0f;

    public int CurrentWave { get; private set; } = 0;
    public event Action<int, int> OnWaveChanged; // currentWave, maxWaves

    private int enemiesAlive;
    private bool isWaveActive;
    private bool canSpawn;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameplayTimerEnded += HandleGameplayTimerEnded;
            GameManager.Instance.OnStartNextRound += HandleStartNextRound;
        }

        if (!isWaveActive)
        {
            StartNextWave();
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameplayTimerEnded -= HandleGameplayTimerEnded;
            GameManager.Instance.OnStartNextRound -= HandleStartNextRound;
        }
    }

    void HandleStartNextRound()
    {
        if (isWaveActive) return;
        StartNextWave();
    }

    void StartNextWave()
    {
        if (CurrentWave >= maxWaves) return;

        CurrentWave++;
        isWaveActive = true;
        canSpawn = true;
        enemiesAlive = 0;

        Debug.Log($"[WaveManager] เริ่ม Wave {CurrentWave}/{maxWaves}");
        OnWaveChanged?.Invoke(CurrentWave, maxWaves);

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        float currentInterval = Mathf.Max(
            minSpawnInterval,
            baseSpawnInterval - (CurrentWave - 1) * intervalDecreasePerWave
        );

        // ถ้าเป็นเวฟบอส ให้เสกบอสออกมาหลังเริ่มเวฟ 2 วินาที
        if (CurrentWave == bossWave && bossPrefab != null)
        {
            yield return new WaitForSeconds(2f);
            if (canSpawn) SpawnUnit(bossPrefab, true);
        }

        // เสกศัตรูธรรมดาเรื่อยๆ ตลอด 60 วินาที
        while (canSpawn)
        {
            if (enemyPrefab != null)
            {
                SpawnUnit(enemyPrefab, false);
            }
            yield return new WaitForSeconds(currentInterval);
        }
    }

    void SpawnUnit(GameObject prefab, bool isBossUnit)
    {
        if (attackPoint == null)
        {
            Debug.LogError("[WaveManager] กรุณาลาก AttackPoint ใส่ใน Inspector!");
            return;
        }

        float randomY = isBossUnit ? 0f : UnityEngine.Random.Range(-spawnYRange, spawnYRange);
        Vector3 spawnPos = new Vector3(
            attackPoint.position.x - spawnDistance,
            attackPoint.position.y + randomY,
            0f
        );

        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyBase unit = obj.GetComponent<EnemyBase>();

        if (unit != null)
        {
            int hp = isBossUnit ? bossHP : baseEnemyHP + (CurrentWave - 1) * hpIncreasePerWave;
            int dmg = isBossUnit ? bossDamage : baseEnemyDamage + (CurrentWave - 1) * damageIncreasePerWave;
            float spd = isBossUnit ? bossMoveSpeed : baseMoveSpeed + (CurrentWave - 1) * speedIncreasePerWave;

            unit.isBoss = isBossUnit;
            unit.Initialize(attackPoint, trainHealth, hp, dmg, spd);
            unit.OnDied += HandleUnitDied;
            enemiesAlive++;
        }
    }

    void HandleGameplayTimerEnded()
    {
        // ครบ 60 วิ -> หยุดเสกศัตรูเพิ่ม รอเคลียร์ตัวที่เหลือบนจอ
        canSpawn = false;
        CheckWaveCleared();
    }

    void HandleUnitDied(EnemyBase unit)
    {
        unit.OnDied -= HandleUnitDied;
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        // ถ้าฆ่าบอสเวฟสุดท้ายตายแล้ว ให้หยุดเสกลูกน้องเพิ่มทันที
        if (unit.isBoss && CurrentWave >= maxWaves)
        {
            canSpawn = false;
        }

        CheckWaveCleared();
    }

    void CheckWaveCleared()
    {
        if (!isWaveActive || canSpawn || enemiesAlive > 0)
            return;

        isWaveActive = false;

        if (CurrentWave >= maxWaves)
        {
            GameManager.Instance?.TriggerVictory();
        }
        else
        {
            GameManager.Instance?.EnterStation();
        }
    }
}