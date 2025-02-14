using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Genel UI")]
    [SerializeField] private TextMeshProUGUI woodCountText;
    [SerializeField] private TextMeshProUGUI waveCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider torchPowerSlider;
    
    [Header("Kart Seçim UI")]
    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private GameObject[] cardObjects;
    [SerializeField] private TextMeshProUGUI[] cardNameTexts;
    [SerializeField] private TextMeshProUGUI[] cardDescriptionTexts;
    [SerializeField] private Image[] cardImages;

    [Header("Sonuç Ekranları")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject pauseMenu;
    
    [Header("UI Elementleri")]
    [SerializeField] private TextMeshProUGUI healthText;
    
    private CardManager cardManager;
    private WaveManager waveManager;
    private TimerManager timerManager;

    private void Start()
    {
        cardManager = FindAnyObjectByType<CardManager>();
        waveManager = FindAnyObjectByType<WaveManager>();
        timerManager = FindAnyObjectByType<TimerManager>();
        
        HideCardSelectionUI();
    }

    public void UpdateWoodCount(int count)
    {
        if (woodCountText != null)
            woodCountText.text = $"Odun: {count}";
    }

    public void UpdateWaveCount(int wave)
    {
        waveCountText.text = $"Dalga: {wave}";
    }

    public void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateTorchPower(float current, float max)
    {
        torchPowerSlider.value = current / max;
    }

    public void UpdateHealth(float health)
    {
        if (healthText != null)
            healthText.text = $"Can: {health:0}";
    }

    public void ShowCardSelectionUI(CardManager.Card[] cards)
    {
        cardSelectionPanel.SetActive(true);
        Time.timeScale = 0f; // Oyunu durdur

        for (int i = 0; i < cardObjects.Length; i++)
        {
            if (i < cards.Length)
            {
                cardObjects[i].SetActive(true);
                cardNameTexts[i].text = cards[i].cardName;
                cardDescriptionTexts[i].text = cards[i].description;
                cardImages[i].sprite = cards[i].cardImage;
            }
            else
            {
                cardObjects[i].SetActive(false);
            }
        }
    }

    public void HideCardSelectionUI()
    {
        cardSelectionPanel.SetActive(false);
        Time.timeScale = 1f; // Oyunu devam ettir
    }

    public void OnCardSelected(int index)
    {
        cardManager.SelectCard(index);
    }

    public void ShowWinScreen() => winScreen?.SetActive(true);
    public void ShowLoseScreen() => loseScreen?.SetActive(true);
    public void ShowPauseMenu() => pauseMenu?.SetActive(true);
    public void HidePauseMenu() => pauseMenu?.SetActive(false);
}
