using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgradeWidget : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelText;
    public TMP_Text statsText;
    public TMP_Text costText;
    public Button upgradeButton;

    [Header("Weapon Icon")]
    public Image towerIcon;

    private TowerData targetData;
    private Action onUpgradedCallback;

    public void Setup(TowerData data, Action onUpgraded)
    {
        targetData = data;
        onUpgradedCallback = onUpgraded;

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }
    }

    void OnClickUpgrade()
    {
        if (targetData == null || TowerPlacementManager.Instance == null)
            return;

        if (TowerPlacementManager.Instance.TryUpgradeTowerType(targetData))
        {
            onUpgradedCallback?.Invoke();
        }
    }

    public void Refresh(int currentScrap)
    {
        if (targetData == null || TowerPlacementManager.Instance == null)
            return;

        TowerData data = targetData;

        // แสดงรูปของอาวุธแต่ละชนิด
        if (towerIcon != null)
        {
            towerIcon.sprite = targetData.icon;
            towerIcon.enabled = targetData.icon != null;
        }

        int currentLevel = TowerPlacementManager.Instance.GetTowerLevel(data);
        int maxLevel = data.maxLevel;

        int currentDamage = data.baseDamage +
            ((currentLevel - 1) * data.damageIncrease);

        float currentFireRate = Mathf.Max(
            data.minFireRate,
            data.baseFireRate - ((currentLevel - 1) * data.fireRateDecrease)
        );

        string levelStr = $"Lv.{currentLevel}/{maxLevel}";
        string statsStr;

        if (currentLevel >= maxLevel)
        {
            statsStr = $"DMG:{currentDamage}\nRate:{currentFireRate:F2}s";
        }
        else
        {
            int nextDamage = currentDamage + data.damageIncrease;

            float nextRate = Mathf.Max(
                data.minFireRate,
                currentFireRate - data.fireRateDecrease
            );

            float rateDiff = currentFireRate - nextRate;

            string dmgLine = $"DMG:{currentDamage} (+{data.damageIncrease})";

            string rateLine = rateDiff > 0.001f
                ? $"Rate:{currentFireRate:F1}s (-{rateDiff:F1}s)"
                : $"Rate:{currentFireRate:F1}s (MAX)";

            statsStr = $"{dmgLine}\n{rateLine}";
        }

        if (levelText != null)
            levelText.text = levelStr;

        if (statsText != null)
            statsText.text = statsStr;

        if (currentLevel >= maxLevel)
        {
            if (costText != null)
                costText.text = "MAX";

            if (upgradeButton != null)
                upgradeButton.interactable = false;
        }
        else
        {
            if (costText != null)
                costText.text = $"{data.upgradeScrapCost}";

            if (upgradeButton != null)
                upgradeButton.interactable = currentScrap >= data.upgradeScrapCost;
        }
    }
}
