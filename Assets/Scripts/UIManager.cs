using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private UIDocument document;
    private VisualElement root;
    private VisualElement gameUI;

    private Label timerLabel;
    private Label scoreLabel;
    private Label woodCounter;
    private ProgressBar healthBar;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        gameUI = root.Q<VisualElement>("game-ui");
        timerLabel = root.Q<Label>("timer");
        scoreLabel = root.Q<Label>("score");
        woodCounter = root.Q<Label>("wood-counter");
        healthBar = root.Q<ProgressBar>("health-bar");
    }

    public void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerLabel.text = $"Süre: {minutes:00}:{seconds:00}";
    }

    public void UpdateScore(int score)
    {
        scoreLabel.text = $"Skor: {score}";
    }

    public void UpdateWoodCount(int count)
    {
        woodCounter.text = $"Odun: {count}";
    }

    public void UpdateHealth(float current, float max)
    {
        healthBar.value = current / max * 100;
    }
}
