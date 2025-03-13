using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private string deathTag = "PhaseableWall"; // What kills the player
    [SerializeField] private float deathYThreshold = -10f; // Y-level for falling off

    [Header("UI Elements")]
    [SerializeField] private GameObject deathScreen; // Assign in the Inspector

    private bool isDead = false;

    private void Update()
    {
        // Check if player falls below the threshold
        if (transform.position.y < deathYThreshold && !isDead)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(deathTag) && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("You Died!");

        // Stop time & show death screen
        Time.timeScale = 0f;
        deathScreen.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Ensure Main Menu is Scene 0 in Build Settings
    }
}
