
using UnityEngine;

public class ScrapManager : MonoBehaviour
{
    public static ScrapManager Instance;

    public int scrap = 0;
    public int scrapPerKill = 5;

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

    public void AddScrap(int amount)
    {
        scrap += amount;

        Debug.Log("Scrap: " + scrap);
    }
}
