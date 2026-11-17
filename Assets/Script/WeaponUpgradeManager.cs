
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager Instance;

    [Header("Weapon Slots")]
    public WeaponSlot weaponSlot1;
    public WeaponSlot weaponSlot2;
    public WeaponSlot weaponSlot3;

    [Header("Game Loop")]
    public GameLoopManager gameLoopManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.uKey.wasPressedThisFrame)
            UpgradeSlot(weaponSlot1);

        if (Keyboard.current.iKey.wasPressedThisFrame)
            UpgradeSlot(weaponSlot2);

        if (Keyboard.current.oKey.wasPressedThisFrame)
            UpgradeSlot(weaponSlot3);
    }


    void UpgradeSlot(WeaponSlot slot)
    {
        if (gameLoopManager == null)
        {
            Debug.LogError("กรุณาตั้ง GameLoopManager!");
            return;
        }

        if (!gameLoopManager.IsStation)
        {
            Debug.Log("Upgrade อาวุธได้เฉพาะตอน Station!");
            return;
        }

        if (slot == null || !slot.isOccupied)
        {
            Debug.Log("ช่องนี้ยังไม่มีอาวุธ!");
            return;
        }

        Weapon weapon = slot.GetComponentInChildren<Weapon>();

        if (weapon == null)
        {
            Debug.Log("ไม่พบอาวุธในช่องนี้!");
            return;
        }

        int oldLevel = weapon.weaponLevel;

        weapon.UpgradeWeapon();

        if (weapon.weaponLevel > oldLevel)
        {
            Debug.Log(
                slot.name + " อัปเกรดสำเร็จ! " +
                weapon.weaponName +
                " Level: " + weapon.weaponLevel
            );
        }
    }

}
