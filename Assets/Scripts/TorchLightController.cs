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
    [SerializeField] private bool isMainTorch = false;
    [SerializeField] private float mainTorchRange = 15f; // Ana meşalenin etki alanı

    [Header("Dusman Etkileri")]
    [SerializeField] private float enemySlowRate = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Meşale Sprite Ayarları")]
    [SerializeField] private Sprite normalTorchSprite;
    [SerializeField] private Sprite powerfulTorchSprite;
    [SerializeField] private float spriteTransitionThreshold = 0.7f;
    [SerializeField] private float flickerIntensity = 0.1f;
    [SerializeField] private float flickerSpeed = 10f;

    private Light2D torchLight;
    private float currentHealth;
    private float currentLightRadius;
    private Transform mainTorch;
    private SpriteRenderer torchSprite;
    private bool isPowerfulMode = false;

    private void Start()
    {
        torchLight = GetComponent<Light2D>();
        torchSprite = GetComponent<SpriteRenderer>();
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
        if (isMainTorch)
        {
            DecreaseLightRadius();
            UpdateTorchSprite();
        }
        else
        {
            // Yan meşaleler ana meşaleden güç alır
            UpdateSatelliteTorch();
        }

        // Işık alanındaki düşmanları yavaşlat
        SlowEnemiesInRange();
        FlickerEffect();
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

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // Meşale söndü
            torchLight.enabled = false;
            enabled = false;

            // Meşale yok olma efekti
            StartCoroutine(DestroyTorch());
        }
    }

    private IEnumerator DestroyTorch()
    {
        // Yok olma efekti (örneğin particle effect)
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    public void AddWood()
    {
        if (isMainTorch)
        {
            currentLightRadius = Mathf.Min(maxLightRadius, currentLightRadius + 2f);
            torchLight.pointLightOuterRadius = currentLightRadius;
            
            // Odun eklendiğinde sprite'ı hemen güncelle
            UpdateTorchSprite();
        }
    }

    public float GetLightRatio()
    {
        return (currentLightRadius - minLightRadius) / (maxLightRadius - minLightRadius);
    }

    private void UpdateTorchSprite()
    {
        float lightRatio = GetLightRatio();
        bool shouldBePowerful = lightRatio >= spriteTransitionThreshold;

        if (shouldBePowerful != isPowerfulMode)
        {
            isPowerfulMode = shouldBePowerful;
            torchSprite.sprite = isPowerfulMode ? powerfulTorchSprite : normalTorchSprite;
        }
    }

    private void FlickerEffect()
    {
        // Rastgele titreşim efekti
        float flicker = 1f + Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity;
        torchSprite.transform.localScale = Vector3.one * flicker;
    }

    internal void SetActive(bool v)
    {
        throw new NotImplementedException();
    }
}