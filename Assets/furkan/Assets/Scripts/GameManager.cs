using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    [Header("UI Screens")]
    public GameObject pauseScreen;
    public GameObject winScreen;
    public GameObject loseScreen;

    private bool isPaused = false;
    private bool gameOver = false;

    void Start()
    {
        // Double-check that all screens are hidden when the game starts
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }

    void Update()
    {
        // --- PAUSE LOGIC ---
        // Toggle pause when the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        // --- TEST HOTKEYS ---
        // These let you manually trigger the screens to make sure they look right!
        // Delete these lines later when you add actual gameplay mechanics.
        if (Input.GetKeyDown(KeyCode.Y) && !gameOver) WinGame();
        if (Input.GetKeyDown(KeyCode.U) && !gameOver) LoseGame();
    }

    // --- SCREEN FUNCTIONS ---

    public void PauseGame()
    {
        pauseScreen.SetActive(true);
        Time.timeScale = 0f; // Freezes game time
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseScreen.SetActive(false);
        Time.timeScale = 1f; // Unfreezes game time
        isPaused = false;
    }

    public void WinGame()
    {
        winScreen.SetActive(true);
        Time.timeScale = 0f; // Freezes the game upon winning
        gameOver = true;
    }

    public void LoseGame()
    {
        loseScreen.SetActive(true);
        Time.timeScale = 0f; // Freezes the game upon losing
        gameOver = true;
    }

    // --- BUTTON FUNCTIONS ---

    public void RestartGame()
    {
        // Make sure time is unfrozen before reloading, or the new scene will be frozen too!
        Time.timeScale = 1f; 
        
        // Reloads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}