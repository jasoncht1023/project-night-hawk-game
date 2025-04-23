using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Assuming you might use the new Input System

public class PauseMenu : MonoBehaviour {
    InputManager inputManager;

    public GameObject pauseMenuUI; // Assign the pause menu panel in the Inspector
    public string mainMenuSceneName = "MainMenuScene"; // Set the name of your main menu scene

    void Start() {
        inputManager = FindFirstObjectByType<InputManager>();

        // Ensure the pause menu is hidden at the start
        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(false);

        }
        else {
            Debug.LogError("Pause Menu UI Panel is not assigned in the Inspector!");
        }

        // Optional: Get PlayerInput if needed for disabling actions
        // playerInput = FindObjectOfType<PlayerInput>();
    }

    void Update() {
        // Using old Input Manager for simplicity, adjust if using new Input System's actions
        if (inputManager.pauseGameInput) {
            inputManager.pauseGameInput = false;
            if (inputManager.isPaused) {
                ResumeGame();
            }
            else {
                PauseGame();
            }
        }
    }

    public void ResumeGame() {
        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f; // Resume time
        inputManager.isPaused = false;
        // Optional: Re-enable player input if disabled during pause
        // if (playerInput != null) playerInput.ActivateInput();
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor again for gameplay
        Cursor.visible = false; // Hide cursor
        Debug.Log("Game Resumed");
    }

    void PauseGame() {
        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f; // Freeze time
        inputManager.isPaused = true;
        // Optional: Disable player input actions if needed
        // if (playerInput != null) playerInput.DeactivateInput();
        Cursor.lockState = CursorLockMode.None; // Unlock cursor
        Cursor.visible = true; // Show cursor for UI interaction
        Debug.Log("Game Paused");
    }

    public void LoadMainMenu() {
        Time.timeScale = 1f; // Ensure time scale is reset before leaving the scene
        SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log("Loading Main Menu...");
    }

}