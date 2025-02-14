using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField] private Sprite cutTreeSprite; // Kesildikten sonraki görünüm
    [SerializeField] private GameObject woodPrefab; // Düşecek odun prefabı
    [SerializeField] private int maxHealth = 3; // Kaç vuruşta kesileceği
    
    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage()
    {
        currentHealth--;
        
        // Hasar efekti (isteğe bağlı)
        StartCoroutine(DamageEffect());
        
        if (currentHealth <= 0)
        {
            Cut();
        }
    }

    public void Cut()
    {
        // Görünümü değiştir
        if (cutTreeSprite != null)
        {
            spriteRenderer.sprite = cutTreeSprite;
        }
        
        // Odun düşür
        if (woodPrefab != null)
        {
            Vector2 dropPosition = transform.position;
            Instantiate(woodPrefab, dropPosition, Quaternion.identity);
        }
        
        // Collider'ı kaldır
        GetComponent<Collider2D>().enabled = false;
        
        // Script'i devre dışı bırak
        enabled = false;
    }

    private System.Collections.IEnumerator DamageEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
} 