using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public Text finalScoreText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button rewardButton;

    [Header("Scene Names")]
    public string gameSceneName = "Level1";
    public string mainMenuSceneName = "Mainmenu";

    private int finalScore;
    private bool rewardClaimed = false;

    private void Start()
    {
        finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        finalScoreText.text = "Final Score: " + finalScore;

        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        rewardButton.onClick.AddListener(ShowRewardedAd);

        // Subscribe to reward event
        if (AdManager.Instance != null)
            AdManager.Instance.OnRewardEarned += DoubleScore;
    }

    private void OnDestroy()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.OnRewardEarned -= DoubleScore;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ShowRewardedAd()
    {
        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewardedAd();
        }
    }

    private void DoubleScore()
    {
        if (rewardClaimed) return; // prevent multiple rewards

        rewardClaimed = true;
        finalScore *= 2;
        PlayerPrefs.SetInt("FinalScore", finalScore);
        finalScoreText.text = "Final Score: " + finalScore;

        rewardButton.interactable = false; // disable after claiming
        Debug.Log("🎯 Score doubled after watching ad!");
    }
}
