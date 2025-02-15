using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DarknessManager : MonoBehaviour
{
    [Header("Karanlik Ayarlari")]
    [SerializeField] private float globalDarknessIntensity = 0.95f; // Daha karanlık
    [SerializeField] private Color darknessColor = new Color(0.05f, 0.05f, 0.1f); // Koyu mavi-siyah
    [SerializeField] private float minLightIntensity = 0.05f; // Minimum ortam ışığı

    [Header("Meşale Ayarları")]
    [SerializeField] private float baseTorchRadius = 3f;
    [SerializeField] private float woodRadiusIncrease = 0.5f;
    [SerializeField] private float maxTorchRadius = 10f;

    [Header("Referanslar")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private TorchLightController mainTorch;

    private void Start()
    {
        SetupGlobalDarkness();
        if (mainTorch != null)
        {
            ConfigureMainTorch();
        }
    }

    private void SetupGlobalDarkness()
    {
        if (globalLight == null)
        {
            CreateGlobalLight();
        }

        // Çok karanlık bir ortam ayarla
        globalLight.intensity = minLightIntensity;
        globalLight.color = darknessColor;
    }

    private void CreateGlobalLight()
    {
        GameObject lightObj = new GameObject("Global Light 2D");
        lightObj.transform.parent = transform;

        globalLight = lightObj.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.intensity = minLightIntensity;
        globalLight.color = darknessColor;
        globalLight.blendStyleIndex = 0;
    }

    private void ConfigureMainTorch()
    {
        // Ana meşalenin başlangıç ayarları
        Light2D torchLight = mainTorch.GetComponent<Light2D>();
        if (torchLight != null)
        {
            torchLight.pointLightOuterRadius = baseTorchRadius;
            torchLight.intensity = 1f;
            torchLight.color = new Color(1f, 0.95f, 0.8f); // Sıcak ışık rengi
        }
    }

    public void IncreaseTorchRadius(float amount)
    {
        if (mainTorch != null)
        {
            Light2D torchLight = mainTorch.GetComponent<Light2D>();
            if (torchLight != null)
            {
                float newRadius = Mathf.Min(torchLight.pointLightOuterRadius + amount, maxTorchRadius);
                torchLight.pointLightOuterRadius = newRadius;
            }
        }
    }

    public void AddWoodToTorch()
    {
        IncreaseTorchRadius(woodRadiusIncrease);
    }
}