using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Setup")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    [Tooltip("เว้นว่าง (None) เพื่อหมุนทั้งตัวป้อม")]
    public Transform rotatingPart;

    [Header("Shooting Sound")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    [Range(0f, 1f)]
    public float shootVolume = 0.7f;

    [Header("Aim Settings")]
    [Tooltip("ติ๊กถูกถ้าอยากให้ป้อมแค่หันซ้าย-ขวา (ฐานไม่เอียงตามความสูงศัตรู)")]
    public bool flipOnly = false;

    [Header("Upgrade Outline (5 สี สำหรับอัปเกรด 5 รอบ)")]
    public SpriteRenderer targetRenderer;
    [Tooltip("ความหนาขอบ (รูปใหญ่ระดับพันพิกเซลแนะนำ 10-20)")]
    public float outlineThickness = 14f;
    public Color[] upgradeOutlineColors = new Color[]
    {
        new Color(0.2f, 1f, 0.3f, 1f),
        new Color(0.2f, 0.75f, 1f, 1f),
        new Color(0.75f, 0.3f, 1f, 1f),
        new Color(1f, 0.8f, 0.1f, 1f),
        new Color(1f, 0.2f, 0.2f, 1f)
    };

    public TowerData Data { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentDamage { get; private set; } = 10;
    public float CurrentFireRate { get; private set; } = 1f;
    public float SearchRange { get; private set; } = 20f;

    public bool IsMaxLevel => Data == null || CurrentLevel >= Data.maxLevel;
    public int UpgradeCost => Data != null ? Data.upgradeScrapCost : 50;

    private float fireTimer;
    private Transform currentTarget;
    private Vector3 initialScale;
    private Material outlineMat;

    void Awake()
    {
        Transform pivot = rotatingPart != null ? rotatingPart : transform;
        initialScale = pivot.localScale;

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            outlineMat = targetRenderer.material;

        // หา AudioSource อัตโนมัติถ้ายังไม่ได้กำหนด
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(TowerData data, int startLevel = 1)
    {
        Data = data;
        SearchRange = data.searchRange;
        ApplyLevel(startLevel);
    }

    public void ApplyLevel(int level)
    {
        if (Data == null) return;

        CurrentLevel = Mathf.Clamp(level, 1, Data.maxLevel);
        int upgradeCount = CurrentLevel - 1;

        CurrentDamage = Data.baseDamage + (upgradeCount * Data.damageIncrease);
        CurrentFireRate = Mathf.Max(
            Data.minFireRate,
            Data.baseFireRate - (upgradeCount * Data.fireRateDecrease)
        );

        UpdateOutlineVisual();
    }

    void UpdateOutlineVisual()
    {
        if (outlineMat == null || !outlineMat.HasProperty("_OutlineSize"))
            return;

        int upgradeCount = CurrentLevel - 1;

        if (upgradeCount <= 0 || upgradeOutlineColors == null || upgradeOutlineColors.Length == 0)
        {
            outlineMat.SetFloat("_OutlineSize", 0f);
            return;
        }

        int colorIndex = Mathf.Clamp(upgradeCount - 1, 0, upgradeOutlineColors.Length - 1);
        outlineMat.SetFloat("_OutlineSize", outlineThickness);
        outlineMat.SetColor("_OutlineColor", upgradeOutlineColors[colorIndex]);
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;
            if (state != GameState.Gameplay && state != GameState.WaitingWaveClear)
                return;
        }

        FindNearestTarget();

        if (currentTarget == null) return;

        AimAtTarget();
        HandleShooting();
    }

    void FindNearestTarget()
    {
        float closestDist = SearchRange;
        currentTarget = null;

        CheckTargetsByTag("Enemy", ref closestDist);
        CheckTargetsByTag("Boss", ref closestDist);
    }

    void CheckTargetsByTag(string tagName, ref float closestDist)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tagName);

        foreach (GameObject obj in targets)
        {
            if (obj == null || !obj.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                currentTarget = obj.transform;
            }
        }
    }

    void AimAtTarget()
    {
        Transform pivot = rotatingPart != null ? rotatingPart : transform;
        Vector2 dir = currentTarget.position - pivot.position;

        if (flipOnly)
        {
            Vector3 scale = initialScale;
            scale.x = (dir.x < 0f) ? -Mathf.Abs(initialScale.x) : Mathf.Abs(initialScale.x);
            pivot.localScale = scale;
            pivot.rotation = Quaternion.identity;
        }
        else
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            pivot.rotation = Quaternion.Euler(0f, 0f, angle);

            Vector3 scale = initialScale;
            scale.y = (Mathf.Abs(angle) > 90f) ? -Mathf.Abs(initialScale.y) : Mathf.Abs(initialScale.y);
            pivot.localScale = scale;
        }
    }

    void HandleShooting()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer < CurrentFireRate) return;

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bulletObj = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

            Bullet bullet = bulletObj.GetComponent<Bullet>();

            if (bullet != null)
            {
                Vector2 dir = currentTarget.position - firePoint.position;
                bullet.Initialize(dir, CurrentDamage);
            }

            // เล่นเสียงเมื่อยิงกระสุน
            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound, shootVolume);
            }
        }

        fireTimer = 0f;
    }

    public bool TryUpgrade()
    {
        if (Data == null || TowerPlacementManager.Instance == null)
            return false;

        return TowerPlacementManager.Instance.TryUpgradeTowerType(Data);
    }
}
