using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private string deathTag = "PhaseableWall"; // What kills the player
    [SerializeField] private float deathYThreshold = -10f; // Y-level for falling off
    [SerializeField] private float minVelocityThreshold = 0.1f; // Min velocity before dying
    [SerializeField] private float gracePeriod = 3f; // Time before player can die

    [Header("UI Elements")]
    [SerializeField] private GameObject deathScreen; // Assign in the Inspector

    [SerializeField] GameManager gameManager;

    private bool isDead = false;
    private Rigidbody rb;
    private bool canDie = false; // Prevent death before grace period ends

    private bool connectionEstablished = false; // Track if connected or override active

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Invoke(nameof(EnableDeath), gracePeriod); // Enable death after grace period
    }

    private void Update()
    {
        // Check if GameManager exists
        if (gameManager == null || Input.GetKeyDown(KeyCode.Delete))
        {
            // No GameManager? Allow movement by default
            connectionEstablished = true;
            Debug.Log("GameManager is null. Free play enabled.");
        }
        else
        {
            // GameManager exists? Wait for connection
            if (gameManager.ConnectionSuccess())
            {
                connectionEstablished = true;
                Debug.Log("Connection Established.");
            }
        }

        if (!connectionEstablished) return;

        // Check if player falls below the threshold (only after grace period)
        if (canDie && transform.position.y < deathYThreshold && !isDead)
        {
            Die();
        }

        // Check if player stops moving (only after grace period)
        if (canDie && rb.linearVelocity.magnitude < minVelocityThreshold && !isDead)
        {
            Debug.Log("Player velocity too low! Died.");
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for a normal collision with the wall (only after grace period)
        if (canDie && collision.gameObject.CompareTag(deathTag) && !isDead)
        {
            Die();
        }
    }

    private void EnableDeath()
    {
        canDie = true;
        Debug.Log("Grace period over. Player can now die.");
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
