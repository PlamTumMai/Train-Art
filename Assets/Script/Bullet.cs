using UnityEngine;
using System.Collections.Generic;

// Interface สำหรับรับ Damage และ Knockback
public interface IDamageable
{
    void TakeDamage(int damage);
    void ApplyKnockback(Vector2 direction, float force);
}

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public float lifeTime = 3f;

    [Header("Knockback Settings")]
    public bool enableKnockback = false;
    public float knockbackForce = 5f;

    [Header("AOE Settings")]
    public bool enableAOE = false;
    public float aoeRadius = 3f;
    public float aoeDamageMultiplier = 1f;

    [Header("Hit VFX")]
    public GameObject hitVFX;
    public float hitVFXLifetime = 2f;

    private Vector2 direction = Vector2.left;
    private int damage = 10;

    private bool hasHit = false;

    public void Initialize(Vector2 shootDirection, int bulletDamage)
    {
        direction = shootDirection.normalized;
        damage = bulletDamage;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        if (!other.CompareTag("Enemy") && !other.CompareTag("Boss"))
            return;

        hasHit = true;

        Vector2 impactPoint = other.ClosestPoint(transform.position);

        // สร้าง VFX ตอนกระสุนโดน
        SpawnHitVFX(impactPoint);

        if (enableAOE)
        {
            DealAOEDamage(impactPoint);
        }
        else
        {
            DealSingleDamage(other, direction);
        }

        Destroy(gameObject);
    }

    void DealSingleDamage(Collider2D other, Vector2 knockbackDirection)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (target == null)
            return;

        target.TakeDamage(damage);

        if (enableKnockback)
        {
            target.ApplyKnockback(knockbackDirection, knockbackForce);
        }
    }

    void DealAOEDamage(Vector2 center)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, aoeRadius);

        // ป้องกันการทำดาเมจซ้ำกับศัตรูที่มีหลาย Collider
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy") && !hit.CompareTag("Boss"))
                continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target == null || !damagedTargets.Add(target))
                continue;

            int aoeDamage = Mathf.RoundToInt(damage * aoeDamageMultiplier);

            target.TakeDamage(aoeDamage);

            if (enableKnockback)
            {
                Vector2 knockbackDirection =
                    ((Vector2)hit.transform.position - center).normalized;

                if (knockbackDirection == Vector2.zero)
                    knockbackDirection = direction;

                target.ApplyKnockback(knockbackDirection, knockbackForce);
            }
        }
    }

    void SpawnHitVFX(Vector2 position)
    {
        if (hitVFX == null)
            return;

        GameObject effect = Instantiate(
            hitVFX,
            position,
            Quaternion.identity
        );

        Destroy(effect, hitVFXLifetime);
    }

    // แสดงรัศมี AOE ใน Scene View
    void OnDrawGizmosSelected()
    {
        if (!enableAOE)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
