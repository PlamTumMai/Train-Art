
using UnityEngine;
using System.Collections;

public class EnemyWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 1;
    public int maxWaves = 10;
    public int enemiesPerWave = 5;
    public float spawnInterval = 3f;
    public float timeBetweenWaves = 5f;
    public int enemiesPerGroup = 3;
    public float timeBetweenGroups = 5f;

    [Header("Enemy")]
    public GameObject enemyPrefab;
    public Transform trainTarget;
    public float spawnDistance = 10f;

    [Header("Enemy Scaling")]
    public int baseHP = 20;
    public int hpIncreasePerWave = 15;
    public int baseDamage = 5;
    public int damageIncreasePerWave = 2;

    [Header("Game Loop")]
    public bool stopAfterCurrentWave;
    public bool isWaveCompleted;
    public bool isSpawning;
    public bool gameCompleted;

    [Header("Boss")]
    public BossSpawner bossSpawner;
    public int bossWaveInterval = 5;

    private int enemiesSpawned;
    private int enemiesAlive;
    private bool isNextWaveStarting;

    void Start()
    {
        if (trainTarget == null || enemyPrefab == null)
        {
            Debug.LogError("EnemyWaveManager: กรุณาตั้ง Enemy Prefab และ Train Target");
            return;
        }

        StartWave();
    }

    void StartWave()
    {
        if (gameCompleted || isSpawning || isNextWaveStarting)
            return;

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        Debug.Log("WAVE " + currentWave + " START!");

        enemiesSpawned = 0;
        enemiesAlive = 0;
        isWaveCompleted = false;
        isSpawning = true;
        isNextWaveStarting = false;

        while (enemiesSpawned < enemiesPerWave)
        {
            int groupSize = Mathf.Min(
                enemiesPerGroup,
                enemiesPerWave - enemiesSpawned
            );

            for (int i = 0; i < groupSize; i++)
            {
                SpawnEnemy();
                enemiesSpawned++;
                enemiesAlive++;
            }

            Debug.Log("SPAWN GROUP: " + groupSize);

            yield return new WaitForSeconds(timeBetweenGroups);
        }

        // Spawn Boss ทุก ๆ 5 Waves
        if (currentWave % bossWaveInterval == 0)
        {
            if (bossSpawner != null)
            {
                enemiesAlive++;
                bossSpawner.SpawnBoss();
                Debug.Log("BOSS WAVE!");
            }
            else
            {
                Debug.LogError("EnemyWaveManager: กรุณาตั้ง BossSpawner!");
            }
        }

        isSpawning = false;

        Debug.Log("WAVE " + currentWave + " SPAWN COMPLETE!");

        CheckWaveComplete();
    }

    void SpawnEnemy()
    {
        Vector2 spawnPosition = new Vector2(
            trainTarget.position.x - spawnDistance,
            trainTarget.position.y + Random.Range(-2f, 2f)
        );

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

        if (movement != null)
        {
            movement.trainTarget = trainTarget;
            movement.damage = baseDamage +
                (currentWave - 1) * damageIncreasePerWave;
        }
        else
        {
            Debug.LogError("Enemy ไม่มี EnemyMovement!");
        }

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (health != null)
        {
            int enemyHP = baseHP +
                (currentWave - 1) * hpIncreasePerWave;

            health.SetMaxHP(enemyHP);
        }
        else
        {
            Debug.LogError("Enemy ไม่มี EnemyHealth!");
        }

        EnemyWaveMember member = enemy.AddComponent<EnemyWaveMember>();
        member.waveManager = this;
    }

    public void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        CheckWaveComplete();
    }

    void CheckWaveComplete()
    {
        if (isSpawning || enemiesAlive > 0)
            return;

        if (enemiesSpawned < enemiesPerWave)
            return;

        if (isWaveCompleted)
            return;

        isWaveCompleted = true;

        Debug.Log("WAVE " + currentWave + " COMPLETE!");

        // จบเกมเมื่อผ่าน Wave สุดท้าย
        if (currentWave >= maxWaves)
        {
            gameCompleted = true;
            stopAfterCurrentWave = true;

            Debug.Log("GAME COMPLETED! ALL WAVES CLEARED!");
            Debug.Log("VICTORY!");

            return;
        }

        if (stopAfterCurrentWave)
        {
            Debug.Log("ROUND COMPLETE!");
            return;
        }

        if (!isNextWaveStarting)
        {
            isNextWaveStarting = true;
            StartCoroutine(NextWave());
        }
    }

    IEnumerator NextWave()
    {
        if (gameCompleted)
            yield break;

        yield return new WaitForSeconds(timeBetweenWaves);

        if (gameCompleted || stopAfterCurrentWave)
        {
            isNextWaveStarting = false;
            yield break;
        }

        currentWave++;
        enemiesPerWave += 3;

        spawnInterval = Mathf.Max(
            0.5f,
            spawnInterval - 0.2f
        );

        isNextWaveStarting = false;

        StartWave();
    }

    public void PrepareNextRound()
    {
        if (gameCompleted)
        {
            Debug.Log("GAME ALREADY COMPLETED!");
            return;
        }

        currentWave++;
        enemiesPerWave += 3;

        spawnInterval = Mathf.Max(
            0.5f,
            spawnInterval - 0.2f
        );

        stopAfterCurrentWave = false;
        isWaveCompleted = false;

        StartWave();
    }
}
