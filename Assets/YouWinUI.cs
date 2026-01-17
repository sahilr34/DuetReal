using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class YouWinUI : MonoBehaviour
{
    public Button restartButton;
    public Button mainMenuButton;

    public string gameSceneName = "GameScene";
    public string mainMenuSceneName = "MainMenu";

    private void Start()
    {
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
