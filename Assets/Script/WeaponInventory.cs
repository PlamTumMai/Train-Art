
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    public GameObject weaponPrefab_1;
    public GameObject weaponPrefab_2;
    public GameObject weaponPrefab_3;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float spacing = 4.0f;

    private bool hasAddedWeapons = false;

    public void AddWeapon()
    {
        if (hasAddedWeapons)
            return;

        if (weaponPrefab_1 == null ||
            weaponPrefab_2 == null ||
            weaponPrefab_3 == null ||
            spawnPoint == null)
        {
            Debug.Log("กรุณาตั้งค่า Weapon Prefabs และ Spawn Point");
            return;
        }

        hasAddedWeapons = true;

        GameObject[] weapons =
        {
            weaponPrefab_1,
            weaponPrefab_2,
            weaponPrefab_3
        };

        for (int i = 0; i < weapons.Length; i++)
        {
            Vector3 position = spawnPoint.position;
            position.x += i * spacing;
            position.z = 0f;

            GameObject weapon = Instantiate(
                weapons[i],
                position,
                Quaternion.identity
            );

            Debug.Log("สร้างปืนชนิดที่ " + (i + 1));
        }

        Debug.Log("สร้างปืนครบ 3 ประเภทแล้ว!");
    }
}
