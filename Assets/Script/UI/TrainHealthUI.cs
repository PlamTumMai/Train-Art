using UnityEngine;
using UnityEngine.UI;

public class TrainHealthUI : MonoBehaviour
{
    [Header("References")]
    public TrainHealth trainHealth;
    public Image fillImage;

    [Header("Settings")]
    [Tooltip("ความเร็วในการค่อยๆ ลด/เพิ่มของแถบเลือด")]
    public float smoothSpeed = 6f;

    void Start()
    {
        if (trainHealth != null && fillImage != null && trainHealth.maxHP > 0)
        {
            fillImage.fillAmount = (float)trainHealth.CurrentHP / trainHealth.maxHP;
        }
    }

    void Update()
    {
        if (trainHealth == null || fillImage == null || trainHealth.maxHP <= 0) return;

        float targetFill = (float)trainHealth.CurrentHP / trainHealth.maxHP;

        // ใช้ unscaledDeltaTime เพื่อให้แถบเลือดยังไหลลดจนหมดหลอดได้
        // แม้ตอน GameOver จะสั่ง Time.timeScale = 0f
        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            targetFill,
            Time.unscaledDeltaTime * smoothSpeed
        );
    }
}