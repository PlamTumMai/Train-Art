using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgradeWidget : MonoBehaviour
{
    public TMP_Text levelText;    // โชว์ Lv.1/6
    public TMP_Text statsText;    // โชว์ DMG +5 | Rate -0.1s (เว้นว่างได้ จะไปต่อท้ายใน levelText แทน)
    public TMP_Text costText;     // โชว์ราคาบนปุ่ม เช่น "50" หรือ "MAX"
    public Button upgradeButton;

    private TowerSlot targetSlot;
    private Action onUpgradedCallback;

    public void Setup(TowerSlot slot, Action onUpgraded)
    {
        targetSlot = slot;
        onUpgradedCallback = onUpgraded;

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }
    }

    void OnClickUpgrade()
    {
        if (targetSlot == null || !targetSlot.IsOccupied) return;

        if (targetSlot.InstalledTower.TryUpgrade())
        {
            onUpgradedCallback?.Invoke();
        }
    }

    public void Refresh(int currentScrap)
    {
        if (targetSlot == null || !targetSlot.IsOccupied) return;

        Tower tower = targetSlot.InstalledTower;
        TowerData data = tower.Data;
        int maxLv = data != null ? data.maxLevel : 6;

        string levelStr = $"Lv.{tower.CurrentLevel}/{maxLv}";
        string statsStr;

        if (tower.IsMaxLevel || data == null)
        {
            statsStr = $"DMG:{tower.CurrentDamage}\nRate:{tower.CurrentFireRate:F2}s";
        }
        else
        {
            // คำนวณส่วนต่างของความเร็วยิงจริง (กรณีถึง minFireRate แล้วจะได้ไม่โชว์ติดลบ)
            float nextRate = Mathf.Max(data.minFireRate, tower.CurrentFireRate - data.fireRateDecrease);
            float rateDiff = tower.CurrentFireRate - nextRate;

            string dmgLine = $"DMG:{tower.CurrentDamage} (+{data.damageIncrease})";
            string rateLine = rateDiff > 0.001f
                ? $"Rate:{tower.CurrentFireRate:F1}s (-{rateDiff:F1}s)"
                : $"Rate:{tower.CurrentFireRate:F1}s (MAX)";

            statsStr = $"{dmgLine}\n{rateLine}";
        }

        if (statsText != null)
        {
            if (levelText != null) levelText.text = levelStr;
            statsText.text = statsStr;
        }
        else if (levelText != null)
        {
            // ถ้าไม่ได้สร้างกล่อง StatsText แยก จะรวมโชว์ใน LevelText อันเดียวให้เลย
            levelText.text = $"{levelStr}\n{statsStr}";
        }

        if (tower.IsMaxLevel)
        {
            if (costText != null) costText.text = "MAX";
            if (upgradeButton != null) upgradeButton.interactable = false;
        }
        else
        {
            if (costText != null) costText.text = $"{tower.UpgradeCost}";
            if (upgradeButton != null) upgradeButton.interactable = currentScrap >= tower.UpgradeCost;
        }
    }
}