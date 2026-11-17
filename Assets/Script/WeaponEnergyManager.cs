
using UnityEngine;

public class WeaponEnergyManager : MonoBehaviour
{
    public static WeaponEnergyManager Instance;

    [Header("Energy Settings")]
    public int maxEnergy = 100;
    public int currentEnergy = 100;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool UseEnergy(int amount)
    {
        if (currentEnergy < amount)
        {
            Debug.Log("Not enough Energy!");
            return false;
        }

        currentEnergy -= amount;

        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);

        return true;
    }

    public void RestoreEnergy(int amount)
    {
        currentEnergy += amount;

        currentEnergy = Mathf.Clamp(
            currentEnergy,
            0,
            maxEnergy
        );

        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);
    }

    public bool HasEnoughEnergy(int amount)
    {
        return currentEnergy >= amount;
    }
}
