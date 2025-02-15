using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class CardManager : MonoBehaviour
{
    [System.Serializable]
    public class Card
    {
        public string cardName;
        public string description;
        public Sprite cardImage;
        public UnityEvent onCardSelected;
    }

    [Header("Kart Ayarlari")]
    [SerializeField] private Card[] availableCards;
    [SerializeField] private int cardsToShow = 2;

    private UIManager uiManager;
    private bool cardDropsActive = false;
    private List<Card> currentCards = new List<Card>();

    private void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
    }

    public void ActivateCardDrops()
    {
        cardDropsActive = true;
    }

    public bool AreCardDropsActive()
    {
        return cardDropsActive;
    }

    public void ShowCardSelection()
    {
        if (!cardDropsActive) return;

        currentCards.Clear();
        List<Card> tempCards = new List<Card>(availableCards);

        // Rastgele kartları seç
        for (int i = 0; i < cardsToShow && tempCards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempCards.Count);
            currentCards.Add(tempCards[randomIndex]);
            tempCards.RemoveAt(randomIndex);
        }

        // UI'da kartları göster
        uiManager.ShowCardSelectionUI(currentCards.ToArray());
    }

    public void SelectCard(int index)
    {
        if (index < 0 || index >= currentCards.Count) return;

        currentCards[index].onCardSelected?.Invoke();
    }

    // Örnek kart yetenekleri
    [System.Obsolete]
    public void IncreasePlayerSpeed()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.IncreaseMoveSpeed(2f);
        }
    }

    [System.Obsolete]
    public void IncreaseTorchPower()
    {
        TorchLightController[] torches = FindObjectsOfType<TorchLightController>();
        foreach (var torch in torches)
        {
            torch.AddWood();
            torch.AddWood();
        }
    }

    [System.Obsolete]
    public void IncreaseWoodGatheringRate()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.IncreaseWoodGatheringRate(1.5f);
        }
    }
}