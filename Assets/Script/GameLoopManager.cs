
using UnityEngine;
using System.Collections;

public class GameLoopManager : MonoBehaviour
{
    [Header("Game Loop")]
    public float gameplayDuration = 60f;
    public float stationDuration = 15f;

    [Header("References")]
    public EnemyWaveManager waveManager;

    private float timer;
    private float stationTimer;

    private bool isGameplay;
    private bool isStation;
    private bool isEndingRound;
    private bool isGameFinished;

    public bool IsStation => isStation;
    public bool IsGameplay => isGameplay;

    public float StationTimeRemaining =>
        Mathf.Max(0f, stationDuration - stationTimer);

    void Start()
    {
        if (waveManager == null)
        {
            Debug.LogError("GameLoopManager: กรุณาตั้ง Wave Manager");
            return;
        }

        StartGameplay();
    }

    void Update()
    {
        if (!isGameplay || isEndingRound || isGameFinished)
            return;

        timer += Time.deltaTime;

        if (timer >= gameplayDuration)
        {
            StartCoroutine(EndGameplay());
        }
    }

    void StartGameplay()
    {
        if (isGameFinished)
            return;

        timer = 0f;

        isGameplay = true;
        isStation = false;
        isEndingRound = false;

        waveManager.stopAfterCurrentWave = false;

        Debug.Log("GAMEPLAY START!");
    }

    IEnumerator EndGameplay()
    {
        isEndingRound = true;
        isGameplay = false;

        waveManager.stopAfterCurrentWave = true;

        Debug.Log("WAITING FOR WAVE COMPLETE...");

        while (!waveManager.isWaveCompleted)
        {
            yield return null;
        }

        if (waveManager.gameCompleted)
        {
            FinishGame();
            yield break;
        }

        StartStation();
    }

    void StartStation()
    {
        if (isGameFinished)
            return;

        isStation = true;

        Debug.Log("STATION PHASE START!");

        StartCoroutine(StationTimer());
    }

    IEnumerator StationTimer()
    {
        stationTimer = 0f;

        while (stationTimer < stationDuration && !isGameFinished)
        {
            stationTimer += Time.deltaTime;
            yield return null;
        }

        if (isGameFinished)
            yield break;

        isStation = false;

        Debug.Log("LEAVE STATION!");

        waveManager.PrepareNextRound();

        StartGameplay();
    }

    void FinishGame()
    {
        isGameplay = false;
        isStation = false;
        isEndingRound = false;
        isGameFinished = true;

        Debug.Log("VICTORY! LAST TRAIN DEFENSE COMPLETED!");
    }
}
