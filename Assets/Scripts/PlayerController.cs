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

    private float currentHealth;
    private int woodCount = 0;
    private bool canGatherWood = true;
    private bool canCutTree = true;
    private float gatherCooldown = 0.5f;
    private float treeCutCooldown = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private Light2D torchLight;
    private Vector2 lastMovementDirection = Vector2.down;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        torchLight = GetComponentInChildren<Light2D>();
        currentHealth = maxHealth;
    }

    [System.Obsolete]
    private void Update()
    {
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

        // Meşaleye odun ekleme (F tuşu)
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryAddWoodToTorch();
        }
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
        rb.linearVelocity = dir * moveSpeed;
        rb.linearVelocity = dir * moveSpeed;
    }

    [System.Obsolete]
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

    [System.Obsolete]
    private void Die()
    {
        // Ölüm animasyonu
        if (animator.HasParameter("Die"))
        {
            animator.SetTrigger("Die");
            
        }
        // GameManager'i bilgilendir
        var gameManager = FindObjectOfType<GameManager>();
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

    public int GetWoodCount()
    {
        return woodCount;
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
        var treeComponent = tree.GetComponent<Tree>();
        if (treeComponent != null)
        {
            treeComponent.Cut();
        }

        canCutTree = true;
    }

    [System.Obsolete]
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
            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateWoodCount(woodCount);
            }
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