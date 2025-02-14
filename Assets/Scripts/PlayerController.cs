using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Oyuncu Özellikleri")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float woodGatherRadius = 2f;
    [SerializeField] private float woodGatheringRate = 1f;
    [SerializeField] private float treeCuttingRange = 1.5f;
    [SerializeField] private int woodPerTree = 3;

    [Header("Ses Efektleri")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip woodGatherSound;
    [SerializeField] private AudioClip torchSound;
    [SerializeField] private AudioClip treeCutSound;

    private float currentHealth;
    private int woodCount = 0;
    private bool canGatherWood = true;
    private bool canCutTree = true;
    private float gatherCooldown = 0.5f;
    private float treeCutCooldown = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private Vector2 lastMovementDirection;
    private AudioSource audioSource;
    private Light2D torchLight;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        torchLight = GetComponentInChildren<Light2D>();

        lastMovementDirection = Vector2.down;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Hareket inputları
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // Animasyonları güncelle
        UpdateAnimationParameters();

        // Odun toplama (E tuşu)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryGatherWood();
        }

        // Ağaç kesme (Space tuşu)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryCutTree();
        }

        // Meşaleye odun ekleme (F tuşu)
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryAddWoodToTorch();
        }
    }

    private void FixedUpdate()
    {
        // Karakteri hareket ettir
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimationParameters()
    {
        if (movement != Vector2.zero)
        {
            // Karakter hareket ediyorsa son hareket yönünü kaydet
            lastMovementDirection = movement;

            // Hareket animasyonlarını çalıştır
            animator.SetBool("IsMoving", true);
            animator.SetFloat("DirectionX", movement.x);
            animator.SetFloat("DirectionY", movement.y);
        }
        else
        {
            // Karakter durunca idle animasyonuna geç
            animator.SetBool("IsMoving", false);
            // Son hareket yönünü idle animasyonu için kullan
            animator.SetFloat("DirectionX", lastMovementDirection.x);
            animator.SetFloat("DirectionY", lastMovementDirection.y);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (audioSource && damageSound)
            audioSource.PlayOneShot(damageSound);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Ölüm animasyonu
        animator.SetTrigger("Die");
        // GameManager'ı bilgilendir
        var gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EndGame(false);
        }

        // Karakteri devre dışı bırak
        enabled = false;
    }

    public void IncreaseMoveSpeed(float multiplier)
    {
        moveSpeed *= multiplier;
    }

    public void IncreaseWoodGatheringRate(float multiplier)
    {
        woodGatheringRate *= multiplier;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Wood") && canGatherWood)
        {
            GatherWood(other.gameObject);
        }
    }

    private void GatherWood(GameObject woodObject)
    {
        canGatherWood = false;
        woodCount++;

        if (audioSource && woodGatherSound)
            audioSource.PlayOneShot(woodGatherSound);
        // UI'ı güncelle
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateWoodCount(woodCount);
        }

        Destroy(woodObject);

        // Toplama bekleme süresini başlat
        StartCoroutine(GatherCooldown());
    }

    private IEnumerator GatherCooldown()
    {
        yield return new WaitForSeconds(gatherCooldown / woodGatheringRate);
        canGatherWood = true;
    }

    public int GetWoodCount()
    {
        return woodCount;
    }

    public void UseWood(int amount)
    {
        if (woodCount >= amount)
        {
            woodCount -= amount;
            var uiManager = FindAnyObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateWoodCount(woodCount);
            }
        }
    }

    private void TryCutTree()
    {
        if (!canCutTree) return;

        // Karakterin baktığı yönde ağaç ara
        Vector2 cutDirection = lastMovementDirection;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, cutDirection, treeCuttingRange, LayerMask.GetMask("Tree"));

        if (hit.collider != null && hit.collider.CompareTag("Tree"))
        {
            StartCoroutine(CutTree(hit.collider.gameObject));
        }
    }

    private IEnumerator CutTree(GameObject tree)
    {
        canCutTree = false;

        // Kesme animasyonu
        animator.SetTrigger("Cut");

        // Kesme sesi
        if (audioSource && treeCutSound)
        {
            audioSource.PlayOneShot(treeCutSound);
        }

        yield return new WaitForSeconds(treeCutCooldown);

        // Ağaçtan odun al
        woodCount += woodPerTree;

        // Ağacı yok et veya görünümünü değiştir
        tree.GetComponent<Tree>()?.Cut();

        canCutTree = true;
    }

    private void TryAddWoodToTorch()
    {
        if (woodCount <= 0) return;

        // Ana meşaleyi bul
        var mainTorch = GameObject.FindGameObjectWithTag("MainTorch");
        if (mainTorch == null) return;

        // Mesafe kontrolü
        float distance = Vector2.Distance(transform.position, mainTorch.transform.position);
        if (distance <= 2f) // Etkileşim mesafesi
        {
            woodCount--;
            mainTorch.GetComponent<TorchLightController>()?.AddWood();

            // Ses efekti
            if (audioSource && torchSound)
            {
                audioSource.PlayOneShot(torchSound);
            }

            // UI güncelle
            var uiManager = FindAnyObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateWoodCount(woodCount);
            }
        }
    }

    private void TryGatherWood()
    {
        if (!canGatherWood) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, woodGatherRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Wood"))
            {
                GatherWood(hitCollider.gameObject);
                break;
            }
        }
    }
}