
using UnityEngine;

public class WeaponSlot : MonoBehaviour
{
    public bool isOccupied = false;

    public void InstallWeapon(GameObject weapon)
    {
        if (isOccupied || weapon == null)
            return;

        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        WeaponDrag drag = weapon.GetComponent<WeaponDrag>();
        if (drag != null)
            drag.LockWeapon();

        Rigidbody2D rb = weapon.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        WeaponShoot_1 shoot1 = weapon.GetComponent<WeaponShoot_1>();
        if (shoot1 != null)
            shoot1.EnableShooting();

        WeaponShoot_2 shoot2 = weapon.GetComponent<WeaponShoot_2>();
        if (shoot2 != null)
            shoot2.EnableShooting();

        WeaponShoot_3 shoot3 = weapon.GetComponent<WeaponShoot_3>();
        if (shoot3 != null)
            shoot3.EnableShooting();

        isOccupied = true;
    }
}
