using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TowerSlot : MonoBehaviour
{
    [Header("Visual Highlight")]
    public SpriteRenderer highlightRenderer;
    public Color availableColor = new Color(0f, 1f, 0f, 0.65f);
    public Color unavailableColor = new Color(1f, 0.2f, 0.2f, 0.5f);

    public Tower InstalledTower { get; private set; }
    public bool IsOccupied => InstalledTower != null;

    void Awake()
    {
        if (highlightRenderer == null)
            highlightRenderer = GetComponent<SpriteRenderer>();

        SetHighlight(false, false);
    }

    public void SetHighlight(bool isPlacingMode, bool canAfford)
    {
        if (highlightRenderer == null) return;

        if (IsOccupied || !isPlacingMode)
        {
            highlightRenderer.enabled = false;
            return;
        }

        highlightRenderer.enabled = true;
        highlightRenderer.color = canAfford ? availableColor : unavailableColor;
    }

    public bool InstallTower(TowerData data)
    {
        if (IsOccupied || data == null || data.towerPrefab == null)
            return false;

        GameObject towerObj = Instantiate(data.towerPrefab, transform.position, Quaternion.identity, transform);
        towerObj.transform.localPosition = Vector3.zero;

        Tower tower = towerObj.GetComponent<Tower>();
        if (tower != null)
        {
            // ดึงเลเวลที่เคยอัปเกรดไว้มาใช้ทันที
            int savedLevel = TowerPlacementManager.Instance != null
                ? TowerPlacementManager.Instance.GetTowerLevel(data)
                : 1;

            tower.Initialize(data, savedLevel);
            InstalledTower = tower;
        }

        SetHighlight(false, false);
        return true;
    }

    public void ClearSlot()
    {
        if (InstalledTower != null)
        {
            Destroy(InstalledTower.gameObject);
            InstalledTower = null;
        }

        SetHighlight(false, false);
    }
}