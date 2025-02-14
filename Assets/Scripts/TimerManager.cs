using UnityEngine;
using UnityEngine.Events;

public class TimerManager : MonoBehaviour
{
    [Header("Zaman Ayarlari")]
    [SerializeField] private float gameDuration = 600f; // 10 dakika
    [SerializeField] private float warningTime = 60f; // Son 1 dakika
    
    [Header("Events")]
    public UnityEvent onTimeUp;
    public UnityEvent onWarningTime;
    
    private float currentTime;
    private bool isWarningTriggered = false;
    private bool isRunning = true;
    private UIManager uiManager;

    private void Start()
    {
        currentTime = gameDuration;
        uiManager = FindObjectOfType<UIManager>();
        UpdateUI();
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        UpdateUI();

        // Uyarı zamanı kontrolü
        if (!isWarningTriggered && currentTime <= warningTime)
        {
            isWarningTriggered = true;
            onWarningTime?.Invoke();
        }

        // Süre doldu kontrolü
        if (currentTime <= 0)
        {
            TimeUp();
        }
    }

    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateTimer(Mathf.Max(0, currentTime));
        }
    }

    private void TimeUp()
    {
        isRunning = false;
        currentTime = 0;
        onTimeUp?.Invoke();
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        if (currentTime > gameDuration)
        {
            currentTime = gameDuration;
        }
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}
