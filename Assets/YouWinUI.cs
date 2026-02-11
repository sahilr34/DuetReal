using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class YouWinUI : MonoBehaviour
{
    public Text finalScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    public string gameSceneName = "GameScene";
    public string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // Get final score from PlayerPrefs and display it
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        finalScoreText.text = "Target Score: " + finalScore;

        restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(gameSceneName);
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(mainMenuSceneName);
        });
    }
}