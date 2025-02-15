using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private UIDocument document;
    private VisualElement root;
    private VisualElement mainMenu;
    private VisualElement gameUI;
    private VisualElement pauseMenu;
    private VisualElement gameOver;

    private Label timerLabel;
    private Label scoreLabel;
    private Label woodCounter;
    private ProgressBar healthBar;
    private Label finalScore;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        // UI elementlerini al
        mainMenu = root.Q<VisualElement>("main-menu");
        gameUI = root.Q<VisualElement>("game-ui");
        pauseMenu = root.Q<VisualElement>("pause-menu");
        gameOver = root.Q<VisualElement>("game-over");

        timerLabel = root.Q<Label>("timer");
        scoreLabel = root.Q<Label>("score");
        woodCounter = root.Q<Label>("wood-counter");
        healthBar = root.Q<ProgressBar>("health-bar");
        finalScore = root.Q<Label>("final-score");

        // Buton olaylarını ayarla
        SetupButtons();
    }

    private void SetupButtons()
    {
        root.Q<Button>("start-button").clicked += StartGame;
        root.Q<Button>("credits-button").clicked += ShowCredits;
        root.Q<Button>("quit-button").clicked += QuitGame;
        root.Q<Button>("resume-button").clicked += ResumeGame;
        root.Q<Button>("restart-button").clicked += RestartGame;
        root.Q<Button>("menu-button").clicked += ReturnToMainMenu;

        root.Q<Slider>("volume-slider").RegisterValueChangedCallback(evt => 
        {
            AudioListener.volume = evt.newValue;
        });
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

    private void ShowMainMenu()
    {
        mainMenu.style.display = DisplayStyle.Flex;
        gameUI.style.display = DisplayStyle.None;
        pauseMenu.style.display = DisplayStyle.None;
        gameOver.style.display = DisplayStyle.None;
    }

    private void StartGame()
    {
        mainMenu.style.display = DisplayStyle.None;
        gameUI.style.display = DisplayStyle.Flex;
        Time.timeScale = 1f;
    }

    private void ShowCredits()
    {
        // Kredi ekranını göster
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void ResumeGame()
    {
        pauseMenu.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void ReturnToMainMenu()
    {
        ShowMainMenu();
        Time.timeScale = 0f;
    }

    public void ShowGameOver(int finalScoreValue)
    {
        gameOver.style.display = DisplayStyle.Flex;
        gameUI.style.display = DisplayStyle.None;
        finalScore.text = $"Final Skor: {finalScoreValue}";
        Time.timeScale = 0f;
    }
}
