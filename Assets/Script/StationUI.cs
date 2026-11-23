using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StationUI : MonoBehaviour
{
    [Header("Panel & Header")] public GameObject stationPanel;
    public TMP_Text stationTimerText;
    public Button departButton;

    [Header("Train Upgrade (ลากปุ่มไปวางใต้หัวรถจักรได้เลย)")]
    public TrainHealth trainHealth;

    public TMP_Text trainInfoText;
    public TMP_Text trainCostText;
    public Button trainUpgradeButton;
    [Header("Energy Upgrade")]
    public TMP_Text energyInfoText;
    public TMP_Text energyCostText;
    public Button energyUpgradeButton;
    [Header("Auto Tower Upgrade Buttons")]
    [Tooltip("กล่องว่าง (Stretch เต็มจอ) ใน StationPanel สำหรับเก็บปุ่มที่เสกออกมา")]
    public RectTransform widgetContainer;

    public TowerUpgradeWidget towerWidgetPrefab;

    [Tooltip("ระยะขยับจากตำแหน่งป้อม (ค่า Y ติดลบ = ลงมาอยู่ใต้ป้อม)")]
    public Vector3 widgetWorldOffset = new Vector3(0f, -0.75f, 0f);

    private readonly List<TowerUpgradeWidget> activeWidgets = new List<TowerUpgradeWidget>();
    private Camera mainCam;
    private Canvas parentCanvas;

    void Start()
    {
        mainCam = Camera.main;
        
        if (widgetContainer != null)
            parentCanvas = widgetContainer.GetComponentInParent<Canvas>();

        if (departButton != null)
            departButton.onClick.AddListener(() => GameManager.Instance?.LeaveStationEarly());

        if (trainUpgradeButton != null)
            trainUpgradeButton.onClick.AddListener(OnClickUpgradeTrain);
        
        if (energyUpgradeButton != null)
            energyUpgradeButton.onClick.AddListener(OnClickUpgradeEnergy);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnScrapChanged += HandleScrapChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
        else if (stationPanel != null)
        {
            stationPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnScrapChanged -= HandleScrapChanged;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Station)
            return;

        if (stationTimerText != null)
        {
            int sec = Mathf.CeilToInt(GameManager.Instance.StationTimeRemaining);
            stationTimerText.text = $"Departing in {sec}s";
        }
    }

    void HandleStateChanged(GameState state)
    {
        bool isStation = (state == GameState.Station);

        if (stationPanel != null)
            stationPanel.SetActive(isStation);

        if (isStation)
        {
            RebuildTowerWidgets();
            RefreshAllUI();
        }
        else
        {
            ClearWidgets();
        }
    }

    void HandleScrapChanged(int scrap)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Station)
            RefreshAllUI();
    }

    void OnClickUpgradeTrain()
    {
        if (trainHealth != null && trainHealth.TryUpgradeTrain())
            RefreshAllUI();
    }
    void OnClickUpgradeEnergy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TryUpgradeEnergy())
            RefreshAllUI();
    }

    void RebuildTowerWidgets()
    {
        ClearWidgets();

        if (towerWidgetPrefab == null || widgetContainer == null)
            return;

        if (TowerPlacementManager.Instance == null)
            return;

        TowerData[] availableTowers =
            TowerPlacementManager.Instance.availableTowers;

        if (availableTowers == null)
            return;

        int currentScrap = GameManager.Instance != null
            ? GameManager.Instance.Scrap
            : 0;

        foreach (TowerData data in availableTowers)
        {
            if (data == null) continue;

            TowerUpgradeWidget widget =
                Instantiate(towerWidgetPrefab, widgetContainer);

            // ผูก Widget กับประเภทอาวุธโดยตรง
            widget.Setup(data, RefreshAllUI);

            widget.Refresh(currentScrap);

            activeWidgets.Add(widget);
        }
    }

    void ClearWidgets()
    {
        foreach (TowerUpgradeWidget w in activeWidgets)
        {
            if (w != null) Destroy(w.gameObject);
        }

        activeWidgets.Clear();
    }

    void RefreshAllUI()
    {
        int currentScrap = GameManager.Instance != null ? GameManager.Instance.Scrap : 0;

        // 1. อัปเดตปุ่มและข้อความอัปเกรดรถไฟ
        if (trainHealth != null)
        {
            if (trainInfoText != null)
            {
                trainInfoText.text = trainHealth.IsMaxLevel
                    ? $"Train Lv.{trainHealth.trainLevel}/{trainHealth.maxLevel} (MAX)\nHP: {trainHealth.CurrentHP}/{trainHealth.maxHP}"
                    : $"Train Lv.{trainHealth.trainLevel}/{trainHealth.maxLevel}\nHP: {trainHealth.CurrentHP}/{trainHealth.maxHP} (+{trainHealth.hpIncrease} HP)";
            }

            if (trainHealth.IsMaxLevel)
            {
                if (trainCostText != null) trainCostText.text = "MAX";
                if (trainUpgradeButton != null) trainUpgradeButton.interactable = false;
            }
            else
            {
                if (trainCostText != null) trainCostText.text = $"{trainHealth.upgradeCost} Scrap";
                if (trainUpgradeButton != null) trainUpgradeButton.interactable = currentScrap >= trainHealth.upgradeCost;
            }
        }

        // 2. อัปเดตปุ่มและข้อความอัปเกรดความเร็ว Energy (แยกออกมาอยู่นอก if ของรถไฟแล้ว)
        if (GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;

            if (energyInfoText != null)
            {
                if (gm.IsEnergyMaxLevel)
                {
                    energyInfoText.text =
                        $"Energy Lv.{gm.energyLevel}/{gm.maxEnergyLevel} (MAX)\n" +
                        $"Regen: {gm.energyRegenPerSecond:F1}/s";
                }
                else
                {
                    string maxBonus = gm.maxEnergyIncreasePerLevel > 0f
                        ? $"\nMax: {gm.maxEnergy:F0} (+{gm.maxEnergyIncreasePerLevel:F0})"
                        : "";

                    energyInfoText.text =
                        $"Energy Lv.{gm.energyLevel}/{gm.maxEnergyLevel}\n" +
                        $"Regen: {gm.energyRegenPerSecond:F1}/s (+{gm.regenIncreasePerLevel:F1}/s){maxBonus}";
                }
            }

            if (gm.IsEnergyMaxLevel)
            {
                if (energyCostText != null) energyCostText.text = "MAX";
                if (energyUpgradeButton != null) energyUpgradeButton.interactable = false;
            }
            else
            {
                if (energyCostText != null) energyCostText.text = $"{gm.energyUpgradeCost} Scrap";
                if (energyUpgradeButton != null) energyUpgradeButton.interactable = currentScrap >= gm.energyUpgradeCost;
            }
        }

        // 3. อัปเดตปุ่มอัปเกรดป้อมทั้งหมด
        foreach (TowerUpgradeWidget w in activeWidgets)
        {
            if (w != null) w.Refresh(currentScrap);
        }
    }
}