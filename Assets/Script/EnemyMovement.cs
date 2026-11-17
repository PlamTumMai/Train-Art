
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.8f;
    public float attackDistance = 1.5f;
    public Transform trainTarget;

    [Header("Speed Progression")]
    public float mediumSpeed = 1.5f;
    public float fastSpeed = 2.5f;
    public float veryFastSpeed = 3.5f;

    [Header("Attack")]
    public int damage = 5;
    public float attackInterval = 1f;

    private float attackTimer;
    private TrainHealth trainHealth;

    void Start()
    {
        if (trainTarget != null)
        {
            trainHealth =
                trainTarget.GetComponentInParent<TrainHealth>();
        }
    }

    void Update()
    {
        UpdateSpeed();

        if (trainTarget == null) return;

        float distance = Vector2.Distance(
            transform.position,
            trainTarget.position
        );

        if (distance > attackDistance)
        {
            Vector2 direction =
                ((Vector2)trainTarget.position -
                 (Vector2)transform.position).normalized;

            transform.position +=
                (Vector3)(direction * moveSpeed * Time.deltaTime);

            attackTimer = 0f;
            return;
        }

        AttackTrain();
    }

    void UpdateSpeed()
    {
        float elapsedTime = Time.timeSinceLevelLoad;

        if (elapsedTime < 20f)
        {
            moveSpeed = 0.8f;
        }
        else if (elapsedTime < 40f)
        {
            moveSpeed = mediumSpeed;
        }
        else if (elapsedTime < 60f)
        {
            moveSpeed = fastSpeed;
        }
        else
        {
            moveSpeed = veryFastSpeed;
        }
    }

    void AttackTrain()
    {
        if (trainHealth == null) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            trainHealth.TakeDamage(damage);
            attackTimer = 0f;
        }
    }
}
