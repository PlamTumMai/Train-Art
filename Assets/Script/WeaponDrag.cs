
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponDrag : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;

    private bool isDragging;
    private bool isInstalled;

    [Header("Energy Cost")]
    public int energyCost = 20;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null || cam == null || isInstalled)
            return;

        Vector2 mouse = Mouse.current.position.ReadValue();

        Vector3 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mouse.x, mouse.y, -cam.transform.position.z)
        );

        mouseWorld.z = transform.position.z;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                offset = transform.position - mouseWorld;
            }
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            transform.position = mouseWorld + offset;
        }

        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;

            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

            foreach (Collider2D collider in hits)
            {
                WeaponSlot slot = collider.GetComponentInParent<WeaponSlot>();

                if (slot != null && !slot.isOccupied)
                {
                    WeaponEnergyManager energy =
                        WeaponEnergyManager.Instance;

                    if (energy == null)
                    {
                        Debug.Log("ไม่พบ WeaponEnergyManager");
                        return;
                    }

                    if (!energy.HasEnoughEnergy(energyCost))
                    {
                        Debug.Log("Energy ไม่เพียงพอ!");
                        return;
                    }

                    energy.UseEnergy(energyCost);

                    slot.InstallWeapon(gameObject);

                    Debug.Log("ติดตั้งอาวุธสำเร็จ!");
                    return;
                }
            }

            Debug.Log("วางปืนไม่สำเร็จ");
        }
    }

    public void LockWeapon()
    {
        isInstalled = true;
        isDragging = false;
    }
}
