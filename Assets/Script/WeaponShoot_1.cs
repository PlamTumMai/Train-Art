
using UnityEngine;

public class WeaponShoot_1 : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Shoot")]
    public float fireRate = 1f;

    [Header("Target")]
    public float searchRange = 20f;

    private float fireTimer;
    private bool canShoot;
    private Transform target;
    private Weapon weapon;

    void Awake()
    {
        weapon = GetComponent<Weapon>();
    }

    public void EnableShooting()
    {
        canShoot = true;
    }

    void Update()
    {
        if (!canShoot)
            return;

        FindTarget();

        if (target == null)
            return;

        AimTarget();
        ShootTarget();
    }

    void FindTarget()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        GameObject[] bosses =
            GameObject.FindGameObjectsWithTag("Boss");

        float closestDistance = searchRange;
        target = null;

        foreach (GameObject enemy in enemies)
        {
            CheckTarget(enemy, ref closestDistance);
        }

        foreach (GameObject boss in bosses)
        {
            CheckTarget(boss, ref closestDistance);
        }
    }

    void CheckTarget(GameObject enemy, ref float closestDistance)
    {
        float distance = Vector2.Distance(
            transform.position,
            enemy.transform.position
        );

        if (distance < closestDistance)
        {
            closestDistance = distance;
            target = enemy.transform;
        }
    }

    void AimTarget()
    {
        Vector2 direction =
            target.position - firePoint.position;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );
    }

    void ShootTarget()
    {
        fireTimer += Time.deltaTime;

        float currentFireRate = weapon != null
            ? weapon.fireRate
            : fireRate;

        if (fireTimer >= currentFireRate)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

            Bullet bulletScript =
                bullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                Vector2 direction =
                    target.position - firePoint.position;

                bulletScript.SetDirection(direction);
            }

            fireTimer = 0f;
        }
    }
}
