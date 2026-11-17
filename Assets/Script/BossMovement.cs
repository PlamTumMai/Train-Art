
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Boss Movement")]
    public float moveSpeed = 1.5f;
    public float attackDistance = 2f;

    [Header("Target")]
    public Transform trainTarget;

    void Update()
    {
        if (trainTarget == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            trainTarget.position
        );

        if (distance > attackDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                trainTarget.position,
                moveSpeed * Time.deltaTime
            );
        }
    }
}
