using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "TrainTD/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Info & UI")]
    public string towerName = "Basic Turret";
    public Sprite icon;                 // รูปสำหรับโชว์บน Hotbar
    public Sprite previewSprite;        // รูปตอนลอยตามเมาส์ (ถ้าเว้นว่างจะใช้ icon แทน)

    [Header("Placement")]
    public float energyCost = 20f;      // ค่า Energy ที่ใช้ติดตั้ง (เหมือนแร่ Line Rangers)
    public GameObject towerPrefab;      // Prefab ตัวป้อมที่มีสคริปต์ Tower แปะอยู่

    [Header("Combat Stats")]
    public int baseDamage = 10;
    public float baseFireRate = 1f;     // ยิง 1 นัดต่อกี่วินาที (ค่าน้อย = ยิงไว)
    public float searchRange = 20f;

    [Header("Upgrade Stats (ใช้ Scrap ในหน้า Station)")]
    public int maxLevel = 5;
    public int upgradeScrapCost = 50;
    public int damageIncrease = 5;
    public float fireRateDecrease = 0.1f;
    public float minFireRate = 0.2f;
}