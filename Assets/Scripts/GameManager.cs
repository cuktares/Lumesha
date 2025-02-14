using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Oyun Ayarlari")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "GameScene";
    
    private bool isGameOver = false;

    public void EndGame(bool isWin)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log(isWin ? "Oyun Kazanildi!" : "Oyun Kaybedildi!");
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
