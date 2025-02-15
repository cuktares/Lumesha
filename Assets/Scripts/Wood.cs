using UnityEngine;

public class Wood : MonoBehaviour
{
    [Header("Toplama Ayarları")]
    [SerializeField] private float collectRange = 1.5f;
    [SerializeField] private float collectDelay = 0.5f;
    [SerializeField] private KeyCode collectKey = KeyCode.E;
    
    [Header("Saçılma Ayarları")]
    [SerializeField] private float minScatterDistance = 0.5f;
    [SerializeField] private float maxScatterDistance = 1.5f;
    
    private bool isCollectable = false;
    private bool isInRange = false;
    private PlayerController nearbyPlayer;

    private void Start()
    {
        Debug.Log($"Odun oluşturuldu: {gameObject.name}");
        
        // Rastgele bir noktaya yerleştir
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minScatterDistance, maxScatterDistance);
        Vector2 offset = randomDirection * randomDistance;
        
        transform.position += new Vector3(offset.x, offset.y, 0);
        Debug.Log($"Odun yerleştirildi: {transform.position}");

        Invoke("MakeCollectable", collectDelay);
    }

    private void Update()
    {
        if (isInRange && Input.GetKeyDown(collectKey))
        {
            Debug.Log("E tuşuna basıldı, odun toplama deneniyor...");
            CollectWood();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.CollectWood();
                Debug.Log("Odun otomatik toplandı!");
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            nearbyPlayer = null;
            Debug.Log("Oyuncu odun menzilinden çıktı.");
        }
    }

    private void CollectWood()
    {
        if (!isCollectable)
        {
            Debug.Log("Odun henüz toplanamaz durumda!");
            return;
        }

        if (nearbyPlayer != null)
        {
            Debug.Log("Odun toplanıyor...");
            nearbyPlayer.CollectWood();
            Destroy(gameObject);
        }
    }

    private void MakeCollectable()
    {
        isCollectable = true;
        Debug.Log($"Odun toplanabilir duruma geldi: {gameObject.name}");
    }
} 