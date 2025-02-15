using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DarknessManager : MonoBehaviour
{
    [Header("Karanlik Ayarlari")]
    [SerializeField] private float globalDarknessIntensity = 0.95f; // Daha karanlık
    [SerializeField] private Color darknessColor = new Color(0.05f, 0.05f, 0.1f); // Koyu mavi-siyah
    [SerializeField] private float minLightIntensity = 0.05f; // Minimum ortam ışığı

    [Header("Meşale Güçlendirme Ayarları")]
    [SerializeField] private float baseRadius = 5f;
    [SerializeField] private float radiusIncreasePerWood = 2f;
    [SerializeField] private float maxRadius = 20f;
    
    [Header("Işık Yoğunluğu Ayarları")]
    [SerializeField] private float baseLightIntensity = 1f;
    [SerializeField] private float intensityIncreasePerWood = 0.2f;
    [SerializeField] private float maxLightIntensity = 3f;

    [Header("Referanslar")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private TorchLightController mainTorch;

    private float currentRadius;
    private float currentIntensity;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeLights();
    }

    private void InitializeLights()
    {
        if (!isInitialized)
        {
            currentRadius = baseRadius;
            currentIntensity = baseLightIntensity;
            UpdateAllTorchLights();
            isInitialized = true;
            Debug.Log($"Meşaleler başlangıç değerlerine ayarlandı - Yarıçap: {currentRadius}, Yoğunluk: {currentIntensity}");
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
            torchLight.pointLightOuterRadius = baseRadius;
            torchLight.intensity = 1f;
            torchLight.color = new Color(1f, 0.95f, 0.8f); // Sıcak ışık rengi
        }
    }

    public void AddWoodToTorch()
    {
        if (!isInitialized)
        {
            InitializeLights();
        }

        // Eski değerleri sakla
        float oldRadius = currentRadius;
        float oldIntensity = currentIntensity;

        // Yeni değerleri hesapla
        currentRadius = Mathf.Min(currentRadius + radiusIncreasePerWood, maxRadius);
        currentIntensity = Mathf.Min(currentIntensity + intensityIncreasePerWood, maxLightIntensity);

        // Değişiklikleri uygula
        UpdateAllTorchLights();

        // Debug bilgisi
        Debug.Log($"Meşale güçlendirme: \n" +
                  $"Yarıçap: {oldRadius:F1} -> {currentRadius:F1}\n" +
                  $"Yoğunluk: {oldIntensity:F1} -> {currentIntensity:F1}");
    }

    private void UpdateAllTorchLights()
    {
        TorchLightController[] allTorches = FindObjectsByType<TorchLightController>(FindObjectsSortMode.None);
        int updatedCount = 0;

        foreach (var torch in allTorches)
        {
            if (!torch.isMainTorch)
            {
                Light2D light = torch.GetComponent<Light2D>();
                if (light != null)
                {
                    // Değerleri doğrudan ayarla
                    light.pointLightOuterRadius = currentRadius;
                    light.intensity = currentIntensity;
                    updatedCount++;
                }
            }
        }

        Debug.Log($"{updatedCount} meşale güncellendi");
    }

    // Debug için mevcut değerleri göster
    public void LogCurrentValues()
    {
        Debug.Log($"Mevcut Değerler:\n" +
                  $"Yarıçap: {currentRadius:F1} (Max: {maxRadius})\n" +
                  $"Yoğunluk: {currentIntensity:F1} (Max: {maxLightIntensity})");
    }

    public void AddMultipleWoods(int woodCount)
    {
        if (woodCount <= 0) return;

        float oldRadius = currentRadius;
        float oldIntensity = currentIntensity;

        // Tüm odunlar için toplam artışı hesapla
        float totalRadiusIncrease = radiusIncreasePerWood * woodCount;
        float totalIntensityIncrease = intensityIncreasePerWood * woodCount;

        // Yeni değerleri hesapla ve maksimum değerlerle sınırla
        currentRadius = Mathf.Min(currentRadius + totalRadiusIncrease, maxRadius);
        currentIntensity = Mathf.Min(currentIntensity + totalIntensityIncrease, maxLightIntensity);

        UpdateAllTorchLights();

        Debug.Log($"Toplu meşale güçlendirme ({woodCount} odun):\n" +
                  $"Yarıçap: {oldRadius:F1} -> {currentRadius:F1}\n" +
                  $"Yoğunluk: {oldIntensity:F1} -> {currentIntensity:F1}");
    }
}