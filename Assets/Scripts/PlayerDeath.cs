using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private string deathTag = "PhaseableWall"; // What kills the player
    [SerializeField] private float deathYThreshold = -10f; // Y-level for falling off
    [SerializeField] private float minVelocityThreshold = 0.1f; // Min velocity before dying

    [Header("UI Elements")]
    [SerializeField] private GameObject deathScreen; // Assign in the Inspector

    private bool isDead = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Check if player falls below the threshold
        if (transform.position.y < deathYThreshold && !isDead)
        {
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for a normal collision with the wall
        if (collision.gameObject.CompareTag(deathTag) && !isDead)
        {
            Die();
        }

        // Check if player stops moving
        if (rb.velocity.magnitude < minVelocityThreshold && !isDead)
        {
            Debug.Log("Player velocity too low! Died.");
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
