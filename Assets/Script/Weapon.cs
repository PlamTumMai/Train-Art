
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "Basic Gun";

    [Header("Weapon Stats")]
    public float damage = 10f;
    public float fireRate = 1f;

    [Header("Upgrade")]
    public int weaponLevel = 1;
    public int maxLevel = 5;

    public int upgradeCost = 50;
    public float damageIncrease = 5f;
    public float fireRateDecrease = 0.1f;

    public void UpgradeWeapon()
    {
        if (weaponLevel >= maxLevel)
        {
            Debug.Log(weaponName + " Level Max!");
            return;
        }

        if (ScrapManager.Instance == null)
        {
            Debug.Log("ไม่พบ ScrapManager");
            return;
        }

        if (ScrapManager.Instance.scrap < upgradeCost)
        {
            Debug.Log("Scrap ไม่เพียงพอ!");
            return;
        }

        ScrapManager.Instance.scrap -= upgradeCost;

        weaponLevel++;
        damage += damageIncrease;
        fireRate = Mathf.Max(0.2f, fireRate - fireRateDecrease);

        Debug.Log(
            weaponName + " Level: " + weaponLevel +
            " | Damage: " + damage +
            " | Fire Rate: " + fireRate +
            " | Scrap: " + ScrapManager.Instance.scrap
        );
    }
}
