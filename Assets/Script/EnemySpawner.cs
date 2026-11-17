
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Train")]
    public Transform trainTarget;

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    public float spawnDistance = 10f;

    private float spawnTimer;

    void Update()
    {
        if (enemyPrefab == null || trainTarget == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // Spawn ด้านหลังรถไฟ (ทางซ้าย)
        Vector2 spawnPosition = new Vector2(
            trainTarget.position.x - spawnDistance,
            trainTarget.position.y + Random.Range(-2f, 2f)
        );

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyMovement movement =
            enemy.GetComponent<EnemyMovement>();

        if (movement != null)
        {
            movement.trainTarget = trainTarget;
        }
    }
}
