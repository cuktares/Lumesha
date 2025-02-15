using UnityEngine;
using UnityEngine.UIElements;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement healthBarContainer;
    private ProgressBar healthBar;
    
    private void Awake()
    {
        var root = uiDocument.rootVisualElement;
        healthBarContainer = root.Q<VisualElement>("enemy-health-container");
        healthBar = root.Q<ProgressBar>("enemy-health-bar");
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            float healthPercentage = (currentHealth / maxHealth) * 100f;
            healthBar.value = healthPercentage;
        }
    }

    public void SetPosition(Vector2 screenPosition)
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.style.left = new StyleLength(screenPosition.x);
            healthBarContainer.style.top = new StyleLength(screenPosition.y);
        }
    }
} 