using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchLightController : MonoBehaviour
{
    [Header("Işık Ayarları")]
    [SerializeField] private float maxLightRadius = 10f;
    [SerializeField] private float minLightRadius = 2f;
    [SerializeField] private float lightDecreaseRate = 0.1f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool isMainTorch = false;
    
    [Header("Düşman Etkileri")]
    [SerializeField] private float enemySlowRate = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    private Light2D torchLight;
    private float currentHealth;
    private float currentLightRadius;
    private TorchNetwork torchNetwork;

    private void Start()
    {
        torchLight = GetComponent<Light2D>();
        currentHealth = maxHealth;
        currentLightRadius = isMainTorch ? maxLightRadius : minLightRadius;
        torchLight.pointLightOuterRadius = currentLightRadius;
        
        if (!isMainTorch)
        {
            torchNetwork = FindAnyObjectByType<TorchNetwork>();
        }
    }

    private void Update()
    {
        if (isMainTorch)
        {
            // Ana meşale sürekli azalır
            DecreaseLightRadius();
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
        if (torchNetwork != null)
        {
            float mainTorchPower = torchNetwork.GetMainTorchPower();
            currentLightRadius = Mathf.Lerp(minLightRadius, maxLightRadius, mainTorchPower);
            torchLight.pointLightOuterRadius = currentLightRadius;
        }
    }

    private void SlowEnemiesInRange()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 
            currentLightRadius, enemyLayer);
        
        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<EnemyAI>()?.SetSlowEffect(enemySlowRate);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            // Meşale söndü
            torchLight.enabled = false;
            enabled = false;
        }
    }

    public void AddWood()
    {
        if (isMainTorch)
        {
            currentLightRadius = Mathf.Min(maxLightRadius, currentLightRadius + 2f);
            torchLight.pointLightOuterRadius = currentLightRadius;
        }
    }

    public float GetLightRatio()
    {
        return (currentLightRadius - minLightRadius) / (maxLightRadius - minLightRadius);
    }
}
