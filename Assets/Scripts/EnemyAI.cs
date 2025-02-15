using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D için bu namespace'i ekleyin

public class EnemyAI : MonoBehaviour
{
    [Header("Dusman ozellikleri")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("dusus ihtimalleri")]
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] [Range(0f, 1f)] private float woodDropChance = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float cardDropChance = 0.1f;

    [Header("Işık Etkisi Ayarları")]
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float lightSlowdownSpeed = 1.5f; // Işıktaki yavaş hız
    [SerializeField] private LayerMask torchLightLayer;
    
    private float currentSpeed;
    private bool isInLight = false;

    private Transform player;
    private Transform nearestTorch;
    private Rigidbody2D rb;
    private float nextAttackTime;
    private bool isDead = false;
    private float slowEffectMultiplier = 1f;

    [Header("Merdiven Ayarları")]
    [SerializeField] private float climbSpeed = 2f;
    private bool isOnLadder = false;

    private void Awake()
    {
        Debug.Log("Enemy Awake çağrıldı");
    }

    private void Start()
    {
        Debug.Log("Enemy Start başladı");
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D bulunamadı!");

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Debug.Log($"Player aranıyor... Tag ile bulunan obje: {(player != null ? player.name : "Bulunamadı")}");

        // Alternatif arama yöntemi
        var allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Debug.Log($"Sahnede bulunan PlayerController sayısı: {allPlayers.Length}");
        UpdateNearestTorch();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            Debug.Log("Enemy ölü durumda");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player referansı yok!");
            return;
        }

        UpdateNearestTorch();
        ChooseTargetAndMove();
        TryAttack();
    }

    private void UpdateNearestTorch()
    {
        GameObject[] torches = GameObject.FindGameObjectsWithTag("Torch");
        float nearestDistance = Mathf.Infinity;
        nearestTorch = null; // Her frame'de sıfırla

        foreach (GameObject torch in torches)
        {
            if (torch != null && torch.activeInHierarchy)
            {
                float distance = Vector2.Distance(transform.position, torch.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTorch = torch.transform;
                    Debug.Log($"En yakın meşale bulundu: {distance} birim uzaklıkta");
                }
            }
        }
    }

    private void ChooseTargetAndMove()
    {
        if (player == null)
        {
            Debug.LogError("Player referansı yok!");
            return;
        }

        float playerDistance = Vector2.Distance(transform.position, player.position);
        float torchDistance = nearestTorch != null ? Vector2.Distance(transform.position, nearestTorch.position) : Mathf.Infinity;
        
        Vector2 direction = Vector2.zero;
        
        // Önce oyuncuya bakıyoruz
        if (playerDistance <= detectionRange)
        {
            direction = (player.position - transform.position).normalized;
            Debug.Log($"Oyuncuya doğru hareket: {direction}");
        }
        // Oyuncu menzilde değilse ve meşale varsa
        else if (torchDistance <= detectionRange)
        {
            direction = (nearestTorch.position - transform.position).normalized;
            Debug.Log($"Meşaleye doğru hareket: {direction}");
        }
        
        // Hareket kontrolü
        if (direction != Vector2.zero)
        {
            rb.linearVelocity = direction * currentSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            Debug.Log("Hedef yok - Duruyorum");
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            Debug.Log($"Saldırı bekleme süresi: {nextAttackTime - Time.time:F1} saniye");
            return;
        }

        float playerDistance = Vector2.Distance(transform.position, player.position);
        float torchDistance = nearestTorch != null ? Vector2.Distance(transform.position, nearestTorch.position) : Mathf.Infinity;

        // Önce oyuncuya saldır
        if (playerDistance <= attackRange)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
                Debug.Log($"Oyuncuya {attackDamage} hasar verildi! Mesafe: {playerDistance:F2}");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // Sonra meşaleye saldır
        else if (nearestTorch != null && torchDistance <= attackRange)
        {
            var torchController = nearestTorch.GetComponent<TorchLightController>();
            if (torchController != null)
            {
                torchController.TakeDamage(attackDamage);
                Debug.Log($"Meşaleye {attackDamage} hasar verildi! Mesafe: {torchDistance:F2}");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            Debug.Log($"Saldırı menzili dışında - Player: {playerDistance:F2}, Torch: {torchDistance:F2}, Menzil: {attackRange}");
        }
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero; // Ölünce hareketi durdur
        
        if (Random.value <= woodDropChance)
        {
            Instantiate(woodPrefab, transform.position, Quaternion.identity);
        }
        
        if (Random.value <= cardDropChance)
        {
            Instantiate(cardPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }

    public void SetSlowEffect(float slowRate)
    {
        slowEffectMultiplier = 1f - slowRate;
    }

    private void Update()
    {
        CheckLightStatus();
        HandleLadderMovement();
    }

    private void CheckLightStatus()
    {
        // Tüm meşaleleri bul
        TorchLightController[] torches = FindObjectsByType<TorchLightController>(FindObjectsSortMode.None);
        isInLight = false;

        foreach (var torch in torches)
        {
            float distance = Vector2.Distance(transform.position, torch.transform.position);
            float lightRadius = torch.GetComponent<Light2D>().pointLightOuterRadius;

            if (distance <= lightRadius)
            {
                isInLight = true;
                break;
            }
        }

        // Hızı güncelle
        currentSpeed = isInLight ? lightSlowdownSpeed : normalSpeed;
        Debug.Log($"Düşman ışık durumu: {(isInLight ? "Işıkta" : "Karanlıkta")}, Hız: {currentSpeed}");
    }

    private void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * currentSpeed;
        }
    }

    private void HandleLadderMovement()
    {
        if (isOnLadder && player != null)
        {
            // Hedefin yüksekliğine göre tırmanma
            float heightDifference = player.position.y - transform.position.y;
            
            if (Mathf.Abs(heightDifference) > 0.1f)
            {
                float verticalMovement = Mathf.Sign(heightDifference) * climbSpeed;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalMovement);
                rb.gravityScale = 0f;
            }
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = true;
            Debug.Log("Düşman merdivene girdi");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;
            Debug.Log("Düşman merdivenden çıktı");
        }
    }
}
