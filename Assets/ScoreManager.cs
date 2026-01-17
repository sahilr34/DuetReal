using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//  This script works with your AdManager singleton (AdManager.Instance)
public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public Text scoreText; // UI Text for in-game score

    [Header("Score Settings")]
    private int score = 0;
    private bool isScoring = true;
    private int nextAdScore = 3; // Next score threshold to show ad (10, 20, 30...)

    [Header("Win Condition")]
    public int targetScore = 40; // Score needed to win
    public string youWinSceneName = "YouWin"; // Name of YouWin scene
    public string chaseSceneName = "Chase"; // Scene where win condition is active
    public string endlessSceneName = "Endless"; // Endless scene (no win)

    private bool allowWinCondition = false;

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Win condition only allowed in Chase scene
        allowWinCondition = currentScene == chaseSceneName;

        UpdateScoreText();

        // Start scoring every second
        InvokeRepeating(nameof(AddScore), 1f, 1f);
    }

    private void AddScore()
    {
        if (!isScoring) return;

        score++;
        UpdateScoreText();

        // ?? Show ad when score hits 10, 20, 30, etc.
        if (score >= nextAdScore)
        {
            ShowAd();
            nextAdScore += 3; // Set next ad trigger
        }

        // ?? Check for win condition
        if (allowWinCondition && score >= targetScore)
        {
            WinGame();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void WinGame()
    {
        isScoring = false;
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
        SceneManager.LoadScene(youWinSceneName);
    }

    public void StopAndSaveScore()
    {
        isScoring = false;
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
    }

    //  Safely calls your AdManager to show an interstitial ad
    private void ShowAd()
    {
        // Make sure AdManager exists before calling
        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowInterstitialAd();
            Debug.Log("Showing Interstitial Ad at score: " + score);
        }
        else
        {
            Debug.LogWarning(" AdManager not found in scene!");
        }
    }
}
