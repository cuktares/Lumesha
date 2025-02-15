using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarlari")]
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

    [Header("Meşale Güçlendirme")]
    [SerializeField] private KeyCode powerUpTorchKey = KeyCode.F;
    [SerializeField] private float torchCheckRadius = 2f;

    [Header("Işık Etkisi Ayarları")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float darkSlowdownSpeed = 2.5f; // Karanlıktaki hız
    [SerializeField] private LayerMask torchLightLayer; // Meşale ışığı layer'ı

    private float currentHealth;
    private int woodCount = 0;
    public int WoodCount => woodCount; // Dışarıdan okuma için property
    private bool canGatherWood = true;
    private bool canCutTree = true;
    private float gatherCooldown = 0.5f;
    private float treeCutCooldown = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private Light2D torchLight;
    private Vector2 lastMovementDirection = Vector2.down;

    private float currentSpeed;
    private bool isInLight = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        torchLight = GetComponentInChildren<Light2D>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        CheckLightStatus();
        // Hareket inputları ve animasyon
        HandleMovementAndAnimation();

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

        // Ana meşaleye odun ekleme kontrolü
        if (Input.GetKeyDown(powerUpTorchKey))
        {
            UseAllWoodsForTorch();
        }
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
        Vector2 dir = Vector2.zero;

        // Yatay hareket (A/D veya Sol/Sağ Ok)
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dir.x = -1;
            animator.SetInteger("Direction", 3); // Sol
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dir.x = 1;
            animator.SetInteger("Direction", 2); // Sağ
        }

        // Dikey hareket (W/S veya Yukarı/Aşağı Ok)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            dir.y = 1;
            animator.SetInteger("Direction", 1); // Yukarı
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            dir.y = -1;
            animator.SetInteger("Direction", 0); // Aşağı
        }
        dir.Normalize();
        animator.SetBool("IsMoving", dir.magnitude > 0);

        // Son hareket yönünü kaydet
        if (dir != Vector2.zero)
        {
            lastMovementDirection = dir;
        }

        // Hareketi uygula
        rb.linearVelocity = dir * currentSpeed;
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
        moveSpeed *= multiplier;
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
        var uiManager = FindObjectOfType<UIManager>();
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
            var uiManager = FindObjectOfType<UIManager>();
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
        if (woodCount <= 0)
        {
            Debug.Log("Kullanılabilir odun yok!");
            return;
        }

        GameObject mainTorch = GameObject.FindGameObjectWithTag("MainTorch");
        if (mainTorch == null)
        {
            Debug.LogError("Ana meşale bulunamadı!");
            return;
        }

        float distance = Vector2.Distance(transform.position, mainTorch.transform.position);
        if (distance <= torchCheckRadius)
        {
            DarknessManager darknessManager = FindAnyObjectByType<DarknessManager>();
            if (darknessManager != null)
            {
                Debug.Log($"Tüm odunlar kullanılıyor! Mevcut odun sayısı: {woodCount}");
                darknessManager.AddMultipleWoods(woodCount);
                woodCount = 0; // Tüm odunları kullan
                Debug.Log("Tüm odunlar kullanıldı!");
            }
        }
        else
        {
            Debug.Log("Ana meşaleye çok uzaksın!");
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
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateWoodCount(woodCount);
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