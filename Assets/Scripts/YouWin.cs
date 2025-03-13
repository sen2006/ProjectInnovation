using UnityEngine;
using UnityEngine.SceneManagement;

public class YouWin : MonoBehaviour
{
    [Header("Win Settings")]
    [SerializeField] private string playerTag = "Player"; // Tag for the player
    [SerializeField] private GameObject winScreen; // Assign this in the Inspector

    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasWon)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        hasWon = true;
        Debug.Log("You Win!");

        // Pause the game
        Time.timeScale = 0f;
        winScreen.SetActive(true);
    }
}
