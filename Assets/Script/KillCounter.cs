
using UnityEngine;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance;

    public int killCount = 0;

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

    public void AddKill()
    {
        killCount++;

        Debug.Log("Kill Count: " + killCount);
    }
}
