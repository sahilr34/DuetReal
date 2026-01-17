using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    // Scene names you want to load
    public string endlessSceneName = "Endless";
    public string chaseSceneName = "Chase";

    // Called when Endless button is pressed
    public void PlayEndless()
    {
        SceneManager.LoadScene(endlessSceneName);
    }

    // Called when Chase button is pressed
    public void PlayChase()
    {
        SceneManager.LoadScene(chaseSceneName);
    }
}
