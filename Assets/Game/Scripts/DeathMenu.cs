using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // For UI effects like blur

public class DeathMenu : MonoBehaviour {
    InputManager inputManager;
    CutsceneTrigger cutsceneTrigger;
    public GameObject deathMenuUI; // Assign the death menu panel in the Inspector
    public string mainMenuSceneName = "MainMenuScene"; // Set the name of your main menu scene

    [Header("UI Effects")]
    public Image blurOverlay; // Reference to an Image component for blurring the background

    private PlayerMovement playerMovement;

    void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        cutsceneTrigger = FindFirstObjectByType<CutsceneTrigger>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        // Ensure the death menu is hidden at the start
        if (deathMenuUI != null) {
            deathMenuUI.SetActive(false);
        }
        else {
            Debug.LogError("Death Menu UI Panel is not assigned in the Inspector!");
        }
    }

    void Update() {
        // Check player health only if player is not already dead
        if (playerMovement != null && playerMovement.currentHealth <= 0) {
            ShowDeathMenu();
        }
    }

    void ShowDeathMenu() {
        if (deathMenuUI != null) {
            deathMenuUI.SetActive(true);

            // Enable blur effect if available
            if (blurOverlay != null) {
                blurOverlay.enabled = true;
            }
        }

        Time.timeScale = 0f; // Freeze time
        inputManager.isPaused = true;

        // Show cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Player died. Death menu shown.");
    }

    public void RestartLevel() {
        // Reset time scale before reloading the scene
        Time.timeScale = 1f;
        inputManager.isPaused = false;

        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        Debug.Log("Restarting level: " + currentScene.name);
    }

    public void ExitToMainMenu() {
        // Reset time scale before loading the main menu
        Time.timeScale = 1f;

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);

        Debug.Log("Loading Main Menu...");
    }
}