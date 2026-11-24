using System.Collections;
using UnityEngine;

public class TrainHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;

    public int CurrentHP => currentHP;
    public bool IsMaxLevel => trainLevel >= maxLevel;

    [Header("Train Upgrade")]
    public int trainLevel = 1;
    public int maxLevel = 5;
    public int upgradeCost = 100;
    public int hpIncrease = 50;

    [Header("Damage Sound")]
    public AudioClip damageSound;

    [Range(0f, 1f)]
    public float damageVolume = 0.8f;

    [Header("Fire VFX")]
    public ParticleSystem fireVFX;

    [Header("Camera Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.15f;

    private Transform mainCamera;
    private Vector3 cameraOriginalPosition;
    private Coroutine shakeCoroutine;

    void Start()
    {
        currentHP = maxHP;

        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
            cameraOriginalPosition = mainCamera.localPosition;
        }
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.GameOver)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        // เล่นเสียงตอนเลือดลด
        PlayDamageSound();

        // แสดงเอฟเฟกต์ไฟ
        PlayFireVFX();

        // ทำให้หน้าจอสั่น
        ShakeCamera();

        if (currentHP <= 0)
        {
            GameManager.Instance?.TriggerGameOver();
        }
    }

    void PlayDamageSound()
    {
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(
                damageSound,
                transform.position,
                damageVolume
            );
        }
    }

    void PlayFireVFX()
    {
        if (fireVFX != null)
        {
            // เพิ่มอนุภาคใหม่โดยไม่ลบอนุภาคเก่า
            fireVFX.Emit(25);
        }
    }

    void ShakeCamera()
    {
        if (mainCamera == null)
        {
            if (Camera.main != null)
            {
                mainCamera = Camera.main.transform;
                cameraOriginalPosition = mainCamera.localPosition;
            }
            else
            {
                return;
            }
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(CameraShakeCoroutine());
    }

    IEnumerator CameraShakeCoroutine()
    {
        cameraOriginalPosition = mainCamera.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            mainCamera.localPosition = cameraOriginalPosition +
                new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        mainCamera.localPosition = cameraOriginalPosition;
        shakeCoroutine = null;
    }

    // เรียกจากปุ่ม Upgrade Train บนหน้า Station UI
    public bool TryUpgradeTrain()
    {
        if (IsMaxLevel) return false;
        if (GameManager.Instance == null) return false;

        if (GameManager.Instance.CurrentState != GameState.Station)
            return false;

        if (!GameManager.Instance.SpendScrap(upgradeCost))
            return false;

        trainLevel++;
        maxHP += hpIncrease;
        currentHP = maxHP;

        return true;
    }

    void OnDisable()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (mainCamera != null)
        {
            mainCamera.localPosition = cameraOriginalPosition;
        }
    }
}
