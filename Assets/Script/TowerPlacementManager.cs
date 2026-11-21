using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance { get; private set; }

    [Header("Hotbar Towers (เรียงตามปุ่ม 1, 2, 3, 4)")]
    public TowerData[] availableTowers;

    [Header("Ghost Preview Settings")]
    public Color ghostValidColor = new Color(1f, 1f, 1f, 0.7f);
    public Color ghostInvalidColor = new Color(1f, 0.3f, 0.3f, 0.6f);
    public Vector3 ghostScale = Vector3.one;

    public int SelectedIndex { get; private set; } = -1;
    public TowerSlot[] AllSlots { get; private set; }

    public event Action<int> OnSelectedSlotChanged;

    // จำเลเวลของป้อมแต่ละชนิดตลอดรอบเกม (ไม่ทับไฟล์ ScriptableObject)
    private readonly Dictionary<TowerData, int> towerLevels = new Dictionary<TowerData, int>();

    private Camera mainCam;
    private GameObject ghostObject;
    private SpriteRenderer ghostRenderer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        mainCam = Camera.main;
        AllSlots = FindObjectsByType<TowerSlot>(FindObjectsSortMode.None);

        CreateGhostObject();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnergyChanged += HandleEnergyChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnStartNextRound += ClearAllTowers;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnergyChanged -= HandleEnergyChanged;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnStartNextRound -= ClearAllTowers;
        }
    }

    // ดึงเลเวลปัจจุบันของป้อมชนิดนั้น
    public int GetTowerLevel(TowerData data)
    {
        if (data == null) return 1;
        return towerLevels.TryGetValue(data, out int level) ? level : 1;
    }

    // อัปเกรดป้อมชนิดนี้ถาวร + อัปเดตทุกกระบอกชนิดเดียวกันที่วางอยู่บนรถไฟทันที
    public bool TryUpgradeTowerType(TowerData data)
    {
        if (data == null || GameManager.Instance == null) return false;
        if (GameManager.Instance.CurrentState != GameState.Station) return false;

        int currentLevel = GetTowerLevel(data);
        if (currentLevel >= data.maxLevel) return false;
        if (!GameManager.Instance.SpendScrap(data.upgradeScrapCost)) return false;

        int newLevel = currentLevel + 1;
        towerLevels[data] = newLevel;

        if (AllSlots != null)
        {
            foreach (TowerSlot slot in AllSlots)
            {
                if (slot != null && slot.IsOccupied && slot.InstalledTower.Data == data)
                {
                    slot.InstalledTower.ApplyLevel(newLevel);
                }
            }
        }

        Debug.Log($"[Placement] อัปเกรด {data.towerName} ถาวร -> Lv.{newLevel}");
        return true;
    }

    public void ClearAllTowers()
    {
        CancelSelection();

        if (AllSlots == null) return;

        foreach (TowerSlot slot in AllSlots)
        {
            if (slot != null)
                slot.ClearSlot();
        }
    }

    void CreateGhostObject()
    {
        ghostObject = new GameObject("TowerGhostPreview");
        ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        ghostRenderer.sortingOrder = 50;
        ghostObject.transform.localScale = ghostScale;
        ghostObject.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;
            if (state == GameState.GameOver || state == GameState.Victory)
                return;
        }

        HandleHotbarInput();

        if (SelectedIndex != -1)
        {
            UpdateGhostAndClick();
        }
    }

    void HandleHotbarInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) ToggleSelectTower(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) ToggleSelectTower(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) ToggleSelectTower(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) ToggleSelectTower(3);
        else if (Keyboard.current.escapeKey.wasPressedThisFrame) CancelSelection();

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelSelection();
        }
    }

    public void ToggleSelectTower(int index)
    {
        if (availableTowers == null || index < 0 || index >= availableTowers.Length)
            return;

        if (availableTowers[index] == null)
            return;

        if (SelectedIndex == index)
        {
            CancelSelection();
            return;
        }

        SelectedIndex = index;
        TowerData data = availableTowers[SelectedIndex];

        ghostRenderer.sprite = data.previewSprite != null ? data.previewSprite : data.icon;
        ghostObject.SetActive(true);

        RefreshHighlights();
        OnSelectedSlotChanged?.Invoke(SelectedIndex);
    }

    public void CancelSelection()
    {
        if (SelectedIndex == -1) return;

        SelectedIndex = -1;
        if (ghostObject != null) ghostObject.SetActive(false);

        RefreshHighlights();
        OnSelectedSlotChanged?.Invoke(SelectedIndex);
    }

    void UpdateGhostAndClick()
    {
        if (Mouse.current == null || mainCam == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, -mainCam.transform.position.z)
        );
        mouseWorld.z = 0f;

        TowerData currentData = availableTowers[SelectedIndex];
        bool canAfford = GameManager.Instance != null && GameManager.Instance.HasEnoughEnergy(currentData.energyCost);

        TowerSlot hoveredSlot = GetSlotUnderPoint(mouseWorld);

        ghostObject.transform.position = (hoveredSlot != null && !hoveredSlot.IsOccupied)
            ? hoveredSlot.transform.position
            : mouseWorld;

        ghostRenderer.color = canAfford ? ghostValidColor : ghostInvalidColor;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (hoveredSlot != null && !hoveredSlot.IsOccupied)
            {
                if (GameManager.Instance != null && GameManager.Instance.SpendEnergy(currentData.energyCost))
                {
                    hoveredSlot.InstallTower(currentData);
                    RefreshHighlights();
                }
            }
        }
    }

    TowerSlot GetSlotUnderPoint(Vector2 worldPos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        foreach (Collider2D hit in hits)
        {
            TowerSlot slot = hit.GetComponent<TowerSlot>();
            if (slot != null) return slot;
        }
        return null;
    }

    void RefreshHighlights()
    {
        bool isPlacing = SelectedIndex != -1;
        bool canAfford = isPlacing && GameManager.Instance != null &&
                         GameManager.Instance.HasEnoughEnergy(availableTowers[SelectedIndex].energyCost);

        foreach (TowerSlot slot in AllSlots)
        {
            if (slot != null)
                slot.SetHighlight(isPlacing, canAfford);
        }
    }

    void HandleEnergyChanged(float current, float max)
    {
        if (SelectedIndex != -1) RefreshHighlights();
    }

    void HandleStateChanged(GameState newState)
    {
        if (newState == GameState.GameOver || newState == GameState.Victory)
            CancelSelection();
    }
}