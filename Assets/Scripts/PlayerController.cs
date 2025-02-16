using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float darkSlowdownSpeed = 2.5f;
    
    [Header("Meşale Güçlendirme")]
    [SerializeField] private KeyCode powerUpTorchKey = KeyCode.F;
    [SerializeField] private float torchCheckRadius = 2f;

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
    private AudioSource audioSource;

    [Header("Merdiven Ayarları")]
    [SerializeField] private float climbSpeed = 3f;
    private bool isOnLadder = false;

    private bool isOnStairs = false;
    private Rigidbody2D rb;
    private float currentHealth;
    private int woodCount = 0;
    public int WoodCount => woodCount; // Dışarıdan okuma için property
    private bool canGatherWood = true;
    private bool canCutTree = true;
    private float gatherCooldown = 0.5f;
    private float treeCutCooldown = 1f;

    [Header("Animasyon")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastMovementDirection = Vector2.right; // Son hareket yönünü takip et

    private float currentSpeed;
    private bool isInLight = false;
    private UIManager uiManager;

    [Header("Meşale Referansları")]
    [SerializeField] private GameObject mainTorch;
    [SerializeField] private LayerMask torchLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        uiManager = FindAnyObjectByType<UIManager>();
    }

    private void Start()
    {
        // Başlangıçta referansları al
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
    }

    private void Update()
    {
        HandleMovementAndAnimation();
        CheckLightStatus();
        
        // Meşale güçlendirme kontrolü
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseAllWoodsForTorch();
        }

        // Ağaç kesme (Space tuşu)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryCutTree();
        }

        HandleLadderMovement();
    }
        
    private void CheckLightStatus()
    {
        // Çevredeki ışıkları kontrol et
        Collider2D[] lights = Physics2D.OverlapCircleAll(transform.position, 1f, torchLayer);
        bool isInLight = false;

        foreach (Collider2D lightCollider in lights)
        {
            Light2D light = lightCollider.GetComponent<Light2D>();
            if (light != null)
            {
                float distanceToLight = Vector2.Distance(transform.position, lightCollider.transform.position);
                if (distanceToLight <= light.pointLightOuterRadius)
                {
                    isInLight = true;
                    break;
                }
            }
        }

        // Hızı güncelle
        currentSpeed = isInLight ? normalSpeed : darkSlowdownSpeed;
        Debug.Log($"Player ışık durumu: {(isInLight ? "Işıkta" : "Karanlıkta")}, Hız: {currentSpeed}");
    }

    private void Move()
    {
        // Mevcut hareket kodunuzu currentSpeed ile güncelleyin
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movement.Normalize();
        rb.linearVelocity = movement * currentSpeed;
    }

    private void HandleMovementAndAnimation()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        if (movement != Vector2.zero)
        {
            lastMovementDirection = movement;
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
            
            // Hızı doğrudan ayarla
            rb.linearVelocity = movement * currentSpeed;
        }
        else
        {
            animator.SetBool("isMoving", false);
            // Durunca hemen dursun
            rb.linearVelocity = Vector2.zero;
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
        if (animator.HasParameter("Die"))
        {
            animator.SetTrigger("Die");
        }
        
        // GameManager'i bilgilendir
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
        normalSpeed *= multiplier;
    }

    public void IncreaseWoodGatheringRate(float multiplier)
    {
        woodGatheringRate *= multiplier;
    }

    [System.Obsolete]
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Wood") && canGatherWood)
        {
            GatherWood(other.gameObject);
        }
    }

    [System.Obsolete]
    private void GatherWood(GameObject woodObject)
    {
        canGatherWood = false;
        woodCount++;

        if (audioSource && woodGatherSound)
            audioSource.PlayOneShot(woodGatherSound);
        // UI'ı güncelle
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

    [System.Obsolete]
    public void UseWood(int amount)
    {
        if (woodCount >= amount)
        {
            woodCount -= amount;
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

        Debug.Log($"Ağaç kesme denemesi - Yön: {cutDirection}, Menzil: {treeCuttingRange}");

        if (hit.collider != null && hit.collider.CompareTag("Tree"))
        {
            Debug.Log($"Ağaç bulundu! Mesafe: {hit.distance}");
            StartCoroutine(CutTree(hit.collider.gameObject));
        }
        else
        {
            Debug.Log("Menzilde ağaç bulunamadı!");
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
        var treeComponent = tree.GetComponent<Tree>();
        if (treeComponent != null)
        {
            treeComponent.Cut();
        }

        canCutTree = true;
    }

    private void UseAllWoodsForTorch()
    {
        if (mainTorch == null || uiManager == null) return;

        if (woodCount > 0)
        {
            if (Vector2.Distance(transform.position, mainTorch.transform.position) <= torchCheckRadius)
            {
                var torchController = mainTorch.GetComponent<TorchLightController>();
                if (torchController != null)
                {
                    torchController.AddWood(woodCount);
                    
                    // Odunları sıfırla
                    int usedWood = woodCount;
                    woodCount = 0;
                    uiManager.UpdateWoodCount(woodCount);
                    
                    if (audioSource != null && torchSound != null)
                        audioSource.PlayOneShot(torchSound);
                        
                    Debug.Log($"Meşaleye {usedWood} odun eklendi!");
                }
            }
            else
            {
                Debug.LogWarning("Meşaleye yakın değilsin!");
            }
        }
        else
        {
            Debug.LogWarning("Hiç odunun yok!");
        }
    }

    [System.Obsolete]
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

    public void CollectWood()
    {
        woodCount++;
        Debug.Log($"Odun toplandı! Toplam odun: {woodCount}");

        // Ses efekti
        if (audioSource && woodGatherSound)
        {
            audioSource.PlayOneShot(woodGatherSound);
        }

        // UI güncelle
        if (uiManager != null)
        {
            uiManager.UpdateWoodCount(woodCount);
        }
    }

    private void HandleLadderMovement()
    {
        if (isOnLadder)
        {
            // Merdivende dikey hareket
            float verticalInput = Input.GetAxisRaw("Vertical");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
            rb.gravityScale = 0f; // Merdivende yerçekimini kapat
        }
        else
        {
            rb.gravityScale = 1f; // Normal yerçekimi
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = true;
            Debug.Log("Merdivene girildi");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;
            Debug.Log("Merdivenden çıkıldı");
        }
    }
}

// Extension method to check if Animator has a parameter
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
}