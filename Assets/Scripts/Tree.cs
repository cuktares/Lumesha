using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField] private GameObject cutTreeSprite; // Kesilmis agac sprite'i
    [SerializeField] private GameObject woodPrefab; // Dusecek odun prefabi
    [SerializeField] private int maxHealth = 3; // Kaç vurusta kesilecegi
    [SerializeField] private int woodSpawnCount = 3;

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
        // Kesilmiş ağaç görünümünü aktifleştir
        if (cutTreeSprite != null)
        {
            cutTreeSprite.SetActive(true);
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Odun oluştur
        for (int i = 0; i < woodSpawnCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            Instantiate(woodPrefab, spawnPosition, Quaternion.identity);
        }
        
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f); // 2 saniye sonra yok et
    }

    private System.Collections.IEnumerator DamageEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}