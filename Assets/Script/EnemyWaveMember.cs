
using UnityEngine;

public class EnemyWaveMember : MonoBehaviour
{
    public EnemyWaveManager waveManager;
    private bool counted;

    public void EnemyDied()
    {
        if (counted) return;

        counted = true;

        if (waveManager != null)
        {
            waveManager.EnemyDied();
        }
    }

    private void OnDestroy()
    {
        EnemyDied();
    }
}
