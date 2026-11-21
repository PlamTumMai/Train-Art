using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayHUD : MonoBehaviour
{
    [Serializable]
    public class HotbarSlotUI
    {
        public Button slotButton;
        public Image iconImage;
        public Image highlightFrame;   // กรอบไฮไลต์เมื่อเลือกช่องนี้ (เหมือนกรอบขาวใน Minecraft)
        public TMP_Text keyNumberText; // เลข 1, 2, 3
        public TMP_Text costText;      // ราคา Energy
    }

    [Header("Top HUD Texts")]
    public TMP_Text scrapText;
    public TMP_Text waveText;
    public TMP_Text timerText;

    [Header("Energy UI")]
    public TMP_Text energyText;
    public Image energyFillImage;      // ใส่ Image แบบ Filled (ถ้ามีหลอด Energy)

    [Header("Minecraft-Style Hotbar")]
    public HotbarSlotUI[] hotbarSlots;
    public Color affordableColor = Color.white;
    public Color unaffordableColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    void Start()
    {
        SetupHotbar();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScrapChanged += UpdateScrapUI;
            GameManager.Instance.OnEnergyChanged += UpdateEnergyUI;
            GameManager.Instance.OnStateChanged += UpdateStateUI;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveChanged += UpdateWaveUI;
        }

        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.OnSelectedSlotChanged += UpdateHotbarSelection;
        }

        RefreshInitialValues();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScrapChanged -= UpdateScrapUI;
            GameManager.Instance.OnEnergyChanged -= UpdateEnergyUI;
            GameManager.Instance.OnStateChanged -= UpdateStateUI;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveChanged -= UpdateWaveUI;
        }

        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.OnSelectedSlotChanged -= UpdateHotbarSelection;
        }
    }

    void SetupHotbar()
    {
        if (TowerPlacementManager.Instance == null) return;
        TowerData[] towers = TowerPlacementManager.Instance.availableTowers;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            HotbarSlotUI slot = hotbarSlots[i];
            int index = i;

            if (slot.keyNumberText != null)
                slot.keyNumberText.text = (i + 1).ToString();

            if (slot.highlightFrame != null)
                slot.highlightFrame.enabled = false;

            bool hasTower = towers != null && i < towers.Length && towers[i] != null;

            if (hasTower)
            {
                TowerData data = towers[i];
                if (slot.iconImage != null)
                {
                    slot.iconImage.enabled = true;
                    slot.iconImage.sprite = data.icon;
                }
                if (slot.costText != null)
                    slot.costText.text = Mathf.CeilToInt(data.energyCost).ToString();

                if (slot.slotButton != null)
                {
                    slot.slotButton.interactable = true;
                    slot.slotButton.onClick.AddListener(() =>
                        TowerPlacementManager.Instance.ToggleSelectTower(index)
                    );
                }
            }
            else
            {
                if (slot.iconImage != null) slot.iconImage.enabled = false;
                if (slot.costText != null) slot.costText.text = "";
                if (slot.slotButton != null) slot.slotButton.interactable = false;
            }
        }
    }

    void RefreshInitialValues()
    {
        if (GameManager.Instance != null)
        {
            UpdateScrapUI(GameManager.Instance.Scrap);
            UpdateEnergyUI(GameManager.Instance.CurrentEnergy, GameManager.Instance.maxEnergy);
            UpdateStateUI(GameManager.Instance.CurrentState);
        }

        if (WaveManager.Instance != null && WaveManager.Instance.CurrentWave > 0)
        {
            UpdateWaveUI(WaveManager.Instance.CurrentWave, WaveManager.Instance.maxWaves);
        }

        if (TowerPlacementManager.Instance != null)
        {
            UpdateHotbarSelection(TowerPlacementManager.Instance.SelectedIndex);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || timerText == null) return;

        GameState state = GameManager.Instance.CurrentState;
        if (state == GameState.Gameplay)
        {
            float t = GameManager.Instance.GameplayTimeRemaining;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void UpdateStateUI(GameState state)
    {
        if (timerText == null) return;

        if (state == GameState.WaitingWaveClear)
            timerText.text = "CLEAR ENEMIES!";
        else if (state == GameState.Station)
            timerText.text = "STATION";
    }

    void UpdateScrapUI(int scrap)
    {
        if (scrapText != null)
            scrapText.text = $"Scrap: {scrap}";
    }

    void UpdateEnergyUI(float current, float max)
    {
        if (energyText != null)
            energyText.text = $"Energy: {Mathf.FloorToInt(current)}/{Mathf.FloorToInt(max)}";

        if (energyFillImage != null && max > 0f)
            energyFillImage.fillAmount = current / max;

        UpdateHotbarAffordability(current);
    }

    void UpdateHotbarAffordability(float currentEnergy)
    {
        if (TowerPlacementManager.Instance == null) return;
        TowerData[] towers = TowerPlacementManager.Instance.availableTowers;
        if (towers == null) return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (i >= towers.Length || towers[i] == null) continue;
            if (hotbarSlots[i].iconImage == null) continue;

            bool canAfford = currentEnergy >= towers[i].energyCost;
            hotbarSlots[i].iconImage.color = canAfford ? affordableColor : unaffordableColor;
        }
    }

    void UpdateWaveUI(int currentWave, int maxWaves)
    {
        if (waveText != null)
            waveText.text = $"Wave {currentWave}/{maxWaves}";
    }

    void UpdateHotbarSelection(int selectedIndex)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].highlightFrame != null)
                hotbarSlots[i].highlightFrame.enabled = (i == selectedIndex);
        }
    }
}