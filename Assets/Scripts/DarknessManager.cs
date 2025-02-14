using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DarknessManager : MonoBehaviour
{
    [Header("Karanlık Ayarları")]
    [SerializeField] private float globalDarknessIntensity = 0.8f;
    [SerializeField] private Color darknessColor = Color.black;

    [Header("Referanslar")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D[] excludedLights;

    private void Start()
    {
        SetupGlobalDarkness();
    }

    private void SetupGlobalDarkness()
    {
        if (globalLight == null)
        {
            CreateGlobalLight();
        }

        globalLight.intensity = 1 - globalDarknessIntensity;
        globalLight.color = darknessColor;

        // Diğer ışıkları ayarla
        ConfigureExcludedLights();
    }

    private void CreateGlobalLight()
    {
        GameObject lightObj = new GameObject("Global Darkness Light");
        lightObj.transform.parent = transform;

        globalLight = lightObj.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.blendStyle = 0;
    }

    private void ConfigureExcludedLights()
    {
        if (excludedLights == null) return;

        foreach (var light in excludedLights)
        {
            if (light != null)
            {
                // Bu ışıkların global karanlıktan etkilenmemesini sağla
                light.useNormalMap = true;
                light.renderingLayerMask = 1;
            }
        }
    }

    public void SetDarknessIntensity(float intensity)
    {
        globalDarknessIntensity = Mathf.Clamp01(intensity);
        if (globalLight != null)
        {
            globalLight.intensity = 1 - globalDarknessIntensity;
        }
    }

    public void AddExcludedLight(Light2D light)
    {
        if (light == null) return;

        // Yeni bir dizi oluştur ve eski ışıkları kopyala
        Light2D[] newLights = new Light2D[excludedLights.Length + 1];
        excludedLights.CopyTo(newLights, 0);
        newLights[excludedLights.Length] = light;
        excludedLights = newLights;

        // Yeni ışığı yapılandır
        light.useNormalMap = true;
        light.renderingLayerMask = 1;
    }

    public void RemoveExcludedLight(Light2D light)
    {
        if (light == null) return;

        var lightsList = new System.Collections.Generic.List<Light2D>(excludedLights);
        lightsList.Remove(light);
        excludedLights = lightsList.ToArray();
    }
}