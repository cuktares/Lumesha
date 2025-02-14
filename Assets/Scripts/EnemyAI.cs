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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        UpdateNearestTorch();
    }

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
        Transform target = null;
        float playerDistance = Vector2.Distance(transform.position, player.position);
        float torchDistance = nearestTorch != null ?
            Vector2.Distance(transform.position, nearestTorch.position) : Mathf.Infinity;

        // En yakın hedefi seç
        if (playerDistance <= detectionRange && playerDistance <= torchDistance)
        {
            target = player;
        }
        else if (torchDistance <= detectionRange)
        {
            target = nearestTorch;
        }

        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.MovePosition((Vector2)transform.position + direction * moveSpeed * currentSpeedMultiplier * Time.deltaTime);
        }
    }

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
            nearestTorch.GetComponent<TorchLightController>()?.SetActive(false);
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