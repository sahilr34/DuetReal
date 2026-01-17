using UnityEngine;
using UnityEngine.SceneManagement;

public class DuetPCController : MonoBehaviour
{
    public float rotateSpeed = 180f;
    public string gameOverSceneName = "GameOver";

    private float dir = 0f;

    private void Update()
    {
        // 
        if (Input.GetKey(KeyCode.A)) dir = -1f; // Left key  rotate left
        else if (Input.GetKey(KeyCode.D)) dir = 1f; // Right key  rotate right
        else dir = 0f;

        // 
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
                {
                    if (touch.position.x < Screen.width / 2)
                        dir = -1f; // Left half  rotate left
                    else
                        dir = 1f;  // Right half  rotate right
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    dir = 0f; // Stop when finger lifts
                }
            }
        }

        // Rotate
        transform.Rotate(0f, 0f, dir * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
                scoreManager.StopAndSaveScore();

            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
