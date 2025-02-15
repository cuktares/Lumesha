using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Prefab Referansları")]
    [SerializeField] private GameObject cutTreeSprite; // Kesilmis agac sprite'i
    [SerializeField] private GameObject woodPrefab; // Dusecek odun prefabi
    [SerializeField] private int maxHealth = 3; // Kaç vurusta kesilecegi
    [SerializeField] private int woodSpawnCount = 3;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isCut = false;
    private GameObject loadedWoodPrefab;

    private void Awake()
    {
        // Prefab referansını Resources klasöründen al ve sakla
        if (woodPrefab == null)
        {
            loadedWoodPrefab = Resources.Load<GameObject>("Prefabs/Wood");
            if (loadedWoodPrefab != null)
            {
                woodPrefab = loadedWoodPrefab;
                Debug.Log("Wood prefab başarıyla yüklendi");
            }
            else
            {
                Debug.LogError("Wood prefab Resources/Prefabs/Wood'da bulunamadı!");
            }
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Animator'ın varlığını kontrol et
        if (animator == null)
        {
            Debug.LogError("Animator component bulunamadı!");
        }
        else
        {
            Debug.Log("Animator component bulundu");
            // Parametre kontrolü
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                Debug.Log($"Animator parametresi: {param.name}, Type: {param.type}");
            }
        }
    }

    public void TakeDamage()
    {
        if (isCut) 
        {
            Debug.Log("Ağaç zaten kesilmiş!");
            return;
        }

        currentHealth--;
        Debug.Log($"Ağaç hasar aldı! Kalan can: {currentHealth}/{maxHealth}");

        StartCoroutine(DamageEffect());

        if (currentHealth <= 0)
        {
            Debug.Log("Ağaç kesiliyor...");
            Cut();
        }
    }

    public void Cut()
    {
        if (isCut) return;
        
        isCut = true;
        Debug.Log("Ağaç kesme başlatıldı");

        // Animasyon kontrolü
        if (animator != null)
        {
            Debug.Log("TreeFall animasyonu tetikleniyor");
            animator.SetTrigger("TreeFall");
        }
        else
        {
            Debug.LogWarning("Animator bulunamadı! Animasyonsuz devam ediliyor.");
        }

        // Odun oluşturma kontrolü
        if (woodPrefab != null)
        {
            StartCoroutine(SpawnWoodsAfterAnimation());
        }
        else
        {
            Debug.LogError("Wood prefab hala null! Odunlar oluşturulamayacak.");
            // Prefabı tekrar yüklemeyi dene
            woodPrefab = Resources.Load<GameObject>("Prefabs/Wood");
            if (woodPrefab != null)
            {
                StartCoroutine(SpawnWoodsAfterAnimation());
            }
        }
    }

    private System.Collections.IEnumerator SpawnWoodsAfterAnimation()
    {
        yield return new WaitForSeconds(1f);

        if (woodPrefab == null)
        {
            Debug.LogError("Wood prefab referansı kayıp! Odunlar oluşturulamıyor.");
            yield break;
        }

        Debug.Log("Odunlar oluşturuluyor...");

        for (int i = 0; i < woodSpawnCount; i++)
        {
            try
            {
                Vector3 spawnPosition = transform.position;
                GameObject wood = Instantiate(woodPrefab, spawnPosition, Quaternion.identity);
                
                if (wood != null)
                {
                    Debug.Log($"Odun {i + 1} başarıyla oluşturuldu");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Odun oluşturma hatası: {e.Message}");
            }
        }

        if (cutTreeSprite != null)
        {
            cutTreeSprite.SetActive(true);
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private System.Collections.IEnumerator DamageEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            Debug.Log("Hasar efekti gösterildi");
        }
    }
}