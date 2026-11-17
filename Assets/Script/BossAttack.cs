
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Boss Attack")]
    public int damage = 20;
    public float attackInterval = 2f;
    public float attackDistance = 2f;

    [Header("Target")]
    public Transform trainTarget;

    private float attackTimer;

    void Update()
    {
        if (trainTarget == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            trainTarget.position
        );

        if (distance <= attackDistance)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                AttackTrain();
                attackTimer = 0f;
            }
        }
        else
        {
            attackTimer = 0f;
        }
    }

    void AttackTrain()
    {
        TrainHealth trainHealth =
            trainTarget.GetComponent<TrainHealth>();

        if (trainHealth == null)
        {
            Debug.LogError("BossAttack: ไม่พบ TrainHealth!");
            return;
        }

        trainHealth.TakeDamage(damage);

        Debug.Log("BOSS ATTACK! DAMAGE: " + damage);
    }
}
