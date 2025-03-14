using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private string[] deathTags = { "PhaseableWall", "PhaseableWallSpawn" }; // Multiple tags
    [SerializeField] private float deathYThreshold = -10f; // Y-level for falling off
    [SerializeField] private float minVelocityThreshold = 0.1f; // Min velocity before dying
    [SerializeField] private float gracePeriod = 3f; // Time before player can die

    [Header("UI Elements")]
    [SerializeField] private GameObject deathScreen; // Assign in the Inspector
    [SerializeField] private Transform spawnPoint; // Assign this in the Inspector

    [SerializeField] GameManager gameManager;

    private float elapsedTime = 0f;
    private bool isDead = false;
    private Rigidbody rb;
    private bool canDie = false; // Prevent death before grace period ends
    private bool deathEnabled = false;
    private bool connectionEstablished = false;

    private Movement movementScript; // Reference to movement script

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<Movement>(); // Get movement script
    }

    private void Update()
    {
        if (gameManager == null || Input.GetKeyDown(KeyCode.Delete))
        {
            connectionEstablished = true;
            Debug.Log("GameManager is null. Free play enabled.");
        }
        else
        {
            if (gameManager.ConnectionSuccess())
            {
                connectionEstablished = true;
                Debug.Log("Connection Established.");
            }
        }

        if (!connectionEstablished) return;

        if (!deathEnabled)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= gracePeriod)
            {
                EnableDeath();
                deathEnabled = true;
            }
        }

        if (movementScript != null && movementScript.IsSliding)
        {
            return; // Player is safe while sliding
        }

        if (canDie && transform.position.y < deathYThreshold && !isDead)
        {
            Die();
        }

        if (canDie && rb.linearVelocity.magnitude < minVelocityThreshold && !isDead)
        {
            Debug.Log("Player velocity too low! Died.");
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (movementScript != null && movementScript.IsSliding)
        {
            return; // Player is safe while sliding
        }

        if (canDie && IsDeathTag(collision.gameObject.tag) && !isDead)
        {
            Die();
        }
    }

    private bool IsDeathTag(string tag)
    {
        foreach (string deathTag in deathTags)
        {
            if (tag == deathTag)
                return true;
        }
        return false;
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
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point is not set! Assign a spawn point in the Inspector.");
            return;
        }

        // Reset player position
        transform.position = spawnPoint.position;
        rb.linearVelocity = Vector3.zero; // Reset velocity
        isDead = false;
        canDie = false; // Give the player a brief grace period after respawn

        // Hide death screen
        deathScreen.SetActive(false);
        FindObjectOfType<MusicManager>().ResetMusicManager();

        // Resume time
        Time.timeScale = 1f;

        // Start grace period again
        elapsedTime = 0f;
        deathEnabled = false;

        Debug.Log("Player Respawned!");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Ensure Main Menu is Scene 0 in Build Settings
    }
}
