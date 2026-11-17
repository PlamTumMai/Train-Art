
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss")]
    public GameObject bossPrefab;

    [Header("Target")]
    public Transform trainTarget;

    [Header("Spawn")]
    public float spawnDistance = 15f;

    [Header("Wave")]
    public EnemyWaveManager waveManager;

    public void SpawnBoss()
    {
        if (bossPrefab == null || trainTarget == null)
        {
            Debug.LogError("BossSpawner: กรุณาตั้ง Boss Prefab และ Train Target");
            return;
        }

        Vector2 spawnPosition = new Vector2(
            trainTarget.position.x - spawnDistance,
            trainTarget.position.y
        );

        GameObject boss = Instantiate(
            bossPrefab,
            spawnPosition,
            Quaternion.identity
        );

        BossMovement movement = boss.GetComponent<BossMovement>();

        if (movement != null)
        {
            movement.trainTarget = trainTarget;
        }
        BossAttack attack = boss.GetComponent<BossAttack>();

        if (attack != null)
        {
            attack.trainTarget = trainTarget;
        }

        EnemyWaveMember member = boss.AddComponent<EnemyWaveMember>();
        member.waveManager = waveManager;

        Debug.Log("BOSS SPAWNED! WAVE " + waveManager.currentWave);
    }
}
