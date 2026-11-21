using System;
using System.Collections;
using UnityEngine;

public enum GameState
{
    Gameplay,
    WaitingWaveClear, // ครบ 60 วิแล้ว กำลังรอเคลียร์ศัตรูที่เหลือ
    Station,
    Victory,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Loop Settings")]
    public float gameplayDuration = 60f;
    public float stationDuration = 15f;

    [Header("Energy (ใช้เรียกป้อม - เด้งเองแบบ Line Rangers)")]
    public float maxEnergy = 100f;
    public float startEnergy = 50f;
    public float energyRegenPerSecond = 8f;
    public bool resetEnergyEachWave = true;
    [Header("Energy Upgrade (อัปเกรดในหน้า Station)")]
    public int energyLevel = 1;
    public int maxEnergyLevel = 5;
    public int energyUpgradeCost = 80;
    public float regenIncreasePerLevel = 4f;
    [Tooltip("เพิ่ม Energy สูงสุดด้วยไหม (ใส่ 0 ถ้าอยากเพิ่มแค่ความเร็วเด้งอย่างเดียว)")]
    public float maxEnergyIncreasePerLevel = 20f;
    
    public bool IsEnergyMaxLevel => energyLevel >= maxEnergyLevel;
    [Header("Scrap (ใช้อัปเกรดตอนเข้าสถานี)")]
    public int startScrap = 0;

    public GameState CurrentState { get; private set; }
    public float CurrentEnergy { get; private set; }
    public int Scrap { get; private set; }
    public int Score { get; private set; }
    public int KillCount { get; private set; }

    public float GameplayTimeRemaining => Mathf.Max(0f, gameplayDuration - gameplayTimer);
    public float StationTimeRemaining => Mathf.Max(0f, stationDuration - stationTimer);

    private float gameplayTimer;
    private float stationTimer;
    private Coroutine stationCoroutine;

    public event Action<GameState> OnStateChanged;
    public event Action OnGameplayTimerEnded;
    public event Action OnStartNextRound;
    public event Action<float, float> OnEnergyChanged;
    public event Action<int> OnScrapChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        Time.timeScale = 1f;
        CurrentEnergy = Mathf.Clamp(startEnergy, 0f, maxEnergy);
        Scrap = startScrap;

        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        OnScrapChanged?.Invoke(Scrap);

        StartGameplayPhase();
    }

    void Update()
    {
        if (CurrentState == GameState.Gameplay || CurrentState == GameState.WaitingWaveClear)
        {
            RegenerateEnergy();
        }

        if (CurrentState == GameState.Gameplay)
        {
            gameplayTimer += Time.deltaTime;
            if (gameplayTimer >= gameplayDuration)
            {
                ChangeState(GameState.WaitingWaveClear);

                if (OnGameplayTimerEnded != null)
                    OnGameplayTimerEnded.Invoke();
                else
                    EnterStation();
            }
        }
    }

    void RegenerateEnergy()
    {
        if (CurrentEnergy >= maxEnergy) return;

        CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + energyRegenPerSecond * Time.deltaTime);
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    public bool HasEnoughEnergy(float cost) => CurrentEnergy >= cost;

    public bool SpendEnergy(float cost)
    {
        if (CurrentEnergy < cost) return false;

        CurrentEnergy -= cost;
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        return true;
    }
    public bool TryUpgradeEnergy()
    {
        if (IsEnergyMaxLevel) return false;
        if (CurrentState != GameState.Station) return false;
        if (!SpendScrap(energyUpgradeCost)) return false;

        energyLevel++;
        energyRegenPerSecond += regenIncreasePerLevel;
        maxEnergy += maxEnergyIncreasePerLevel;

        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        Debug.Log($"[GameManager] Energy Lv.{energyLevel} -> Regen: {energyRegenPerSecond}/s | Max: {maxEnergy}");
        return true;
    }
    public void AddScrap(int amount)
    {
        Scrap += amount;
        OnScrapChanged?.Invoke(Scrap);
    }

    public bool SpendScrap(int amount)
    {
        if (Scrap < amount) return false;

        Scrap -= amount;
        OnScrapChanged?.Invoke(Scrap);
        return true;
    }

    public void AddKillAndScore(int scoreReward, int scrapReward)
    {
        KillCount++;
        Score += scoreReward;
        AddScrap(scrapReward);
    }

    // --- ตรงนี้ครับ StartGameplayPhase() ---
    public void StartGameplayPhase()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) return;

        gameplayTimer = 0f;

        if (resetEnergyEachWave)
        {
            CurrentEnergy = Mathf.Clamp(startEnergy, 0f, maxEnergy);
            OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        }

        ChangeState(GameState.Gameplay);
        OnStartNextRound?.Invoke();
    }

    public void EnterStation()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) return;

        ChangeState(GameState.Station);
        if (stationCoroutine != null) StopCoroutine(stationCoroutine);
        stationCoroutine = StartCoroutine(StationCountdownRoutine());
    }

    IEnumerator StationCountdownRoutine()
    {
        stationTimer = 0f;
        while (stationTimer < stationDuration && CurrentState == GameState.Station)
        {
            stationTimer += Time.deltaTime;
            yield return null;
        }

        if (CurrentState == GameState.Station)
        {
            LeaveStationEarly();
        }
    }

    public void LeaveStationEarly()
    {
        if (CurrentState != GameState.Station) return;

        if (stationCoroutine != null) StopCoroutine(stationCoroutine);
        StartGameplayPhase();
    }

    public void TriggerVictory()
    {
        ChangeState(GameState.Victory);
        Time.timeScale = 0f;
    }

    public void TriggerGameOver()
    {
        ChangeState(GameState.GameOver);
        Time.timeScale = 0f;
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] State -> {newState}");
        OnStateChanged?.Invoke(newState);
    }
}