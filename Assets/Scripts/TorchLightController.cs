using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System;

public class TorchLightController : MonoBehaviour
{
    [Header("Isik Ayarlari")]
    [SerializeField] private float maxLightRadius = 10f;
    [SerializeField] private float minLightRadius = 2f;
    [SerializeField] private float lightDecreaseRate = 0.1f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] public bool isMainTorch = false;
    [SerializeField] private float mainTorchRange = 15f; // Ana meşalenin etki alanı

    [Header("Dusman Etkileri")]
    [SerializeField] private float enemySlowRate = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Meşale Sprite Ayarları")]
    [SerializeField] private Sprite normalTorchSprite;
    [SerializeField] private Sprite powerfulTorchSprite;


    [Header("Meşale Animasyon Ayarları")]
    [SerializeField] private float spriteTransitionThreshold = 0.7f;
    [SerializeField] private float flickerIntensity = 0.1f;
    [SerializeField] private float flickerSpeed = 10f;

    [Header("Hasar ve Sönme Ayarları")]
    [SerializeField] private float damageDecayRate = 0.5f; // Hasar alınca ışığın azalma hızı
    [SerializeField] private float minHealthBeforeDestroy = 10f; // Bu değerin altında sönmeye başlar

    [SerializeField] private Light2D torchLight;
    private float currentHealth;
    private float currentLightRadius;
    private Transform mainTorch;
    private SpriteRenderer torchSprite;
    private Animator animator;
    private bool isPowerfulMode = false;

    [SerializeField] private float baseRadius = 3f;
    [SerializeField] private float radiusIncreasePerWood = 0.5f;
    [SerializeField] private float maxRadius = 6f;

    private bool isDead = false;

    private void Awake()
    {
        if (torchLight == null)
            torchLight = GetComponent<Light2D>();
            
        currentHealth = baseRadius;
        maxHealth = maxRadius;
        UpdateLightRadius();
    }

    private void Start()
    {
        torchSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        currentLightRadius = isMainTorch ? maxLightRadius : minLightRadius;
        torchLight.pointLightOuterRadius = currentLightRadius;

        if (!isMainTorch)
        {
            // Ana meşaleyi bul
            var mainTorchObj = GameObject.FindGameObjectWithTag("MainTorch");
            if (mainTorchObj != null)
            {
                mainTorch = mainTorchObj.transform;
            }
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            // Sürekli can azalt
            currentHealth -= 0.1f * Time.deltaTime;
            UpdateLightRadius();

            // Meşale söndü mü kontrol et
            if (currentHealth <= 0)
            {
                isDead = true;
                torchLight.pointLightOuterRadius = 0;
            }
        }

        if (isMainTorch)
        {
            DecreaseLightRadius();
            UpdateTorchAnimation();
        }
        else
        {
            // Yan meşaleler ana meşaleden güç alır
            UpdateSatelliteTorch();
        }

        // Işık alanındaki düşmanları yavaşlat
        SlowEnemiesInRange();
    }

    private void DecreaseLightRadius()
    {
        currentLightRadius = Mathf.Max(minLightRadius,
            currentLightRadius - (lightDecreaseRate * Time.deltaTime));
        torchLight.pointLightOuterRadius = currentLightRadius;
    }

    private void UpdateSatelliteTorch()
    {
        if (mainTorch != null)
        {
            // Ana meşaleye olan uzaklığı hesapla
            float distanceToMain = Vector2.Distance(transform.position, mainTorch.position);

            // Ana meşalenin gücünü al
            var mainTorchController = mainTorch.GetComponent<TorchLightController>();
            float mainTorchPower = mainTorchController.GetLightRatio();

            // Uzaklığa göre güç hesapla
            float distanceRatio = 1f - Mathf.Clamp01(distanceToMain / mainTorchRange);
            float finalPower = mainTorchPower * distanceRatio;

            // Işık yarıçapını güncelle
            currentLightRadius = Mathf.Lerp(minLightRadius, maxLightRadius, finalPower);
            torchLight.pointLightOuterRadius = currentLightRadius;
        }
    }

    private void SlowEnemiesInRange()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position,
            currentLightRadius, enemyLayer);

        foreach (Collider2D enemy in enemies)
       {
          var enemyAI = enemy.GetComponent<EnemyAI>();
         if (enemyAI != null)
           {
                enemyAI.SetSlowEffect(enemySlowRate);
           }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isMainTorch) return; // Ana meşale hasar almaz

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Meşale hasar aldı! Kalan can: {currentHealth:F1}");

        // Işık yarıçapını kademeli olarak azalt
        float targetRadius = Mathf.Lerp(minLightRadius, maxLightRadius, currentHealth / maxHealth);
        currentLightRadius = Mathf.Lerp(currentLightRadius, targetRadius, damageDecayRate * Time.deltaTime);
        torchLight.pointLightOuterRadius = currentLightRadius;
        Debug.Log($"Meşale ışığı azalıyor! Yeni yarıçap: {currentLightRadius:F2}");

        if (currentHealth <= minHealthBeforeDestroy)
        {
            StartCoroutine(SlowlyDiminish());
        }
    }

    private IEnumerator SlowlyDiminish()
    {
        float startIntensity = torchLight.intensity;
        float diminishDuration = 3f; // Sönme süresi
        float elapsedTime = 0f;

        while (elapsedTime < diminishDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / diminishDuration;

            // Işık yoğunluğunu ve yarıçapını kademeli olarak azalt
            torchLight.intensity = Mathf.Lerp(startIntensity, 0f, progress);
            currentLightRadius = Mathf.Lerp(currentLightRadius, minLightRadius, progress);
            torchLight.pointLightOuterRadius = currentLightRadius;

            Debug.Log($"Meşale sönüyor... Yoğunluk: {torchLight.intensity:F2}, Yarıçap: {currentLightRadius:F2}");
            yield return null;
        }

        Debug.Log("Meşale tamamen söndü!");
        enabled = false;
        gameObject.SetActive(false);
    }

    public void AddWood()
    {
        if (isMainTorch)
        {
            // DarknessManager'ı bul ve meşale yarıçapını artır
            var darknessManager = FindAnyObjectByType<DarknessManager>();
            if (darknessManager != null)
            {
                darknessManager.AddWoodToTorch();
            }
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + 2f);
            torchLight.pointLightOuterRadius = currentHealth;
            UpdateTorchAnimation();
        }
    }

    public float GetLightRatio()
    {
        return (currentLightRadius - minLightRadius) / (maxLightRadius - minLightRadius);
    }

    private void UpdateTorchAnimation()
    {
        float lightRatio = GetLightRatio();
        bool shouldBePowerful = lightRatio >= spriteTransitionThreshold;

        if (shouldBePowerful != isPowerfulMode)
        {
            isPowerfulMode = shouldBePowerful;
            animator.SetBool("IsPowerful", isPowerfulMode);
        }
    }

    internal void SetActive(bool v)
    {
        throw new NotImplementedException();
    }

    public void UpdateLightRadius()
    {
        torchLight.pointLightOuterRadius = Mathf.Max(0, currentHealth);
    }

    public void UpdateLightIntensity(float newIntensity)
    {
        if (torchLight != null)
        {
            torchLight.intensity = newIntensity;
            Debug.Log($"Meşale yoğunluğu: {newIntensity}");
        }
    }

    public void IncreaseLight()
    {
        currentHealth = Mathf.Min(currentHealth + radiusIncreasePerWood * 3, maxHealth); // 3 odun değeri
        UpdateLightRadius();
        Debug.Log($"Meşale güçlendirildi! Yeni yarıçap: {currentHealth}");
    }
}