using UnityEngine;

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

    private Transform player;
    private Transform nearestTorch;
    private Rigidbody2D rb;
    private float nextAttackTime;
    private bool isDead = false;

    private float baseSpeed = 5f;
    private float currentSpeedMultiplier = 1f;

    private float randomDirectionTimer = 0f;
    private Vector2 randomDirection;
    [SerializeField] private float randomMoveInterval = 2f; // Ne sıklıkla yön değiştireceği
    [SerializeField] private float randomMoveSpeed = 2f; // Random hareketin hızı

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        UpdateNearestTorch();
    }

    [System.Obsolete]
    private void Update()
    {
        if (isDead) return;

        UpdateNearestTorch();
        ChooseTargetAndMove();
        TryAttack();
    }

    private void UpdateNearestTorch()
    {
        GameObject[] torches = GameObject.FindGameObjectsWithTag("Torch");
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject torch in torches)
        {
            float distance = Vector2.Distance(transform.position, torch.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTorch = torch.transform;
            }
        }
    }

    private void ChooseTargetAndMove()
    {
        if (isDead) return;

        Transform target = null;
        float playerDistance = Vector2.Distance(transform.position, player.position);

        // Önce oyuncuyu kontrol et
        if (playerDistance <= detectionRange)
        {
            target = player;
        }
        // Oyuncu yoksa yan meşaleleri kontrol et
        else
        {
            GameObject[] torches = GameObject.FindGameObjectsWithTag("Torch");
            float nearestDistance = Mathf.Infinity;

            foreach (GameObject torch in torches)
            {
                if (torch.GetComponent<TorchLightController>().isMainTorch) continue;

                float distance = Vector2.Distance(transform.position, torch.transform.position);
                if (distance < nearestDistance && distance <= detectionRange)
                {
                    nearestDistance = distance;
                    target = torch.transform;
                }
            }
        }

        // Hedef varsa hedefe doğru hareket et
        if (target != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed * currentSpeedMultiplier;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Hedef menzildeyse saldır
            float targetDistance = Vector2.Distance(transform.position, target.position);
            if (targetDistance <= attackRange && Time.time >= nextAttackTime)
            {
                if (target == player)
                {
                    player.GetComponent<PlayerController>()?.TakeDamage(attackDamage);
                }
                else
                {
                    target.GetComponent<TorchLightController>()?.TakeDamage(attackDamage);
                }
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // Hedef yoksa random hareket et
        else
        {
            RandomMove();
        }
    }

    private void RandomMove()
    {
        randomDirectionTimer -= Time.deltaTime;

        if (randomDirectionTimer <= 0)
        {
            // Yeni random yön belirle
            randomDirection = Random.insideUnitCircle.normalized;
            randomDirectionTimer = randomMoveInterval;
        }

        // Random yönde hareket et
        rb.linearVelocity = randomDirection * randomMoveSpeed * currentSpeedMultiplier;

        // Hareket yönüne dön
        float angle = Mathf.Atan2(randomDirection.y, randomDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    [System.Obsolete]
    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            // Oyuncuya saldır
            player.GetComponent<PlayerController>()?.TakeDamage(attackDamage);
            nextAttackTime = Time.time + attackCooldown;
        }
        else if (nearestTorch != null &&
                 Vector2.Distance(transform.position, nearestTorch.position) <= attackRange)
        {
            // Meşaleye saldır
            var torchController = nearestTorch.GetComponent<TorchLightController>();
            if (torchController != null)
            {
                torchController.enabled = false; // Meşaleyi devre dışı bırak
            }
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void Die()
    {
        isDead = true;

        // Eşya düşürme
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
        currentSpeedMultiplier = 1f - slowRate;
    }
}