using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public Text scoreText;

    [Header("Score Settings")]
    private int score = 0;
    private bool isScoring = true;
    private int nextAdScore = 3;

    [Header("Win Condition")]
    public int targetScore = 40;
    public string youWinSceneName = "YouWin";
    public string chaseSceneName = "Chase";
    public string endlessSceneName = "Endless";

    public static ScoreManager Instance; // Singleton बनाएं

    private bool allowWinCondition = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Scene बदलने पर नष्ट न हो
            SceneManager.sceneLoaded += OnSceneLoaded; // Scene change track करें
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // हर नए scene में UI reference update करें
        FindScoreText();

        // अगर restart scene है तो score reset करें
        string currentScene = scene.name;
        if (currentScene == chaseSceneName || currentScene == endlessSceneName)
        {
            ResetScore(); // Score reset करें
            StartScoring(); // फिर से scoring start करें
        }
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        allowWinCondition = currentScene == chaseSceneName;

        FindScoreText();
        UpdateScoreText();

        // Cancel any existing invoke
        CancelInvoke(nameof(AddScore));
        StartScoring();
    }

    private void FindScoreText()
    {
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.FindGameObjectWithTag("ScoreText");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<Text>();
            }
        }
    }

    private void StartScoring()
    {
        isScoring = true;
        CancelInvoke(nameof(AddScore));
        InvokeRepeating(nameof(AddScore), 1f, 1f);
    }

    public void ResetScore()
    {
        score = 0;
        nextAdScore = 10;
        isScoring = false;
        UpdateScoreText();
    }

    private void AddScore()
    {
        if (!isScoring) return;

        score++;
        UpdateScoreText();

        if (score >= nextAdScore)
        {
            ShowAd();
            nextAdScore += 19;
        }

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
        CancelInvoke(nameof(AddScore));
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
        SceneManager.LoadScene(youWinSceneName);
    }

    public void StopAndSaveScore()
    {
        isScoring = false;
        CancelInvoke(nameof(AddScore));
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
    }

    private void ShowAd()
    {
        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowInterstitialAd();
            Debug.Log("Showing Interstitial Ad at score: " + score);
        }
        else
        {
            Debug.LogWarning("AdManager not found in scene!");
        }
    }

    // Restart game method
    public void RestartGame()
    {
        // Save current score
        StopAndSaveScore();

        // Use AdManager to show ad before restarting
        if (AdManager.Instance != null)
        {
            AdManager.Instance.RequestRestartWithAd(() =>
            {
                // यह callback ad के बाद call होगा
                ResetScore();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
        else
        {
            // No AdManager, just restart
            ResetScore();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}