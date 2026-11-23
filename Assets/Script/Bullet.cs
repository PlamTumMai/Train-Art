using UnityEngine;

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

    private Vector2 direction = Vector2.left;
    private int damage = 10;

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
        if (!other.CompareTag("Enemy") && !other.CompareTag("Boss"))
            return;

        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(damage);

            if (enableKnockback)
            {
                target.ApplyKnockback(direction, knockbackForce);
            }
        }

        Destroy(gameObject);
    }
}
